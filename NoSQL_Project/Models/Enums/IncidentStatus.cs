using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSQL_Project.Models.Enums
{
    public enum IncidentStatus
    {
        // Matches: "open" | "resolved" | "closed_without_resolve"
        [BsonRepresentation(BsonType.String)]
        open,
        [BsonRepresentation(BsonType.String)]
        resolved,
        [BsonRepresentation(BsonType.String)]
        closed_without_resolve
    }
}
