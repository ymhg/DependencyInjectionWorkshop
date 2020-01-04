using System;
using System.Net.Http;

#pragma warning disable 1591

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};

            //check account locked
            var isLocked = GetAccountIsLocked(accountId, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() {AccountId = accountId};
            }

            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId, httpClient);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                ResetFailedCount(accountId, httpClient);

                return true;
            }
            else
            {
                //失敗
                AddFailedCount(accountId, httpClient);

                LogFailedCount(accountId, httpClient);

                _slackAdapter.Notify(accountId);

                return false;
            }
        }

        /// <summary>
        /// Gets the account is locked.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <returns></returns>
        private static bool GetAccountIsLocked(string accountId, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        /// <summary>
        /// Logs the failed count.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="httpClient">The HTTP client.</param>
        private static void LogFailedCount(string accountId, HttpClient httpClient)
        {
            //紀錄失敗次數 
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        /// <summary>
        /// Adds the failed count.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="httpClient">The HTTP client.</param>
        private static void AddFailedCount(string accountId, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Resets the failed count.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="httpClient">The HTTP client.</param>
        private static void ResetFailedCount(string accountId, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}