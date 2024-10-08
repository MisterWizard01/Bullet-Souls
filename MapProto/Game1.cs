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

/* Bugs/crashes:
 * game enters infinite loop when player is squeezed between an enemy and a wall
 * player gets ejected from wall when dashing into it
 */

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
    private int _nextWave, _wave;

    private KeyboardState _prevKeyboardState;
    private int _frameNumber;

    public Game1()
    {
        _graphics = new(this);
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnResizeWindow;
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
        _nextWave = 60;
        _wave = 1;
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

        #region destroy too-old shots
        for (int i = _shots.CountChildren - 1; i >= 0; i--)
        {
            var shot = _shots.GetChild(i) as ShotNode;
            if (shot?.Age > 120)
            {
                _shots.RemoveChild(i);
            }
        }
        #endregion
        #region destroy dead enemies
        if (keyboardState.IsKeyDown(Keys.Q))
        {
            _enemies.RemoveChild(0);
            if (_enemies.CountChildren == 0)
                _nextWave = _frameNumber + 120;
        }
        #endregion
        #region update player and shots
        _player.Update(null, _frameNumber, inputState);
        _shots.Update(null, _frameNumber, inputState);
        #endregion
        #region update enemies' movement and collision
        for (int i = 0; i < _enemies.CountChildren; i++)
        {
            var enemy = _enemies.GetChild(i) as EnemyNode;
            var angle = i * Math.Tau / _enemies.CountChildren;
            enemy.Target = _player.Position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 20;
            enemy.Update(_enemies, _frameNumber, inputState);

            List<Collision> collisions;
            do
            { 
                collisions = CollisionManager.GetCollisions(enemy, _enemies);
                collisions.AddRange(CollisionManager.GetCollisions(enemy, _walls));
                if (collisions.Count == 0)
                    break;
                CollisionManager.SortCollisions(collisions);
                var response = CollisionManager.HandleSolidCollision(collisions[0], 0, 1);
                enemy.Position += response.Node1Translation;
            } while (collisions.Count > 0);
        }

        List<Collision> enemyCollisions = new List<Collision>();

        #endregion
        #region player shooting
        if (_player.Shoot)
        {
            var shot = new ShotNode(_player.Position, _player.Facing * 3, _contentManager.Sprites["player shot"]);
            _shots.Add(shot);
            _player.Shoot = false;
        }
        #endregion
        #region spawn enemies
        if (_enemies.CountChildren == 0 && _frameNumber >= _nextWave)
        {
            for (int i = 0; i < _wave; i++)
            {
                var angle = i * Math.Tau / _wave;
                var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Math.Max(_camera.Size.X, _camera.Size.Y);
                var enemyPosition = new Vector2((float)Math.Floor(offset.X + _camera.Size.X / 2), (float)Math.Floor(offset.Y) + _camera.Size.Y / 2);
                var enemy = new EnemyNode(enemyPosition, _contentManager.Sprites["enemy"]);
                _enemies.Add(enemy);
            }
            _wave++;
        }
        #endregion
        #region solid player collisions
        Node2D coupledLeft = null, coupledRight = null, coupledUp = null, coupledDown = null;
        List<Collision> playerCollisions;
        do
        {
            //get collisions between the player and walls
            playerCollisions = CollisionManager.GetCollisions(_player, _walls);
            playerCollisions.AddRange(CollisionManager.GetCollisions(_player, _enemies));
            if (playerCollisions.Count == 0)
                break;
            //find the earliest collision
            CollisionManager.SortCollisions(playerCollisions);
            //calculate the correct response
            var response = CollisionManager.HandleSolidCollision(playerCollisions[0], 0, 1);
            //using the response, find out if any other nodes would be affected
            Node2D coupling = null;
            if (response.Node1Translation.X > 0 || response.Node2Translation.X < 0)
            {
                coupledLeft = playerCollisions[0].Node2;
                coupling = coupledRight;
            }
            else if (response.Node1Translation.X < 0 || response.Node2Translation.X > 0)
            {
                coupledRight = playerCollisions[0].Node2;
                coupling = coupledLeft;
            }

            if (response.Node1Translation.Y > 0 || response.Node2Translation.Y < 0)
            {
                coupledUp = playerCollisions[0].Node2;
                coupling = coupledDown;
            }
            else if (response.Node1Translation.Y < 0 || response.Node2Translation.Y > 0)
            {
                coupledDown = playerCollisions[0].Node2;
                coupling = coupledUp;
            }

            //see if we're coupled to a wall
            if (coupling is ColliderNode)
            {
                //recalculate the response because the player can't move
                playerCollisions[0].Node2.Position -= response.Node1Translation;
            }
            else
            {
                //move the player and all coupled nodes based on the original collision response
                _player.Position += response.Node1Translation;
                _player.Position += response.Node1Velocity * (1 - playerCollisions[0].Time);
                if (coupling is not null)
                {
                    coupling.Position += response.Node1Translation;
                    coupling.Position += response.Node1Velocity * (1 - playerCollisions[0].Time);
                }
            }

            if (_player.State == PlayerNode.PlayerStates.Sprint)
                _player.StopSprinting();
            if (_player.State == PlayerNode.PlayerStates.Slide)
                _player.SetSlideVector(response.Node1Velocity * 0.8f);
        }
        while (playerCollisions.Count > 0);
        #endregion

        //move the camera
        _camera.Position = (_player.Position - _camera.Size.ToVector2() / 2).ToPoint();

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

        DrawHitbox(_player, Vector2.Zero, Color.LightBlue);
        for (int i = 0; i < _enemies.CountChildren; i++)
        {
            var enemy = _enemies.GetChild(i) as EnemyNode;
            DrawHitbox(enemy, Vector2.Zero, Color.White);
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
    /*
    protected void EnemyEnemyCollisions(EnemyNode enemy1)
    {
        var collisions = new List<Collision>();
        var collidedWith = new List<EnemyNode>();
        for (int j = 0; j < _enemies.CountChildren; j++)
        {
            var enemy2 = _enemies.GetChild(j) as EnemyNode;
            var collision = CollisionManager.CheckCollision(enemy1.Collider, enemy2.Collider, enemy1.Position, enemy2.Position, enemy1.PreviousPosition, enemy2.PreviousPosition);
            if (collision is not null)
            {
                collisions.Add(collision.Value);
                collidedWith.Add(enemy2);
            }
        }

        //sort the collisions
        for (int i = 0; i < collisions.Count - 1; i++)
        {
            for (int j = 0; j < collisions.Count; j++)
            {
                if (collisions[i].Time > collisions[j].Time)
                {
                    (collisions[i], collisions[j]) = (collisions[j], collisions[i]);
                    (collidedWith[i], collidedWith[j]) = (collidedWith[j], collidedWith[i]);
                }
            }
        }

        for (int i = 0; i < collisions.Count; i++)
        {
            //get the colliding enemies and handle the collision
            var earliestCollision = collisions[i];
            var enemy2 = collidedWith[i];
            var collisionResponse = CollisionManager.HandleSolidCollision(earliestCollision, 1, 1);
            enemy1.Position += collisionResponse.Node1Translation;
            enemy2.Position += collisionResponse.Node2Translation;
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
            //if the enemy isn't moving, just ignore it
            if (enemy.Velocity.LengthSquared() == 0)
            {
                totalCollisions -= collisions[earliestIndex].Count;
                collisions.RemoveAt(earliestIndex);
                continue;
            }
            var collisionResponse = CollisionManager.HandleSolidCollision(earliestCollision, enemy.Velocity, Vector2.Zero, 0, 1);
            enemy.Position += collisionResponse.Node1Translation;

            //update that enemy's collisions in the master list
            totalCollisions -= collisions[earliestIndex].Count;
            collisions[earliestIndex] = CollisionManager.GetCollisions(enemy, _walls, Vector2.Zero, Vector2.Zero);
            totalCollisions += collisions[earliestIndex].Count;
        }
    }
    */
}