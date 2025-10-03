using NoSQL_Project.Models;
using NoSQL_Project.ViewModels;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IIncidentService
    {
        IQueryable<Incident> GetAll();

		void CreateNewIncidentAsync(NewIncidentViewModel model);
	}
}
