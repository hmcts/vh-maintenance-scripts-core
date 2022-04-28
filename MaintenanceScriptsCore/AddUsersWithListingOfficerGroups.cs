using MaintenanceScriptsCore.Reference;

namespace MaintenanceScriptsCore
{
    public class AddUsersWithListingOfficerGroups : AddUsersBase
    {
        public AddUsersWithListingOfficerGroups(GraphApiService graphApiService, string filePath) : base(graphApiService, filePath)
        {
            _graphApiGroups = graphApiService.ListingOfficerGroups;
        }

    }
}