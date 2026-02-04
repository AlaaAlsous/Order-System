using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Created";
        public void Save(SqliteConnection conn)
        {
            using var command = conn.CreateCommand();
            long unixTime = ((DateTimeOffset)OrderDate).ToUnixTimeSeconds();
            command.CommandText = @"
            INSERT INTO orders (customer_id, order_date, status)
            VALUES (@customer_id, @order_date, @status)
            RETURNING Id;
            ";
            command.Parameters.AddWithValue("@customer_id", CustomerId);
            command.Parameters.AddWithValue("@order_date", unixTime);
            command.Parameters.AddWithValue("@status", Status);

            Id = Convert.ToInt32(command.ExecuteScalar());
        }
    }
}