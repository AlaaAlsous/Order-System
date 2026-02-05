using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class OrderItem
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long? ProductId { get; set; }
        public string Description { get; set; } = "";
        public long Quantity { get; set; }
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

            Id = Convert.ToInt64(command.ExecuteScalar());
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
                if (!long.TryParse(input, out long orderId))
                {
                    ConsoleHelper.TextColor("⚠️ Invalid input. Please enter a valid number.\n", ConsoleColor.Red);
                    continue;
                }
                if (!OrderExists(conn, orderId))
                {
                    ConsoleHelper.TextColor($"⚠️ Order with ID (( {orderId} )) does not exist. Please enter a valid Order ID.\n", ConsoleColor.Red);
                    continue;
                }
                orderItem.OrderId = orderId;
                break;
            }
            long? availableStock = null;
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
                if (!long.TryParse(input, out long productId))
                {
                    ConsoleHelper.TextColor("⚠️ Invalid input. Please enter a valid number.\n", ConsoleColor.Red);
                    continue;
                }
                if (!ProductExists(conn, productId))
                {
                    ConsoleHelper.TextColor($"⚠️ Product with ID (( {productId} )) does not exist. Please enter a valid Product ID.\n", ConsoleColor.Red);
                    continue;
                }
                availableStock = GetProductStock(conn, productId);
                if (availableStock.HasValue)
                {
                    ConsoleHelper.TextColor($"📢 Available stock: {availableStock.Value}", ConsoleColor.DarkYellow);
                }
                var productPrice = GetProductPrice(conn, productId);
                if (productPrice.HasValue)
                {
                    ConsoleHelper.TextColor($"📢 Product unit price: {productPrice.Value:F2} kr", ConsoleColor.DarkYellow);
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
                ConsoleHelper.TextColor("⚠️ Invalid input. Description cannot exceed 25 characters.\n", ConsoleColor.Red);
            }

            if (!orderItem.ProductId.HasValue && string.IsNullOrEmpty(orderItem.Description))
            {
                ConsoleHelper.TextColor("⚠️ You must provide either a Product ID or a Description (or both).\n", ConsoleColor.Red);
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
                if (!long.TryParse(input, out long quantity))
                {
                    ConsoleHelper.TextColor("⚠️ Invalid input. Please enter a valid number for Quantity.\n", ConsoleColor.Red);
                    continue;
                }
                if (quantity <= 0)
                {
                    ConsoleHelper.TextColor("⚠️ Quantity must be greater than 0.\n", ConsoleColor.Red);
                    continue;
                }
                if (availableStock.HasValue && availableStock.Value <= 0)
                {
                    ConsoleHelper.TextColor($"⚠️ Warning: Product with ID (( {orderItem.ProductId} )) is out of stock.\n", ConsoleColor.Yellow);
                    continue;
                }
                if (availableStock.HasValue && quantity > availableStock.Value)
                {
                    ConsoleHelper.TextColor($"⚠️ Warning: Insufficient stock for Product ID (( {orderItem.ProductId} )). Available: {availableStock.Value}, Required: {quantity}.\n", ConsoleColor.Yellow);
                    continue;
                }

                orderItem.Quantity = quantity;
                break;
            }
            while (true)
            {
                Console.Write("Price: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (!decimal.TryParse(input, out decimal price))
                {
                    ConsoleHelper.TextColor("⚠️ Invalid input. Please enter a valid unit price.\n", ConsoleColor.Red);
                    continue;
                }
                if (price <= 0)
                {
                    ConsoleHelper.TextColor("⚠️ Price must be greater than 0.\n", ConsoleColor.Red);
                    continue;
                }

                orderItem.Price = price;
                break;
            }

            orderItem.Save(conn);

            if (orderItem.ProductId.HasValue)
            {
                UpdateProductStock(conn, orderItem.ProductId.Value, orderItem.Quantity);
            }

            Console.WriteLine();
            ConsoleHelper.TextColor($"✅ Order item (( {orderItem.Id} )) added successfully\n", ConsoleColor.DarkGreen);
            ConsoleHelper.TextColor("Press any key to continue...", ConsoleColor.Gray);
            Console.ReadKey();
        }
        public static bool OrderExists(SqliteConnection conn, long orderId)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"SELECT EXISTS(SELECT 1 FROM orders WHERE id = @orderId);";
            command.Parameters.AddWithValue("@orderId", orderId);
            return Convert.ToBoolean(command.ExecuteScalar());
        }

        public static bool ProductExists(SqliteConnection conn, long productId)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"SELECT EXISTS(SELECT 1 FROM products WHERE id = @productId);";
            command.Parameters.AddWithValue("@productId", productId);
            return Convert.ToBoolean(command.ExecuteScalar());
        }

        public static long? GetProductStock(SqliteConnection conn, long productId)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"SELECT stock FROM products WHERE id = @productId;";
            command.Parameters.AddWithValue("@productId", productId);
            var result = command.ExecuteScalar();
            return result != DBNull.Value ? (long?)Convert.ToInt64(result) : null;
        }

        public static decimal? GetProductPrice(SqliteConnection conn, long productId)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"SELECT unit_price FROM products WHERE id = @productId;";
            command.Parameters.AddWithValue("@productId", productId);
            var result = command.ExecuteScalar();
            if (result != DBNull.Value)
            {
                return (decimal)Convert.ToDecimal(result);
            }
            return null;
        }

        public static void UpdateProductStock(SqliteConnection conn, long productId, long quantity)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"UPDATE products SET stock = stock - @quantity WHERE id = @productId;";
            command.Parameters.AddWithValue("@productId", productId);
            command.Parameters.AddWithValue("@quantity", quantity);
            command.ExecuteNonQuery();
        }
    }
}