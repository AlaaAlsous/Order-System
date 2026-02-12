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

        public bool Save(SqliteConnection conn)
        {
            try
            {
                Id = conn.QuerySingle<long>(@"
                    INSERT INTO customers (name, email, phone)
                    VALUES (@name, @email, @phone)
                    RETURNING Id;
                ", new { name = Name, email = Email, phone = Phone });
                return true;
            }
            catch (SqliteException ex)
            {
                ConsoleHelper.TextColor($"⚠️ Failed to create customer. Error: {ex.Message}\n", ConsoleColor.Red);
                return false;
            }
        }

        public static void Add(SqliteConnection conn)
        {
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╔═════════════════════════════════════╗", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║           CREATE CUSTOMER           ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╚═════════════════════════════════════╝", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("Press ESC any time to cancel\n", Console.WindowWidth - 1), ConsoleColor.DarkGray);

            Customer customer = new Customer();

            while (true)
            {
                Console.Write("Name: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                // Table: Name col ≈20; here max 20 to fit.
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
                Console.Write("Address Type [1= Delivery | 2= Billing]: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (input == "1")
                {
                    address.AddressType = "Delivery";
                    break;
                }
                else if (input == "2")
                {
                    address.AddressType = "Billing";
                    break;
                }
                ConsoleHelper.TextColor("⚠️ Invalid choice. Please enter 1 for Delivery or 2 for Billing.\n", ConsoleColor.Red);
            }

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

            if (customer.Save(conn))
            {
                address.CustomerId = customer.Id;
                if (address.Save(conn))
                {
                    Console.WriteLine();
                    ConsoleHelper.TextColor($"✅ Customer (( {customer.Name} )) created successfully with ID: {customer.Id}\n", ConsoleColor.DarkGreen);
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        }
        public static void ShowCustomers(SqliteConnection conn)
        {
            var customers = conn.Query("SELECT * FROM customers ORDER BY name ASC");
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╔═════════════════════════════════════╗", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║              CUSTOMERS              ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╚═════════════════════════════════════╝", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();

            string separator = new string('-', 88);
            int tableWidth = separator.Length;
            string padding = ConsoleHelper.GetTablePadding(tableWidth);

            Console.Write(padding);
            ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);
            Console.Write(padding);
            ConsoleHelper.WriteTableRow(new string[]
            {
                ConsoleHelper.CenterText("ID", 5),
                ConsoleHelper.CenterText("Name", 20),
                ConsoleHelper.CenterText("Email", 30),
                ConsoleHelper.CenterText("Phone", 20)
            }, ConsoleColor.Cyan, ConsoleColor.DarkGray);
            Console.Write(padding);
            ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);

            if (!customers.Any())
            {
                Console.WriteLine();
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("⚠️ No Customers found.\n", Console.WindowWidth - 1), ConsoleColor.Yellow);
            }

            foreach (var customer in customers)
            {
                long id = customer.Id;
                string name = customer.name;
                string email = customer.email;
                string phone = customer.phone;
                Console.Write(padding);
                ConsoleHelper.WriteTableRow(new string[]
                {
                    ConsoleHelper.CenterText(id.ToString(), 5),
                    ConsoleHelper.CenterText(name, 20),
                    ConsoleHelper.CenterText(email, 30),
                    ConsoleHelper.CenterText(phone, 20)
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
                conn.Execute("DELETE FROM customers WHERE id = @id", new { id = Id });
                return true;
            }
            catch (SqliteException ex)
            {
                ConsoleHelper.TextColor($"⚠️ An error occurred while deleting customer ID: {Id}. Error: {ex.Message}\n", ConsoleColor.Red);
                return false;
            }
        }
        public static void DeleteCustomer(SqliteConnection conn)
        {
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╔═════════════════════════════════════╗", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║           DELETE CUSTOMER           ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╚═════════════════════════════════════╝", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("Press ESC any time to cancel\n", Console.WindowWidth - 1), ConsoleColor.DarkGray);

            while (true)
            {
                Console.Write("Customer ID to delete: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (!long.TryParse(input, out long customerId) || customerId <= 0)
                {
                    ConsoleHelper.TextColor("⚠️ Invalid input. Please enter a valid customer ID.\n", ConsoleColor.Red);
                    continue;
                }
                if (!Customer.CustomerExists(conn, customerId))
                {
                    ConsoleHelper.TextColor($"⚠️ Customer with ID (( {customerId} )) does not exist.\n", ConsoleColor.Red);
                    continue;
                }
                if (HasOrders(conn, customerId))
                {
                    ConsoleHelper.TextColor($"⚠️ Cannot delete customer with ID: {customerId} because they have associated orders. Please delete the orders first.\n", ConsoleColor.Red);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }
                var customerName = Customer.GetCustomerName(conn, customerId);
                ConsoleHelper.TextColor($"⚠️ Are you sure you want to delete customer (( {customerName} ))? (y/n):", ConsoleColor.DarkYellow);
                Console.Write("Answer: ");
                var deletechoice = ConsoleHelper.ReadLineWithEscape();
                if (deletechoice == null) return;
                if (deletechoice.Trim().ToLower() != "y" && deletechoice.Trim().ToLower() != "yes")
                {
                    ConsoleHelper.TextColor("❎ Deletion cancelled.\n", ConsoleColor.Red);
                    break;
                }
                Customer customer = new Customer { Id = customerId };
                if (customer.Delete(conn))
                {
                    Console.WriteLine();
                    ConsoleHelper.TextColor($"✅ Customer with ID: {customerId} deleted successfully.\n", ConsoleColor.DarkGreen);
                }
                break;
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static bool EmailExists(SqliteConnection conn, string email)
        {
            return conn.QuerySingle<bool>(@"SELECT EXISTS(SELECT 1 FROM customers WHERE email = @email COLLATE NOCASE);", new { email });
        }

        public static bool CustomerExists(SqliteConnection conn, long customerId)
        {
            return conn.QuerySingle<bool>("SELECT EXISTS(SELECT 1 FROM customers WHERE id = @id)", new { id = customerId });
        }

        public static bool HasOrders(SqliteConnection conn, long customerId)
        {
            return conn.QuerySingle<bool>("SELECT EXISTS(SELECT 1 FROM orders WHERE customer_id = @customerId)", new { customerId });
        }

        public static string GetCustomerName(SqliteConnection conn, long customerId)
        {
            return conn.QuerySingle<string>("SELECT name FROM customers WHERE id = @id", new { id = customerId });
        }
    }
}
