using MongoDB.Driver;

namespace NoSQL_Project.Repositories
{
    public class ArchiveDatabase
    {
        public IMongoDatabase Database { get; }
        public ArchiveDatabase(IMongoDatabase db) => Database = db;
    }
}
