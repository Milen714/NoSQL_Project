using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoSQL_Project.Models
{
    public class UserForTransferDto
    {
        public ObjectId UserId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int TotalIncidents { get; set; }

    }
}