using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSQL_Project.Models
{
    public class LocationSnapshot
    {
        [BsonElement("locationId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId LocationId { get; set; }

        [BsonElement("branch")]
        public string Branch { get; set; } = default!;

        [BsonElement("region")]
        public string Region { get; set; } = default!;

        [BsonElement("post_code")]
        [BsonIgnoreIfNull]
        public string? PostCode { get; set; }

        [BsonElement("branch_code")]
        [BsonIgnoreIfNull]
        public string? BranchCode { get; set; }


        public void MapLocationSnapshot(Location location)
        {
            LocationId = ObjectId.Parse(location.Id);
            Branch = location.Branch;
            Region = location.Region;
            PostCode = location.PostCode;
            BranchCode = location.BranchCode;

        }
    }
}
