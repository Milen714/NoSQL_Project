using MongoDB.Bson;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Locations;

namespace NoSQL_Project.Services.Locations
{
	public class LocationService : ILocationService
	{
		private readonly ILocationRepository _locationRepository;

		public LocationService(ILocationRepository locationRepository)
		{
			_locationRepository = locationRepository;
		}

		public async Task<Location> GetLocationByName(string locationBranchName)
		{
			Location location = await _locationRepository.GetLocationByName(locationBranchName);			
			return location;
		}

		public async Task<LocationSnapshot> GetLocationSnapshotAsync(string locationBranchName)
		{
			Location fullLocation = await GetLocationByName(locationBranchName);

			LocationSnapshot locationSnapshot = new LocationSnapshot
			{
				LocationId = ObjectId.Parse(fullLocation.Id),
				Branch = fullLocation.Branch,
				Region = fullLocation.Region,
				PostCode = fullLocation.PostCode,
				BranchCode = fullLocation.BranchCode,
			};

			return locationSnapshot;
		}		
	}
}
