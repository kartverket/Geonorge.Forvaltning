using Geonorge.Forvaltning.Models.Api.User;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace Geonorge.Forvaltning.Services
{
    public class AuthService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IOptions<DbConfiguration> dbConfig,
        IOptions<SupabaseConfiguration> supabaseConfig,
        ILogger<AuthService> logger) : IAuthService
    {
        public async Task<User> GetUserFromSupabaseAsync()
        {
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var authTokens);
            var authToken = authTokens.SingleOrDefault()?.Replace("Bearer ", "");

            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Apikey", out var apiKeys);
            var apikey = apiKeys.SingleOrDefault();

            if (!string.IsNullOrWhiteSpace(authToken) && !string.IsNullOrWhiteSpace(apikey))
                return await GetUserFromSupabaseAsync(authToken, apikey);

            return null;
        }

        private async Task<User> GetUserFromSupabaseAsync(string authToken, string apikey)
        {
            try
            {
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, supabaseConfig.Value.Url + "/auth/v1/user");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                requestMessage.Headers.Add("Apikey", apikey);

                using var response = await httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                var userInfo = await response.Content.ReadFromJsonAsync<JsonObject>();

                var userId = userInfo["identities"].AsArray()
                    .FirstOrDefault()?["user_id"]
                    .GetValue<string>();

                if (!string.IsNullOrWhiteSpace(userId))
                    return await GetUserInfoFromSupabaseAsync(userInfo, userId);

                logger.LogError($"Could not get user info from token.");
                return null;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Could not get user info from token.");
                return null;
            }
        }

        private async Task<User> GetUserInfoFromSupabaseAsync(JsonObject userInfo, string userId)
        {
            var user = new User
            {
                Name = userInfo["user_metadata"]?["name"]?.GetValue<string>() ?? "",
                Email = userInfo["user_metadata"]?["email"]?.GetValue<string>() ?? ""
            };

            var sql = "SELECT organization from public.users where id::text = $1";
            
            await using var connection = new NpgsqlConnection(dbConfig.Value.ForvaltningApiDatabase);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand();
            command.Parameters.AddWithValue(userId);
            command.Connection = connection;
            command.CommandText = sql;

            await using var reader = await command.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                await reader.ReadAsync();
                
                if (!reader.IsDBNull(0))
                    user.OrganizationNumber = reader.GetString(0);
            }

            connection.Close();

            return user;
        }
    }

    public interface IAuthService
    {
        Task<User> GetUserFromSupabaseAsync();
    }
}