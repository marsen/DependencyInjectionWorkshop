using System.Net.Http;

namespace DependencyInjectionWorkshop.Models.Interface
{
    public interface IOtpService
    {
        string CurrentOtp(string accountId, HttpClient httpClient);
    }
}