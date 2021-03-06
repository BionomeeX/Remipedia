namespace Remipedia
{
    public class ColorString
    {
        public static ColorString Default = new()
        {
            First = ColorValue.R,
            Second = ColorValue.G,
            Third = ColorValue.B
        };

        public ColorValue First { init; get; }
        public ColorValue Second { init; get; }
        public ColorValue Third { init; get; }

        public override string ToString()
        {
            return ((int)First).ToString() + ((int)Second).ToString() + ((int)Third).ToString();
        }

        public enum ColorValue
        {
            R,
            G,
            B
        }
    }
}
