using BTCode4.Controllers;
using BTCode4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace TestProject5
{
    public class Tests
    {
        private OtpController _otpController;
        private IMemoryCache _memoryCache;

        [SetUp]
        public void Setup()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _otpController = new OtpController(_memoryCache);
        }

        [TearDown]
        public void TearDown()
        {
            _memoryCache.Dispose();
        }

        [Test]
        public void GenerateOtp_Returns_Ok()
        {
            var result = _otpController.GenerateOtp();

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public void ValidateOtp_With_Valid_Otp_Returns_Ok()
        {
            var otp = "valid_otp";
            var secretKey = "valid_secret_key";
            var generationTime = DateTime.Now.AddSeconds(30);

            var base32EncodedSecretKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(secretKey));

            _memoryCache.Set("otpSecretKeyMap", new Dictionary<string, OtpData>
            {
                { otp, new OtpData { SecretKey = base32EncodedSecretKey, GenerationTime = generationTime } }
            });

            var result = _otpController.ValidateOtp(new ValidateOtpRequest { Otp = otp });

            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.That(okResult.Value, Is.EqualTo("OTP is valid"));
        }


        [Test]
        public void ValidateOtp_With_Expired_Otp_Returns_BadRequest_Expired() 
        {
            var otp = "expired_otp";
            var secretKey = "valid_secret_key";
            var defaultValidityPeriodMinutes = 1;
            var generationTime = DateTime.Now.AddMinutes(-(defaultValidityPeriodMinutes + 1));
            _memoryCache.Set("otpSecretKeyMap", new Dictionary<string, OtpData>
            {
                { otp, new OtpData { SecretKey = secretKey, GenerationTime = generationTime } }
            });

            var result = _otpController.ValidateOtp(new ValidateOtpRequest { Otp = otp });

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public void ValidateOtp_With_Invalid_Otp_Returns_BadRequest_Invalid()
        {
            var invalidOtp = "invalid_otp";

            var result = _otpController.ValidateOtp(new ValidateOtpRequest { Otp = invalidOtp });

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.That(badRequestResult.Value, Is.EqualTo("OTP is invalid or expired"));
        }
    }
}
