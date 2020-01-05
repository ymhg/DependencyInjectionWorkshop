using System.Net.Http;

#pragma warning disable 1591

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IProfile _profile;
        private readonly ILogger _logger;

        public AuthenticationService(IOtpService otpService, IProfile profile, IHash hash, ILogger logger)
        {
            _otpService = otpService;
            _profile = profile;
            _hash = hash;
            _logger = logger;
        }

        public AuthenticationService(ILogger logger)
        {
            _logger = logger;
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var passwordFromDb = _profile.GetPassword(accountId);
            var hashedPassword = _hash.Compute(password);
            var currentOtp = _otpService.GetCurrentOtp(accountId);
            _logger.Info($"current otp:{currentOtp}");

            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}