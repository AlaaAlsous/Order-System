namespace OrderSystem
{
    public class ConsoleHelper
    {
        public static string CenterText(string text, int width)
        {
            if (text.Length >= width) return text;
            int leftPadding = (width - text.Length) / 2;
            return text.PadLeft(leftPadding + text.Length).PadRight(width);
        }
    }
}
