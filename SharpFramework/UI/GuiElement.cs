using Raylib_cs;
using SharpFramework.Core;

namespace SharpFramework.UI
{
    public abstract class GuiElement
    {
        public Rectangle Rect;

        public GuiElement(float x, float y, float w, float h)
        {
            Rect = new Rectangle(x, y, w, h);
        }

        /// <summary>
        /// 描画メソッド（派生クラスで実装）
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// ピクセルスケールを取得（ボーダー幅などに利用可能）
        /// </summary>
        protected float GetPixelScale()
        {
            int baseWidth = 1920;
            int screenWidth = Raylib.GetScreenWidth();
            float scaleX = (float)baseWidth / screenWidth;
            return (int)Math.Round(scaleX);
        }

        /// <summary>
        /// マウスが当たっているかの判定（アスペクト比と黒帯考慮済み）
        /// </summary>
        public bool IsMouseOver()
        {
            int targetWidth = RenderSurface.Width;
            int targetHeight = RenderSurface.Height;
            int windowWidth = Raylib.GetScreenWidth();
            int windowHeight = Raylib.GetScreenHeight();

            float windowAspect = (float)windowWidth / windowHeight;
            float targetAspect = (float)targetWidth / targetHeight;

            int renderWidth, renderHeight;
            int offsetX = 0, offsetY = 0;

            if (windowAspect > targetAspect)
            {
                renderHeight = windowHeight;
                renderWidth = (int)(windowHeight * targetAspect);
                offsetX = (windowWidth - renderWidth) / 2;
            }
            else
            {
                renderWidth = windowWidth;
                renderHeight = (int)(windowWidth / targetAspect);
                offsetY = (windowHeight - renderHeight) / 2;
            }

            float mouseX = (Raylib.GetMouseX() - offsetX) * (float)targetWidth / renderWidth;
            float mouseY = (Raylib.GetMouseY() - offsetY) * (float)targetHeight / renderHeight;

            return mouseX >= Rect.X && mouseX <= (Rect.X + Rect.Width) &&
                   mouseY >= Rect.Y && mouseY <= (Rect.Y + Rect.Height);
        }
    }
}
