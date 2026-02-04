using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double UnitPrice { get; set; }
        public int Stock { get; set; }
        public void Save(SqliteConnection conn)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"
            INSERT INTO products (name, unit_price, stock)
            VALUES (@name, @unit_price, @stock)
            RETURNING Id;
            ";
            command.Parameters.AddWithValue("@name", Name);
            command.Parameters.AddWithValue("@unit_price", UnitPrice);
            command.Parameters.AddWithValue("@stock", Stock);

            Id = Convert.ToInt32(command.ExecuteScalar());
        }
        public static void Add(SqliteConnection conn)
        {
            Console.WriteLine("--- Create Product ---");
            Product product = new Product();

            Console.Write("Name: "); product.Name = Console.ReadLine() ?? "";
            Console.Write("Unit Price: "); product.UnitPrice = double.Parse(Console.ReadLine() ?? "0");
            Console.Write("Stock: "); product.Stock = int.Parse(Console.ReadLine() ?? "0");

            product.Save(conn);

            Console.WriteLine($"Product '{product.Name}' created successfully with ID: {product.Id}");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}