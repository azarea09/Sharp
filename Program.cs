using Raylib_cs;
using System.Numerics;
using Sharp;

namespace Sharp_Test;

class Program
{
    // STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
    [STAThread]
    public static void Main()
    {
        Audio audio = null;
        Texture texture = null;

        Sharp.Sharp.Init(
            beforeInit: () =>
            {
                Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
            },
            afterInit: () =>
            {
                audio = new Audio("メドレー「淫夢MAD」.ogg", true, 160, 170);
                texture = new Texture("elephant_1f418.png");
            },
            WindowSize: new Vector2(1280, 720),
            SceneSize: new Vector2(1920, 1080),
            WindowTitle: "Sharp Test"
        );

        while (!Raylib.WindowShouldClose())
        {
            Sharp.Sharp.Loop(() =>
            {
                if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    audio?.PlayOneShot();
                }

                Raylib.ClearBackground(Color.RayWhite);
                Raylib.DrawText("FPS : " + Raylib.GetFPS(), 12, 12, 20, Color.Black);

                // スケール0.8倍、透明度125のテクスチャを中心に描画
                texture.Scaled(0.8).Colored(125).DrawAt(0, 0);

                // スケール0.5倍、透明度255のテクスチャを左上原点で、右上基準(300,100)に描画
                texture.Scaled(0.5);
                texture.Colored(255);
                texture.Origined(ReferencePoint.TopRight);
                texture.Blended(BlendState.Additive);
                texture.Draw(300, 100);
                texture.Blended(BlendState.Alpha);

                // スケール0.5倍、透明度255のテクスチャを左上原点で、右上基準(600,100)に描画 (色を赤に) Coloredは(RGBA)
                texture.Scaled(0.5).Colored(255, 0, 0, 255).Origined(ReferencePoint.TopRight).Draw(600, 100);
            });
        }
    }
}