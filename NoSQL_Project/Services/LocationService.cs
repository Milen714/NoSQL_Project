using MongoDB.Bson;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;
        public LocationService(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }
        public Task<List<Location>> GetAllLocations()
        {
            return _locationRepository.GetAllLocations();
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

		public async Task<Location> GetLocationById(string id)
        {
            return await _locationRepository.GetLocationById(id);
        }


    }
}
