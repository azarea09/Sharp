using Raylib_cs;
using System.Numerics;

namespace SharpEngine.Gui
{
    public class Button : GuiElement
    {
        public string Label;
        public Action? OnClick;

        public Button(string label, float x = 0, float y = 0, Action? onClick = null, float w = 210, float h = 36)  : base(x, y, w, h)
        {
            Label = label;
            OnClick = onClick;
        }

        public override void Draw()
        {
            Color backColor = IsMouseOver() ? RaylibTranslator.FromHtml("#a9a9c0") : RaylibTranslator.FromHtml("#4a4a5e");
            Color foreColor = IsMouseOver() ? RaylibTranslator.FromHtml("#5c5f74") : RaylibTranslator.FromHtml("#3d3d4c");

            Raylib.DrawRectangleRec(Rect, backColor);

            float border = MathF.Max(1, GetPixelScale());
            Raylib.DrawRectangleRec(
                new Rectangle(Rect.X + border, Rect.Y + border, Rect.Width - border * 2, Rect.Height - border * 2),
                foreColor
            );

            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && IsMouseOver())
            {
                OnClick?.Invoke();
            }
        }
    }
}
