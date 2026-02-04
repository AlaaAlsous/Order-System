using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double UnitPrice { get; set; }
        public int Stock { get; set; }
    }
}