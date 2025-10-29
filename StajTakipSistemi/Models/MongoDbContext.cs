using MongoDB.Driver;

namespace StajTakipSistemi.Models
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            _database = client.GetDatabase("StajTakip");
        }
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Staj> Stajlar => _database.GetCollection<Staj>("Stajlar");
    }
}