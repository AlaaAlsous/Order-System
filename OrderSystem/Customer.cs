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
                if (!string.IsNullOrWhiteSpace(input) && input.Contains("@") && input.Contains("."))
                {
                    customer.Email = input; break;
                }
                ConsoleHelper.TextColor("⚠️ Email cannot be empty. You must provide a valid email.", ConsoleColor.Red);
            }
        }
    }
}

