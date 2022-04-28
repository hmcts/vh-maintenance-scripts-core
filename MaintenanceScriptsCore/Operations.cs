using System.Collections.Generic;
using System.Threading.Tasks;
using MaintenanceScriptsCore.Reference;

namespace MaintenanceScriptsCore
{
    public abstract class Operations
    {
        protected readonly GraphApiService _graphApiService;
        protected List<string> _graphApiGroups;
        protected List<CreateUserRequest> _users;

        protected Operations(GraphApiService graphApiService)
        {
            _graphApiService = graphApiService;
        }

        public abstract Task Run();
    }
}