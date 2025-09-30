using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSQL_Project.Models
{
    public class UserLocationRef
    {
        [BsonElement("locationId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId LocationId { get; set; }

        [BsonElement("branch")]
        [BsonIgnoreIfNull]
        public string? Branch { get; set; }

        [BsonElement("region")]
        [BsonIgnoreIfNull]
        public string? Region { get; set; }

        [BsonElement("branch_code")]
        [BsonIgnoreIfNull]
        public string? BranchCode { get; set; }

        public UserLocationRef()
        {
        }
        public void MapLocation(Location location)
        {
            LocationId = location.Id;
            Branch = location.Branch;
            Region = location.Region;
            BranchCode = location.BranchCode;
        }
        
    }
}
