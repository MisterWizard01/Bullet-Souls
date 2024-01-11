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

    private Effect _betterBlend;
    private Camera _camera;
    private PlayerNode _player;
    private Node _walls, _tileLayers;

    private KeyboardState _prevKeyboardState;
    private int frameNumber;

    public Game1()
    {
        _graphics = new(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _inputManager = new(InputMode.MouseAndKeyboard);
        _contentManager = new();
        _tiledParser = new();
    }

    protected override void Initialize()
    {
        PlayerNode.DefaultControls(_inputManager);
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _camera = new(_spriteBatch, new Rectangle(0, 0, 192, 144));
        _rasterizerState = new() { ScissorTestEnable = true };
        OnResizeWindow(null, null);
        frameNumber = 0;

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

        //update objects
        _player.Update(null, frameNumber, inputState);

        //collisions player x walls
        var wallCollisions = CollisionManager.GetCollisions(_player, _walls, _player.MoveVector, Vector2.Zero);
        Debug.WriteLine(wallCollisions.Count + " collisions");

        //ready next frame
        _prevKeyboardState = keyboardState;
        frameNumber++;
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

        _spriteBatch.End();
        base.Draw(gameTime);
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
}