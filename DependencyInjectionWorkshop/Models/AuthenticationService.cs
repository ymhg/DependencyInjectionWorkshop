using System;
using System.Net.Http;

#pragma warning disable 1591

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly FailedCounter _failedCounter;
        private readonly NLogAdapter _nLogAdapter;
        private readonly OtpService _otpService;
        private readonly IProfile _profileDao;
        private readonly IHash _hash;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService(FailedCounter failedCounter, NLogAdapter nLogAdapter, OtpService otpService,
            IProfile profileDao, IHash hash, SlackAdapter slackAdapter)
        {
            _failedCounter = failedCounter;
            _nLogAdapter = nLogAdapter;
            _otpService = otpService;
            _profileDao = profileDao;
            _hash = hash;
            _slackAdapter = slackAdapter;
        }

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _nLogAdapter = new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            //check account locked
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() {AccountId = accountId};
            }

            var passwordFromDb = _profileDao.GetPassword(accountId);

            var hashedPassword = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failedCounter.Reset(accountId);

                return true;
            }
            else
            {
                //失敗
                _failedCounter.AddFailedCount(accountId);

                LogFailedCount(accountId);

                _slackAdapter.Notify(accountId);

                return false;
            }
        }

        private void LogFailedCount(string accountId)
        {
            //紀錄失敗次數 
            var failedCount = _failedCounter.GetFailedCount(accountId);
            _nLogAdapter.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}