using Dapper;
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
            try
            {
                Id = conn.QuerySingle<long>(@"
                INSERT INTO order_rows (order_id, product_id, description, quantity, unit_price)
                VALUES (@order_id, @product_id, @description, @quantity, @unit_price)
                RETURNING Id;
            ", new { order_id = OrderId, product_id = ProductId, description = Description, quantity = Quantity, unit_price = Price });
            }
            catch (SqliteException ex)
            {
                throw new InvalidOperationException("⚠️ Failed to add order item. Please ensure all fields are valid.", ex);
            }
        }

        public static void Add(SqliteConnection conn)
        {
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("CREATE ORDER ITEM", Console.WindowWidth - 1), ConsoleColor.Cyan);
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
                Console.Write("Price [Leave empty to use product price]: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    if (orderItem.ProductId.HasValue)
                    {
                        var productPrice = GetProductPrice(conn, orderItem.ProductId.Value);
                        if (productPrice.HasValue && productPrice.Value > 0)
                        {
                            orderItem.Price = productPrice.Value;
                            ConsoleHelper.TextColor($"✓ Using product price: {productPrice.Value:F2} kr\n", ConsoleColor.Green);
                            break;
                        }
                    }
                }
                if (!decimal.TryParse(input, out decimal price) || price <= 0)
                {
                    ConsoleHelper.TextColor("⚠️ Invalid input. Please enter a valid unit price.\n", ConsoleColor.Red);
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
            return conn.QuerySingle<bool>(@"SELECT EXISTS(SELECT 1 FROM orders WHERE id = @orderId);", new { orderId });
        }

        public static bool ProductExists(SqliteConnection conn, long productId)
        {
            return conn.QuerySingle<bool>(@"SELECT EXISTS(SELECT 1 FROM products WHERE id = @productId);", new { productId });
        }

        public static long? GetProductStock(SqliteConnection conn, long productId)
        {
            return conn.QuerySingle<long?>(@"SELECT stock FROM products WHERE id = @productId;", new { productId });
        }

        public static decimal? GetProductPrice(SqliteConnection conn, long productId)
        {
            return conn.QuerySingle<decimal?>(@"SELECT unit_price FROM products WHERE id = @productId;", new { productId });
        }

        public static void UpdateProductStock(SqliteConnection conn, long productId, long quantity)
        {
            conn.Execute(@"UPDATE products SET stock = stock - @quantity WHERE id = @productId;", new { productId, quantity });
        }
    }
}