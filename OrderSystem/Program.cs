using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            using var connection = new SqliteConnection("Data Source = order_system.sqlite");
            connection.Open();

            var command = connection.CreateCommand();
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
                name TEXT NOT NULL UNIQUE,
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
            "; command.ExecuteNonQuery();

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
        }
    }
}
