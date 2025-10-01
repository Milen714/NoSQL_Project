using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSQL_Project.Models.Enums
{
    public enum Priority
    {
        // Matches: "low" | "medium" | "high" | "critical"
        [BsonRepresentation(BsonType.String)]
        low,
        [BsonRepresentation(BsonType.String)]
        medium,
        [BsonRepresentation(BsonType.String)]
        high,
        [BsonRepresentation(BsonType.String)]
        critical
    }
}
