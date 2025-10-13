using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;

public class IncidentFilterBuilder
{
    private readonly IMongoCollection<Incident> _incidents;
    public IncidentFilterBuilder(IMongoDatabase db)
    {
        _incidents = db.GetCollection<Incident>("INCIDENTS");
    }
    public FilterDefinition<Incident> Build(string statusFilter, string typeFilter, string branch)
    {
        var filters = new List<FilterDefinition<Incident>>();

        // Status filter (enum to string)
        if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
        {
            if (Enum.TryParse<IncidentStatus>(statusFilter, true, out var parsedStatus))
            {
                filters.Add(Builders<Incident>.Filter.Eq(i => i.Status, parsedStatus.ToString()));
            }
        }

        // Type filter (enum to string)
        if (!string.IsNullOrWhiteSpace(typeFilter))
        {
            if (Enum.TryParse<IncidentType>(typeFilter, true, out var parsedType))
            {
                filters.Add(Builders<Incident>.Filter.Eq(i => i.IncidentType, parsedType.ToString()));
            }
        }

        // Branch filter
        if (!string.IsNullOrWhiteSpace(branch))
        {
            filters.Add(Builders<Incident>.Filter.Regex("location.branch", new BsonRegularExpression(branch, "i")));
        }

        return filters.Count > 0
            ? Builders<Incident>.Filter.And(filters)
            : Builders<Incident>.Filter.Empty;
    }

    public async Task<List<Incident>> GetAllIncidentsPerStatus(IncidentStatus status, string branch)
    {
        FilterDefinition<Incident> branchFilter = FilterDefinition<Incident>.Empty;
        try
        {
            if (!string.IsNullOrWhiteSpace(branch))
            {
                branchFilter = Builders<Incident>.Filter.Regex("location.branch", new BsonRegularExpression(branch, "i"));
            }
            var filter = Builders<Incident>.Filter.And(
                Builders<Incident>.Filter.Eq(i => i.Status, status),
            branchFilter
            );

            var result = await _incidents.Find(filter).SortBy(p => p.Priority).ToListAsync();
            return result;
        }
        catch (Exception ex)
        {

            throw new Exception($"Could not retrieve incidents: {ex.Message}");
        }
    }
    
    public async Task<List<Incident>> GetAllIncidentsByType(IncidentType type, string branch)
    {
        FilterDefinition<Incident> branchFilter = FilterDefinition<Incident>.Empty;

        try
        {
            var filter = Builders<Incident>.Filter.And(
                Builders<Incident>.Filter.Eq(i => i.IncidentType, type), branchFilter);
            if (!string.IsNullOrWhiteSpace(branch))
                branchFilter = Builders<Incident>.Filter.Regex("location.branch", new BsonRegularExpression(branch, "i"));


            var result = await _incidents.Find(filter).SortBy(p => p.Priority).ToListAsync();
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Could not retrieve incidents: {ex.Message}");
        }
    }

    public async Task<List<Incident>> GetIncidentsByStatusAndType(IncidentStatus status, IncidentType type, string branch)
    {
        FilterDefinition<Incident> branchFilter = FilterDefinition<Incident>.Empty;
        try
        {
            if (!string.IsNullOrWhiteSpace(branch))
                branchFilter = Builders<Incident>.Filter.Regex("location.branch", new BsonRegularExpression(branch, "i"));

            var filter = Builders<Incident>.Filter.And(
                Builders<Incident>.Filter.Eq(i => i.Status, status),
                Builders<Incident>.Filter.Eq(i => i.IncidentType, type), branchFilter);

            return await _incidents.Find(filter).SortBy(p => p.Priority).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Could not retrieve incidents: {ex.Message}");
        }
    }

}