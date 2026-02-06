using Dapper;
using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class Customer
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";

        public void Save(SqliteConnection conn)
        {
            try
            {
                Id = conn.QuerySingle<long>(@"
                    INSERT INTO customers (name, email, phone)
                    VALUES (@name, @email, @phone)
                    RETURNING Id;
                ", new { name = Name, email = Email, phone = Phone });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                throw new InvalidOperationException("⚠️ This email already exists. Please use a different email.", ex);
            }
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
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (input.Length >= 3 && input.Length <= 20)
                {
                    customer.Name = input; break;
                }
                ConsoleHelper.TextColor("⚠️ Name cannot be empty. And you must provide a valid name (between 3 and 20 characters).\n", ConsoleColor.Red);
            }
            while (true)
            {
                Console.Write("Email: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (input.Length == 0 || !input.Contains("@") || !input.Contains(".") || input.IndexOf("@") > input.LastIndexOf(".") || input.Contains(" "))
                {
                    ConsoleHelper.TextColor("⚠️ Email cannot be empty. And you must provide a valid email.\n", ConsoleColor.Red);
                    continue;
                }
                if (EmailExists(conn, input))
                {
                    ConsoleHelper.TextColor("⚠️ This email already exists. Please use a different email.\n", ConsoleColor.Red);
                    continue;
                }
                customer.Email = input;
                break;
            }
            while (true)
            {
                Console.Write("Phone: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (input.Length >= 5 && input.Length <= 20 && long.TryParse(input, out _))
                {
                    customer.Phone = input; break;
                }
                ConsoleHelper.TextColor("⚠️ Phone cannot be empty. And please provide a valid phone number.\n", ConsoleColor.Red);
            }
            Console.WriteLine();
            ConsoleHelper.TextColor("--- Address Information ---\n", ConsoleColor.Yellow);

            Address address = new Address();

            while (true)
            {
                Console.Write("Street: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (input.Length >= 3 && input.Length <= 50)
                {
                    address.Street = input; break;
                }
                ConsoleHelper.TextColor("⚠️ Street cannot be empty. And please provide a valid street (between 3 and 50 characters).\n", ConsoleColor.Red);
            }

            while (true)
            {
                Console.Write("City: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (input.Length >= 2 && input.Length <= 20)
                {
                    address.City = input; break;
                }
                ConsoleHelper.TextColor("⚠️ City cannot be empty. And please provide a valid city (between 2 and 20 characters).\n", ConsoleColor.Red);
            }

            while (true)
            {
                Console.Write("Zip Code: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (input.Length >= 3 && input.Length <= 15)
                {
                    address.ZipCode = input; break;
                }
                ConsoleHelper.TextColor("⚠️ Zip code cannot be empty. And please provide a valid zip code (between 3 and 15 characters).\n", ConsoleColor.Red);
            }

            while (true)
            {
                Console.Write("Country: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (input.Length >= 2 && input.Length <= 50)
                {
                    address.Country = input; break;
                }
                ConsoleHelper.TextColor("⚠️ Country cannot be empty. And please provide a valid country (between 2 and 50 characters).\n", ConsoleColor.Red);
            }

            customer.Save(conn);
            address.CustomerId = customer.Id;
            address.Save(conn);

            Console.WriteLine();
            ConsoleHelper.TextColor($"✅ Customer (( {customer.Name} )) created successfully with ID: {customer.Id}\n", ConsoleColor.DarkGreen);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        }
        public static bool EmailExists(SqliteConnection conn, string email)
        {
            return conn.QuerySingle<bool>(@"SELECT EXISTS(SELECT 1 FROM customers WHERE email = @email);", new { email });
        }
    }
}

