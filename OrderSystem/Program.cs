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
        }
    }
}
