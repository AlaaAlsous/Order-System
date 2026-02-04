using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            using var connection = new SqliteConnection("Data Source = order_system.sqlite");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                PRAGMA foreign_keys = ON;
            ";
            command.ExecuteNonQuery();

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS customers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                email TEXT NOT NULL UNIQUE,
                phone TEXT,
                address TEXT
                );
            ";
            command.ExecuteNonQuery();

            command.CommandText = @"
                Create TABLE IF NOT EXISTS products (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL UNIQUE CHECK(length(name)<=15),
                unit_price REAL NOT NULL,
                stock INTEGER NOT NULL
                );
            ";
            command.ExecuteNonQuery();

            command.CommandText = @"
                Create TABLE IF NOT EXISTS orders (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
	            customer_id INTEGER NOT NULL,
	            order_date INTEGER NOT NULL,
	            status TEXT NOT NULL CHECK(status IN('Created','Paid','Delivered')),
	            FOREIGN KEY (customer_id) REFERENCES customers(id)
                );
            ";
            command.ExecuteNonQuery();

            command.CommandText = @"
                Create TABLE IF NOT EXISTS order_rows (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
	            order_id INTEGER NOT NULL,
                product_id INTEGER,
                description TEXT CHECK(length(description)<=25),
	            quantity INTEGER NOT NULL,
                unit_price REAL NOT NULL,
	            FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
	            FOREIGN KEY (product_id) REFERENCES products(id),
                CHECK (product_id IS NOT NULL OR description IS NOT NULL)
                );
            ";
            command.ExecuteNonQuery();

            string[] menu = {
            ConsoleHelper.CenterText("Create Customer", 15),
            ConsoleHelper.CenterText("Create Product", 15),
            ConsoleHelper.CenterText("Create Order", 15),
            ConsoleHelper.CenterText("Add Order Item", 15),
            ConsoleHelper.CenterText("Show Orders", 15),
            ConsoleHelper.CenterText("Exit", 15)
             };
            int position = 0;

            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("ORDER SYSTEM", Console.WindowWidth - 1), ConsoleColor.Cyan);
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                Console.WriteLine();
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("Use arrow keys to navigate and Enter to select.\n", Console.WindowWidth - 1), ConsoleColor.Blue);
                Console.CursorVisible = false;
                for (int i = 0; i < menu.Length; i++)
                {
                    if (i == position)
                    {
                        ConsoleHelper.TextColor(ConsoleHelper.CenterText($">>> {menu[i],-15} <<<", Console.WindowWidth - 1), ConsoleColor.Cyan);
                    }
                    else
                    {
                        ConsoleHelper.TextColor(ConsoleHelper.CenterText($"  {menu[i],-15}  ", Console.WindowWidth - 1), ConsoleColor.DarkGray);
                    }
                }
                var Key = Console.ReadKey(true).Key;
                if (Key == ConsoleKey.DownArrow)
                {
                    position++;
                    if (position == menu.Length) position = 0;
                }
                else if (Key == ConsoleKey.UpArrow)
                {
                    position--;
                    if (position < 0) position = menu.Length - 1;
                }
                else if (Key == ConsoleKey.Enter)
                {
                    Console.Clear();
                    switch (position)
                    {
                        case 0: Customer.Add(connection); break;
                        case 1: Product.Add(connection); break;
                        case 2: Order.Add(connection); break;
                        case 3: OrderItem.Add(connection); break;
                        case 4: ShowOrders(connection); break;
                        case 5:
                            Console.WriteLine();
                            ConsoleHelper.TextColor(ConsoleHelper.CenterText("Thank you for choosing the Order System App!\n", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                            ConsoleHelper.TextColor(ConsoleHelper.CenterText("Press any key to exit...\n", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                            Console.ReadKey();
                            return;
                    }
                }
            }
        }
        public static void ShowOrders(SqliteConnection conn)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"
               SELECT 
	                orders.id AS 'Order ID',
	                customers.name AS 'Customer',
	                orders.order_date AS 'Order Date',
	                orders.status AS 'Status',
	                order_rows.id AS 'Order Item ID',
	                products.name AS 'Product Name',
	                order_rows.description AS 'Description',
	                order_rows.quantity AS 'Quantity',
	                order_rows.unit_price  AS 'Unit Price',
	                (order_rows.quantity*order_rows.unit_price) AS 'Total Price'
                FROM orders 
                    JOIN customers ON orders.customer_id = customers.Id
                    JOIN order_rows ON orders.id = order_rows.order_id
                    LEFT JOIN products ON order_rows.product_id = products.id
            ";
            using var reader = command.ExecuteReader();
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("ORDERS", Console.WindowWidth - 1), ConsoleColor.Cyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();
            string separator = new string('-', 152);
            ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);
            ConsoleHelper.WriteTableRow(new string[]
            {
                ConsoleHelper.CenterText("Order", 5),
                ConsoleHelper.CenterText("Customer", 20),
                ConsoleHelper.CenterText("Date", 10),
                ConsoleHelper.CenterText("Status", 9),
                ConsoleHelper.CenterText("Item ID", 7),
                ConsoleHelper.CenterText("Product", 15),
                ConsoleHelper.CenterText("Description", 25),
                ConsoleHelper.CenterText("Quantity", 8),
                ConsoleHelper.CenterText("Unit Price", 10),
                ConsoleHelper.CenterText("Total Price", 12)
            }, ConsoleColor.Cyan, ConsoleColor.DarkGray);
            ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);

            while (reader.Read())
            {
                int orderId = reader.GetInt32(0);
                string customerName = reader.GetString(1);
                long orderDateUnix = reader.GetInt64(2);
                DateTime orderDate = DateTimeOffset.FromUnixTimeSeconds(orderDateUnix).DateTime;
                string status = reader.GetString(3);
                int orderItemId = reader.GetInt32(4);
                string productName = reader.IsDBNull(5) ? "" : reader.GetString(5);
                string description = reader.IsDBNull(6) ? "" : reader.GetString(6);
                int quantity = reader.GetInt32(7);
                decimal unitPrice = reader.GetDecimal(8);
                decimal totalPrice = reader.GetDecimal(9);

                ConsoleHelper.WriteTableRow(new string[]
                {
                    ConsoleHelper.CenterText(orderId.ToString(), 5),
                    ConsoleHelper.CenterText(customerName, 20),
                    ConsoleHelper.CenterText(orderDate.ToString("yyyy-MM-dd"), 10),
                    ConsoleHelper.CenterText(status, 9),
                    ConsoleHelper.CenterText(orderItemId.ToString(), 7),
                    ConsoleHelper.CenterText(productName, 15),
                    ConsoleHelper.CenterText(description, 25),
                    ConsoleHelper.CenterText(quantity.ToString(), 8),
                    ConsoleHelper.CenterText(unitPrice.ToString("F2") + " kr", 10),
                    ConsoleHelper.CenterText(totalPrice.ToString("F2") + " kr", 12)
                }, ConsoleColor.White, ConsoleColor.DarkGray);
                ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);
            }
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
