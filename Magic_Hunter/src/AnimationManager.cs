using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Magic_Hunter.src
{
    public class AnimationManager
    {
        private Texture2D _texture;
        private List<Rectangle> _frames = new();
        private float _frameTime;
        private bool _isLooping;

        private int _currentFrame;
        private double _timer;

        public bool IsDone { get; private set; }

        public AnimationManager(Texture2D texture, int frameCount, int frameWidth, int frameHeight, float frameTime)
        {
            _texture = texture;
            _frameTime = frameTime;
            _isLooping = true;
            IsDone = false;

            for (int i = 0; i < frameCount; i++)
            {   
                _frames.Add(new Rectangle(i * frameWidth, 0, frameWidth, frameHeight));
            }
        }

        public void Play(bool isLooping)
        {
            _isLooping = isLooping;
            _currentFrame = 0;
            _timer = 0;
            IsDone = false;
        }
        public void Update(GameTime gameTime)
        {
            if (IsDone) return;
            _timer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer >= _frameTime)
            {
                _timer -= _frameTime;
                _currentFrame++;

                if (_currentFrame >= _frames.Count)
                {
                    if (_isLooping)
                    {
                        _currentFrame = 0;
                    }
                    else
                    {
                        _currentFrame = _frames.Count - 1;
                        IsDone = true;
                    }
                }
            }
        }
        
        // MODIFICADO: Se agregó el parámetro opcional 'effects'
        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float depth, float width, float height, SpriteEffects effects = SpriteEffects.None)
        {
            Rectangle destinationRect = new Rectangle(
                (int)(position.X - width / 2f),
                (int)(position.Y - height / 2f),
                (int)width,
                (int)height
            );

            spriteBatch.Draw(
                _texture,
                destinationRect,
                _frames[_currentFrame],
                color,
                0f,
                Vector2.Zero,
                effects, // Se usa el efecto pasado
                depth
            );
        }
    }
}