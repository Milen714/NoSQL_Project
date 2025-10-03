using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.ConstrainedExecution;
using NoSQL_Project.Repositories.Incidents;

namespace NoSQL_Project.Repositories.Incidents
{
	public class IncidentRepository : IIncidentRepository
	{
		//a private field representing the mongoDB collection of incidents documents 
		//IMongoCollection<Incident> is the interface with acces to the crud operations on the collection of incidents
		private readonly IMongoCollection<Incident> _incident;

		//get the collection "INCIDENTS" and type it as Incident objects
		public IncidentRepository(IMongoDatabase db)
		{
			_incident = db.GetCollection<Incident>("INCIDENTS");
		}

		public async void CreateNewIncidentAsync(Incident newIncident)
		{
			_incident.InsertOne(newIncident);
		}
	}
}



