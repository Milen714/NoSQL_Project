using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSQL_Project.Models
{
    public class ReporterSnapshot
    {
        [BsonElement("userId")]
        public ObjectId UserId { get; set; }

        [BsonElement("first_name")]
        public string FirstName { get; set; } = default!;

        [BsonElement("last_name")]
        [BsonIgnoreIfNull]
        public string? LastName { get; set; }

        [BsonElement("email_address")]
        [BsonIgnoreIfNull]
        public string? EmailAddress { get; set; }

        public void MapReporter(User user)
        {
            UserId = ObjectId.Parse(user.Id!);
            FirstName = user.FirstName;
            LastName = user.LastName;
            EmailAddress = user.EmailAddress;
        }
    }
}
