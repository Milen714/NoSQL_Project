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

        public async Task<Location> GetLocationById(string id)
        {
            return await _locationRepository.GetLocationById(id);
        }
        public async Task<Location> GetLocationByName(string locationBranchName)
        {
            Location location = await _locationRepository.GetLocationByName(locationBranchName);
            return location;
        }

        public async Task<LocationSnapshot> GetLocationSnapshotAsync(string locationBranchName)
        {
            string normalizedBranchName = System.Text.RegularExpressions.Regex.Replace(locationBranchName, "(\\B[A-Z])", " $1");


            Location fullLocation = await GetLocationByName(normalizedBranchName);

            LocationSnapshot locationSnapshot = new LocationSnapshot();
            locationSnapshot.MapLocationSnapshot(fullLocation);

            return locationSnapshot;
        }
    }
}
