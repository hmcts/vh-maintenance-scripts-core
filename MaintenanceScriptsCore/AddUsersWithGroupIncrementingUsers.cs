using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceScriptsCore.Reference;

namespace MaintenanceScriptsCore
{
    public class AddUsersWithGroupIncrementingUsers : Operations
    {
        public AddUsersWithGroupIncrementingUsers(GraphApiService graphApiService) : base(graphApiService)
        {
        }

        public override async Task Run()
        {
            var usersToCreate = CreateIncrementingCreateUserRequests();
            var groupsToAdd = _graphApiService.RepresentativeGroups.Concat(_graphApiService.TestAccountsGroups);

            Console.WriteLine($"{string.Join(", ", groupsToAdd)}");

            foreach (var userToCreate in usersToCreate)
            {
                // Console.WriteLine(
                //     $"{userToCreate.UserPrincipalName} - {userToCreate.FirstName} - {userToCreate.LastName} - {userToCreate.RecoveryEmail}");
                // continue;
                //
                // if (await UserAlreadyExists(userToCreate)) break;

                var user = await _graphApiService.CreateUserAsync(userToCreate);

                foreach (var groupName in groupsToAdd)
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
                            throw new Exception($"{groupName}: Skipped group not found");
                        }

                        _graphApiService.groupCache.Add(groupName, group);
                    }

                    var success = await _graphApiService.AddUserToGroup(user, group);

                    if (!success) throw new Exception($"Failed to add {user.UserPrincipalName} to group: {groupName}");

                    await Task.Delay(60);
                }

                Console.WriteLine($"{user.UserPrincipalName}");
            }
        }

        public IEnumerable<CreateUserRequest> CreateIncrementingCreateUserRequests()
        {
            var totalPadLength = 4;

            foreach (var index in Enumerable.Range(1, 150))
            {
                int decimalLength = index.ToString("D").Length + (totalPadLength - index.ToString().Length);
                var cuurentIndexWithPadding = index.ToString("D" + decimalLength.ToString());

                foreach (var subIndex in Enumerable.Range(3, 5))
                {
                    var firstName = "TP";
                    var lastName = $"Representative{cuurentIndexWithPadding}_{subIndex}";
                    var displayName = $"{firstName}{lastName}";

                    yield return new CreateUserRequest
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        DisplayName = displayName,
                        UserPrincipalName = $"{displayName}@hearings.reform.hmcts.net",
                        RecoveryEmail = $"{displayName}@email.com",
                        MailNickname = $"{displayName}".ToLower(),
                        AccountEnabled = true,
                        ForceChangePasswordNextSignIn = false,
                        Password = "qwerty123!"
                    };
                }
            }
        }

        private async Task<bool> UserAlreadyExists(CreateUserRequest userToCreate, GraphApiService _graphApiService)
        {
            if (await _graphApiService.GetUserByFilter($"userPrincipalName  eq '{userToCreate.RecoveryEmail}'") != null)
            {
                Console.WriteLine($"{userToCreate.RecoveryEmail}: User already exists");

                return true;
            }

            return false;
        }
    }
}