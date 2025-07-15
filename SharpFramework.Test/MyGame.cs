using Raylib_cs;
using SharpFramework.Core;

namespace SharpFramework.Test
{
    public class MyGame : Game
    {
        private int x = 10;
        public override void Init()
        {
            Window.SetTitle("ちぶりゲーム");
            Window.SetDarkMode(true);
            Window.Resize(1280, 720);
            RenderSurface.Resize(1920, 1080);
            RenderSurface.SetUseRenderSurface(false);
            Sharp.SetVsync(false);
        }

        public override void Update()
        {
            // 入力処理とか
            if (Raylib.IsKeyPressed(KeyboardKey.Left))
                x -= 100;
            if (Raylib.IsKeyPressed(KeyboardKey.Right))
                x += 100;
        }

        public override void Draw()
        {
            Raylib.DrawFPS(x + 1000, 10);
            Sharp.DrawText("うおｗ", 0, 0, Sharp.FontSize.Medium);
        }

        public override void End()
        {

        }
    }
}