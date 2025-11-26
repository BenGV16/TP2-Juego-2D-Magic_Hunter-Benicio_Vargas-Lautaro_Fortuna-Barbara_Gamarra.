using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Magic_Hunter.src;

public class GamePlay
{
    public bool IsGameOver { get; private set; } = false;

    private Texture2D _pixel;
    private List<Enemies> _entities = new();
    private MouseState _previousMouseState;
    
    // Oleadas
    private Queue<int> _enemyWaveQueue = new();
    private double _spawnTimer = 0;
    private double _spawnInterval = 2.0;
    private int _waveNumber = 0;
    private bool _waveInProgress = false;
    private float _difficultyMultiplier = 1.0f;
    private double _waveBreakTimer = 0; 

    // Mensajes
    private double _waveMessageTimer = 0;
    private bool _showingWaveMessage = false;
    private string _waveMessageText = "";
    
    // Jugador y Vida
    private int _playerMaxHealth = 100;
    private int _playerHealth = 100;
    private Texture2D _wandTexture;
    private PlayerWand _playerWand;
    private float _wandXPosition; 

    // --- SISTEMA DE ESCUDO ---
    private Texture2D _shieldTexture;
    private Rectangle[] _shieldSourceRects; 
    
    // El frame siempre será 0 visualmente, pero mantenemos lógica de durabilidad
    private float _shieldDurability = 100f;
    private float _maxShieldDurability = 100f;
    private bool _isShieldBroken = false;
    
    // --- SISTEMA DE PROYECTILES ---
    private Texture2D _projectileTexture;
    private List<Projectile> _projectiles = new();
    
    // Recursos
    private ContentManager _content;
    private Texture2D _hadaTexture;
    private Texture2D _sapoTexture;
    private Texture2D _gusanoSaliendoTexture;
    private Texture2D _gusanoNormalTexture;
    private SpriteFont _font; 

    private float depth = 1.0f;
    private Random _random = new();

    private class Projectile
    {
        public Vector2 Position;
        public Vector2 Target; 
        public float Scale;    
        public float Speed;
        public int Damage;
        public bool IsActive;
    }

    public void Initialize(GraphicsDevice graphicsDevice, Viewport viewport, ContentManager content)
    {
        _content = content;
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _wandXPosition = viewport.Width * 0.8f; 

        try
        {
            _hadaTexture = _content.Load<Texture2D>("pixilart-sprite(Hada1)");
            _sapoTexture = _content.Load<Texture2D>("pixilart-sprite (Sapito)");
            _gusanoSaliendoTexture = _content.Load<Texture2D>("pixilart-sprite (Gusano-Saliendo)");
            _gusanoNormalTexture = _content.Load<Texture2D>("pixil-sprite(Gusano)");
            _wandTexture = _content.Load<Texture2D>("pixilart-sprite (varita)");
            _projectileTexture = _content.Load<Texture2D>("pixilart-sprite(bola de fuego)");
            
            _shieldTexture = _content.Load<Texture2D>("pixilart-sprite(Escudo)");
            int sWidth = _shieldTexture.Width / 3;
            int sHeight = _shieldTexture.Height;
            _shieldSourceRects = new Rectangle[3];
            _shieldSourceRects[0] = new Rectangle(0, 0, sWidth, sHeight);       // Sprite 1
            _shieldSourceRects[1] = new Rectangle(sWidth, 0, sWidth, sHeight);  // Sprite 2
            _shieldSourceRects[2] = new Rectangle(sWidth * 2, 0, sWidth, sHeight); // Sprite 3

            try { _font = _content.Load<SpriteFont>("PixelFont"); } catch { } 
        }
        catch (Exception e) { System.Diagnostics.Debug.WriteLine(e.Message); }

        if (_wandTexture != null)
        {
            Vector2 wandPos = new Vector2(_wandXPosition, viewport.Height - 400f);
            _playerWand = new PlayerWand(_wandTexture, 1, 3, _wandTexture.Width / 3, _wandTexture.Height, 0.05f, wandPos);
        }

        Reset(); 
    }

