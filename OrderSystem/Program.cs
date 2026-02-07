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
                name TEXT NOT NULL UNIQUE CHECK(length(name)<=15),
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
                description TEXT CHECK(length(description)<=25),
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
                    customers.id AS customerid,
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
                    JOIN order_rows ON orders.id = order_rows.order_id
                    LEFT JOIN products ON order_rows.product_id = products.id;
            ");

            string[] menu = {
            ConsoleHelper.CenterText("--- CUSTOMER ---", 25),
            ConsoleHelper.CenterText("CREATE CUSTOMER", 25),
            ConsoleHelper.CenterText("SHOW CUSTOMERS", 25),
            ConsoleHelper.CenterText("DELETE CUSTOMER", 25),
            ConsoleHelper.CenterText("ADD ADDRESS TO CUSTOMER", 25),
            "",
            ConsoleHelper.CenterText("--- PRODUCT ---", 25),
            ConsoleHelper.CenterText("CREATE PRODUCT", 25),
            ConsoleHelper.CenterText("SHOW PRODUCTS", 25),
            ConsoleHelper.CenterText("DELETE PRODUCT", 25),
            "",
            ConsoleHelper.CenterText("--- ORDER ---", 25),
            ConsoleHelper.CenterText("CREATE ORDER", 25),
            ConsoleHelper.CenterText("SHOW ORDERS", 25),
            ConsoleHelper.CenterText("DELETE ORDER", 25),
            "",
            ConsoleHelper.CenterText("--- ORDER ITEM ---", 25),
            ConsoleHelper.CenterText("CREATE ORDER ITEM", 25),
            ConsoleHelper.CenterText("SHOW ORDER ITEMS", 25),
            ConsoleHelper.CenterText("DELETE ORDER ITEM", 25),
            "",
            ConsoleHelper.CenterText("EXIT", 25)
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
                ConsoleHelper.TextColor(ConsoleHelper.CenterText("Use ↑ and ↓ arrow keys to navigate and Enter to select.\n", Console.WindowWidth - 1), ConsoleColor.White);
                Console.CursorVisible = false;
                for (int i = 0; i < menu.Length; i++)
                {
                    if (menu[i] == "")
                    {
                        Console.WriteLine();
                        continue;
                    }
                    
                    if (menu[i].Contains("---"))
                    {
                        ConsoleHelper.TextColor(ConsoleHelper.CenterText(menu[i], Console.WindowWidth - 1), ConsoleColor.Blue);
                        continue;
                    }
                    
                    if (i == position)
                    {
                        ConsoleHelper.TextColor(ConsoleHelper.CenterText($">>> {menu[i],-20} <<<", Console.WindowWidth - 1), ConsoleColor.Cyan);
                    }
                    else
                    {
                        ConsoleHelper.TextColor(ConsoleHelper.CenterText($"  {menu[i],-20}  ", Console.WindowWidth - 1), ConsoleColor.DarkGray);
                    }
                }
                var Key = Console.ReadKey(true).Key;
                if (Key == ConsoleKey.DownArrow)
                {
                    do
                    {
                        position++;
                        if (position == menu.Length) position = 0;
                    } while (menu[position] == "" || menu[position].Contains("---"));
                }
                else if (Key == ConsoleKey.UpArrow)
                {
                    do
                    {
                        position--;
                        if (position < 0) position = menu.Length - 1;
                    } while (menu[position] == "" || menu[position].Contains("---"));
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

                        case 7: Product.Add(database); break;
                        //case 8: ShowProducts(database); break;
                        //case 9: DeleteProduct(database); break;

                        case 12: Order.Add(database); break;
                        //case 13: ShowOrders(database); break;
                        case 14: Order.DeleteOrder(database); break;

                        case 17: OrderItem.Add(database); break;
                        case 18: OrderItem.ShowOrderItems(database); break;
                        //case 19: DeleteOrderItems(database); break;

                        case 21:
                            Console.WriteLine();
                            ConsoleHelper.TextColor(ConsoleHelper.CenterText("Thank you for choosing the Order System App!\n", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                            ConsoleHelper.TextColor(ConsoleHelper.CenterText("Press any key to exit...\n", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
                            Console.ReadKey();
                            return;
                    }
                }
            }
        }
    }
}
