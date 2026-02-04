using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
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
            command.Parameters.AddWithValue("@product_id", ProductId.HasValue ? (object)ProductId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@description", string.IsNullOrEmpty(Description) ? DBNull.Value : Description);
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
            int? availableStock = null;
            while (true)
            {
                Console.Write("Product ID [Leave empty to skip]: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    orderItem.ProductId = null;
                    break;
                }
                if (!int.TryParse(input, out int productId))
                {
                    ConsoleHelper.TextColor("Invalid input. Please enter a valid number for Product ID.", ConsoleColor.Red);
                    continue;
                }
                if (!ProductExists(conn, productId))
                {
                    ConsoleHelper.TextColor($"Product with ID (( {productId} )) does not exist. Please enter a valid Product ID.", ConsoleColor.Red);
                    continue;
                }
                availableStock = GetProductStock(conn, productId);
                if (availableStock.HasValue)
                {
                    Console.WriteLine($"Available stock: {availableStock.Value}");
                }
                orderItem.ProductId = productId;
                break;
            }
            while (true)
            {
                Console.Write("Description [Leave empty to skip]: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (input.Length <= 25)
                {
                    orderItem.Description = input;
                    break;
                }
                ConsoleHelper.TextColor("Invalid input. Description cannot exceed 25 characters.", ConsoleColor.Red);
            }

            if (!orderItem.ProductId.HasValue && string.IsNullOrEmpty(orderItem.Description))
            {
                ConsoleHelper.TextColor("⚠️ You must provide either a Product ID or a Description (or both).", ConsoleColor.Red);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            while (true)
            {
                Console.Write("Quantity: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (!int.TryParse(input, out int quantity))
                {
                    ConsoleHelper.TextColor("Invalid input. Please enter a valid number for Quantity.", ConsoleColor.Red);
                    continue;
                }
                if (quantity <= 0)
                {
                    ConsoleHelper.TextColor("Quantity must be greater than 0.", ConsoleColor.Red);
                    continue;
                }
                if (availableStock.HasValue && availableStock.Value <= 0)
                {
                    ConsoleHelper.TextColor($"Warning: Product with ID (( {orderItem.ProductId} )) is out of stock.", ConsoleColor.Yellow);
                    continue;
                }
                if (availableStock.HasValue && quantity > availableStock.Value)
                {
                    ConsoleHelper.TextColor($"Warning: Insufficient stock for Product ID (( {orderItem.ProductId} )). Available: {availableStock.Value}, Required: {quantity}.", ConsoleColor.Yellow);
                    continue;
                }

                orderItem.Quantity = quantity;
                break;
            }
            Console.Write("Price: "); orderItem.Price = decimal.Parse(Console.ReadLine() ?? "0");

            orderItem.Save(conn);

            Console.WriteLine($"Order item '{orderItem.Id}' added successfully with ID: {orderItem.Id}.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        public static bool OrderExists(SqliteConnection conn, int orderId)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"SELECT EXISTS(SELECT 1 FROM orders WHERE id = @orderId);";
            command.Parameters.AddWithValue("@orderId", orderId);
            return Convert.ToBoolean(command.ExecuteScalar());
        }
        public static bool ProductExists(SqliteConnection conn, int productId)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"SELECT EXISTS(SELECT 1 FROM products WHERE id = @productId);";
            command.Parameters.AddWithValue("@productId", productId);
            return Convert.ToBoolean(command.ExecuteScalar());
        }
        public static int? GetProductStock(SqliteConnection conn, int productId)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"SELECT stock FROM products WHERE id = @productId;";
            command.Parameters.AddWithValue("@productId", productId);
            var result = command.ExecuteScalar();
            return result != DBNull.Value ? (int?)Convert.ToInt32(result) : null;
        }
    }
}