    public void Reset()
    {
        _entities.Clear();
        _projectiles.Clear();
        _enemyWaveQueue.Clear();
        _waveNumber = 0;
        _playerHealth = _playerMaxHealth;
        _shieldDurability = _maxShieldDurability;
        _isShieldBroken = false;
        _difficultyMultiplier = 1.0f;
        IsGameOver = false;
        StartNextWave();
    }

    private void StartNextWave()
    {
        _waveNumber++;
        _waveInProgress = true;
        _spawnTimer = 0;
        
        _showingWaveMessage = true;
        _waveMessageTimer = 3.0; 
        // CAMBIO: Números normales en lugar de romanos
        _waveMessageText = "OLEADA " + _waveNumber; 

        if (_waveNumber > 1)
        {
            _difficultyMultiplier += 0.1f; 
            _spawnInterval = Math.Max(0.8, _spawnInterval - 0.1);
        }

        GenerateWaveEnemies();
    }

    private void GenerateWaveEnemies()
    {
        int totalEnemies = 3 + _waveNumber; 
        int maxSapos = 1 + (_waveNumber / 3); 
        int currentSapos = 0;

        List<int> waveComposition = new List<int>();

        for (int i = 0; i < totalEnemies; i++)
        {
            int type = _random.Next(4); 
            if (type == 2) 
            {
                if (currentSapos < maxSapos) { currentSapos++; waveComposition.Add(type); }
                else waveComposition.Add(_random.Next(0, 2)); 
            }
            else waveComposition.Add(type);
        }
        
        waveComposition = waveComposition.OrderBy(x => _random.Next()).ToList();
        foreach (var t in waveComposition) _enemyWaveQueue.Enqueue(t);
    }

    public void Update(GameTime gameTime, Viewport viewport, MouseState mouseState)
    {
        if (IsGameOver) return;

        var ms = Mouse.GetState();
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Point mousePos = new Point(mouseState.X, mouseState.Y);

        // 1. Lógica del Escudo
        bool isShielding = false;

        // Recuperación pasiva (lenta)
        if (_shieldDurability < _maxShieldDurability && !isShielding && !_isShieldBroken)
            _shieldDurability += 2f * delta;

        // Recuperación de rotura (lenta)
        if (_isShieldBroken)
        {
            _shieldDurability += 4f * delta;
            if (_shieldDurability >= _maxShieldDurability * 0.5f) 
                _isShieldBroken = false;
        }

        // INPUT DEL ESCUDO: CLIC DERECHO (Para permitir uso simultáneo con Varita)
        if (mouseState.RightButton == ButtonState.Pressed && !_isShieldBroken)
        {
            isShielding = true;
        }

        // 2. Mensajes y Oleadas
        if (_showingWaveMessage)
        {
            _waveMessageTimer -= delta;
            if (_waveMessageTimer <= 0) _showingWaveMessage = false;
        }

        if (_waveInProgress)
        {
            if (_enemyWaveQueue.Count > 0)
            {
                if (!_showingWaveMessage) 
                {
                    _spawnTimer += delta;
                    if (_spawnTimer >= _spawnInterval)
                    {
                        SpawnEnemy(viewport, _enemyWaveQueue.Dequeue());
                        _spawnTimer = 0;
                    }
                }
            }
            else if (_entities.Count == 0)
            {
                _waveInProgress = false;
                _waveBreakTimer = 3.0;
                
                int healAmount = (int)(_playerMaxHealth * 0.10f);
                _playerHealth += healAmount;
                if (_playerHealth > _playerMaxHealth) _playerHealth = _playerMaxHealth;
                
                _shieldDurability = _maxShieldDurability;
                _isShieldBroken = false;
            }
        }
        else
        {
            _waveBreakTimer -= delta;
            if (_waveBreakTimer <= 0) StartNextWave();
        }

        // 3. Actualizar Entidades
        for (int i = _entities.Count - 1; i >= 0; i--)
        {
            var entity = _entities[i];
            entity.Update(gameTime, viewport);

            // CAMBIO: Probabilidad de disparo reducida (de 0.01 a 0.002) para que disparen más lento
            if (_random.NextDouble() < 0.002 * _difficultyMultiplier) 
            {
                SpawnProjectile(entity.Rect.Center.ToVector2(), viewport);
            }
        }

        // 4. Actualizar Proyectiles
        UpdateProjectiles(gameTime, viewport, isShielding, mousePos);

        // 5. Game Over
        if (_playerHealth <= 0)
        {
            _playerHealth = 0;
            IsGameOver = true;
        }

        // 6. Disparo: CLIC IZQUIERDO (Al presionar)
        // Usamos Pressed + Previous Released para disparo semiautomático (un clic = un tiro)
        if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            _playerWand?.Attack();

            var clicked = _entities.LastOrDefault(e => e.Rect.Contains(mousePos)); 
            if (clicked != null)
            {
                if (clicked.TakeDamage(1))
                {
                    _entities.Remove(clicked);
                }
            }
        }

