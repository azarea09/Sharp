using Raylib_cs;
using System.Numerics;

namespace SharpEngine.Gui
{
    public class Button
    {
        public Rectangle Rect;
        public string Label;
        public Action? OnClick;

        public Button(string label, float x = 0, float y = 0, float w = 140, float h = 24, Action? onClick = null)
        {
            Rect = new Rectangle(x, y, w, h);
            Label = label;
            OnClick = onClick;
        }

        public void Draw()
        {
            Color backColor = CheckCollisionPointRec(Rect) ? RaylibTranslator.FromHtml("#a9a9c0") : RaylibTranslator.FromHtml("#4a4a5e");
            Color foreColor = CheckCollisionPointRec(Rect) ? RaylibTranslator.FromHtml("#5c5f74") : RaylibTranslator.FromHtml("#3d3d4c");
            Raylib.DrawRectangleRec(Rect, backColor);
            float scale = GetPixelScale();
            float border = MathF.Max(1, scale); // 最低1pxは維持
            Raylib.DrawRectangleRec(new Rectangle(Rect.X + border, Rect.Y + border, Rect.Width - border * 2, Rect.Height - border * 2), foreColor);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && CheckCollisionPointRec(Rect))
            {
                OnClick?.Invoke();
            }
        }

        private float GetPixelScale()
        {
            int baseWidth = 1920;
            int screenWidth = Raylib.GetScreenWidth();
            float scaleX = (float)baseWidth / screenWidth;
            return (int)Math.Round(scaleX);
        }

        private bool CheckCollisionPointRec(Rectangle rect)
        {

            int targetWidth = Sharp.SceneWidth;
            int targetHeight = Sharp.SceneHeight;
            int windowWidth = Raylib.GetScreenWidth();
            int windowHeight = Raylib.GetScreenHeight();

            // ウィンドウのアスペクト比
            float windowAspect = (float)windowWidth / windowHeight;
            float targetAspect = (float)targetWidth / targetHeight;

            int renderWidth, renderHeight;
            int offsetX = 0, offsetY = 0;

            if (windowAspect > targetAspect)
            {
                // ウィンドウが横長 → 左右に黒帯
                renderHeight = windowHeight;
                renderWidth = (int)(windowHeight * targetAspect);
                offsetX = (windowWidth - renderWidth) / 2;
            }
            else
            {
                // ウィンドウが縦長 → 上下に黒帯
                renderWidth = windowWidth;
                renderHeight = (int)(windowWidth / targetAspect);
                offsetY = (windowHeight - renderHeight) / 2;
            }

            // 黒帯を考慮したマウス位置
            float mouseX = (Raylib.GetMouseX() - offsetX) * (float)targetWidth / renderWidth;
            float mouseY = (Raylib.GetMouseY() - offsetY) * (float)targetHeight / renderHeight;

            return mouseX >= rect.X && mouseX <= (rect.X + rect.Width) &&
                   mouseY >= rect.Y && mouseY <= (rect.Y + rect.Height);
        }
    }

}
