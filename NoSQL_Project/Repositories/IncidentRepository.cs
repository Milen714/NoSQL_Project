using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;

namespace NoSQL_Project.Repositories
{
    public class IncidentRepository : IIncidentRepository
    {

		//a private field representing the mongoDB collection of incidents documents 
		//IMongoCollection<Incident> is the interface with acces to the crud operations on the collection of incidents
		private readonly IMongoCollection<Incident> _incidents;

		//get the collection "INCIDENTS" and type it as Incident objects
		public IncidentRepository(IMongoDatabase db)
		{
			_incidents = db.GetCollection<Incident>("INCIDENTS");
		}

		public async void CreateNewIncidentAsync(Incident newIncident)
		{
			_incidents.InsertOne(newIncident);
		}


		public IQueryable<Incident> GetAll()
        {
            try
            {
                IQueryable<Incident> incidents = _incidents.AsQueryable();
                return incidents;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception($"Could not retrieve incidents: {ex.Message}");
            }
        }
    }
}
