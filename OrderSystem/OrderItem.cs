using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string Description { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public void Save(SqliteConnection conn)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"
            INSERT INTO order_rows (order_id, product_id, description, quantity, unit_price)
            VALUES (@order_id, @product_id, @description, @quantity, @unit_price)
            RETURNING Id;
            ";
            command.Parameters.AddWithValue("@order_id", OrderId);
            command.Parameters.AddWithValue("@product_id", ProductId);
            command.Parameters.AddWithValue("@description", Description);
            command.Parameters.AddWithValue("@quantity", Quantity);
            command.Parameters.AddWithValue("@unit_price", Price);

            Id = Convert.ToInt32(command.ExecuteScalar());
        }

        public static void Add(SqliteConnection conn)
        {
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("ADD ORDER ITEM", Console.WindowWidth - 1), ConsoleColor.Cyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();

            OrderItem orderItem = new OrderItem();

            while (true)
            {
                Console.Write("Order ID: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (!int.TryParse(input, out int orderId))
                {
                    ConsoleHelper.TextColor("Invalid input. Please enter a valid number for Order ID.", ConsoleColor.Red);
                    continue;
                }
                if (!OrderExists(conn, orderId))
                {
                    ConsoleHelper.TextColor($"Order with ID (( {orderId} )) does not exist. Please enter a valid Order ID.", ConsoleColor.Red);
                    continue;
                }
                orderItem.OrderId = orderId;
                break;
            }
            while (true)
            {
                Console.Write("Product ID: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (!int.TryParse(input, out int productId))
                {
                    ConsoleHelper.TextColor("Invalid input. Please enter a valid integer for Product ID.", ConsoleColor.Red);
                    continue;
                }
                if (!ProductExists(conn, productId))
                {
                    ConsoleHelper.TextColor($"Product with ID (( {productId} )) does not exist. Please enter a valid Product ID.", ConsoleColor.Red);
                    continue;
                }
                orderItem.ProductId = productId;
                break;
            }
            Console.Write("Description: "); orderItem.Description = Console.ReadLine() ?? "";
            Console.Write("Quantity: "); orderItem.Quantity = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Price: "); orderItem.Price = decimal.Parse(Console.ReadLine() ?? "0");

            orderItem.Save(conn);

            Console.WriteLine($"Order item '{orderItem.Id}' added successfully with ID: {orderItem.Id}.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        public static bool OrderExists(SqliteConnection conn, int orderId)
        {
            using var command = conn.CreateCommand();
            command.CommandText = "SELECT EXISTS(SELECT 1 FROM orders WHERE id = @orderId);";
            command.Parameters.AddWithValue("@orderId", orderId);
            return Convert.ToBoolean(command.ExecuteScalar());
        }
        public static bool ProductExists(SqliteConnection conn, int productId)
        {
            using var command = conn.CreateCommand();
            command.CommandText = "SELECT EXISTS(SELECT 1 FROM products WHERE id = @productId);";
            command.Parameters.AddWithValue("@productId", productId);
            return Convert.ToBoolean(command.ExecuteScalar());
        }
    }
}