using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaintenanceScriptsCore.Reference;

namespace MaintenanceScriptsCore
{
    public class GetUsers : Operations
    {
        public GetUsers(GraphApiService graphApiService) : base(graphApiService)
        {
        }

        public override async Task Run()
        {
            var users = new List<string>
            {
                "a1.a1@email.com",
                "a2.a2@email.com",
            };

            foreach (var username in users)
            {
                try
                {
                    //var user = await graphApiService.GetUserByFilter($"userPrincipalName  eq '{username}'").Dump();
                    var user = await _graphApiService.GetUserByFilter($"startswith(userPrincipalName,'{username}')");
                    //var user = await graphApiService.GetUsersByFilter($"otherMails/any(c:c eq '{username}')").Dump();
                    
                    Console.WriteLine(user);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{username}, Error: {ex.Message}");
                }
            }
        }
    }
}