using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Magic_Hunter.src;

public abstract class Enemies
{
    public Rectangle Rect;
    protected Vector2 _position;
    protected float _width = 200f;
    protected float _height = 200f;
    protected float _baseSpeed = 100f;
    protected float _speed;
    protected Random _random;
    protected double _timer = 0;
    protected double _hitTimer = 0; 

    protected SpriteEffects _spriteEffect = SpriteEffects.None;

    public int Health { get; protected set; }
    public int Damage { get; protected set; }

    public Color Color { get; protected set; }
    public float Depth { get; protected set; }
    protected Texture2D _texture;
    protected AnimationManager _animator;

    public Enemies(Vector2 startPos, Random random, float depth, Color color, Texture2D texture, int health, int damage, float speedMultiplier)
    {
        _position = startPos;
        _random = random;
        Depth = depth;
        Color = color;
        _texture = texture;
        
        Health = health;
        Damage = (int)(damage * speedMultiplier);
        _speed = _baseSpeed * speedMultiplier;

        UpdateRect();
    }

    public bool TakeDamage(int amount)
    {
        Health -= amount;
        _hitTimer = 0.1;
        return Health <= 0;
    }

    public virtual void Update(GameTime gameTime, Viewport viewport)
    {
        if (_hitTimer > 0)
        {
            _hitTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    protected void UpdateRect()
    {
        Rect = new Rectangle(
            (int)(_position.X - _width / 2f),
            (int)(_position.Y - _height / 2f),
            (int)_width,
            (int)_height
        );
    }

    public virtual void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        Color drawColor = (_hitTimer > 0) ? Color.Red : Color;

        if (_animator != null)
        {
            _animator.Draw(spriteBatch, _position, drawColor, Depth, _width, _height, _spriteEffect);
        }
        else
        {
            spriteBatch.Draw(pixel, Rect, null, drawColor * 0.8f, 0f, Vector2.Zero, SpriteEffects.None, Depth);
        }
    }
}

// --- MODIFICADO: HADA 1 (Crece y brilla) ---
public class Hada : Enemies
{
    private Vector2 _target;
    // Eliminamos _idleAnim y _isMoving, ya no se detendrá.

    // Variables para el efecto de brillo
    private float _baseWidth;
    private float _attackWidth = 400f; // Tamaño al que ataca

    public bool IsReadyToAttack => _width >= _attackWidth; 

    public Hada(Vector2 startPos, Random random, float depth, Color color, Texture2D texture, int frameCount, int frameWidth, int frameHeight, float frameTime, float speedMultiplier)
        : base(startPos, random, depth, color, texture, 1, 5, speedMultiplier)
    {
        // Velocidad aumentada considerablemente (2.5 veces la base)
        _speed *= 2.5f;
        _baseWidth = _width; // Guardamos el tamaño inicial (200f)

        _target = new Vector2(startPos.X, startPos.Y + 100); 
        // Solo usa la animación de movimiento
        _animator = new AnimationManager(texture, frameCount, frameWidth, frameHeight, frameTime);
        _animator.Play(true);
    }

    public override void Update(GameTime gameTime, Viewport viewport)
    {
        base.Update(gameTime, viewport);

        // Crecimiento constante
        float growthRate = 35f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        _width += growthRate;
        _height += growthRate;

        // Movimiento continuo: Si está cerca del objetivo, elige otro inmediatamente.
        Vector2 dir = _target - _position;
        if (dir.Length() < 10f)
        {
            // Elegir nuevo objetivo aleatorio en pantalla
            _target = new Vector2(
                _random.Next(50, viewport.Width - 50),
                _random.Next(50, (int)(viewport.Height * 0.7))
            );
            dir = _target - _position; // Recalcular dirección
        }

        if (dir.Length() > 0)
        {
            dir.Normalize();
            _position += dir * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        
        _animator.Update(gameTime);
        UpdateRect();
    }

    // SOBRESCRIBIMOS DRAW PARA EL EFECTO DE BRILLO
    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        // 1. Calcular el porcentaje de "carga/tamaño" (de 0.0 a 1.0)
        float ratio = (_width - _baseWidth) / (_attackWidth - _baseWidth);
        ratio = MathHelper.Clamp(ratio, 0f, 1f);

        // 2. Calcular el color de brillo.
        // Interpolamos entre Blanco normal y un Amarillo muy brillante intenso.
        // Cuanto más grande, más amarillo/blanco intenso se verá el sprite.
        Color brightnessColor = Color.Lerp(Color.White, new Color(255, 255, 180), ratio);

        // 3. Si está golpeada (hitTimer > 0), el rojo tiene prioridad. Si no, usa el brillo.
        Color finalColor = (_hitTimer > 0) ? Color.Red : brightnessColor;

        _animator.Draw(spriteBatch, _position, finalColor, Depth, _width, _height, _spriteEffect);
    }
}

public class Lobo : Enemies
{
    private float _targetX;

