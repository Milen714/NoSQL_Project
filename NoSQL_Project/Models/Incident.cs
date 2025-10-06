using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using NoSQL_Project.Models.Enums;

namespace NoSQL_Project.Models
{
    public class Incident
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("reported_at")]
        [BsonDateTimeOptions(Kind = System.DateTimeKind.Utc)]
        public System.DateTime ReportedAt { get; set; }

        [BsonElement("subject")]
        public string Subject { get; set; } = default!;

        [BsonElement("incident_type")]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public IncidentType IncidentType { get; set; }

        [BsonElement("reported_by")]
        public ReporterSnapshot ReportedBy { get; set; } = default!;

        [BsonElement("priority")]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Priority Priority { get; set; }

        [BsonElement("deadline")]
        [BsonDateTimeOptions(Kind = System.DateTimeKind.Utc)]
        public System.DateTime Deadline { get; set; }

        [BsonElement("description")]
        public string Description { get; set; } = default!;

        [BsonElement("status")]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public IncidentStatus Status { get; set; }

        [BsonElement("location")]
        public LocationSnapshot Location { get; set; } = default!;

        [BsonElement("assigned_to")]
        [BsonIgnoreIfNull]
        public AssigneeSnapshot? AssignedTo { get; set; }

        [BsonElement("tags")]
        [BsonIgnoreIfNull]
        public List<string>? Tags { get; set; }

       
    }
}
