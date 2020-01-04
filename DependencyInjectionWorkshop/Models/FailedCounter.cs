using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounter
    {
        public FailedCounter()
        {
        }

        /// <summary>
        /// Resets the failed count.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public void Reset(string accountId, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }
    }
}