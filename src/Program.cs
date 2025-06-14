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
        Nuunlm nuunlm = null;

        Sharp.Sharp.Init(
            beforeInit: () =>
            {
                Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
            },
            afterInit: () =>
            {
                audio = new Audio("メドレー「淫夢MAD」.ogg", true);
                texture = new Texture("elephant_1f418.png");
                nuunlm = new Nuunlm();
                nuunlm.Load("donbg_a_06_1p\\donbg_a_06_1p.nuunlm");
                nuunlm.Start(true);
            },
            WindowSize: new Vector2(1280, 720),
            SceneSize: new Vector2(1920, 1080),
            WindowTitle: "Sharp Test"
        );

        while (!Raylib.WindowShouldClose())
        {
            Sharp.Sharp.Loop(() =>
            {
                Raylib.ClearBackground(Color.RayWhite);

                if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    audio?.PlayOneShot();
                }

                nuunlm.Update();

                nuunlm.Draw(0, 0);


                Raylib.DrawText("FPS : " + Raylib.GetFPS(), 12, 12, 20, Color.Black);

            });
        }
    }
}