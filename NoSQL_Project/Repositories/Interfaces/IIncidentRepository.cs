using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IIncidentRepository
    {
        IQueryable<Incident> GetAll();

		void CreateNewIncidentAsync(Incident newIncident);
	}
}
