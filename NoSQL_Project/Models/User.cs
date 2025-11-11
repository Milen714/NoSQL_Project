using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NoSQL_Project.Models.Enums;

namespace NoSQL_Project.Models
{
    
    public class User
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("first_name")]
        public string FirstName { get; set; } = default!;

        [BsonElement("last_name")]
        public string LastName { get; set; } = default!;

        [BsonElement("user_type")]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public UserType UserType { get; set; }

        [BsonElement("email_address")]
        public string EmailAddress { get; set; } = default!;

        [BsonElement("phone_number")]
        public string PhoneNumber { get; set; } = default!;
        [BsonElement("active")]
        public bool Active { get; set; }

        [BsonElement("password_hash")]
        [BsonIgnoreIfNull]
        public string? PasswordHash { get; set; }

        [BsonElement("location")]
        public UserLocationRef Location { get; set; } = default!;

        [BsonElement("resset_token")]
        public string? RessetToken { get; set; }

        [BsonElement("resset_token_expiry")]
        public DateTime? RessetTokenExpiry { get; set; }
    }
}