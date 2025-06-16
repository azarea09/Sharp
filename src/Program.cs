using Raylib_cs;
using SharpEngine;
using System.Numerics;

namespace Sharp_Test;

class Program
{
    public static void Main()
    {
        Audio audio = null;
        Texture texture = null;
        Nuunlm nuunlm = null;
        TaikoAtlasFont font = null;

        Sharp.Init(
            beforeInit: () =>
            {
                Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
            },
            afterInit: () =>
            {
                audio = new Audio("メドレー「淫夢MAD」.ogg", true);
                texture = new Texture("nuun.png");
                font = new TaikoAtlasFont("jp_64.dds", "jp_64.xml");
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
            Sharp.Loop(() =>
            {
                Raylib.ClearBackground(Color.RayWhite);

                if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    audio?.PlayOneShot();
                }

                nuunlm.Update();

                nuunlm.Draw(0, 0);

                double scale = 1.0;
                var (textWidth, textHeight) = font.GetTextDimensions("それをしません←やめてね←うおｗ←どわーｗ←ガチイク！！", 0.8425, 0.8425, true, 7.8);
                if (textWidth >= 1000)
                {
                    scale = (1000 / textWidth) * 0.8425;
                }

                font.Draw(1878, 29, "それをしません←やめてね←うおｗ←どわーｗ←ガチイク！！", 7.8, new Vector2(0.8425f * (float)scale), 255, ReferencePoint.TopRight);


                Raylib.DrawText("FPS : " + Raylib.GetFPS(), 12, 12, 28, Color.Black);
            });
        }
    }
}