using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace NoSQL_Project.Models.Enums
{
    
    public enum UserType
    {
        // Matches: "Reg_employee" | "Service_employee"
        [BsonRepresentation(BsonType.String)]
        Reg_employee,
        [BsonRepresentation(BsonType.String)]
        Service_employee,
        [BsonRepresentation(BsonType.String)]
        All_employee
    }
}
