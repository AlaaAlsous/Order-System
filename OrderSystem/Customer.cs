using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";

        public void Save(SqliteConnection conn)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"
                INSERT INTO customers (name, email, phone, address)
                VALUES (@name, @email, @phone, @address)
                RETURNING Id;
            ";
            command.Parameters.AddWithValue("@name", Name);
            command.Parameters.AddWithValue("@email", Email);
            command.Parameters.AddWithValue("@phone", Phone);
            command.Parameters.AddWithValue("@address", Address);

            Id = Convert.ToInt32(command.ExecuteScalar());
        }

        public static void Add(SqliteConnection conn)
        {
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("CREATE CUSTOMER", Console.WindowWidth - 1), ConsoleColor.Cyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();

            Customer customer = new Customer();

            while (true)
            {
                Console.Write("Name: ");
                var input = Console.ReadLine()?.Trim() ?? "";
                if (input.Length >= 3 && input.Length <= 20)
                {
                    customer.Name = input; break;
                }
                ConsoleHelper.TextColor("⚠️ Name cannot be empty. You must provide a valid name (between 3 and 20 characters).\n", ConsoleColor.Red
            );
            }
            while (true)
            {
                Console.Write("Email: ");
                var input = Console.ReadLine()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(input) || !input.Contains("@") || !input.Contains("."))
                {
                    ConsoleHelper.TextColor("⚠️ Email cannot be empty. You must provide a valid email.", ConsoleColor.Red);
                    continue;
                }
                if (EmailExists(conn, input))
                {
                    ConsoleHelper.TextColor("⚠️ This email already exists. Please use a different email.", ConsoleColor.Red);
                    continue;
                }
                customer.Email = input;
                break;
            }
            while (true)
            {
                Console.Write("Phone: ");
                var input = Console.ReadLine()?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(input) && input.Length >= 5 && input.Length <= 15 && long.TryParse(input, out _))
                {
                    customer.Phone = input; break;
                }
                ConsoleHelper.TextColor("⚠️ Phone cannot be empty. You must provide a valid phone number.", ConsoleColor.Red);
            }
            while (true)
            {
                Console.Write("Address: ");
                var input = Console.ReadLine()?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(input) && input.Length >= 5 && input.Length <= 100)
                {
                    customer.Address = input; break;
                }
                ConsoleHelper.TextColor("⚠️ Address cannot be empty. You must provide a valid address.", ConsoleColor.Red);
            }

            customer.Save(conn);

            Console.WriteLine();
            ConsoleHelper.TextColor($"✅ Customer '{customer.Name}' created successfully with ID: {customer.Id}\n", ConsoleColor.DarkGreen);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        public static bool EmailExists(SqliteConnection conn, string email)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"SELECT COUNT(*) FROM customers WHERE email = @email;";
            command.Parameters.AddWithValue("@email", email);

            var count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }
    }
}

