using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using MaintenanceScriptsCore.Reference;
using Microsoft.Graph;

namespace MaintenanceScriptsCore
{
    public abstract class AddUsersBase : Operations
    {
        protected string _filePath;

        protected AddUsersBase(GraphApiService graphApiService, string filePath) : base(graphApiService)
        {
            _filePath = filePath;
        }

        protected IEnumerable<CsvModelBase> Records
        {
            get
            {
                using var reader = new StreamReader(@_filePath);
                using var csv = new CsvReader(reader);
                csv.Configuration.HasHeaderRecord = false;
                csv.Configuration.MissingFieldFound = null;
                return csv.GetRecords<CsvModelBase>().ToList();
            }
        }


        public override async Task Run()
        {
            _users = Records.Select(record => CreateRequest(record.FirstName, record.LastName, record.Email)).ToList();

            foreach (var userToCreate in _users)
            {
                if (await _graphApiService.GetUserByFilter($"userPrincipalName  eq '{userToCreate.RecoveryEmail}'") !=
                    null)
                {
                    Console.WriteLine($"{userToCreate.RecoveryEmail}: Skipped user was found");
                    break;
                }

                User userFromCreateUserResponse;
                try
                {
                    userFromCreateUserResponse = await _graphApiService.CreateUserAsync(userToCreate);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unable to create user {userToCreate.FirstName} {userToCreate.LastName}");
                    Console.WriteLine("Error log:");
                    Console.WriteLine(e);
                    continue;
                }

                foreach (var groupName in _graphApiGroups)
                {
                    Group group;

                    if (_graphApiService.groupCache.ContainsKey(groupName))
                    {
                        group = _graphApiService.groupCache[groupName];
                    }
                    else
                    {
                        group = await _graphApiService.GetGroupByName(groupName);

                        if (group == null)
                        {
                            Console.WriteLine($"{groupName}: Skipped group not found");
                        }

                        _graphApiService.groupCache.Add(groupName, group);
                    }

                    var success = await _graphApiService.AddUserToGroup(userFromCreateUserResponse, group);

                    if (!success)
                    {
                        Console.WriteLine($"Failed to add {userFromCreateUserResponse.UserPrincipalName} to group: {groupName}");
                    }

                    await Task.Delay(100);
                }

                Console.WriteLine($"{userFromCreateUserResponse.UserPrincipalName}");
            }
        }

        CreateUserRequest CreateRequest(string firstname, string lastname, string email)
        {
            var trimmedFirstName = string.Concat(firstname.Where(c => !char.IsWhiteSpace(c)));
            var trimmedLastName = string.Concat(lastname.Where(c => !char.IsWhiteSpace(c)));
            var displayName = $"{trimmedFirstName.Trim()}.{trimmedLastName.Trim()}";
            return new CreateUserRequest
            {
                FirstName = firstname,
                LastName = lastname,
                DisplayName = displayName,
                UserPrincipalName = $"{displayName}@hearings.reform.hmcts.net",
                RecoveryEmail = string.IsNullOrEmpty(email) ? $"{displayName}@hmcts.net" : $"{email}",
                MailNickname = $"{displayName}".ToLower(),
                AccountEnabled = true,
                ForceChangePasswordNextSignIn = true,
                Password = "qwerty123!"
            };
        }
    }
}