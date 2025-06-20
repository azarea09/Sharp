namespace SharpEngine
{
    public static class RaylibTranslator
    {
        public static Raylib_cs.Color FromHtml(string htmlColor)
        {
            if (string.IsNullOrEmpty(htmlColor) || htmlColor[0] != '#' || (htmlColor.Length != 7 && htmlColor.Length != 9))
            {
                throw new ArgumentException("Invalid HTML color format. Expected format: #RRGGBB or #RRGGBBAA.");
            }

            byte r = Convert.ToByte(htmlColor.Substring(1, 2), 16);
            byte g = Convert.ToByte(htmlColor.Substring(3, 2), 16);
            byte b = Convert.ToByte(htmlColor.Substring(5, 2), 16);
            byte a = htmlColor.Length == 9 ? Convert.ToByte(htmlColor.Substring(7, 2), 16) : (byte)255;

            return new Raylib_cs.Color(r, g, b, a);
        }

        public static Raylib_cs.Color FromSysDrawingColor(System.Drawing.Color color)
        {
            return new Raylib_cs.Color(color.R, color.G, color.B, color.A);
        }

        public static Raylib_cs.Rectangle FromSysDrawingRectangle(System.Drawing.Rectangle rect)
        {
            return new Raylib_cs.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}
