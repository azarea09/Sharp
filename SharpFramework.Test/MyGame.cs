using Raylib_cs;
using SharpFramework;

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
            RenderSurface.Resize(1280, 720);
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
            Raylib.DrawFPS(x, 10);
            Raylib.DrawText("Sharp Framework!", x + 200, 10, 32, Color.Black);
        }

        public override void End()
        {
            
        }
    }
}