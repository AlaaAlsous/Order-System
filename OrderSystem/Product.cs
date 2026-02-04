using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double UnitPrice { get; set; }
        public int Stock { get; set; }
        public void Save(SqliteConnection conn)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"
            INSERT INTO products (name, unit_price, stock)
            VALUES (@name, @unit_price, @stock)
            RETURNING Id;
            ";
            command.Parameters.AddWithValue("@name", Name);
            command.Parameters.AddWithValue("@unit_price", UnitPrice);
            command.Parameters.AddWithValue("@stock", Stock);

            Id = Convert.ToInt32(command.ExecuteScalar());
        }
        public static void Add(SqliteConnection conn)
        {
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("CREATE PRODUCT", Console.WindowWidth - 1), ConsoleColor.Cyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();

            Product product = new Product();

            while (true)
            {
                Console.Write("Name: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (input.Length >= 2 && input.Length <= 15)
                {
                    product.Name = input;
                    break;
                }
                ConsoleHelper.TextColor("Name cannot be empty. Please enter a valid name (between 2 and 20 characters).", ConsoleColor.Red);
            }
            while (true)
            {
                Console.Write("Unit Price: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                if (double.TryParse(input, out double unitPrice) && unitPrice > 0)
                {
                    product.UnitPrice = unitPrice;
                    break;
                }
                ConsoleHelper.TextColor("⚠️ Invalid input. Please enter a valid unit price.", ConsoleColor.Red);
            }
            while (true)
            {
                Console.Write("Stock: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                if (int.TryParse(input, out int stock) && stock > 0)
                {
                    product.Stock = stock;
                    break;
                }
                ConsoleHelper.TextColor("⚠️ Invalid input. Please enter a valid stock quantity.", ConsoleColor.Red);
            }

            product.Save(conn);

            Console.WriteLine();
            ConsoleHelper.TextColor($"✅ Product (( {product.Name} )) created successfully with ID: {product.Id}\n", ConsoleColor.DarkGreen);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}