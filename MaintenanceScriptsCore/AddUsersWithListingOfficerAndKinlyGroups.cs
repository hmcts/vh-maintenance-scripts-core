using MaintenanceScriptsCore.Reference;

namespace MaintenanceScriptsCore
{
    public class AddUsersWithListingOfficerAndKinlyGroups : AddUsersBase
    {
        public AddUsersWithListingOfficerAndKinlyGroups(GraphApiService graphApiService, string filePath) : base(graphApiService, filePath)
        {
            _graphApiGroups = graphApiService.ListingOfficerGroupsWithKinlyGroups;
        }

    }
}