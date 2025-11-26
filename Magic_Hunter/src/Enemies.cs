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

    // Nuevo: Variable para controlar el espejo
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
            // MODIFICADO: Pasamos _spriteEffect al animador
            _animator.Draw(spriteBatch, _position, drawColor, Depth, _width, _height, _spriteEffect);
        }
        else
        {
            spriteBatch.Draw(pixel, Rect, null, drawColor * 0.8f, 0f, Vector2.Zero, SpriteEffects.None, Depth);
        }
    }
}

public class Hada : Enemies
{
    private Vector2 _target;
    private AnimationManager _moveAnim;
    private AnimationManager _idleAnim;
    private bool _isMoving = false;

    public Hada(Vector2 startPos, Random random, float depth, Color color, Texture2D texture, int frameCount, int frameWidth, int frameHeight, float frameTime, float speedMultiplier)
        : base(startPos, random, depth, color, texture, 1, 5, speedMultiplier)
    {
        _target = new Vector2(startPos.X, startPos.Y + 100); 
        _moveAnim = new AnimationManager(texture, frameCount, frameWidth, frameHeight, frameTime);
        _idleAnim = new AnimationManager(texture, 1, frameWidth, frameHeight, 0f);
        _animator = _idleAnim;
        _animator.Play(true);
    }

    public override void Update(GameTime gameTime, Viewport viewport)
    {
        base.Update(gameTime, viewport);

        _timer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_timer > 2.0)
        {
            _target = new Vector2(
                _random.Next(0, viewport.Width),
                _random.Next(0, (int)(viewport.Height * 0.6))
            );
            _timer = 0;
        }

        Vector2 dir = _target - _position;
        if (dir.Length() > 1f)
        {
            if (!_isMoving)
            {
                _isMoving = true;
                _animator = _moveAnim;
                _animator.Play(true);
            }
            dir.Normalize();
            _position += dir * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        else
        {
            if (_isMoving)
            {
                _isMoving = false;
                _animator = _idleAnim;
                _animator.Play(true);
            }
        }
        _animator.Update(gameTime);
        UpdateRect();
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

        // MODIFICADO: Lógica de espejo
        // Si está en la mitad izquierda (< ancho/2), invertimos horizontalmente
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