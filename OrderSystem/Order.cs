using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Created";
        public void Save(SqliteConnection conn)
        {
            using var command = conn.CreateCommand();
            long unixTime = ((DateTimeOffset)OrderDate).ToUnixTimeSeconds();
            command.CommandText = @"
            INSERT INTO orders (customer_id, order_date, status)
            VALUES (@customer_id, @order_date, @status)
            RETURNING Id;
            ";
            command.Parameters.AddWithValue("@customer_id", CustomerId);
            command.Parameters.AddWithValue("@order_date", unixTime);
            command.Parameters.AddWithValue("@status", Status);

            Id = Convert.ToInt32(command.ExecuteScalar());
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
                if (!int.TryParse(input, out int customerId) || customerId <= 0)
                {
                    ConsoleHelper.TextColor("⚠️ Invalid Customer ID. Please enter a valid number.", ConsoleColor.Red);
                }
                if (!CustomerExists(conn, customerId))
                {
                    ConsoleHelper.TextColor("⚠️ Customer ID does not exist. Please enter a valid Customer ID.", ConsoleColor.Red);
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
                ConsoleHelper.TextColor("⚠️ Invalid date format. Please use 'YYYY-MM-DD'.", ConsoleColor.Red);
            }
            Console.WriteLine($"Status: {order.Status}");

            order.Save(conn);

            Console.WriteLine($"Order '{order.Id}' added successfully with ID: {order.Id}");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        public static bool CustomerExists(SqliteConnection conn, int customerId)
        {
            using var command = conn.CreateCommand();
            command.CommandText = "SELECT EXISTS(SELECT 1 FROM customers WHERE id = @id)";
            command.Parameters.AddWithValue("@id", customerId);
            return Convert.ToBoolean(command.ExecuteScalar());
        }
    }
}