using Raylib_cs;
using SharpEngine;
using SharpEngine.Gui;
using System.Numerics;

namespace Sharp_Test;

class Program
{
    public static void Main()
    {
        Audio audio = null;
        Audio don = null;
        Audio ka = null;
        Texture texture = null;

        Sharp.Init(
            beforeInit: () =>
            {
                Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
            },
            afterInit: () =>
            {
                audio = new Audio("メドレー「淫夢MAD」.ogg", true);
                audio.Pitch = 1.0;
                audio.Volume = 0.7;
                don = new Audio("se_neiro_00_v12a_don_c.ogg", false);
                ka = new Audio("se_neiro_00_v12a_katsu_c.ogg", false);
            },
            WindowSize: new Vector2(1280, 720),
            SceneSize: new Vector2(1920, 1080),
            WindowTitle: "Sharp Test"
        );

        while (!Raylib.WindowShouldClose())
        {
            Sharp.Loop(() =>
            {
                Raylib.ClearBackground(RaylibTranslator.FromHtml("#2a2b34"));

                if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    audio?.PlayOneShot();
                }

                if (Raylib.IsKeyPressed(KeyboardKey.F) || Raylib.IsKeyPressed(KeyboardKey.J))
                {
                    don?.PlayOneShot();
                }

                if (Raylib.IsKeyPressed(KeyboardKey.D) || Raylib.IsKeyPressed(KeyboardKey.K))
                {
                    ka?.PlayOneShot();
                }

                if (Raylib.IsKeyPressed(KeyboardKey.Left))
                {
                    audio.Pan -= 0.1;
                }

                if (Raylib.IsKeyPressed(KeyboardKey.Right))
                {
                    audio.Pan += 0.1;
                }

                if (Raylib.IsKeyPressed(KeyboardKey.Up))
                {
                    audio.PlaySpeed += 0.05;
                }

                if (Raylib.IsKeyPressed(KeyboardKey.Down))
                {
                    audio.PlaySpeed -= 0.05;
                }

                Raylib.DrawText("FPS : " + Raylib.GetFPS(), 12, 12, 28, Color.Black);
            });
        }
    }
}