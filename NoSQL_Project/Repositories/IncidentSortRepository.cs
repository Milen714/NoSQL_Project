using MongoDB.Bson;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;


namespace NoSQL_Project.Repositories
{
    public class IncidentSortRepository : IIncidentSortRepository
    {
        private readonly IMongoCollection<Incident> _incidents;
        public IncidentSortRepository(IMongoDatabase db)
        {
            _incidents = db.GetCollection<Incident>("INCIDENTS");
        }

        public async Task<List<Incident>> GetIncidentsSortedByPriorityAsync(string branch = "", bool sortPriorityAscending = false)
        {
            try
            {
                var pipeline = new List<BsonDocument>();

                if (!string.IsNullOrWhiteSpace(branch))
                {
                    var branchMatchStage = new BsonDocument("$match",
                        new BsonDocument("location.branch",
                            new BsonDocument("$regex", branch).Add("$options", "i")));
                    pipeline.Add(branchMatchStage);
                }

                var addFieldsStage = new BsonDocument("$addFields", new BsonDocument
                {
                    { "priority_order", new BsonDocument("$switch", new BsonDocument
                        {
                            { "branches", new BsonArray
                                {
                                    new BsonDocument
                                    {
                                        { "case", new BsonDocument("$eq", new BsonArray { "$priority", "critical" }) },
                                        { "then", 0 }
                                    },
                                    new BsonDocument
                                    {
                                        { "case", new BsonDocument("$eq", new BsonArray { "$priority", "high" }) },
                                        { "then", 1 }
                                    },
                                    new BsonDocument
                                    {
                                        { "case", new BsonDocument("$eq", new BsonArray { "$priority", "medium" }) },
                                        { "then", 2 }
                                    },
                                    new BsonDocument
                                    {
                                        { "case", new BsonDocument("$eq", new BsonArray { "$priority", "low" }) },
                                        { "then", 3 }
                                    }
                                }
                            },
                        })
                    }
                });
                pipeline.Add(addFieldsStage);

                var sortDirection = sortPriorityAscending ? -1 : 1;
                var sortStage = new BsonDocument("$sort", new BsonDocument
                {
                    { "priority_order", sortDirection },      // Priority sort
                    { "reported_at", -1 }                     // Newest first within same priority
                });
                pipeline.Add(sortStage);

                var projectStage = new BsonDocument("$project", new BsonDocument
                {
                    { "priority_order", 0 }
                });
                pipeline.Add(projectStage);

                var result = await _incidents.Aggregate<Incident>(pipeline).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not retrieve incidents sorted by priority: {ex.Message}", ex);
            }
        }
    }
}