using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Models.ViewModels.Bonusly;
using System.Net.Http.Headers;

namespace OceansApp.DataAccess.Repository
{
    public class BonuslyRepository : IBonuslyRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IConfiguration _config;

        public BonuslyRepository(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            _apiKey = Environment.GetEnvironmentVariable(_config["BonuslyApi:ApiKey"]);
            
        }

        public async Task<string?> GetUserByEmailAsync(string email)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            var response = await _httpClient.GetAsync($"https://bonus.ly/api/scim11/Users?filter=userName eq \"{email}\"");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                dynamic userData = JsonConvert.DeserializeObject<dynamic>(jsonString);
                int totalResults = userData.Resources.Count;
                if (totalResults == 0)
                {
                    return null;
                }
                return userData.Resources[0].id;
            }
            else
            {
                throw new Exception($"Error retrieving user: {response.StatusCode}");
            }
        }
        public async Task<decimal> GetRedeemableBalanceByUserIdAsync(string userId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            var response = await _httpClient.GetAsync($"https://bonus.ly/api/v1/users/{userId}");


            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                dynamic userData = JsonConvert.DeserializeObject<dynamic>(jsonString);
                decimal redeemableBalance = userData.result.earning_balance;
                return redeemableBalance;
            }
            else
            {
                throw new Exception($"Error al obtener el saldo canjeable: {response.StatusCode}");
            }
        }

        public async Task<List<RedemptionsVM>> GetRedemptionsByUserIdAsync(string userId, int limit)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            var response = await _httpClient.GetAsync($"https://bonus.ly/api/v1/users/{userId}/redemptions?limit={limit}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                dynamic bonuslyData = JsonConvert.DeserializeObject<dynamic>(jsonString);
                var redemptions = bonuslyData.result;
                List<RedemptionsVM> redemptionsList = new();

                foreach (var redemptionsFromBonusly in redemptions)
                {
                    RedemptionsVM redemtion = new()
                    {
                        CreationDate = redemptionsFromBonusly.created_at,
                        DisplayPrice = redemptionsFromBonusly.reward_details.display_price,
                        ImageUrl = redemptionsFromBonusly.reward_details.image_url,
                        Name = redemptionsFromBonusly.reward_details.name,
                        Status = redemptionsFromBonusly.state
                    };
                    redemptionsList.Add(redemtion);
                }
                
                return redemptionsList;
            }
            else
            {
                throw new Exception($"Error al obtener el saldo canjeable: {response.StatusCode}");
            }
        }

    }



}
