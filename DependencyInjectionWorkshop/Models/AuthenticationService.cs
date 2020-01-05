using System.Net.Http;

#pragma warning disable 1591

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IHash _hash;
        private readonly ILogger _logger;
        private readonly IOtpService _otpService;
        private readonly IProfile _profile;

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
            //_logger.Info($"parameters:{accountId} | {password} | {otp}");

            var passwordFromDb = _profile.GetPassword(accountId);
            var hashedPassword = _hash.Compute(password);
            var currentOtp = _otpService.GetCurrentOtp(accountId);

            var isValid = passwordFromDb == hashedPassword && currentOtp == otp;
            //_logger.Info($"return value:{isValid}");
            return isValid;
        }
    }
}