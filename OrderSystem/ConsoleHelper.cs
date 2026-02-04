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
        public static void TextColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void WriteTableRow(string[] columns, ConsoleColor columnColor, ConsoleColor separatorColor)
        {
            Console.ForegroundColor = separatorColor;
            Console.Write("| ");

            for (int i = 0; i < columns.Length; i++)
            {
                Console.ForegroundColor = columnColor;
                Console.Write(columns[i]);
                Console.ForegroundColor = separatorColor;
                Console.Write(" | ");
            }
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}
