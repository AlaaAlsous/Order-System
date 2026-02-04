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
    }
}