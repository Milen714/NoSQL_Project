using NoSQL_Project.ViewModels;

namespace NoSQL_Project.Services.Incidents
{
	public interface IIncidentService
	{
		void CreateNewIncidentAsync(NewIncidentViewModel model);
	}
}