        _previousMouseState = ms;
        _playerWand?.Update(gameTime);
    }

    private void SpawnProjectile(Vector2 startPos, Viewport viewport)
    {
        Projectile p = new Projectile();
        p.Position = startPos;
        p.Scale = 0.1f; 
        p.Speed = 200f * _difficultyMultiplier; 
        p.IsActive = true;
        p.Damage = 10;
        
        Vector2 screenCenter = new Vector2(viewport.Width / 2, viewport.Height / 2);
        p.Target = screenCenter; 
        
        _projectiles.Add(p);
    }

    private void UpdateProjectiles(GameTime gameTime, Viewport viewport, bool isShielding, Point mousePos)
    {
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            var p = _projectiles[i];
            p.Scale += 0.5f * delta; 
            
            Vector2 dir = p.Target - p.Position;
            if (dir.Length() > 0) dir.Normalize();
            p.Position += dir * (p.Speed * 0.1f) * delta; 

            int pSize = (int)(_projectileTexture.Width * p.Scale);
            Rectangle pRect = new Rectangle(
                (int)(p.Position.X - pSize / 2),
                (int)(p.Position.Y - pSize / 2),
                pSize, pSize
            );

            if (p.Scale > 1.5f) 
            {
                bool blocked = false;

                if (isShielding)
                {
                    int shieldRadius = 100; 
                    Rectangle shieldRect = new Rectangle(mousePos.X - shieldRadius, mousePos.Y - shieldRadius, shieldRadius * 2, shieldRadius * 2);
                    
                    if (shieldRect.Intersects(pRect))
                    {
                        blocked = true;
                        _shieldDurability -= p.Damage * 2; 
                        if (_shieldDurability <= 0)
                        {
                            _shieldDurability = 0;
                            _isShieldBroken = true;
                        }
                    }
                }

                if (!blocked)
                {
                    _playerHealth -= p.Damage;
                }

                _projectiles.RemoveAt(i); 
            }
        }
    }

    private void SpawnEnemy(Viewport viewport, int type)
    {
        Enemies entity = null;
        Vector2 startPos = Vector2.Zero;

        switch (type)
        {
            case 0: 
                if (_hadaTexture != null)
                {
                    float spawnX = _random.Next(100, viewport.Width - 100);
                    startPos = new Vector2(spawnX, -50f); 
                    entity = new Hada(startPos, _random, depth, Color.White, _hadaTexture, 2, _hadaTexture.Width/2, _hadaTexture.Height, 0.3f, _difficultyMultiplier);
                }
                break;

            case 1: 
                startPos = new Vector2(viewport.Width * 0.5f, viewport.Height * 0.46f);
                entity = new Lobo(startPos, _random, depth, Color.White, null, _difficultyMultiplier);
                break;

            case 2: 
                if (_sapoTexture != null)
                {
                    startPos = new Vector2(viewport.Width * 0.5f, viewport.Height * 0.46f);
                    entity = new Sapo(startPos, _random, depth, Color.White, _sapoTexture, 1, 500, 500, 0f, _difficultyMultiplier);
                }
                break;

            case 3: 
                if (_gusanoSaliendoTexture != null)
                {
                    int minX = (int)(viewport.Width * 0.1f);
                    int maxX = (int)(viewport.Width * 0.7f); 
                    float spawnX = _random.Next(minX, maxX);
                    
                    int minY = (int)(viewport.Height * 0.5f);
                    int maxY = (int)(viewport.Height * 0.8f);
                    float spawnY = _random.Next(minY, maxY);

                    startPos = new Vector2(spawnX, spawnY);
                    
                    int eW = _gusanoSaliendoTexture.Width / 14;
                    int iW = _gusanoNormalTexture.Width;

                    entity = new Gusano(startPos, _random, depth, Color.Orange,
                        _gusanoSaliendoTexture, _gusanoNormalTexture,
                        14, 1, eW, _gusanoSaliendoTexture.Height, iW, _gusanoNormalTexture.Height, 
                        0.15f, _difficultyMultiplier);
                }
                break;
        }

        if (entity != null) _entities.Add(entity);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var entity in _entities.OrderBy(e => e.Rect.Y))
        {
            entity.Draw(spriteBatch, _pixel);
        }

        foreach (var p in _projectiles)
        {
            int size = (int)(_projectileTexture.Width * p.Scale);
            Rectangle dest = new Rectangle((int)p.Position.X - size/2, (int)p.Position.Y - size/2, size, size);
            spriteBatch.Draw(_projectileTexture, dest, Color.White);
        }

        _playerWand?.Draw(spriteBatch);

        // --- UI ---
        int margin = 20;
        int barW = 200;
        int barH = 20;

        // 1. Barra de Vida (Violeta)
        spriteBatch.Draw(_pixel, new Rectangle(margin, margin, barW, barH), Color.DarkRed);
        float hpPct = (float)_playerHealth / _playerMaxHealth;
        spriteBatch.Draw(_pixel, new Rectangle(margin, margin, (int)(barW * hpPct), barH), Color.Purple);

        // 2. Barra de Durabilidad
        int shieldY = margin + barH + 5;
        spriteBatch.Draw(_pixel, new Rectangle(margin, shieldY, barW, barH/2), Color.Gray);
        float shieldPct = _shieldDurability / _maxShieldDurability;
        Color shieldColor = _isShieldBroken ? Color.Red : Color.Cyan; 
        spriteBatch.Draw(_pixel, new Rectangle(margin, shieldY, (int)(barW * shieldPct), barH/2), shieldColor);

        // 3. Mensajes
        if (_showingWaveMessage && _font != null)
        {
            Vector2 textSize = _font.MeasureString(_waveMessageText);
            Vector2 centerPos = new Vector2(
                (spriteBatch.GraphicsDevice.Viewport.Width / 2) - (textSize.X / 2),
                (spriteBatch.GraphicsDevice.Viewport.Height / 2) - (textSize.Y / 2)
            );
            spriteBatch.DrawString(_font, _waveMessageText, centerPos + new Vector2(2, 2), Color.Black);
            spriteBatch.DrawString(_font, _waveMessageText, centerPos, Color.Gold);
        }
        else if (_font != null)
        {
            spriteBatch.DrawString(_font, $"Oleada: {_waveNumber}", new Vector2(margin, shieldY + 20), Color.White);
        }

        // 4. DIBUJAR MIRILLA / ESCUDO (Siempre Sprite 1)
        MouseState ms = Mouse.GetState();
        if (_shieldTexture != null)
        {
            Rectangle source = _shieldSourceRects[0]; 
            Vector2 origin = new Vector2(source.Width / 2, source.Height / 2);
            spriteBatch.Draw(_shieldTexture, new Vector2(ms.X, ms.Y), source, Color.White, 0f, origin, 0.5f, SpriteEffects.None, 0f);
        }
    }
}