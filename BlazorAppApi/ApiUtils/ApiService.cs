using APICoches.Models;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BlazorAppApi.ApiUtils
{
    public class ApiService
    {
        private const string HealthEndpoint = "hc",
            GetAllCarsEndpoint = "cars/getall",
            GetCarByIdEndpoint = "cars/GetById?id={0}",
            PostCarEndpoint = "cars/post?model={0}&brand={1}",
            DeleteCarEndpoint = "cars/delete?id={0}",
            PutCarEndpoint = "cars/put?id={0}&model={1}&brand={2}",
            GetSalesByCarIdEndpoint = "sales/getbycarid?id={0}";

        private static HttpClient _httpClient = new HttpClient
        { 
            BaseAddress = new Uri(Environment.GetEnvironmentVariable("API_URI"))
        };

        public static async Task<HttpResponseMessage> GetHealthReport()
        {
            return await _httpClient.GetAsync(HealthEndpoint);
        }

        public static async Task<List<Car>> GetAllCars()
        {
            return await _httpClient.GetFromJsonAsync<List<Car>>(GetAllCarsEndpoint);
        }

        public static async Task<Car> getCarById(int id) { 
            return await _httpClient.GetFromJsonAsync<Car>(string.Format(GetCarByIdEndpoint, id));
        }

        public static async Task<HttpResponseMessage> PostCar(string model, string brand)
        {
            return await _httpClient.PostAsJsonAsync(string.Format(PostCarEndpoint, model, brand), new {});
        }

        public static async Task<HttpResponseMessage> DeleteCar(int id)
        {
            return await _httpClient.DeleteAsync(string.Format(DeleteCarEndpoint, id));
        }

        public static async Task<HttpResponseMessage> PutCar(int id, string model, string brand)
        {
            return await _httpClient.PutAsJsonAsync(string.Format(PutCarEndpoint, id, model, brand), new { });
        }

        public static async Task<List<Sale>> GetSalesByCarId(int id)
        {
            return await _httpClient.GetFromJsonAsync<List<Sale>>(string.Format(GetSalesByCarIdEndpoint, id));
        }
    }
}
