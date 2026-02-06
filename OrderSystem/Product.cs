using Dapper;
using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class Product
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public long Stock { get; set; }

        public void Save(SqliteConnection conn)
        {
            try
            {
                Id = conn.QuerySingle<long>(@"
                    INSERT INTO products (name, unit_price, stock)
                    VALUES (@name, @unit_price, @stock)
                    RETURNING Id;
                ", new { name = Name, unit_price = UnitPrice, stock = Stock });

            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                throw new InvalidOperationException("⚠️ A product with this name already exists. Please use a different name.", ex);
            }
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
                if (input.Length < 2 || input.Length > 20)
                {
                    ConsoleHelper.TextColor("⚠️ Name cannot be empty. Please enter a valid name (between 2 and 20 characters).\n", ConsoleColor.Red);
                    continue;
                }
                if (CheckName(conn, input))
                {
                    ConsoleHelper.TextColor("⚠️ A product with this name already exists. Please enter a different name.\n", ConsoleColor.Red);
                    continue;
                }
                product.Name = input;
                break;
            }
            while (true)
            {
                Console.Write("Unit Price: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                if (decimal.TryParse(input, out decimal unitPrice) && unitPrice > 0)
                {
                    product.UnitPrice = unitPrice;
                    break;
                }
                ConsoleHelper.TextColor("⚠️ Invalid input. Please enter a valid unit price.\n", ConsoleColor.Red);
            }
            while (true)
            {
                Console.Write("Stock: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                if (long.TryParse(input, out long stock) && stock > 0)
                {
                    product.Stock = stock;
                    break;
                }
                ConsoleHelper.TextColor("⚠️ Invalid input. Please enter a valid stock quantity.\n", ConsoleColor.Red);
            }

            product.Save(conn);

            Console.WriteLine();
            ConsoleHelper.TextColor($"✅ Product (( {product.Name} )) created successfully with ID: {product.Id}\n", ConsoleColor.DarkGreen);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static bool CheckName(SqliteConnection conn, string name)
        {
            return conn.QuerySingle<bool>(@"SELECT EXISTS(SELECT 1 FROM products WHERE name = @name COLLATE NOCASE);", new { name });
        }
    }
}