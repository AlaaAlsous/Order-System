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
    }
}

