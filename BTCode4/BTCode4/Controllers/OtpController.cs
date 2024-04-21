using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OtpNet;

namespace BTCode4.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OtpController : ControllerBase
    {
        private const int DefaultValidityPeriodMinutes = 1;
        private readonly IMemoryCache _cache;

        public OtpController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet("generate")]
        public IActionResult GenerateOtp()
        {
            try
            {

                string secretKey = GenerateRandomBase32Secret();
                string otp = GenerateTOTP(secretKey);

                var otpData = new OtpData { SecretKey = secretKey, GenerationTime = DateTime.Now };
                UpdateOtpCache(otp, otpData);

                Console.WriteLine(GetOtpCacheContents());
                return Ok(new { Otp = otp });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to generate OTP: {ex.Message}");
            }
        }

        [HttpPost("validate")]
        public IActionResult ValidateOtp([FromBody] ValidateOtpRequest request)
        {
            try
            {
               var cachedMap = _cache.Get<Dictionary<string, OtpData>>("otpSecretKeyMap");

                if (cachedMap != null)
                {

                    if (!string.IsNullOrEmpty(request.Otp) && cachedMap.TryGetValue(request.Otp, out var otpData))
                    {
                        if ((DateTime.Now - otpData.GenerationTime).TotalMinutes < DefaultValidityPeriodMinutes)
                        {
                            var secretBytes = Base32Encoding.ToBytes(otpData.SecretKey);
                            var totp = new Totp(secretBytes, step: 30, mode: OtpHashMode.Sha1);
                            bool isValid = totp.VerifyTotp(request.Otp, out _, new VerificationWindow(DefaultValidityPeriodMinutes * 60));

                            if (isValid)
                            {
                                RemoveFromOtpCache(request.Otp);
                                return Ok("OTP is valid");
                            }
                        }
                    }
                }
              
                return BadRequest("OTP is invalid or expired");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to validate OTP: {ex.Message}");
            }
        }

        private void UpdateOtpCache(string otp, OtpData otpData)
        {
            _cache.Set("otpSecretKeyMap", GetUpdatedOtpMap(otp, otpData));
        }

        private void RemoveFromOtpCache(string otp)
        {
            var updatedMap = _cache.Get<Dictionary<string, OtpData>>("otpSecretKeyMap");
            if (updatedMap != null)
            {
                updatedMap.Remove(otp);
                _cache.Set("otpSecretKeyMap", updatedMap);
            }
        }

        private Dictionary<string, OtpData> GetUpdatedOtpMap(string otp, OtpData otpData)
        {
            var existingMap = _cache.Get<Dictionary<string, OtpData>>("otpSecretKeyMap") ?? new Dictionary<string, OtpData>();
            existingMap[otp] = otpData;
            return existingMap;
        }

        private string GenerateRandomBase32Secret(int length = 16)
        {
            var randomBytes = new byte[length];
            var random = new Random();
            random.NextBytes(randomBytes);
            return Base32Encoding.ToString(randomBytes).Substring(0, length);
        }

        private string GenerateTOTP(string secretKey)
        {
            var secretBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(secretBytes, step: 30, mode: OtpHashMode.Sha1);
            return totp.ComputeTotp();
        }

        private string GetOtpCacheContents()
        {
            var cachedMap = _cache.Get<Dictionary<string, OtpData>>("otpSecretKeyMap");
            if (cachedMap != null)
            {
                var contents = string.Join(", ", cachedMap.Select(kvp => $"Key: {kvp.Key}, SecretKey: {kvp.Value.SecretKey}, GenerationTime: {kvp.Value.GenerationTime}"));
                return contents;
            }
            return "Cache is empty";
        }

    }
}
