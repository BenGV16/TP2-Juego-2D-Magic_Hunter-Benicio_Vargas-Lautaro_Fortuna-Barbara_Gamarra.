using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Magic_Hunter.src;

public class MenuManager
{
    public Rectangle[] Buttons { get; private set; }
    public int SelectedIndex { get; private set; }
    public string[] Options { get; } = { "Comenzar", "Opciones", "Salir" };
    public Texture2D Pixel => _pixel;

    private Texture2D _pixel;
    private SpriteFont _font;
    private string _title = "MAGIC HUNTER"; 

    public void Initialize(Viewport viewport, GraphicsDevice graphicsDevice)
    {
        Buttons = new Rectangle[Options.Length];
        int buttonWidth = 200;
        int buttonHeight = 40;
        int spacing = 20;

        for (int i = 0; i < Options.Length; i++)
        {
            int x = (viewport.Width - buttonWidth) / 2;
            int y = (viewport.Height - (Options.Length * (buttonHeight + spacing))) / 2 + i * (buttonHeight + spacing);
            Buttons[i] = new Rectangle(x, y, buttonWidth, buttonHeight);
        }

        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void LoadContent(SpriteFont font)
    {
        _font = font;
    }

    public int HandleInput(MouseState mouseState)
    {
        Point mousePosition = new Point(mouseState.X, mouseState.Y);
        for (int i = 0; i < Buttons.Length; i++)
        {
            if (Buttons[i].Contains(mousePosition))
            {
                SelectedIndex = i;
                if (mouseState.LeftButton == ButtonState.Pressed)
                    return i;
                break;
            }
        }
        return -1;
    }

    public void Draw(SpriteBatch spriteBatch, Viewport viewport)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black);

        float scale = 2.0f;
        Vector2 titleSize = _font.MeasureString(_title) * scale;
        Vector2 titlePos = new Vector2(viewport.Width / 2 - titleSize.X / 2, 100);
        spriteBatch.DrawString(_font, _title, titlePos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);


        for (int i = 0; i < Buttons.Length; i++)
        {
            Color fillColor = (i == SelectedIndex) ? Color.Yellow : Color.White;
            Color borderColor = (i == SelectedIndex) ? Color.Gold : Color.Gray;

            spriteBatch.Draw(_pixel, Buttons[i], fillColor * 0.7f);

            int border = 2;
            spriteBatch.Draw(_pixel, new Rectangle(Buttons[i].X, Buttons[i].Y, Buttons[i].Width, border), borderColor); // top
            spriteBatch.Draw(_pixel, new Rectangle(Buttons[i].X, Buttons[i].Y + Buttons[i].Height - border, Buttons[i].Width, border), borderColor); // bottom
            spriteBatch.Draw(_pixel, new Rectangle(Buttons[i].X, Buttons[i].Y, border, Buttons[i].Height), borderColor); // left
            spriteBatch.Draw(_pixel, new Rectangle(Buttons[i].X + Buttons[i].Width - border, Buttons[i].Y, border, Buttons[i].Height), borderColor); // right

            Vector2 textSize = _font.MeasureString(Options[i]);
            Vector2 textPos = new Vector2(
                Buttons[i].X + Buttons[i].Width / 2 - textSize.X / 2,
                Buttons[i].Y + Buttons[i].Height / 2 - textSize.Y / 2
            );
            spriteBatch.DrawString(_font, Options[i], textPos, Color.White);
        }
    }
}
