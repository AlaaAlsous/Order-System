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
                        case 2: ShowCustomers(database); break;
                        //case 3: DeleteCustomer(database); break;
                        case 4: Address.Add(database); break;

                        case 7: Product.Add(database); break;
                        //case 8: ShowProducts(database); break;
                        //case 9: DeleteProduct(database); break;

                        case 12: Order.Add(database); break;
                        //case 13: ShowOrders(database); break;
                        //case 14: DeleteOrder(database); break;

                        case 17: OrderItem.Add(database); break;
                        case 18: ShowOrderItems(database); break;
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
        public static void ShowOrderItems(SqliteConnection conn)
        {
            var orderItems = conn.Query("SELECT * FROM order_overview");

            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("ORDER OVERVIEW", Console.WindowWidth - 1), ConsoleColor.Cyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();
            string separator = new string('-', 152);
            int tableWidth = separator.Length;
            string padding = ConsoleHelper.GetTablePadding(tableWidth);

            Console.Write(padding);
            ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);
            Console.Write(padding);
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
            Console.Write(padding);
            ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);

            foreach (var item in orderItems)
            {
                long orderId = item.orderid;
                string customerName = item.customername;
                long orderDateUnix = item.orderdate;
                DateTime orderDate = DateTimeOffset.FromUnixTimeSeconds(orderDateUnix).DateTime;
                string status = item.status;
                long orderItemId = item.orderitemid;
                string productName = item.productname ?? "";
                string description = item.description ?? "";
                long quantity = item.quantity;
                decimal unitPrice = Convert.ToDecimal(item.unitprice);
                decimal totalPrice = Convert.ToDecimal(item.totalprice);

                Console.Write(padding);
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
                Console.Write(padding);
                ConsoleHelper.TextColor(separator, ConsoleColor.DarkGray);
            }
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public static void ShowCustomers(SqliteConnection conn)
        {
            var customers = conn.Query("SELECT * FROM customers");
            Console.WriteLine();
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("CUSTOMERS", Console.WindowWidth - 1), ConsoleColor.Cyan);
            ConsoleHelper.TextColor(ConsoleHelper.CenterText("═══════════════════════════════════════", Console.WindowWidth - 1), ConsoleColor.DarkCyan);
            Console.WriteLine();
            string separator = new string('-', 88);
            int tableWidth = separator.Length + 4;
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
    }
}
