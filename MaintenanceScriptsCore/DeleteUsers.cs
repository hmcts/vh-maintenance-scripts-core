using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MaintenanceScriptsCore.Reference;

namespace MaintenanceScriptsCore
{
    public class DeleteUsers : Operations
    {
        public DeleteUsers(GraphApiService graphApiService) : base(graphApiService)
        {
        }
        
        public override async Task Run()
        {
            foreach (var username in GetUsers())
            {
                var response = await DeleteUserAsync(username);

                Console.WriteLine($"{(response.IsSuccessStatusCode ? "Delete success" : "failure")} ({(int)response.StatusCode}) - {username}");
            }
        }

        public async Task<HttpResponseMessage> DeleteUserAsync(string username)
        {
            var response = await _graphApiService.DeleteUserAsync(username);
	
            return response;
        }

        private IEnumerable<string> GetUsers()
        {
            return new List<string>
            {
                "dummy.user1@hearings.reform.hmcts.net",
                "dummy.user.2@hearings.reform.hmcts.net"
            };
        }

    }
}