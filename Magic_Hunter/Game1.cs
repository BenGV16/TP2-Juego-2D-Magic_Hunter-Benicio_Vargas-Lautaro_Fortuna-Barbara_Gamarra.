using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Magic_Hunter.src;

namespace Magic_Hunter;

public class Game1 : Game
{
    private SpriteSheetManager _coliseumSheet;
    private int _currentFrame = 0;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public enum GameState
    {
        Menu,
        Playing
    }
    private GameState _currentState = GameState.Menu;
    private MenuManager _menuManager;
    private GamePlay _gamePlay;

    private KeyboardState _previousKeyboardState;
    private Point _windowedSize;
    private Point _windowedPosition;
    private SpriteFont _pixelFont;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        
        // CAMBIO: Ocultamos el mouse del sistema para usar la mirilla del escudo
        IsMouseVisible = false; 

        _graphics.SynchronizeWithVerticalRetrace = true;
        IsFixedTimeStep = true;
    }

    protected override void Initialize()
    {
        _windowedSize = new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        _windowedPosition = new Point(Window.Position.X, Window.Position.Y);
        FullscreenHelper.ApplyFullscreen(_graphics, Window, ref _windowedSize, ref _windowedPosition);
        _menuManager = new MenuManager();
        _gamePlay = new GamePlay();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Texture2D coliseumTexture = Content.Load<Texture2D>("pixilart-sprite(Escenario)");
        _coliseumSheet = new SpriteSheetManager(coliseumTexture, 640, 360);
        
        try { _pixelFont = Content.Load<SpriteFont>("PixelFont"); } catch { }
        
        _menuManager.Initialize(GraphicsDevice.Viewport, GraphicsDevice);
        _menuManager.LoadContent(_pixelFont);
        _gamePlay.Initialize(GraphicsDevice, GraphicsDevice.Viewport, Content);
    }

    protected override void Update(GameTime gameTime)
    {
        _currentFrame = (int)(gameTime.TotalGameTime.TotalSeconds / 0.2) % _coliseumSheet.FrameCount;
        var kb = Keyboard.GetState();
        if (kb.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
        {
            if (_currentState == GameState.Menu)
                Exit();
            else
            {
                _currentState = GameState.Menu;
                _gamePlay.Reset(); 
                IsMouseVisible = true; // Mostrar mouse en menú si se desea
            }
            return;
        }

        if (_currentState == GameState.Menu)
        {
            IsMouseVisible = true; // Asegurar mouse visible en menú
            var mouseState = Mouse.GetState();
            int selected = _menuManager.HandleInput(mouseState);
            if (selected == 0) 
            {
                _currentState = GameState.Playing;
                _gamePlay.Reset();
                IsMouseVisible = false; // Ocultar mouse al jugar
            }
            else if (selected == 1) System.Diagnostics.Debug.WriteLine("Options selected");
            else if (selected == 2) Exit();
        }
        else if (_currentState == GameState.Playing)
        {
            _gamePlay.Update(gameTime, GraphicsDevice.Viewport, Mouse.GetState());
            
            if (_gamePlay.IsGameOver)
            {
                _currentState = GameState.Menu;
                _gamePlay.Reset();
                IsMouseVisible = true;
            }
        }

        _previousKeyboardState = kb;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_coliseumSheet.Texture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), _coliseumSheet.GetFrame(_currentFrame), Color.White);

        if (_currentState == GameState.Menu)
        {
            _menuManager.Draw(_spriteBatch, GraphicsDevice.Viewport);
        }
        else if (_currentState == GameState.Playing)
        {
            _gamePlay.Draw(_spriteBatch);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}