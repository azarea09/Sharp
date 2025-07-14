using Raylib_cs;
using SharpFramework.Core;
using SharpFramework.Graphics;

namespace SharpFramework.Test
{
    public class MyGame : Game
    {
        private int x = 10;
        public override void Init()
        {
            Window.SetTitle("My Cool Game");
            Window.SetDarkMode(true);
            Window.Resize(1280, 720);
            RenderSurface.Resize(1920, 1080);
            Sharp.SetVsync(false);
        }

        public override void Update()
        {
            // 入力処理とか
            if (Raylib.IsKeyPressedRepeat(KeyboardKey.Left))
                x -= 100;
            if (Raylib.IsKeyPressedRepeat(KeyboardKey.Right))
                x += 100;
        }

        public override void Draw()
        {
            Raylib.DrawFPS(x + 1000, 10);
            Sharp.DrawText("Sharp Framework! うおｗｗｗｗｗｗｗｗ", 0, 0, Sharp.FontSize.Medium);
        }

        public override void End()
        {
            
        }
    }
}