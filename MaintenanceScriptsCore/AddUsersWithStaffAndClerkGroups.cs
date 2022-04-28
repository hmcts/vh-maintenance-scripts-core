using MaintenanceScriptsCore.Reference;

namespace MaintenanceScriptsCore
{
    public class AddUsersWithStaffAndClerkGroups : AddUsersBase
    {
        public AddUsersWithStaffAndClerkGroups(GraphApiService graphApiService, string filePath) : base(graphApiService, filePath)
        {
            _graphApiGroups = graphApiService.StaffAndClerkGroups;
        }

    }
}