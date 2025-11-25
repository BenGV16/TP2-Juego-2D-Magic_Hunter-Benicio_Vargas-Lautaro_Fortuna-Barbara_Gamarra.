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
    protected float _speed = 100f;
    protected Random _random;
    protected double _timer = 0;
    public Color Color { get; protected set; }
    public float Depth { get; protected set; }
    protected Texture2D _texture;
    protected AnimationManager _animator;

    public Enemies(Vector2 startPos, Random random, float depth, Color color, Texture2D texture)
    {
        _position = startPos;
        _random = random;
        Depth = depth;
        Color = color;
        _texture = texture;
        UpdateRect();
    }

    public abstract void Update(GameTime gameTime, Viewport viewport, bool wasClicked);

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
        if (_animator != null)
        {
            _animator.Draw(spriteBatch, _position, Color, Depth, _width, _height);
        }
        else
        {
            spriteBatch.Draw(pixel, Rect, null, Color * 0.8f, 0f, Vector2.Zero, SpriteEffects.None, Depth);
        }
    }
}

public class Hada : Enemies
{
    private Vector2 _target;
    private AnimationManager _moveAnim;
    private AnimationManager _idleAnim;
    private bool _isMoving = false;

    public Hada(Vector2 startPos, Random random, float depth, Color color, Texture2D texture, int frameCount, int frameWidth, int frameHeight, float frameTime)
        : base(startPos, random, depth, color, texture)
    {
        _target = startPos;
        _moveAnim = new AnimationManager(texture, frameCount, frameWidth, frameHeight, frameTime);
        _idleAnim = new AnimationManager(texture, 1, frameWidth, frameHeight, 0f);
        _animator = _idleAnim;
        _animator.Play(true);
    }

    public override void Update(GameTime gameTime, Viewport viewport, bool wasClicked)
    {
        _timer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_timer > 2.0)
        {
            _target = new Vector2(
                _random.Next(0, viewport.Width),
                _random.Next(0, viewport.Height)
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
            _width += 0.2f;
            _height += 0.2f;
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

    public Lobo(Vector2 startPos, Random random, float depth, Color color, Texture2D texture)
        : base(startPos, random, depth, color, texture)
    {
        _targetX = startPos.X;
    }

    public override void Update(GameTime gameTime, Viewport viewport, bool wasClicked)
    {
        _timer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_timer > 2.0)
        {
            _targetX = _random.Next(0, viewport.Width);
            _timer = 0;
        }

        float dir = _targetX - _position.X;
        if (Math.Abs(dir) > 1f)
        {
            _position.X += Math.Sign(dir) * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _width += 0.2f;
            _height += 0.2f;
        }

        UpdateRect();
    }
}

public class Sapo : Enemies
{
    private bool _activated = false;

    public Sapo(Vector2 startPos, Random random, float depth, Color color, Texture2D texture, int frameCount, int frameWidth, int frameHeight, float frameTime)
        : base(startPos, random, depth, color, texture)
    {
        _animator = new AnimationManager(texture, frameCount, frameWidth, frameHeight, frameTime);
        _animator.Play(true);
    }

    public override void Update(GameTime gameTime, Viewport viewport, bool wasClicked)
    {
        if (wasClicked) _activated = true;

        if (_activated)
        {
            _timer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer > 1.5)
            {
                _width += 2f;
                _height += 2f;
                _timer = 0;
            }
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
              float frameTime)
    : base(startPos, random, depth, color, emergeTexture)
    {
        _emergeAnim = new AnimationManager(emergeTexture, emergeFrames, emergeFrameWidth, emergeFrameHeight, frameTime);
        _idleAnim = new AnimationManager(idleTexture, idleFrames, idleFrameWidth, idleFrameHeight, frameTime);
        _state = State.Emerging;
        _animator = _emergeAnim;
        _animator.Play(isLooping: false);
    }

    public override void Update(GameTime gameTime, Viewport viewport, bool wasClicked)
    {
        _animator.Update(gameTime);
        if (_state == State.Emerging && _animator.IsDone)
        {
            _state = State.Idle;
            _animator = _idleAnim;
            _animator.Play(isLooping: true);
        }
        UpdateRect();
    }
}
