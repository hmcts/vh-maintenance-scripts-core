using MaintenanceScriptsCore.Reference;

namespace MaintenanceScriptsCore
{
    public class AddUsersWithJudgeGroups : AddUsersBase
    {
        public AddUsersWithJudgeGroups(GraphApiService graphApiService, string filePath) : base(graphApiService, filePath)
        {
            _graphApiGroups = graphApiService.JudgeGroups;
        }
    }
}