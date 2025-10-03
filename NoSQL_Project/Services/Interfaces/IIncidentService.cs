using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IIncidentService
    {
        List<Incident> GetAll();
    }
}
