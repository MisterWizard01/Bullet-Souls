using BulletSoulsLibrary;
using Engine;
using Engine.Managers;
using Engine.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MapProto;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RasterizerState _rasterizerState;
    private readonly InputManager _inputManager;
    private readonly JuicyContentManager _contentManager;
    private readonly CustomTiledParser _tiledParser;

    private Random _random;
    private Effect _betterBlend;
    private Camera _camera;
    private PlayerNode _player;
    private Node _tileLayers, _walls;
    private ContainerNode _shots, _enemies;
    private int _nextEnemy;

    private KeyboardState _prevKeyboardState;
    private int _frameNumber;

    public Game1()
    {
        _graphics = new(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _inputManager = new(InputMode.KeyboardOnly);
        _contentManager = new();
        _tiledParser = new();
    }

    protected override void Initialize()
    {
        PlayerNode.DefaultControls(_inputManager);
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _camera = new(_spriteBatch, new Rectangle(0, 0, 192, 144));
        _shots = new("shot");
        _enemies = new("enemies");
        _nextEnemy = 60;
        _rasterizerState = new() { ScissorTestEnable = true };
        OnResizeWindow(null, null);
        _frameNumber = 0;
        _random = new Random();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        var whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        whitePixel.SetData(new Color[] { Color.White });
        _contentManager.Textures.Add("whitePixel", whitePixel);

        _betterBlend = Content.Load<Effect>("betterBlend");

        var commonFolder = FileManager.GetCommonFolder();
        _contentManager.LoadTextures(Content, FileManager.GetContentFolder());
        _contentManager.LoadSprites(Path.Combine(commonFolder, "Sprites.json"));
        _contentManager.LoadTilesets(Path.Combine(commonFolder, "Tilesets.json"));
        //LoadSceneFromSave(Path.Combine(commonFolder, "data\\map.json"));
        LoadSceneFromTiled(Path.Combine(commonFolder, "tiled\\test.json"));
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
            Exit();
        //if (keyboardState.IsKeyDown(Keys.F1) && _prevKeyboardState.IsKeyUp(Keys.F1))
        //    ToggleFullScreen();
        _inputManager.Update(_camera.GameToView(_player.Position));
        var inputState = _inputManager.InputState;

        //destroy objects
        for (int i = _shots.CountChildren - 1; i >= 0; i--)
        {
            var shot = _shots.GetChild(i) as ShotNode;
            if (shot?.Age > 120)
            {
                _shots.RemoveChild(i);
            }
        }

        //update objects
        _player.Update(null, _frameNumber, inputState);
        _shots.Update(null, _frameNumber, inputState);

        for (int i = 0; i < _enemies.CountChildren; i++)
        {
            var enemy = _enemies.GetChild(i) as EnemyNode;
            enemy.Target = _player.Position;
        }
        _enemies.Update(null, _frameNumber, inputState);

        //create objects
        if (_player.Shoot)
        {
            var shot = new ShotNode(_player.Position, _player.Facing * 3, _contentManager.Sprites["player shot"]);
            _shots.Add(shot);
            _player.Shoot = false;
        }

        if (_frameNumber >= _nextEnemy)
        {
            var angle = _random.NextDouble() * Math.Tau;
            var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Math.Max(_camera.Size.X, _camera.Size.Y);
            var enemyPosition = new Vector2((float)Math.Floor(_player.X + offset.X), (float)Math.Floor(_player.Y + offset.Y));
            var enemy = new EnemyNode(enemyPosition, _contentManager.Sprites["enemy"]);
            _enemies.Add(enemy);
            _nextEnemy += 300;
        }

        //resuable list
        List<Collision> collisions;

        //collisions player x walls
        do
        {
            collisions = CollisionManager.GetCollisions(_player, _walls, _player.Velocity, Vector2.Zero);
            //Debug.WriteLine(wallCollisions.Count + " collisions");

            if (collisions.Count > 0)
            {
                //find earliest collision
                var earliestCollision = collisions[0];
                for (int i = 1; i < collisions.Count; i++)
                {
                    if (collisions[i].Time < earliestCollision.Time)
                    {
                        earliestCollision = collisions[i];
                    }
                }

                //react to the collision
                var response = CollisionManager.HandleSolidCollision(earliestCollision, _player.Velocity, Vector2.Zero, 0, 1);
                _player.Position += response.Node1Translation;
            }
        }
        while (collisions.Count > 1);

        EnemyWallCollisions();

        //move the camera
        _camera.Position = (BetterPoint)(_player.Position - _camera.Size.ToVector2() / 2);

        //ready next frame
        _prevKeyboardState = keyboardState;
        _frameNumber++;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            rasterizerState: _rasterizerState,
            effect: _betterBlend,
            blendState: BlendState.NonPremultiplied
        );
        _spriteBatch.GraphicsDevice.ScissorRectangle = _camera.ViewRect;

        _camera.Draw(_contentManager.Textures["whitePixel"], _camera.GameRect, new(38, 92, 66)); // dark green
        _tileLayers.Draw(null, _camera, Vector2.Zero);

        _player.Draw(null, _camera, Vector2.Zero);
        _shots.Draw(null, _camera, Vector2.Zero);
        _enemies.Draw(null, _camera, Vector2.Zero);

        _tileLayers.GetChild("Wall Tops").Draw(null, _camera, Vector2.Zero);

        DrawHitbox(_player.GetChild("collider") as ColliderNode, _player.Position, Color.LightBlue);
        for (int i = 0; i < _enemies.CountChildren; i++)
        {
            var enemy = _enemies.GetChild(i) as EnemyNode;
            DrawHitbox(enemy.GetChild("collider") as ColliderNode, enemy.Position, Color.White);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    protected void DrawHitbox(ColliderNode collider, Vector2 referencePoint, Color color)
    {
        var destination = new Rectangle(
            (collider.Position + referencePoint - collider.Dimensions / 2).ToPoint(),
            collider.Dimensions.ToPoint());
        _camera.Draw(_contentManager.Textures["whitePixel"], destination, color);
    }

    protected void LoadSceneFromTiled(string filePath)
    {
        using StreamReader reader = new(filePath);
        var json = reader.ReadToEnd();
        var jObject = JObject.Parse(json);

        var scene = _tiledParser.ParseMap(_contentManager, jObject);
        if (scene == null)
        {
            Debug.WriteLine("Could not read JSON sprite file.");
            return;
        }

        _player = scene.GetChild("Player").GetChild("player") as PlayerNode;
        _player.Initialize();

        _walls = scene.GetChild("Walls");
        _walls.Initialize();

        _tileLayers = new Node();
        for (int i = 0; i < scene.CountChildren; i++)
        {
            if (scene.GetChild(i) is TileLayerNode tileLayer)
            {
                _tileLayers.AddChild(scene.GetChildName(i), tileLayer);
            }
        }
    }

    protected void OnResizeWindow(object sender, EventArgs e)
    {
        int xScale = Math.Max(1, Window.ClientBounds.Width / _camera.GameRect.Width);
        int yScale = Math.Max(1, Window.ClientBounds.Height / _camera.GameRect.Height);
        int scale = Math.Min(xScale, yScale);
        Point size = new(_camera.GameRect.Width * scale, _camera.GameRect.Height * scale);
        Point location = new((Window.ClientBounds.Width - size.X) / 2, (Window.ClientBounds.Height - size.Y) / 2);
        _camera.ViewRect = new Rectangle(location, size);
    }

    protected void EnemyEnemyCollisions()
    {
        var collisions = new List<Collision>();
        for (int i = 0; i < _enemies.CountChildren - 1; i++)
        {
            var enemy1 = _enemies.GetChild(i);
            for (int j = i + 1; j < _enemies.CountChildren; j++)
            {
                var enemy2 = _enemies.GetChild(j);
                collisions.AddRange(CollisionManager.GetCollisions(enemy1, enemy2, Vector2.Zero, Vector2.Zero));
            }
        }

        if (collisions.Count > 0)
        {

        }
    }

    protected void EnemyWallCollisions()
    {
        var collisions = new List<List<Collision>>();
        var totalCollisions = 0;
        for (int i = 0; i < _enemies.CountChildren; i++)
        {
            var enemy = _enemies.GetChild(i);
            collisions.Add(CollisionManager.GetCollisions(enemy, _walls, Vector2.Zero, Vector2.Zero));
            totalCollisions += collisions[i].Count;

            //sort collisions for this enemy
            for (int j = 0; j < collisions[i].Count - 1; j++)
            {
                for (int k = j + 1; k < collisions[i].Count; k++)
                {
                    if (collisions[i][j].Time > collisions[i][k].Time)
                        (collisions[i][k], collisions[i][j]) = (collisions[i][j], collisions[i][k]);
                }
            }
        }

        while (totalCollisions > 0) {
            //figure out which enemy has the earliest collision
            var earliestIndex = -1;
            for (int i = 0; i < collisions.Count; i++)
            {
                if (collisions[i].Count > 0
                && (earliestIndex == -1 || collisions[i][0].Time < collisions[earliestIndex][0].Time))
                    earliestIndex = i;
            }

            //get the colliding enemy and handle the collision
            var earliestCollision = collisions[earliestIndex][0];
            var enemy = _enemies.GetChild(earliestIndex) as EnemyNode;
            var collisionResponse = CollisionManager.HandleSolidCollision(earliestCollision, enemy.Velocity, Vector2.Zero, 0, 1);
            enemy.Position += collisionResponse.Node1Translation;

            //update that enemy's collisions in the master list
            totalCollisions -= collisions[earliestIndex].Count;
            collisions[earliestIndex] = CollisionManager.GetCollisions(enemy, _walls, Vector2.Zero, Vector2.Zero);
            totalCollisions += collisions[earliestIndex].Count;
        }
    }
}