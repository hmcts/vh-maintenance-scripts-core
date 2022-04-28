using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaintenanceScriptsCore.Reference;

namespace MaintenanceScriptsCore
{
    public class AddUsersToGroups : Operations
    {
        public AddUsersToGroups(GraphApiService graphApiService) : base(graphApiService)
        {
        }

        public override async Task Run()
        {
            var users = new List<string>
            {
                "dummy.user1@hearings.reform.hmcts.net",
                "dummy.user.2@hearings.reform.hmcts.net"
            };

            foreach (var username in users)
            {
                try
                {
                    var user = await _graphApiService.GetUserByFilter($"userPrincipalName  eq '{username}'");

                    if (user == null)
                    {
                        Console.WriteLine($"{username}: Skipped user not found");
                        continue;
                    }


                    foreach (var groupName in _graphApiService.KinlyGroups)
                    {
                        Microsoft.Graph.Group group;

                        if (_graphApiService.groupCache.ContainsKey(groupName))
                        {
                            group = _graphApiService.groupCache[groupName];
                        }
                        else
                        {
                            group = await _graphApiService.GetGroupByName(groupName);

                            if (group == null)
                            {
                                throw new Exception($"{groupName}: Group not found");
                            }

                            _graphApiService.groupCache.Add(groupName, group);
                        }

                        var success = await _graphApiService.AddUserToGroup(user, group);

                        if (!success)
                        {
                            Console.WriteLine($"{user.UserPrincipalName} failed to add to group: {groupName}");
                        }

                        await Task.Delay(100);
                    }

                    Console.WriteLine($"{user.UserPrincipalName} done!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{username}, Error: {ex.Message}");
                }
            }
        }
    }
}