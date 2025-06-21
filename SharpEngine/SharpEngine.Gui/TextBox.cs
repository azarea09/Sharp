using Raylib_cs;
using System;
using System.Numerics;
using System.Text;

namespace SharpEngine.Gui
{
    public class TextBox : GuiElement
    {
        public string Text = "";
        public int MaxLength = 32;
        public bool IsFocused = false;
        private int cursorBlinkCounter = 0;
        private bool showCursor = true;

        public TextBox(float x, float y, float w = 210, float h = 36) : base(x, y, w, h)
        {
        }

        public override void Draw()
        {
            // フォーカス切り替え
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                IsFocused = IsMouseOver();
            }

            Color backColor = IsMouseOver() ? RaylibTranslator.FromHtml("#a9a9c0") : RaylibTranslator.FromHtml("#4a4a5e");
            Color foreColor = IsMouseOver() ? RaylibTranslator.FromHtml("#5c5f74") : RaylibTranslator.FromHtml("#3d3d4c");

            Raylib.DrawRectangleRec(Rect, backColor);

            float border = MathF.Max(1, GetPixelScale());
            Raylib.DrawRectangleRec(
                new Rectangle(Rect.X + border, Rect.Y + border, Rect.Width - border * 2, Rect.Height - border * 2),
                foreColor
            );

            // テキスト描画
            string displayText = Text;
            float fontSize = 20;
            Vector2 textPos = new Vector2(Rect.X + 6, Rect.Y + Rect.Height / 2 - fontSize / 2);
            Raylib.DrawText(displayText, (int)textPos.X, (int)textPos.Y, (int)fontSize, Color.White);

            // 入力処理
            if (IsFocused)
            {
                // 文字入力
                int c;
                while ((c = Raylib.GetCharPressed()) != 0)
                {
                    if (Text.Length < MaxLength && c >= 32 && c <= 126)
                        Text += (char)c;
                }

                // バックスペース
                if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && Text.Length > 0)
                {
                    Text = Text[..^1];
                }

                // カーソル描画
                cursorBlinkCounter++;
                if (cursorBlinkCounter > 30)
                {
                    showCursor = !showCursor;
                    cursorBlinkCounter = 0;
                }

                if (showCursor)
                {
                    int textWidth = Raylib.MeasureText(displayText, (int)fontSize);
                    float cursorX = textPos.X + textWidth + 1;
                    Raylib.DrawLine((int)cursorX, (int)textPos.Y, (int)cursorX, (int)(textPos.Y + fontSize), Color.White);
                }
            }
        }
    }
}
