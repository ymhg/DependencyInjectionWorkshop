﻿using System;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IAuthentication _authentication;
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IOtp _otp;
        private IProfile _profile;

        [SetUp]
        public void SetUp()
        {
            _failedCounter = Substitute.For<IFailedCounter>();
            _hash = Substitute.For<IHash>();
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _otp = Substitute.For<IOtp>();
            _profile = Substitute.For<IProfile>();
            _authentication = new AuthenticationService(_failedCounter, _hash, _logger, _otp, _profile);
            _authentication = new FailedCountDecorator(_authentication, _failedCounter);
            _authentication = new NotificationDecorator(_authentication, _notification);
        }

        [Test]
        public void Valid()
        {
            GivenIsAccountLocked("joey", false);
            GivenPasswordFromDb("joey", "hashed pw");
            GivenHashedPassword("123", "hashed pw");
            GivenCurrentOtp("joey", "000000");

            ShouldBeValid("joey", "123", "000000");
        }

        [Test]
        public void Invalid()
        {
            GivenIsAccountLocked("joey", false);
            GivenPasswordFromDb("joey", "hashed pw");
            GivenHashedPassword("123", "hashed pw");
            GivenCurrentOtp("joey", "000000");

            ShouldBeInvalid("joey", "wrong password", "000000");
        }

        [Test]
        public void Should_add_failed_count_when_invalid()
        {
            WhenInvalid("joey");
            ShouldAddFailedCount();
        }

        [Test]
        public void Should_notify_user_when_invalid()
        {
            WhenInvalid("joey");
            ShouldNotify();
        }

        [Test]
        public void Log_latest_failed_count_when_invalid()
        {
            GivenLatestFailedCount("joey", 5);
            WhenInvalid("joey");
            ShouldLog("joey", "5");
        }

        private void ShouldLog(string accountId, string failedCount)
        {
            _logger.Received().LogInfo(Arg.Is<string>(s => s.Contains(accountId) && s.Contains("5")));
        }

        private void GivenLatestFailedCount(string accountId, int failedCount)
        {
            _failedCounter.Get(accountId).Returns(5);
        }

        private void ShouldNotify()
        {
            _notification.Received(1).Notify("joey");
        }

        private void ShouldAddFailedCount()
        {
            _failedCounter.Received(1).Add("joey");
        }

        private void WhenInvalid(string accountId)
        {
            GivenIsAccountLocked(accountId, false);
            GivenPasswordFromDb(accountId, "hashed pw");
            GivenHashedPassword("123", "hashed pw");
            GivenCurrentOtp(accountId, "000000");

            _authentication.Verify(accountId, "wrong password", "000000");
        }

        private void ShouldBeInvalid(string accountId, string inputPassword, string inputOtp)
        {
            var isValid = _authentication.Verify(accountId, inputPassword, inputOtp);
            Assert.AreEqual(false, isValid);
        }

        [Test]
        public void Reset_failed_count_when_valid()
        {
            WhenValid("joey");
            ShouldResetFailedCount("joey");
        }

        [Test]
        public void Account_is_locked()
        {
            GivenIsAccountLocked("joey", true);
            ShouldThrow<FailedTooManyTimesException>("joey");
        }

        private void ShouldThrow<TException>(string accountId) where TException : Exception
        {
            void LockedVerify() => _authentication.Verify(accountId, "123", "123");
            Assert.Throws<TException>(LockedVerify);
        }

        private void ShouldResetFailedCount(string accountId)
        {
            _failedCounter.Received(1).Reset(accountId);
        }

        private void WhenValid(string accountId)
        {
            GivenIsAccountLocked(accountId, false);
            GivenPasswordFromDb(accountId, "hashed pw");
            GivenHashedPassword("123", "hashed pw");
            GivenCurrentOtp(accountId, "000000");

            _authentication.Verify(accountId, "123", "000000");
        }

        private void GivenCurrentOtp(string accountId, string currentOtp)
        {
            _otp.GetCurrentOtp(accountId).Returns(currentOtp);
        }

        private void GivenHashedPassword(string inputPassword, string hashedResult)
        {
            _hash.Compute(inputPassword).Returns(hashedResult);
        }

        private void GivenPasswordFromDb(string accountId, string password)
        {
            _profile.GetPasswordFromDb(accountId).Returns(password);
        }

        private void GivenIsAccountLocked(string accountId, bool isLocked)
        {
            _failedCounter.IsAccountLocked(accountId).Returns(isLocked);
        }

        private void ShouldBeValid(string accountId, string inputPassword, string inputOtp)
        {
            var isValid = _authentication.Verify(accountId, inputPassword, inputOtp);
            Assert.AreEqual(true, isValid);
        }
    }
}