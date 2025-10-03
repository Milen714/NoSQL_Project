using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Incidents
{
	public interface IIncidentRepository
	{
		void CreateNewIncidentAsync(Incident newIncident);
	}
}
