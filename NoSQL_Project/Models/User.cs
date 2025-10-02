using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NoSQL_Project.Models.Enums;

namespace NoSQL_Project.Models
{
    //public class User
    //{
    //    // This will be the primary key in MongoDB.
    //    // [BsonId] tells MongoDB this field is the "_id".
    //    // [BsonRepresentation(BsonType.ObjectId)] lets us use string in C#,
    //    // while MongoDB still stores it as a real ObjectId internally.
    //    [BsonId]
    //    [BsonRepresentation(BsonType.ObjectId)]
    //    public string? Id { get; set; }

    //    // A simple string field for the user's name.
    //    // Default = "" so it's never null when creating a new User.
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; } 
    //    public UserRoles Role { get; set; } 
    //    public string PhoneNumber { get; set; } = "";

    //    // A simple string field for the user's email.
    //    // Later we could add validation (e.g. DataAnnotations).
    //    public string Email { get; set; } = "";

    //    public string Password { get; set; } = "";
    //    public FranchiseLocation Location { get; set; }

    //    public User()
    //    {

    //    }


    //}
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

        [BsonElement("password_hash")]
        [BsonIgnoreIfNull]
        public string? PasswordHash { get; set; }

        [BsonElement("location")]
        public UserLocationRef Location { get; set; } = default!;

        [BsonElement("resset_token")]
        public string RessetToken { get; set; } = "";

        [BsonElement("resset_token_expiry")]
        public DateTime? RessetTokenExpiry { get; set; }
    }
}