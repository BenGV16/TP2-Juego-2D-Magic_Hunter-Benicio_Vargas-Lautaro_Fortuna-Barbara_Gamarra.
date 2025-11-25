using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Magic_Hunter.src;

public class SpriteSheetManager
{
    private Texture2D _texture;
    private List<Rectangle> _frames = new();

    public SpriteSheetManager(Texture2D texture, int frameWidth, int frameHeight)
    {
        _texture = texture;
        _frames.Add(new Rectangle(0, 0, 1000, 500));
        _frames.Add(new Rectangle(1000, 0, 1000, 500));
        _frames.Add(new Rectangle(2000, 0, 1000, 500));
    }
    public Texture2D Texture => _texture;
    public Rectangle GetFrame(int index)
    {
        if (index < 0 || index >= _frames.Count)
            return _frames[0];
        return _frames[index];
    }
    public int FrameCount => _frames.Count;
}