using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSQL_Project.Models
{
    [BsonIgnoreExtraElements]
    public class AssigneeSnapshot
    {
        [BsonElement("userId")]
        public ObjectId UserId { get; set; }

        [BsonElement("first_name")]
        [BsonIgnoreIfNull]
        public string? FirstName { get; set; }

        [BsonElement("last_name")]
        [BsonIgnoreIfNull]
        public string? LastName { get; set; }

        [BsonElement("is_active")]
        public bool IsActive { get; set; }

        public void MapAssignee(User user)
        {
            UserId = ObjectId.Parse(user.Id!);
            FirstName = user.FirstName;
            LastName = user.LastName;
        }
    }
}
