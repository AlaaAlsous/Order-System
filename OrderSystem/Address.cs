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

        public void Save(SqliteConnection conn)
        {
            Id = conn.QuerySingle<long>(@"
                INSERT INTO addresses (customer_id, address_type, street, city, zip_code, country)
                VALUES (@customerId, @addressType, @street, @city, @zipCode, @country)
                RETURNING id;
            ", new { customerId = CustomerId, addressType = AddressType, street = Street, city = City, zipCode = ZipCode, country = Country });
        }

        public static void Add(SqliteConnection conn)
        {
            Console.Clear();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("ADD ADDRESS TO CUSTOMER", Console.WindowWidth - 1), ConsoleColor.Cyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();

            Address address = new Address();

            while (true)
            {
                Console.Write("Customer ID: ");
                var input = ConsoleHelper.ReadLineWithEscape();
                if (input == null) return;
                input = input.Trim();
                if (!long.TryParse(input, out long customerId))
                {
                    ConsoleHelper.TextColor("⚠️ Invalid input. Please enter a valid customer ID.\n", ConsoleColor.Red);
                    continue;
                }
                if (!Customer.CustomerExists(conn, customerId))
                {
                    ConsoleHelper.TextColor($"⚠️ Customer with ID (( {customerId} )) does not exist.\n", ConsoleColor.Red);
                    continue;
                }
                address.CustomerId = customerId;
                break;
            }

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

            address.Save(conn);

            Console.WriteLine();
            ConsoleHelper.TextColor($"✅ {address.AddressType} address (( {address.Id} )) added successfully for Customer ID {address.CustomerId}\n", ConsoleColor.DarkGreen);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
