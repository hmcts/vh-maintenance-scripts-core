using System;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceScriptsCore.Reference;

namespace MaintenanceScriptsCore
{
    public class GetUserGroups : Operations
    {
        public GetUserGroups(GraphApiService graphApiService) : base(graphApiService)
        {
        }

        public override async Task Run()
        {
            var username = "1.1@email.com";
            try
            {
                var user = await _graphApiService.GetUserByFilter($"userPrincipalName  eq '{username}'");
                //var user = await graphApiService.GetUserByFilter($"startswith(userPrincipalName,'{username}')").Dump();
                //var user = await graphApiService.GetUsersByFilter($"otherMails/any(c:c eq '{username}')").Dump();

                if(user == null)
                {
                    Console.WriteLine($"User not found: {username}");	
                    return;
                }
		
                Console.WriteLine($"{user.UserPrincipalName}");
                var result = (await _graphApiService.GetGroupsForUserAsync(user.Id))
                    .Select(x => new { x.Id, x.DisplayName, x.Description }).ToList();
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}