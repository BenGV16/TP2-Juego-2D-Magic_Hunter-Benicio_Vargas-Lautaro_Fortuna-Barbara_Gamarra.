using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Magic_Hunter.src;

public class GamePlay
{
    private Texture2D _pixel;
    private List<Enemies> _entities = new();
    private MouseState _previousMouseState;
    private double _spawnTimer = 0;
    private double _spawnInterval;
    private ContentManager _content;
    private Texture2D _hadaTexture;
    private Texture2D _sapoTexture;
    private Texture2D _gusanoSaliendoTexture;
    private Texture2D _gusanoNormalTexture;
    float depth = 1.0f;
    private const int MaxBoxes = 10;
    private Random _random = new();
    private Texture2D _wandTexture;
    private PlayerWand _playerWand;
    public void Initialize(GraphicsDevice graphicsDevice, Viewport viewport, ContentManager content)
    {
        _content = content;
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        try
        {
            _hadaTexture = _content.Load<Texture2D>("pixilart-sprite(Hada1)");
            _sapoTexture = _content.Load<Texture2D>("pixilart-sprite (Sapito)");
            _gusanoSaliendoTexture = _content.Load<Texture2D>("pixilart-sprite (Gusano-Saliendo)");
            _gusanoNormalTexture = _content.Load<Texture2D>("pixil-sprite(Gusano)");
            _wandTexture = _content.Load<Texture2D>("pixilart-sprite (varita)");
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"Error cargando texturas de enemigos: {e.Message}");
        }
        if (_wandTexture != null)
        {
            Vector2 wandPos = new Vector2(viewport.Width * 0.8f, viewport.Height - 400f);
            int totalFrames = 3; 
            int idleFrames = 1;
            int frameWidth = _wandTexture.Width / totalFrames;
            int frameHeight = _wandTexture.Height;
            float frameTime = 0.05f;

            _playerWand = new PlayerWand(
                _wandTexture,
                idleFrames, 
                totalFrames, 
                frameWidth, 
                frameHeight, 
                frameTime, 
                wandPos
            );
        }
        AddBox(viewport);
    }

    public void Update(GameTime gameTime, Viewport viewport, MouseState mouseState)
    {
        var ms = Mouse.GetState();
        _spawnInterval = _random.NextDouble() * 4.0 + 1.0;
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        foreach (var entity in _entities.ToList()) 
        {
            bool wasClicked = mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released &&
            entity.Rect.Contains(new Point(mouseState.X, mouseState.Y));
            entity.Update(gameTime, viewport, wasClicked);
        }
        if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            Point mousePosition = new Point(mouseState.X, mouseState.Y);
            _playerWand?.Attack();
            var clickedBoxes = _entities.Where(box => box.Rect.Contains(mousePosition)).ToList();
            if (clickedBoxes.Count > 0)
            {
                var frontBox = clickedBoxes.OrderBy(box => box.Depth).First();
                _entities.Remove(frontBox);
            }
        }

        _spawnTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_spawnTimer >= _spawnInterval && _entities.Count < MaxBoxes)
        {
            _spawnTimer = 0;
            AddBox(viewport);
        }
        _previousMouseState = ms;
        _playerWand?.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var entity in _entities)
        {
            entity.Draw(spriteBatch, _pixel);
        }
        _playerWand?.Draw(spriteBatch);
    }

    private void AddBox(Viewport viewport)
    {
        int type = _random.Next(4); // 0=Hada, 1=Lobo, 2=Sapo, 3=Gusano
        Enemies entity;
        Vector2 startPos; 

        switch (type)
        {
            case 0: // HADA
                if (_hadaTexture != null) 
                {
                    float spawnX = _random.Next(0, viewport.Width);
                    float spawnY = -20f; 
                    startPos = new Vector2(spawnX, spawnY);
                    int frameCount = 2;
                    int frameWidth = _hadaTexture.Width / frameCount;
                    int frameHeight = _hadaTexture.Height;
                    entity = new Hada(startPos, _random, depth, Color.White, _hadaTexture,
                        frameCount, frameWidth, frameHeight, 0.3f);
                }
                else goto default;
                break;

            case 1: // LOBO
                float spawnX_Lobo = viewport.Width * 0.5f;
                float spawnY_Lobo = viewport.Height * 0.46f;
                startPos = new Vector2(spawnX_Lobo, spawnY_Lobo);
                entity = new Lobo(startPos, _random, depth, Color.White, null);
                break;

            case 2: // SAPO
                if (_sapoTexture != null)
                {
                    float spawnX_Sapo = viewport.Width * 0.5f;
                    float spawnY_Sapo = viewport.Height * 0.46f;
                    startPos = new Vector2(spawnX_Sapo, spawnY_Sapo);
                    int frameCount = 1;
                    int frameWidth = 500;
                    int frameHeight = 500;
                    entity = new Sapo(startPos, _random, depth, Color.White, _sapoTexture,
                        frameCount, frameWidth, frameHeight, 0f);
                }
                else goto default;
                break;

            case 3: // GUSANO
                if (_gusanoSaliendoTexture != null && _gusanoNormalTexture != null)
                {
                    int spawnXMin = (int)(viewport.Width * 0.15f);
                    int spawnXMax = (int)(viewport.Width * 0.85f);
                    float spawnX_Gusano = _random.Next(spawnXMin, spawnXMax);
                    int spawnYMin = (int)(viewport.Height * 0.5f);
                    int spawnYMax = (int)(viewport.Height * 0.8f);
                    float spawnY_Gusano = _random.Next(spawnYMin, spawnYMax);

                    startPos = new Vector2(spawnX_Gusano, spawnY_Gusano);
                    int emergeFrames = 14;
                    int emergeFrameWidth = _gusanoSaliendoTexture.Width / emergeFrames;
                    int emergeFrameHeight = _gusanoSaliendoTexture.Height;
                    int idleFrames = 1;
                    int idleFrameWidth = _gusanoNormalTexture.Width / idleFrames;
                    int idleFrameHeight = _gusanoNormalTexture.Height;

                    entity = new Gusano(startPos, _random, depth, Color.Orange,
                        emergeTexture: _gusanoSaliendoTexture,
                        idleTexture: _gusanoNormalTexture,
                        emergeFrames: emergeFrames,
                        idleFrames: idleFrames,
                        emergeFrameWidth: emergeFrameWidth,
                        emergeFrameHeight: emergeFrameHeight,
                        idleFrameWidth: idleFrameWidth,
                        idleFrameHeight: idleFrameHeight,
                        frameTime: 0.15f);
                }
                else goto default;
                break;

            default:
                startPos = new Vector2(viewport.Width * 0.5f, viewport.Height * 0.5f);
                entity = new Lobo(startPos, _random, depth, Color.White, null);
                break;
        }
        _entities.Add(entity);
    }
    private class BoxEntity
    {
        public Rectangle Rect;
        private Vector2 _position;
        private Vector2 _targetPosition;
        public Color Color { get; private set; }
        private float _depth;
        public bool IsMoving => (_targetPosition - _position).Length() > 1f;
        public float Depth => _depth;
        private float _speed = 100f;
        private float _width = 40f;
        private float _height = 40f;
        private double _moveTimer = 0;
        private double _moveInterval;
        private Random _random;

        public BoxEntity(Vector2 startPos, Random sharedRandom, float depth, Color color)
        {
            _position = startPos;
            _targetPosition = startPos;
            _random = sharedRandom;
            _depth = depth;
            Color = color;
            _moveInterval = _random.NextDouble() * 4.0 + 1.0;
            UpdateRect();
        }
        public void Update(GameTime gameTime, Viewport viewport)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 direction = _targetPosition - _position;
            if (direction.Length() > 1f)
            {
                direction.Normalize();
                _position += direction * _speed * delta;
                _width += 0.2f;
                _height += 0.2f;
                while (_depth > 0.1f)
                {
                    _depth -= 0.001f;
                }
            }
            UpdateRect();
            _moveTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_moveTimer >= _moveInterval)
            {
                _moveTimer = 0;
                int maxX = viewport.Width - (int)_width;
                float newX = _random.Next(0, maxX);
                _targetPosition = new Vector2(newX, _position.Y);
            }
        }
        private void UpdateRect()
        {
            Rect = new Rectangle(
                (int)(_position.X - _width / 2f),
                (int)(_position.Y - _height / 2f),
                (int)_width,
                (int)_height
            );
        }
    }
}