 using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSQL_Project.Models
{
    public class Location
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }


        [BsonElement("branch")]
        public string Branch { get; set; } = default!;

        [BsonElement("branch_code")]
        public string? BranchCode { get; set; }

        [BsonElement("address")]
        public string Address { get; set; } = default!;

        [BsonElement("post_code")]
        public string PostCode { get; set; } = default!;

        [BsonElement("region")]
        public string Region { get; set; } = default!;

        [BsonElement("active")]
        public bool? Active { get; set; } // optional in validator; set in app when creating
    }
}
