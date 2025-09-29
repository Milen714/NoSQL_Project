using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSQL_Project.Models.Enums
{
    public enum IncidentType
    {
        
        [BsonRepresentation(BsonType.String)]
        hardware,
        [BsonRepresentation(BsonType.String)]
        software,
        [BsonRepresentation(BsonType.String)]
        network,
        [BsonRepresentation(BsonType.String)]
        security,
        [BsonRepresentation(BsonType.String)]
        access,
        [BsonRepresentation(BsonType.String)]
        other
    }
}
