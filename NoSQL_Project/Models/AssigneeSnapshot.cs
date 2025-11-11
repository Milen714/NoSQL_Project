using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Net.Mail;

namespace NoSQL_Project.Models
{
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

		[BsonElement("email_address")]
		[BsonIgnoreIfNull]
		public string? EmailAddress { get; set; }

		[BsonElement("message")]
		[BsonIgnoreIfNull]
		public string? Message { get; set; }

		[BsonElement("timestamp")]
		public DateTime Timestamp { get; set; }

		public void MapAssignee(User user, string? message = null)
		{
			UserId = ObjectId.Parse(user.Id!);
			FirstName = user.FirstName;
			LastName = user.LastName;
            EmailAddress = user.EmailAddress;
			Message = message;
			Timestamp = DateTime.UtcNow; 

		}
	}
}
