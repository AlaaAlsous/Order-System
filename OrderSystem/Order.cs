using Dapper;
using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class Order
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Created";

        public bool Save(SqliteConnection conn)
        {
            try
            {
                long unixTime = ((DateTimeOffset)OrderDate).ToUnixTimeSeconds();
                Id = conn.QuerySingle<long>(@"
                    INSERT INTO orders (customer_id, order_date, status)
                    VALUES (@customer_id, @order_date, @status)
                    RETURNING Id;
                ", new { customer_id = CustomerId, order_date = unixTime, status = Status });
                return true;
            }
            catch (SqliteException ex)
            {
                ConsoleHelper.TextColor($"⚠️ Failed to create order. Error: {ex.Message}\n", ConsoleColor.Red);
                return false;
            }
        }

        public static void Add(SqliteConnection conn)
        {
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("CREATE ORDER", Console.WindowWidth - 1), ConsoleColor.Cyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();

            Order order = new Order();
            while (true)
            {
                Console.Write("Customer ID: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (!long.TryParse(input, out long customerId) || customerId <= 0)
                {
                    ConsoleHelper.TextColor("⚠️ Invalid Customer ID. Please enter a valid number.\n", ConsoleColor.Red);
                    continue;
                }
                if (!Customer.CustomerExists(conn, customerId))
                {
                    ConsoleHelper.TextColor("⚠️ Customer ID does not exist. Please enter a valid Customer ID.\n", ConsoleColor.Red);
                    continue;
                }
                order.CustomerId = customerId;
                break;
            }
            while (true)
            {
                Console.Write("Order Date and Time (YYYY-MM-DD) [Leave empty for current UTC time]: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    order.OrderDate = DateTime.UtcNow;
                    break;
                }
                else if (DateTime.TryParseExact(input, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime orderDate))
                {
                    order.OrderDate = orderDate.ToUniversalTime();
                    break;
                }
                ConsoleHelper.TextColor("⚠️ Invalid date format. Please use 'YYYY-MM-DD'.\n", ConsoleColor.Red);
            }
            while (true)
            {
                Console.Write("Status (Created/Paid/Delivered) [Default: Created]: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (string.IsNullOrEmpty(input))
                {
                    order.Status = "Created"; break;
                }
                var validStatuses = new[] { "Created", "Paid", "Delivered" };
                if (validStatuses.Contains(input, StringComparer.OrdinalIgnoreCase))
                {
                    order.Status = char.ToUpper(input[0]) + input.Substring(1).ToLower();
                    break;
                }
                ConsoleHelper.TextColor("⚠️ Invalid status. Please enter 'Created', 'Paid', or 'Delivered'.\n", ConsoleColor.Red);
            }

            if (order.Save(conn))
            {
                Console.WriteLine();
                ConsoleHelper.TextColor($"✅ Order (( {order.Id} )) added successfully\n", ConsoleColor.DarkGreen);
            }

            ConsoleHelper.TextColor("Press any key to continue...", ConsoleColor.Gray);
            Console.ReadKey();
        }

        public static void ShowOrders(SqliteConnection conn)
        {
            var orders = conn.Query(@"
                SELECT 
                    orders.id AS orderid,
                    orders.customer_id AS customerid,
                    customers.name AS customername,
                    orders.order_date AS orderdate,
                    orders.status AS status
                FROM orders
                JOIN customers ON orders.customer_id = customers.id
                ORDER BY orders.id
            ");
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("ORDERS", Console.WindowWidth - 1), ConsoleColor.Cyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();
            string separator = new string('-', 111);
            int tableWidth = separator.Length + 4;
            string padding = ConsoleHelper.GetTablePadding(tableWidth);

            Console.Write(padding);
            ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);
            Console.Write(padding);
            ConsoleHelper.WriteTableRow(new string[]
            {
                ConsoleHelper.CenterText("ID", 5),
                ConsoleHelper.CenterText("Customer ID", 20),
                ConsoleHelper.CenterText("Customer Name", 20),
                ConsoleHelper.CenterText("Order Date", 30),
                ConsoleHelper.CenterText("Status", 20)
            }, ConsoleColor.Cyan, ConsoleColor.DarkGray);
            Console.Write(padding);
            ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);
            foreach (var order in orders)
            {
                long id = order.orderid;
                long customerId = order.customerid;
                string customerName = order.customername;
                DateTime orderDate = DateTimeOffset.FromUnixTimeSeconds(order.orderdate).UtcDateTime;
                string status = order.status;
                Console.Write(padding);
                ConsoleHelper.WriteTableRow(new string[]
                {
                    ConsoleHelper.CenterText(id.ToString(), 5),
                    ConsoleHelper.CenterText(customerId.ToString(), 20),
                    ConsoleHelper.CenterText(customerName, 20),
                    ConsoleHelper.CenterText(orderDate.ToString("yyyy-MM-dd"), 30),
                    ConsoleHelper.CenterText(status, 20)
                }, ConsoleColor.White, ConsoleColor.DarkGray);
                Console.Write(padding);
                ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);
            }

            ConsoleHelper.TextColor("Press any key to continue...", ConsoleColor.Gray);
            Console.ReadKey();
        }

        public bool Delete(SqliteConnection conn)
        {
            try
            {
                conn.Execute("DELETE FROM orders WHERE Id = @id", new { id = Id });
                return true;
            }
            catch (SqliteException ex)
            {
                ConsoleHelper.TextColor($"⚠️ Failed to delete order ID: {Id}. Error: {ex.Message}\n", ConsoleColor.Red);
                return false;
            }
        }

        public static void DeleteOrder(SqliteConnection conn)
        {
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("DELETE ORDER", Console.WindowWidth - 1), ConsoleColor.Cyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();
            while (true)
            {
                Console.Write("Order ID to delete: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (!long.TryParse(input, out long orderId) || orderId <= 0)
                {
                    ConsoleHelper.TextColor("⚠️ Invalid Order ID. Please enter a valid number.\n", ConsoleColor.Red);
                    continue;
                }
                if (!OrderExists(conn, orderId))
                {
                    ConsoleHelper.TextColor($"⚠️ Order with ID (( {orderId} )) does not exist.\n", ConsoleColor.Red);
                    continue;
                }

                Order order = new Order { Id = orderId };
                if (order.Delete(conn))
                {
                    Console.WriteLine();
                    ConsoleHelper.TextColor($"✅ Order (( {orderId} )) deleted successfully\n", ConsoleColor.DarkGreen);
                }
                break;
            }
            ConsoleHelper.TextColor("Press any key to continue...", ConsoleColor.Gray);
            Console.ReadKey();
        }
        public static bool OrderExists(SqliteConnection conn, long orderId)
        {
            return conn.QuerySingle<bool>("SELECT EXISTS(SELECT 1 FROM orders WHERE id = @orderId)", new { orderId });
        }

    }
}
