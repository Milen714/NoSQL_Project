using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Repositories.Interfaces;

namespace NoSQL_Project.Repositories
{
    public class IncidentSearchRepository : IIncidentSearchRepository
    {

        private readonly IMongoCollection<Incident> _incidents;

        public IncidentSearchRepository(IMongoDatabase db)
        {
            _incidents = db.GetCollection<Incident>("INCIDENTS");
        }

        public async Task<List<Incident>> SearchIncidentsAsync(
             string searchTerms,
             SearchOperator searchOperator,
             string branch = "",
             IncidentStatus? status = null,
             IncidentType? type = null)
        {
            try
            {
                // Return empty list if no search terms provided
                if (string.IsNullOrWhiteSpace(searchTerms))
                {
                    return new List<Incident>();
                }

                
                var pipeline = new List<BsonDocument>();

                List<string> terms;
                if (searchOperator == SearchOperator.Or && searchTerms.Contains("|"))
                {
                    // OR operator:
                    terms = searchTerms.Split('|')
                                      .Select(t => t.Trim())
                                      .Where(t => !string.IsNullOrWhiteSpace(t))
                                      .ToList();
                }
                else
                {
                    // AND operator:
                    terms = searchTerms.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(t => t.Trim())
                                      .ToList();
                }

              
                BsonDocument matchStage;

                if (searchOperator == SearchOperator.And)
                {

                    var andConditions = new BsonArray();

                    foreach (var term in terms)
                    {
                        var termCondition = new BsonDocument("$or", new BsonArray
                        {
                            new BsonDocument("subject", new BsonDocument("$regex", term).Add("$options", "i")),
                            new BsonDocument("description", new BsonDocument("$regex", term).Add("$options", "i"))
                        });
                        andConditions.Add(termCondition);
                    }

                    matchStage = new BsonDocument("$match", new BsonDocument("$and", andConditions));
                }
                else
                {
                    // OR Logic:
                    var orConditions = new BsonArray();

                    foreach (var term in terms)
                    {
                        orConditions.Add(new BsonDocument("subject", new BsonDocument("$regex", term).Add("$options", "i")));
                        orConditions.Add(new BsonDocument("description", new BsonDocument("$regex", term).Add("$options", "i")));
                    }

                    matchStage = new BsonDocument("$match", new BsonDocument("$or", orConditions));
                }

                pipeline.Add(matchStage);

                
                if (status.HasValue)
                {
                    
                    if (status.Value == IncidentStatus.closed || status.Value == IncidentStatus.closed_without_resolve)
                    {
                        var statusMatchStage = new BsonDocument("$match",
                            new BsonDocument("status", status.Value.ToString()));
                        pipeline.Add(statusMatchStage);
                    }
                    else
                    {
                       
                        var excludedStatuses = new[] { IncidentStatus.closed, IncidentStatus.closed_without_resolve };
                        var statusFilter = new BsonDocument("$match", new BsonDocument
                        {
                            { "status", status.Value.ToString() }
                        });
                        pipeline.Add(statusFilter);
                    }
                }
                else
                {
                    
                    var excludedStatuses = new[] { "closed", "closed_without_resolve" };
                    var excludeClosedStage = new BsonDocument("$match",
                        new BsonDocument("status", new BsonDocument("$nin", new BsonArray(excludedStatuses))));
                    pipeline.Add(excludeClosedStage);
                }

                
                if (type.HasValue)
                {
                    var typeMatchStage = new BsonDocument("$match",
                        new BsonDocument("incident_type", type.Value.ToString()));
                    pipeline.Add(typeMatchStage);
                }

               
                if (!string.IsNullOrWhiteSpace(branch))
                {
                    var branchMatchStage = new BsonDocument("$match",
                        new BsonDocument("location.branch",
                            new BsonDocument("$regex", branch).Add("$options", "i")));
                    pipeline.Add(branchMatchStage);
                }

               
                var sortStage = new BsonDocument("$sort", new BsonDocument("reported_at", -1));
                pipeline.Add(sortStage);

                
                var result = await _incidents.Aggregate<Incident>(pipeline).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not search incidents: {ex.Message}", ex);
            }
        }
    }
}
