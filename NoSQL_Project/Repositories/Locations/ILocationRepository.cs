using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Locations
{
	public interface ILocationRepository
	{
		public Task<Location> GetLocationByName(string locationName);
	}
}
