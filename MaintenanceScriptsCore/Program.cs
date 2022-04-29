using System;
using System.Threading.Tasks;
using MaintenanceScriptsCore.Helpers.WebApi;
using MaintenanceScriptsCore.Reference;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

namespace MaintenanceScriptsCore
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddUserSecrets("222e97c1-a830-46f1-982e-c20434df1a70").Build();

            var graphApiSettings = config.GetSection("GraphApi");
            
            if (args.Length != 2)
            {
                Console.WriteLine("A valid group case is required, valid groups are:");
                Console.WriteLine("staffandclerk");
                Console.WriteLine("listingofficer");
                Console.WriteLine("listingofficerwithkinly");
                Console.WriteLine("judge");
                Console.WriteLine("A valid file path is also required as the 2nd argument");
                Console.WriteLine("Example command:");
                Console.WriteLine("dotnet run staffandclerk c:\\filepath");
                return;
            }
            
            var groupCase = args[0];
            var filePath = args[1];

            Operations addUsersOperation;
            var _graphApiService = new GraphApiService(new HttpClientService(), graphApiSettings);

            switch (groupCase.ToLowerInvariant())
            {
                case "staffandclerk":
                    addUsersOperation = new AddUsersWithStaffAndClerkGroups(_graphApiService, filePath);
                    break;
                case "listingofficer":
                    addUsersOperation = new AddUsersWithListingOfficerGroups(_graphApiService, filePath);
                    break;
                case "listingofficerwithkinly":
                    addUsersOperation = new AddUsersWithListingOfficerAndKinlyGroups(_graphApiService, filePath);
                    break;
                case "judge":
                    addUsersOperation = new AddUsersWithJudgeGroups(_graphApiService, filePath);
                    break;
                default:
                    Console.WriteLine("A valid group case is required, valid groups are:");
                    Console.WriteLine("staffandclerk");
                    Console.WriteLine("listingofficer");
                    Console.WriteLine("listingofficerwithkinly");
                    Console.WriteLine("judge");
                    return;
            }

            await addUsersOperation.Run();
        }
    }
}

