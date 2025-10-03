using MongoDB.Driver;
using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Locations
{
	public class LocationRepository : ILocationRepository
	{
		private readonly IMongoCollection<Location> _location;

		public LocationRepository(IMongoDatabase db)
		{
			_location = db.GetCollection<Location>("LOCATIONS");
		}

		public async Task<Location> GetLocationByName(string locationName)
		{
			Location location = _location.Find(location => location.Branch == locationName).FirstOrDefault();
			return location;
		}
	}
}
