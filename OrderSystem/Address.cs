using Dapper;
using Microsoft.Data.Sqlite;

namespace OrderSystem
{
    public class Address
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string Street { get; set; } = "";
        public string City { get; set; } = "";
        public string ZipCode { get; set; } = "";
        public string Country { get; set; } = "";

        public void Save(SqliteConnection conn)
        {
            Id = conn.QuerySingle<long>(@"
                INSERT INTO addresses (customer_id, street, city, zip_code, country)
                VALUES (@customerId, @street, @city, @zip_code, @country)
                RETURNING id;
            ", new { customerId = CustomerId, street = Street, city = City, zip_code = ZipCode, country = Country });
        }
    }
}