    public Lobo(Vector2 startPos, Random random, float depth, Color color, Texture2D texture, float speedMultiplier)
        : base(startPos, random, depth, color, texture, 5, 10, speedMultiplier)
    {
        _targetX = _random.Next(0, 800); 
    }

    public override void Update(GameTime gameTime, Viewport viewport)
    {
        base.Update(gameTime, viewport);

        _timer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_timer > 3.0)
        {
            _targetX = _random.Next(0, viewport.Width);
            _timer = 0;
        }

        float dir = _targetX - _position.X;
        if (Math.Abs(dir) > 1f)
        {
            _position.X += Math.Sign(dir) * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _width += 0.1f;
            _height += 0.1f;
            _position.Y += 0.1f;
        }
        UpdateRect();
    }
}

public class Sapo : Enemies
{
    public Sapo(Vector2 startPos, Random random, float depth, Color color, Texture2D texture, int frameCount, int frameWidth, int frameHeight, float frameTime, float speedMultiplier)
        : base(startPos, random, depth, color, texture, 8, 15, speedMultiplier)
    {
        _animator = new AnimationManager(texture, frameCount, frameWidth, frameHeight, frameTime);
        _animator.Play(true);
    }

    public override void Update(GameTime gameTime, Viewport viewport)
    {
        base.Update(gameTime, viewport);

        _timer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_timer > 1.5)
        {
            _width = 220f; 
            _height = 220f;
            if (_timer > 3.0) _timer = 0;
        }
        else
        {
             _width = 200f;
             _height = 200f;
        }
        _animator.Update(gameTime);
        UpdateRect();
    }
}

public class Gusano : Enemies
{
    private enum State { Emerging, Idle }
    private State _state;
    private AnimationManager _emergeAnim;
    private AnimationManager _idleAnim;

    public Gusano(Vector2 startPos, Random random, float depth, Color color,
              Texture2D emergeTexture, Texture2D idleTexture,
              int emergeFrames, int idleFrames,
              int emergeFrameWidth, int emergeFrameHeight,
              int idleFrameWidth, int idleFrameHeight,
              float frameTime, float speedMultiplier)
    : base(startPos, random, depth, color, emergeTexture, 3, 8, speedMultiplier)
    {
        _emergeAnim = new AnimationManager(emergeTexture, emergeFrames, emergeFrameWidth, emergeFrameHeight, frameTime);
        _idleAnim = new AnimationManager(idleTexture, idleFrames, idleFrameWidth, idleFrameHeight, frameTime);
        _state = State.Emerging;
        _animator = _emergeAnim;
        _animator.Play(isLooping: false);
    }

    public override void Update(GameTime gameTime, Viewport viewport)
    {
        base.Update(gameTime, viewport);

        _animator.Update(gameTime);
        if (_state == State.Emerging && _animator.IsDone)
        {
            _state = State.Idle;
            _animator = _idleAnim;
            _animator.Play(isLooping: true);
        }

        if (_position.X < viewport.Width / 2)
        {
            _spriteEffect = SpriteEffects.FlipHorizontally;
        }
        else
        {
            _spriteEffect = SpriteEffects.None;
        }

        UpdateRect();
    }
}

// --- MODIFICADO: HADA 2 (Trampa rapida) ---
public class Hada2 : Enemies
{
    private Vector2 _target;
    private bool _leaving = false;

    public Hada2(Vector2 startPos, Random random, float depth, Color color, Texture2D texture, int frameCount, int frameWidth, int frameHeight, float frameTime, float speedMultiplier)
        : base(startPos, random, depth, color, texture, 1, 0, speedMultiplier) 
    {
        // Velocidad MUY aumentada (3 veces la base) para que sea difícil de acertar
        _speed *= 3.0f; 

        _target = new Vector2(startPos.X, startPos.Y + 150); 
        _animator = new AnimationManager(texture, frameCount, frameWidth, frameHeight, frameTime);
        _animator.Play(true);
    }

    public override void Update(GameTime gameTime, Viewport viewport)
    {
        base.Update(gameTime, viewport);
        _animator.Update(gameTime);

        _timer += gameTime.ElapsedGameTime.TotalSeconds;

        // Se va a los 8 segundos
        if (!_leaving && _timer > 8.0)
        {
            _leaving = true;
            _target = new Vector2(_position.X, -200); 
        }
        // Movimiento continuo: Si llega al destino y NO se está yendo, elige otro inmediatamente.
        else if (!_leaving && Vector2.Distance(_position, _target) < 10f)
        {
             _target = new Vector2(
                _random.Next(50, viewport.Width - 50),
                _random.Next(50, (int)(viewport.Height * 0.7))
             );
        }

        Vector2 dir = _target - _position;
        if (dir.Length() > 0)
        {
            dir.Normalize();
            _position += dir * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        
        // No crece
        UpdateRect();
    }
}