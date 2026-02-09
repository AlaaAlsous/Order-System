using Dapper;
using Microsoft.Data.Sqlite;
using System.Globalization;

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

        public bool Save(SqliteConnection conn)
        {
            try
            {
                Id = conn.QuerySingle<long>(@"
                INSERT INTO order_rows (order_id, product_id, description, quantity, unit_price)
                VALUES (@order_id, @product_id, @description, @quantity, @unit_price)
                RETURNING Id;
            ", new { order_id = OrderId, product_id = ProductId, description = Description, quantity = Quantity, unit_price = Price });
                return true;
            }
            catch (SqliteException ex)
            {
                ConsoleHelper.TextColor($"⚠️ Failed to add order item. Error: {ex.Message}\n", ConsoleColor.Red);
                return false;
            }
        }

        public static void Add(SqliteConnection conn)
        {
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╔═════════════════════════════════════╗", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║          CREATE ORDER ITEM          ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╚═════════════════════════════════════╝", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("Press ESC any time to cancel\n", Console.WindowWidth - 1), ConsoleColor.DarkGray);

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
                var customerName = Customer.GetCustomerName(conn, orderId);
                ConsoleHelper.TextColor($"📢 Adding order item for customer: {customerName}\n", ConsoleColor.DarkYellow);
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
                if (!Product.ProductExists(conn, productId))
                {
                    ConsoleHelper.TextColor($"⚠️ Product with ID (( {productId} )) does not exist. Please enter a valid Product ID.\n", ConsoleColor.Red);
                    continue;
                }
                var productName = Product.GetProductName(conn, productId);
                ConsoleHelper.TextColor($"📢 Selected product: {productName}", ConsoleColor.DarkYellow);
                availableStock = GetProductStock(conn, productId);
                if (availableStock.HasValue)
                {
                    ConsoleHelper.TextColor($"📢 Available stock: {availableStock.Value}", ConsoleColor.DarkYellow);
                }
                var productPrice = GetProductPrice(conn, productId);
                if (productPrice.HasValue)
                {
                    ConsoleHelper.TextColor($"📢 Product unit price: {productPrice.Value.ToString("C2", CultureInfo.CurrentCulture)}", ConsoleColor.DarkYellow);
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
                            ConsoleHelper.TextColor($"✅ Using product price: {productPrice.Value.ToString("C2", CultureInfo.CurrentCulture)}", ConsoleColor.DarkYellow);
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

            if (orderItem.Save(conn))
            {
                if (orderItem.ProductId.HasValue)
                {
                    UpdateProductStock(conn, orderItem.ProductId.Value, orderItem.Quantity);
                }

                Console.WriteLine();
                ConsoleHelper.TextColor($"✅ Order item (( {orderItem.Id} )) added successfully\n", ConsoleColor.DarkGreen);
            }

            ConsoleHelper.TextColor("Press any key to continue...", ConsoleColor.Gray);
            Console.ReadKey();
        }

        public static void ShowOrderItems(SqliteConnection conn)
        {
            var orderItems = conn.Query("SELECT * FROM order_overview WHERE orderitemid IS NOT NULL ORDER BY orderitemid");

            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╔═════════════════════════════════════╗", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║             ORDER ITEMS             ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╚═════════════════════════════════════╝", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();

            string separator = new string('-', 147);
            int tableWidth = separator.Length;
            string padding = ConsoleHelper.GetTablePadding(tableWidth);

            Console.Write(padding);
            ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);
            Console.Write(padding);
            ConsoleHelper.WriteTableRow(new string[]
            {
                ConsoleHelper.CenterText("Item ID", 7),
                ConsoleHelper.CenterText("Order", 5),
                ConsoleHelper.CenterText("Customer", 17),
                ConsoleHelper.CenterText("Date", 10),
                ConsoleHelper.CenterText("Status", 9),
                ConsoleHelper.CenterText("Product", 15),
                ConsoleHelper.CenterText("Description", 17),
                ConsoleHelper.CenterText("Quantity", 8),
                ConsoleHelper.CenterText("Unit Price", 14),
                ConsoleHelper.CenterText("Total Price", 14)
            }, ConsoleColor.Cyan, ConsoleColor.DarkGray);
            Console.Write(padding);
            ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);

            if (!orderItems.Any())
            {
                Console.WriteLine();
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("⚠️ No Order Items found.\n", Console.WindowWidth - 1), ConsoleColor.Yellow);
            }

            foreach (var item in orderItems)
            {
                long orderId = item.orderid;
                string customerName = item.customername;
                long orderDateUnix = item.orderdate;
                DateTime orderDate = DateTimeOffset.FromUnixTimeSeconds(orderDateUnix).DateTime;
                string status = item.status;
                long orderItemId = item.orderitemid;
                string productName = item.productname ?? "";
                string description = item.description ?? "";
                long quantity = item.quantity;
                decimal unitPrice = Convert.ToDecimal(item.unitprice);
                decimal totalPrice = Convert.ToDecimal(item.totalprice);

                Console.Write(padding);
                ConsoleHelper.WriteTableRow(new string[]
                {
                    ConsoleHelper.CenterText(orderItemId.ToString(), 7),
                    ConsoleHelper.CenterText(orderId.ToString(), 5),
                    ConsoleHelper.CenterText(customerName, 17),
                    ConsoleHelper.CenterText(orderDate.ToString("yyyy-MM-dd"), 10),
                    ConsoleHelper.CenterText(status, 9),
                    ConsoleHelper.CenterText(productName, 15),
                    ConsoleHelper.CenterText(description, 17),
                    ConsoleHelper.CenterText(quantity.ToString(), 8),
                    ConsoleHelper.CenterText(unitPrice.ToString("C2", CultureInfo.CurrentCulture), 14),
                    ConsoleHelper.CenterText(totalPrice.ToString("C2", CultureInfo.CurrentCulture), 14)
                }, ConsoleColor.White, ConsoleColor.DarkGray);
                Console.Write(padding);
                ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);
            }
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public bool Delete(SqliteConnection conn)
        {
            try
            {
                conn.Execute(@"DELETE FROM order_rows WHERE id = @id;", new { id = Id });
                return true;
            }
            catch (SqliteException ex)
            {
                ConsoleHelper.TextColor($"⚠️ Failed to delete order item. Error: {ex.Message}\n", ConsoleColor.Red);
                return false;
            }
        }

        public static void DeleteOrderItem(SqliteConnection conn)
        {
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╔═════════════════════════════════════╗", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║          DELETE ORDER ITEM          ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╚═════════════════════════════════════╝", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("Press ESC any time to cancel\n", Console.WindowWidth - 1), ConsoleColor.DarkGray);
            while (true)
            {
                Console.Write("Order Item ID to delete: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (!long.TryParse(input, out long orderItemId) || orderItemId <= 0)
                {
                    ConsoleHelper.TextColor("⚠️ Invalid Order Item ID. Please enter a valid number.\n", ConsoleColor.Red);
                    continue;
                }
                if (!OrderItemExists(conn, orderItemId))
                {
                    ConsoleHelper.TextColor($"⚠️ Order Item with ID (( {orderItemId} )) does not exist.\n", ConsoleColor.Red);
                    continue;
                }
                var orderId = conn.QuerySingle<long>("SELECT order_id FROM order_rows WHERE id = @orderItemId", new { orderItemId });
                var customerId = conn.QuerySingle<long>("SELECT customer_id FROM orders WHERE id = @orderId", new { orderId });
                var customerName = Customer.GetCustomerName(conn, customerId);
                ConsoleHelper.TextColor($"⚠️ Are you sure you want to delete order item for customer (( {customerName} ))? (y/n):", ConsoleColor.DarkYellow);
                Console.Write("Answer: ");
                var deletechoice = ConsoleHelper.ReadLineWithEscape();
                if (deletechoice == null) return;
                if (deletechoice.Trim().ToLower() != "y" && deletechoice.Trim().ToLower() != "yes")
                {
                    ConsoleHelper.TextColor("❎ Deletion cancelled.\n", ConsoleColor.Red);
                    break;
                }
                OrderItem orderItem = new OrderItem { Id = orderItemId };
                if (orderItem.Delete(conn))
                {
                    Console.WriteLine();
                    ConsoleHelper.TextColor($"✅ Order Item (( {orderItemId} )) deleted successfully\n", ConsoleColor.DarkGreen);
                }
                break;
            }
            ConsoleHelper.TextColor("Press any key to continue...", ConsoleColor.Gray);
            Console.ReadKey();
        }
        public static bool OrderExists(SqliteConnection conn, long orderId)
        {
            return conn.QuerySingle<bool>(@"SELECT EXISTS(SELECT 1 FROM orders WHERE id = @orderId);", new { orderId });
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

        public static bool OrderItemExists(SqliteConnection conn, long orderItemId)
        {
            return conn.QuerySingle<bool>(@"SELECT EXISTS(SELECT 1 FROM order_rows WHERE id = @orderItemId);", new { orderItemId });
        }
    }
}