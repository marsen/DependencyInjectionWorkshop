using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IOtpService
    {
        string CurrentOtp(string accountId, HttpClient httpClient);
    }

    public class OtpService : IOtpService
    {
        public string CurrentOtp(string accountId, HttpClient httpClient)
        {
            //get otp
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }
    }
}