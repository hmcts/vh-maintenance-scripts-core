using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MaintenanceScriptsCore.Helpers.WebApi;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MaintenanceScriptsCore.Reference
{
    public class GraphApiService
    {
        public List<string> JudgeGroups = new List<string> { "VirtualRoomJudge", "Internal" };
        public List<string> IndividualGroups = new List<string> { "External" };
        public List<string> RepresentativeGroups = new List<string> { "VirtualRoomProfessionalUser" };
        public List<string> ListingOfficerGroups = new List<string> { "VirtualRoomAdministrator", "External" };
        public List<string> StaffAndClerkGroups = new List<string> { "Staff Member", "Internal" };

        public List<string> VhoGroups = new List<string>
        {
            "VirtualRoomAdministrator", "Tax", "Civil Money Claims", "Financial Remedy", "Generic", "Children Act",
            "Family Law Act", "Tribunal", "Civil"
        };

        public List<string> TestAccountsGroups = new List<string> { "TestAccount", "PerformanceTestAccount" };

        public List<string> KinlyGroups = new List<string>
        {
            "vh_video_kinly_saml2_prod_users", "vh_video_kinly_saml2_preprod_users", "vh_video_kinly_saml2_test_users",
            "vh_video_kinly_saml2_test1_users", "vh_video_kinly_saml2_test2_users", "vh_judges_vh_video_kinly_saml2_dev"
        };

        public List<string> ListingOfficerGroupsWithKinlyGroups = new List<string>
        {
            "VirtualRoomAdministrator", "External",
            "vh_video_kinly_saml2_prod_users", "vh_video_kinly_saml2_preprod_users", "vh_video_kinly_saml2_test_users",
            "vh_video_kinly_saml2_test1_users", "vh_video_kinly_saml2_test2_users", "vh_judges_vh_video_kinly_saml2_dev"
        };

        public IDictionary<string, Microsoft.Graph.Group> groupCache = new Dictionary<string, Microsoft.Graph.Group>();

        public static readonly Compare<UserResponse> CompareJudgeById =
            Compare<UserResponse>.By((x, y) => x.Email == y.Email, x => x.Email.GetHashCode());

        private IHttpClientService _httpClientService;
        private string _accessTokenWindows;
        private string _accessToken;

        private AzureAdConfiguration _azureAdConfiguration = new AzureAdConfiguration
        {
            GraphApiBaseUri = "https://graph.microsoft.com/",
            Authority = "https://login.microsoftonline.com/",
            TenantId = "tenant-id",
            GraphApiBaseUriWindows = "https://graph.windows.net/",
            ClientId = "client-id-for-vh-user-api-dev",
            ClientSecret = "client-secret-for-vh-user-api-dev"
        };

        public GraphApiService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
            _accessTokenWindows = GetClientAccessTokenGraphApi(GraphApiType.GraphWindows);
            _accessToken = GetClientAccessTokenGraphApi(GraphApiType.GraphMicrosoft);
        }

        public async Task<IEnumerable<AzureAdGraphUserResponse>> GetUsersByFilter(string filter)
        {
            var accessUri =
                $"{_azureAdConfiguration.GraphApiBaseUriWindows}{_azureAdConfiguration.TenantId}/users?$filter={filter}&api-version=1.6";

            var queryResponse =
                await _httpClientService.GetAsync<AzureAdGraphQueryResponse<AzureAdGraphUserResponse>>(accessUri,
                    _accessTokenWindows);

            return !queryResponse.Value.Any() ? null : queryResponse.Value;
        }

        public async Task<Microsoft.Graph.User> GetUserByFilter(string filter)
        {
            var accessUri =
                $"{_azureAdConfiguration.GraphApiBaseUriWindows}{_azureAdConfiguration.TenantId}/users?$filter={filter}&api-version=1.6";

            var queryResponse =
                await _httpClientService.GetAsync<AzureAdGraphQueryResponse<AzureAdGraphUserResponse>>(accessUri,
                    _accessTokenWindows);

            if (!queryResponse.Value.Any()) return null;

            //return queryResponse.Value.First();

            var adUser = queryResponse.Value.First();

            return new User
            {
                Id = adUser.ObjectId,
                DisplayName = adUser.DisplayName,
                UserPrincipalName = adUser.UserPrincipalName,
                GivenName = adUser.GivenName,
                Surname = adUser.Surname,
                Mail = adUser.OtherMails?.FirstOrDefault()
            };
        }

        public async Task<Microsoft.Graph.Group> GetGroupByName(string groupName)
        {
            var accessUri = $"{_azureAdConfiguration.GraphApiBaseUri}v1.0/groups?$filter=displayName eq '{groupName}'";

            var queryResponse = await _httpClientService.GetAsync<GraphQueryResponse>(accessUri, _accessToken);

            return queryResponse.Value?.FirstOrDefault();
        }

        public async Task<bool> AddUserToGroup(User user, Microsoft.Graph.Group group)
        {
            var body = new CustomDirectoryObject
            {
                ObjectDataId = $"{_azureAdConfiguration.GraphApiBaseUri}v1.0/directoryObjects/{user.Id}"
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var accessUri = $"{_azureAdConfiguration.GraphApiBaseUri}beta/groups/{group.Id}/members/$ref";

            var response = await _httpClientService.PostAsync(accessUri, stringContent, _accessToken);

            if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                return errorResponse.Contains("already exist") ? true : false;
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<User> CreateUserAsync(CreateUserRequest request)
        {
            var user = new
            {
                displayName = request.DisplayName,
                givenName = request.FirstName,
                surname = request.LastName,
                mailNickname = request.MailNickname,
                otherMails = new List<string> { request.RecoveryEmail },
                accountEnabled = request.AccountEnabled,
                userPrincipalName = request.UserPrincipalName,
                mobilePhone = request.MobilePhone ?? " ",
                businessPhones = new List<string> { request.BusinessPhone ?? " " },
                mail = request.RecoveryEmail,
                passwordProfile = new
                {
                    forceChangePasswordNextSignIn = request.ForceChangePasswordNextSignIn,
                    password = request.Password
                }
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            var accessUri = $"{_azureAdConfiguration.GraphApiBaseUri}v1.0/{_azureAdConfiguration.TenantId}/users";
            var response = await _httpClientService.PostAsync(accessUri, stringContent, _accessToken);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<User>(responseJson);
        }

        public async Task UpdateUserAsync(string username, object user)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            var accessUri =
                $"{_azureAdConfiguration.GraphApiBaseUri}/v1.0/{_azureAdConfiguration.TenantId}/users/{username}";

            var response = await _httpClientService.PatchAsync(accessUri, stringContent, _accessToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new Exception($"Error: Status: {response.StatusCode}, Message: {error}");
            }
        }

        public async Task<List<Microsoft.Graph.Group>> GetGroupsForUserAsync(string userId)
        {
            var accessUri = $"{_azureAdConfiguration.GraphApiBaseUri}v1.0/users/{userId}/memberOf";

            var queryResponse = await _httpClientService.GetAsync<DirectoryObject>(accessUri, _accessToken);

            var groupArray = JArray.Parse(queryResponse?.AdditionalData["value"].ToString());

            var groups = new List<Microsoft.Graph.Group>();
            foreach (var item in groupArray.Children())
            {
                var itemProperties = item.Children<JProperty>();
                var type = itemProperties.FirstOrDefault(x => x.Name == "@odata.type");

                // If #microsoft.graph.directoryRole ignore the group mappings
                if (type.Value.ToString() == "#microsoft.graph.group")
                {
                    var group = JsonConvert.DeserializeObject<Microsoft.Graph.Group>(item.ToString());
                    groups.Add(group);
                }
            }

            return groups;
        }

        public async IAsyncEnumerable<IEnumerable<User>> GetUsersByGroupAsync(Group group)
        {
            var accessUri = $"{_azureAdConfiguration.GraphApiBaseUri}v1.0/groups/{group.Id}/members" +
                            "/microsoft.graph.user?$select=id,userPrincipalName,displayName,givenName,surname&$top=999";

            while (true)
            {
                var directoryObject = await _httpClientService.GetAsync<DirectoryObject>(accessUri, _accessToken);

                yield return JsonConvert.DeserializeObject<List<User>>(directoryObject.AdditionalData["value"]
                    .ToString());

                if (directoryObject.AdditionalData.ContainsKey("@odata.nextLink"))
                {
                    accessUri = directoryObject.AdditionalData["@odata.nextLink"].ToString();
                }
                else
                {
                    break;
                }
            }
        }

        public async Task<HttpResponseMessage> DeleteUserAsync(string username)
        {
            var accessUri =
                $"{_azureAdConfiguration.GraphApiBaseUri}v1.0/{_azureAdConfiguration.TenantId}/users/{username}";

            var response = await _httpClientService.DeleteAsync(accessUri, _accessToken);

            response.EnsureSuccessStatusCode();

            return response;
        }

        private string GetClientAccessTokenGraphApi(GraphApiType graphApiType)
        {
            AuthenticationResult result;
            var credential = new ClientCredential(_azureAdConfiguration.ClientId, _azureAdConfiguration.ClientSecret);
            var authContext =
                new AuthenticationContext($"{_azureAdConfiguration.Authority}{_azureAdConfiguration.TenantId}");

            try
            {
                result = authContext
                    .AcquireTokenAsync(
                        graphApiType == GraphApiType.GraphMicrosoft
                            ? _azureAdConfiguration.GraphApiBaseUri
                            : _azureAdConfiguration.GraphApiBaseUriWindows, credential).Result;
            }
            catch (AdalException)
            {
                throw new UnauthorizedAccessException();
            }

            return result.AccessToken;
        }
    }

// Types
    public enum GraphApiType
    {
        GraphMicrosoft,
        GraphWindows
    }

    public class AzureAdConfiguration
    {
        public string Authority { get; set; }
        public string TenantId { get; set; }
        public string VhUserApiResourceId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string GraphApiBaseUri { get; set; }
        public string VhBookingsApiResourceId { get; set; }
        public string GraphApiBaseUriWindows { get; set; }
    }

    public class AzureAdGraphQueryResponse<T>
    {
        [JsonProperty("@odata.metadata")] public string Context { get; set; }

        [JsonProperty("value")] public IList<T> Value { get; set; }
    }

    public class AzureAdGraphUserResponse
    {
        public string ObjectId { get; set; }
        public string UserPrincipalName { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public List<string> OtherMails { get; set; }
    }

    public class GraphQueryResponse
    {
        [JsonProperty("@odata.context")] public string Context { get; set; }

        [JsonProperty("value")] public IList<Microsoft.Graph.Group> Value { get; set; }
    }

    public class CustomDirectoryObject
    {
        [JsonProperty(PropertyName = "@odata.id")]
        public string ObjectDataId { get; set; }
    }

    public class CreateUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string UserPrincipalName { get; set; }
        public string MailNickname { get; set; }
        public string RecoveryEmail { get; set; }
        public string MobilePhone { get; set; }
        public string BusinessPhone { get; set; }
        public bool AccountEnabled { get; set; }
        public bool ForceChangePasswordNextSignIn { get; set; }
        public string Password { get; set; }
    }

    public class UserResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
    }

    public class Compare<TType> : IEqualityComparer<TType>
    {
        private readonly Func<TType, TType, bool> _condition;
        private readonly Func<TType, int> _hashCode;

        private Compare(Func<TType, TType, bool> condition, Func<TType, int> hashCode)
        {
            _condition = condition;
            _hashCode = hashCode;
        }

        public bool Equals(TType x, TType y)
        {
            return _condition(x, y);
        }

        public int GetHashCode(TType obj)
        {
            return _hashCode(obj);
        }

        public static Compare<TType> By(Func<TType, TType, bool> condition, Func<TType, int> hashCode)
        {
            return new Compare<TType>(condition, hashCode);
        }
    }
}