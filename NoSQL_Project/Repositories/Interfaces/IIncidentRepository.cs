using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IIncidentRepository
    {
        Task<List<Incident>> GetAll();
    }
}
