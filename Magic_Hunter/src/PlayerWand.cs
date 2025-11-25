// Magic_Hunter/src/PlayerWand.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magic_Hunter.src
{
    public class PlayerWand
    {
        private AnimationManager _animator;
        private AnimationManager _idleAnim;
        private AnimationManager _attackAnim;
        
        private Vector2 _position;
        private float _width = 1000f;
        private float _height = 1000f;

        public PlayerWand(Texture2D texture, int idleFrames, int attackFrames, int frameWidth, int frameHeight, float frameTime, Vector2 position)
        {
            _position = position;
            _attackAnim = new AnimationManager(texture, attackFrames, frameWidth, frameHeight, frameTime);
            _idleAnim = new AnimationManager(texture, idleFrames, frameWidth, frameHeight, 0f);
            _animator = _idleAnim;
            _animator.Play(isLooping: true);
        }
        public void Attack()
        {
            if (_animator == _idleAnim)
            {
                _animator = _attackAnim;
                _animator.Play(isLooping: false);
            }
        }

        public void Update(GameTime gameTime)
        {
            _animator.Update(gameTime);
            if (_animator == _attackAnim && _animator.IsDone)
            {
                _animator = _idleAnim;
                _animator.Play(isLooping: true);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _animator.Draw(spriteBatch, _position, Color.White, 0.0f, _width, _height);
        }
    }
}