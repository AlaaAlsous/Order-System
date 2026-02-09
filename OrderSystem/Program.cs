using Dapper;
using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            using var database = new SqliteConnection("Data Source = order_system.sqlite");
            database.Open();

            database.Execute(@"
                PRAGMA foreign_keys = ON;
            ");

            database.Execute(@"
                CREATE TABLE IF NOT EXISTS customers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                email TEXT NOT NULL UNIQUE,
                phone TEXT
                );
            ");

            database.Execute(@"
                CREATE TABLE IF NOT EXISTS addresses (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                customer_id INTEGER NOT NULL,
                address_type TEXT NOT NULL CHECK(address_type IN('Delivery','Billing')),
                street TEXT NOT NULL,
                city TEXT NOT NULL,
                zip_code TEXT NOT NULL,
                country TEXT NOT NULL,
                FOREIGN KEY (customer_id) REFERENCES customers(id) ON DELETE CASCADE
                );
            ");

            database.Execute(@"
                CREATE TABLE IF NOT EXISTS products (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL UNIQUE CHECK(length(name)<=30),
                unit_price REAL NOT NULL,
                stock INTEGER NOT NULL
                );
            ");

            database.Execute(@"
                CREATE TABLE IF NOT EXISTS orders (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
	            customer_id INTEGER NOT NULL,
	            order_date INTEGER NOT NULL,
	            status TEXT NOT NULL CHECK(status IN('Created','Paid','Delivered')),
	            FOREIGN KEY (customer_id) REFERENCES customers(id)
                );
            ");

            database.Execute(@"
                CREATE TABLE IF NOT EXISTS order_rows (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
	            order_id INTEGER NOT NULL,
                product_id INTEGER,
                description TEXT CHECK(length(description)<=30),
	            quantity INTEGER NOT NULL,
                unit_price REAL NOT NULL,
	            FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
	            FOREIGN KEY (product_id) REFERENCES products(id),
                CHECK (product_id IS NOT NULL OR description IS NOT NULL)
                );
            ");

            database.Execute(@"
                CREATE VIEW IF NOT EXISTS order_overview AS
                SELECT 
                    orders.id AS orderid,
                    customers.Id AS customerid,
                    customers.name AS customername,
                    orders.order_date AS orderdate,
                    orders.status AS status,
                    order_rows.id AS orderitemid,
                    products.name AS productname,
                    order_rows.description AS description,
                    order_rows.quantity AS quantity,
                    order_rows.unit_price AS unitprice,
                    (order_rows.quantity * order_rows.unit_price) AS totalprice
                FROM orders 
                    JOIN customers ON orders.customer_id = customers.Id
                    LEFT JOIN order_rows ON orders.id = order_rows.order_id
                    LEFT JOIN products ON order_rows.product_id = products.id;
            ");
            string[] menu = {
            "┌───────── CUSTOMER ─────────┐",
            "│ ◆ CREATE CUSTOMER          │",
            "│ ◆ SHOW CUSTOMERS           │",
            "│ ◆ DELETE CUSTOMER          │",
            "│ ◆ ADD ADDRESS TO CUSTOMER  │",
            "│ ◆ SHOW ADDRESSES           │",
            "└────────────────────────────┘",
            "",
            "┌───────── PRODUCT ──────────┐",
            "│ ◆ CREATE PRODUCT           │",
            "│ ◆ SHOW PRODUCTS            │",
            "│ ◆ DELETE PRODUCT           │",
            "└────────────────────────────┘",
            "",
            "┌────────── ORDER ───────────┐",
            "│ ◆ CREATE ORDER             │",
            "│ ◆ SHOW ORDERS              │",
            "│ ◆ UPDATE ORDER STATUS      │",
            "│ ◆ DELETE ORDER             │",
            "└────────────────────────────┘",
            "",
            "┌──────── ORDER ITEM ────────┐",
            "│ ◆ CREATE ORDER ITEM        │",
            "│ ◆ SHOW ORDER ITEMS         │",
            "│ ◆ DELETE ORDER ITEM        │",
            "└────────────────────────────┘",
            "",
            "┌────────────────────────────┐",
            "│            EXIT            │",
            "└────────────────────────────┘"
             };
            const int menuWidth = 32;
            int position = 0;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n");
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("╔═══════════════════════════════════════════════╗", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                               ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                  ORDER SYSTEM                 ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                               ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("╚═══════════════════════════════════════════════╝", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                Console.WriteLine();
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("Use ↑ and ↓ arrow keys to navigate and Enter to select\n", Console.WindowWidth - 1), ConsoleColor.DarkGray);
                Console.CursorVisible = false;
                string padding = ConsoleHelper.GetTablePadding(menuWidth);

                for (int i = 0; i < menu.Length; i++)
                {
                    if (menu[i] == "")
                    {
                        Console.WriteLine();
                        continue;
                    }

                    if (menu[i].Contains("┌─") || menu[i].Contains("└─"))
                    {
                        ConsoleHelper.TextColor(padding + menu[i], ConsoleColor.White);
                        continue;
                    }

                    if (i == position)
                    {
                        Console.Write(padding);
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.Write(menu[i]);
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                    else
                    {
                        ConsoleHelper.TextColor(padding + menu[i], ConsoleColor.DarkCyan);
                    }
                }
                var Key = Console.ReadKey(true).Key;
                if (Key == ConsoleKey.DownArrow)
                {
                    do
                    {
                        position++;
                        if (position == menu.Length) position = 0;
                    } while (menu[position] == "" || menu[position].Contains("┌─") || menu[position].Contains("└─"));
                }
                else if (Key == ConsoleKey.UpArrow)
                {
                    do
                    {
                        position--;
                        if (position < 0) position = menu.Length - 1;
                    } while (menu[position] == "" || menu[position].Contains("┌─") || menu[position].Contains("└─"));
                }
                else if (Key == ConsoleKey.Enter)
                {
                    Console.Clear();
                    switch (position)
                    {
                        case 1: Customer.Add(database); break;
                        case 2: Customer.ShowCustomers(database); break;
                        case 3: Customer.DeleteCustomer(database); break;
                        case 4: Address.Add(database); break;
                        case 5: Address.ShowAddresses(database); break;

                        case 9: Product.Add(database); break;
                        case 10: Product.ShowProducts(database); break;
                        case 11: Product.DeleteProduct(database); break;

                        case 15: Order.Add(database); break;
                        case 16: Order.ShowOrders(database); break;
                        case 17: Order.UpdateOrderStatus(database); break;
                        case 18: Order.DeleteOrder(database); break;

                        case 22: OrderItem.Add(database); break;
                        case 23: OrderItem.ShowOrderItems(database); break;
                        case 24: OrderItem.DeleteOrderItem(database); break;

                        case 28:
                            Console.Clear();
                            Console.WriteLine();
                            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╔═══════════════════════════════════════════════╗", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                               ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║        Thank you for using Order System!      ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                            ConsoleHelper.TextColor(ConsoleHelper.CenterText("║                                               ║", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                            ConsoleHelper.TextColor(ConsoleHelper.CenterText("╚═══════════════════════════════════════════════╝", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                            Console.WriteLine();
                            ConsoleHelper.TextColor(ConsoleHelper.CenterText("Press any key to exit...\n", Console.WindowWidth - 1), ConsoleColor.DarkGray);
                            Console.ReadKey();
                            return;
                    }
                }
            }
        }
    }
}
