using System;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceScriptsCore.Reference;

namespace MaintenanceScriptsCore
{
    public class GetAllUsersInGroup : Operations
    {
        public GetAllUsersInGroup(GraphApiService graphApiService) : base(graphApiService)
        {
        }

        public override async Task Run()
        {
            const string groupName = "GRC - EJ";
            try
            {
                var group = await _graphApiService.GetGroupByName(groupName);

                if (group == null)
                {
                    Console.WriteLine($"{groupName}: Skipped group not found");
                    return;
                }
		
                await foreach (var users in _graphApiService.GetUsersByGroupAsync(group))
                {
                    var userList  = users.Select(x => new UserResponse
                        {
                            FirstName = x.GivenName,
                            LastName = x.Surname,
                            DisplayName = x.DisplayName,
                            Email = x.UserPrincipalName
                        })
                        .ToList()
                        //.Where(u => !string.IsNullOrWhiteSpace(u.FirstName) && u.FirstName.StartsWith("Muk"))
                        .OrderBy(x => x.Email);
                    Console.WriteLine($"Users for {group}:");
                    foreach (var user in userList)
                    {
                        Console.WriteLine(user.Email);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
    }
}