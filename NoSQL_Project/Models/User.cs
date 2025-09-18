using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSQL_Project.Models
{
    public class User
    {
        // This will be the primary key in MongoDB.
        // [BsonId] tells MongoDB this field is the "_id".
        // [BsonRepresentation(BsonType.ObjectId)] lets us use string in C#,
        // while MongoDB still stores it as a real ObjectId internally.
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // A simple string field for the user's name.
        // Default = "" so it's never null when creating a new User.
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public UserRoles Role { get; set; } = UserRoles.NormalUser;
        public string PhoneNumber { get; set; } = "";

        // A simple string field for the user's email.
        // Later we could add validation (e.g. DataAnnotations).
        public string Email { get; set; } = "";

        public string Password { get; set; } = "";


    }
}