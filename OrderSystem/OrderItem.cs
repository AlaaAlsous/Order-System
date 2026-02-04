using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string Description { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public void Save(SqliteConnection conn)
        {
            using var command = conn.CreateCommand();
            command.CommandText = @"
            INSERT INTO order_rows (order_id, product_id, description, quantity, unit_price)
            VALUES (@order_id, @product_id, @description, @quantity, @unit_price)
            RETURNING Id;
            ";
            command.Parameters.AddWithValue("@order_id", OrderId);
            command.Parameters.AddWithValue("@product_id", ProductId);
            command.Parameters.AddWithValue("@description", Description);
            command.Parameters.AddWithValue("@quantity", Quantity);
            command.Parameters.AddWithValue("@unit_price", Price);

            Id = Convert.ToInt32(command.ExecuteScalar());
        }

        public static void Add(SqliteConnection conn)
        {
            Console.WriteLine("--- Add Order Item ---");
            OrderItem orderItem = new OrderItem();

            Console.Write("Order ID: "); orderItem.OrderId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Product ID: "); orderItem.ProductId = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Description: "); orderItem.Description = Console.ReadLine() ?? "";
            Console.Write("Quantity: "); orderItem.Quantity = int.Parse(Console.ReadLine() ?? "0");
            Console.Write("Price: "); orderItem.Price = decimal.Parse(Console.ReadLine() ?? "0");

            orderItem.Save(conn);

            Console.WriteLine($"Order item '{orderItem.Id}' added successfully with ID: {orderItem.Id}.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}