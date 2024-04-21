# OTP

I have developed a system that generates one-time passwords. The user will receive the OTP in a toastr message and will be able to validate it for 1 minute. The OTP can be validated only once.

When generating an OTP, the process begins by generating a random secret key using the GenerateRandomBase32Secret() function. This secret serves as the private key in the OTP generation algorithm. Using the GenerateTOTP() function, this secret is then utilized to produce an OTP, which is a unique one-time password. Simultaneously, an OtpData object is created to store the generated secret and the generation time. To ensure the persistence of this information, the generated OTP and its associated data are updated in the application's cache using the UpdateOtpCache() method. Finally, the user receives an HTTP OK response containing the generated OTP.

On the other hand, the OTP validation process involves accessing the cache to check if the OTP sent by the user exists and has not expired. If the OTP is found and still valid, it is verified using the Totp.VerifyTotp() function. If the OTP is valid, it is removed from the cache, and the user receives an HTTP OK response with the message "OTP is valid." Otherwise, the user is informed through an HTTP BadRequest response that the OTP is either invalid or has expired.

Auxiliary functions such as UpdateOtpCache(), RemoveFromOtpCache(), and GetUpdatedOtpMap() are responsible for manipulating the OTP cache, ensuring the coherence and correctness of the stored data. Additionally, the GenerateRandomBase32Secret() and GenerateTOTP() functions are essential in generating random secret keys and the corresponding OTPs.
