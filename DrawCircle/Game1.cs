using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Fluid;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private GameManager _gameManager;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Globals.Fps = 144;
        Globals.UISize = new Point(250, 0);
        Globals.GameBounds = new Point(1020, 720);
        Globals.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / Globals.Fps);
        Globals.GraphicsDevice = GraphicsDevice;
        _graphics.PreferredBackBufferWidth = Globals.GameBounds.X;
        _graphics.PreferredBackBufferHeight = Globals.GameBounds.Y;
        _graphics.ApplyChanges();

        
        Globals.Content = Content;
        _gameManager = new GameManager(this);
        
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

        base.Initialize();
    }
    
    private void Window_ClientSizeChanged(object sender, EventArgs e)
    {
        _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        Globals.GameBounds = new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        _graphics.ApplyChanges();
    }


    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Globals.SpriteBatch = _spriteBatch;

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
        TargetElapsedTime = Globals.TargetElapsedTime;

        Globals.Update(gameTime);
        _gameManager.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _spriteBatch.Begin();
        _gameManager.Draw();
        _spriteBatch.End();
        
        _gameManager.DrawUi(gameTime);
        base.Draw(gameTime);
    }
}