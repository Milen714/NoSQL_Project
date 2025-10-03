using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IIncidentRepository
    {
        IQueryable<Incident> GetAll();

		Task CreateNewIncidentAsync(Incident newIncident);
	}
}
