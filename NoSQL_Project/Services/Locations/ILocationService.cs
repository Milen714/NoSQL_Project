using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Locations
{
	public interface ILocationService
	{
		public Task<LocationSnapshot> GetLocationSnapshotAsync(string locationBranchName);
		public Task<Location> GetLocationByName(string locationBranchName);


	}
}
