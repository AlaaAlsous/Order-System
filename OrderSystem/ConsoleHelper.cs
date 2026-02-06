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

        public static string GetTablePadding(int tableWidth)
        {
            int leftPadding = (Console.WindowWidth - tableWidth) / 2;
            return leftPadding > 0 ? new string(' ', leftPadding) : "";
        }

        public static string? ReadLineWithEscape()
        {
            string input = "";
            while (true)
            {
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Escape)
                {
                    ConsoleHelper.TextColor("\n\nOperation Cancelled. Returning to main menu...", ConsoleColor.DarkGray);
                    ConsoleHelper.TextColor("Press any key to continue...\n", ConsoleColor.DarkGray);
                    Console.ReadKey(true);
                    return null;
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input = input.Substring(0, input.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    input += key.KeyChar;
                    Console.Write(key.KeyChar);
                }
            }
        }
    }
}
