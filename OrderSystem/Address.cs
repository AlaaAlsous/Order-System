using Dapper;
using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class Address
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string AddressType { get; set; } = "Delivery";
        public string Street { get; set; } = "";
        public string City { get; set; } = "";
        public string ZipCode { get; set; } = "";
        public string Country { get; set; } = "";

        public bool Save(SqliteConnection conn)
        {
            try
            {
                Id = conn.QuerySingle<long>(@"
                    INSERT INTO addresses (customer_id, address_type, street, city, zip_code, country)
                    VALUES (@customerId, @addressType, @street, @city, @zipCode, @country)
                    RETURNING id;
                ", new { customerId = CustomerId, addressType = AddressType, street = Street, city = City, zipCode = ZipCode, country = Country });
                return true;
            }
            catch (SqliteException ex)
            {
                ConsoleHelper.TextColor($"⚠️ Failed to add address. Error: {ex.Message}\n", ConsoleColor.Red);
                return false;
            }
        }

        public static void Add(SqliteConnection conn)
        {
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╔═════════════════════════════════════╗", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║       ADD ADDRESS TO CUSTOMER       ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╚═════════════════════════════════════╝", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("Press ESC any time to cancel\n", Console.WindowWidth - 1), ConsoleColor.DarkGray);

            Address address = new Address();
            long customerId = 0;
            while (true)
            {
                Console.Write("Customer ID: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (!long.TryParse(input, out customerId))
                {
                    ConsoleHelper.TextColor("⚠️ Invalid input. Please enter a valid customer ID.\n", ConsoleColor.Red);
                    continue;
                }
                if (!Customer.CustomerExists(conn, customerId))
                {
                    ConsoleHelper.TextColor($"⚠️ Customer with ID (( {customerId} )) does not exist.\n", ConsoleColor.Red);
                    continue;
                }
                var customerName = Customer.GetCustomerName(conn, customerId);
                var addressType = Address.GetAddressType(conn, customerId);
                ConsoleHelper.TextColor($"📢 Adding address for Customer: {customerName} (Already has '{addressType}' address)\n", ConsoleColor.DarkYellow);
                address.CustomerId = customerId;
                break;
            }

            while (true)
            {
                Console.Write("Address Type [1= Delivery | 2= Billing]: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (input == "1" && !HasAddressType(conn, customerId, "Delivery"))
                {
                    address.AddressType = "Delivery";
                    break;
                }
                else if (input == "2" && !HasAddressType(conn, customerId, "Billing"))
                {
                    address.AddressType = "Billing";
                    break;
                }
                else if (input == "1" || input == "2")
                {
                    string type = input == "1" ? "Delivery" : "Billing";
                    ConsoleHelper.TextColor($"⚠️ Customer already has a {type} address. Please choose a different type.\n", ConsoleColor.Red);
                }
                else
                {
                    ConsoleHelper.TextColor("⚠️ Invalid choice. Please enter 1 for Delivery or 2 for Billing.\n", ConsoleColor.Red);
                }
            }

            while (true)
            {
                Console.Write("Street: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (input.Length >= 3 && input.Length <= 50)
                {
                    address.Street = input;
                    break;
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
                    address.City = input;
                    break;
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
                    address.ZipCode = input;
                    break;
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
                    address.Country = input;
                    break;
                }
                ConsoleHelper.TextColor("⚠️ Country cannot be empty. And please provide a valid country (between 2 and 50 characters).\n", ConsoleColor.Red);
            }

            if (address.Save(conn))
            {
                Console.WriteLine();
                ConsoleHelper.TextColor($"✅ {address.AddressType} address (( {address.Id} )) added successfully for Customer ID {address.CustomerId}", ConsoleColor.DarkGreen);
            }
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public static void ShowAddresses(SqliteConnection conn)
        {
            var addresses = conn.Query(@"
                SELECT *,  customers.name AS customer_name
                FROM addresses
                JOIN customers ON addresses.customer_id = customers.Id
                ORDER BY customer_id ASC
            ");
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╔═════════════════════════════════════╗", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║              ADDRESSES              ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                     ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╚═════════════════════════════════════╝", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();

            string separator = new string('-', 139);
            int tableWidth = separator.Length;
            string padding = ConsoleHelper.GetTablePadding(tableWidth);

            Console.Write(padding);
            ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);
            Console.Write(padding);
            ConsoleHelper.WriteTableRow(new string[]
            {
                ConsoleHelper.CenterText("ID", 5),
                ConsoleHelper.CenterText("Customer ID", 12),
                ConsoleHelper.CenterText("Customer Name", 15),
                ConsoleHelper.CenterText("Address Type", 15),
                ConsoleHelper.CenterText("Street", 25),
                ConsoleHelper.CenterText("City", 15),
                ConsoleHelper.CenterText("Zip Code", 12),
                ConsoleHelper.CenterText("Country", 15)
            }, ConsoleColor.Cyan, ConsoleColor.DarkGray);
            Console.Write(padding);
            ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);

            if (!addresses.Any())
            {
                Console.WriteLine();
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("⚠️ No Addresses found.\n", Console.WindowWidth - 1), ConsoleColor.Yellow);
            }

            foreach (var address in addresses)
            {
                long id = address.id;
                long customerId = address.customer_id;
                string customerName = address.customer_name;
                string addressType = address.address_type;
                string street = address.street;
                string city = address.city;
                string zipCode = address.zip_code;
                string country = address.country;
                Console.Write(padding);
                ConsoleHelper.WriteTableRow(new string[]
                {
                    ConsoleHelper.CenterText(id.ToString(), 5),
                    ConsoleHelper.CenterText(customerId.ToString(), 12),
                    ConsoleHelper.CenterText(customerName, 15),
                    ConsoleHelper.CenterText(addressType, 15),
                    ConsoleHelper.CenterText(street, 25),
                    ConsoleHelper.CenterText(city, 15),
                    ConsoleHelper.CenterText(zipCode, 12),
                    ConsoleHelper.CenterText(country, 15)
                }, ConsoleColor.White, ConsoleColor.DarkGray);
                Console.Write(padding);
                ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);
            }
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public static string GetAddressType(SqliteConnection conn, long customerId)
        {
            var addressTypes = conn.Query<string>("SELECT address_type FROM addresses WHERE customer_id = @id", new { id = customerId }).ToList();
            return addressTypes.Any() ? string.Join(", ", addressTypes) : "None";
        }

        public static bool HasAddressType(SqliteConnection conn, long customerId, string addressType)
        {
            var count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM addresses WHERE customer_id = @id AND address_type = @type",
                new { id = customerId, type = addressType });
            return count > 0;
        }
    }
}
