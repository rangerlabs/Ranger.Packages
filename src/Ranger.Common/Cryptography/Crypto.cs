using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Ranger.Common {
    //Courtesy of this stackoverflow answer, which is an excellent implimentation
    //http://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp
    public static class Crypto {
        private static IConfigurationRoot Configuration { get; set; }
        private static string alphalowerChars = "abcdefghijklmnopqrstuvwxyz";
        private static string alphaUpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static string NumericChars = "1234567890";
        private static string specialChars = "!@#$%^&*()|+=";

        private static string ValidationKeyFromConfig () {
            var builder = new ConfigurationBuilder ()
                .SetBasePath (Directory.GetCurrentDirectory ())
                .AddJsonFile ("../../appsettings.json");

            Configuration = builder.Build ();
            string validationKey = Configuration["EncryptionConfig:ValidationKey"];
            if (String.IsNullOrWhiteSpace (validationKey)) {
                throw new Exception ("The ValidationKey property of the EncryptionConfig section was null or empty.");
            }
            return validationKey;
        }

        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt (string plainText) {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            if (String.IsNullOrEmpty (plainText))
                return plainText;

            var key = ValidationKeyFromConfig ();
            var saltStringBytes = Generate256BitsOfRandomEntropy ();
            var ivStringBytes = Generate256BitsOfRandomEntropy ();
            var plainTextBytes = Encoding.UTF8.GetBytes (plainText);
            using (var password = new Rfc2898DeriveBytes (key, saltStringBytes, DerivationIterations)) {
                var keyBytes = password.GetBytes (Keysize / 8);
                using (var symmetricKey = new RijndaelManaged ()) {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor (keyBytes, ivStringBytes)) {
                        using (var memoryStream = new MemoryStream ()) {
                            using (var cryptoStream = new CryptoStream (memoryStream, encryptor, CryptoStreamMode.Write)) {
                                cryptoStream.Write (plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock ();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat (ivStringBytes).ToArray ();
                                cipherTextBytes = cipherTextBytes.Concat (memoryStream.ToArray ()).ToArray ();
                                memoryStream.Close ();
                                cryptoStream.Close ();
                                return Convert.ToBase64String (cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt (string cipherText) {
            if (String.IsNullOrEmpty (cipherText))
                return cipherText;

            var key = ValidationKeyFromConfig ();
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String (cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take (Keysize / 8).ToArray ();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip (Keysize / 8).Take (Keysize / 8).ToArray ();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip ((Keysize / 8) * 2).Take (cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray ();

            using (var password = new Rfc2898DeriveBytes (key, saltStringBytes, DerivationIterations)) {
                var keyBytes = password.GetBytes (Keysize / 8);
                using (var symmetricKey = new RijndaelManaged ()) {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor (keyBytes, ivStringBytes)) {
                        using (var memoryStream = new MemoryStream (cipherTextBytes)) {
                            using (var cryptoStream = new CryptoStream (memoryStream, decryptor, CryptoStreamMode.Read)) {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read (plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close ();
                                cryptoStream.Close ();
                                return Encoding.UTF8.GetString (plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy () {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider ()) {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes (randomBytes);
            }
            return randomBytes;
        }

        public static void GenerateSecureRandomString (ref SecureString secureString, int length) {
            byte[] bytes = new byte[1];

            using (RNGCryptoServiceProvider rngCryptoProvider = new RNGCryptoServiceProvider ()) {
                for (int i = 0; i < length; i++) {
                    rngCryptoProvider.GetBytes (bytes);
                    string character = Convert.ToBase64String (bytes);
                    secureString.AppendChar (character.ToCharArray () [0]);
                }
            }
        }

        //http://stackoverflow.com/questions/818704/how-to-convert-securestring-to-system-string
        public static string SecureStringToString (SecureString value) {
            IntPtr bstr = Marshal.SecureStringToBSTR (value);

            try {
                return Marshal.PtrToStringBSTR (bstr);
            } finally {
                Marshal.FreeBSTR (bstr);
            }
        }

        public static string GenerateSudoRandomAlphaNumericString (int length) {
            StringBuilder result = new StringBuilder ();
            Random rand = new Random ();
            return new string (Enumerable.Repeat (alphalowerChars + alphaUpperChars + NumericChars, length).Select (s => s[rand.Next (s.Length)]).ToArray ());
        }

        public static string GenerateSudoRandomPasswordString () {
            Regex hasLowerRegex = new Regex ("[a-z]");
            Regex hasUpperRegex = new Regex ("[A-Z]");
            Regex hasNumberRegex = new Regex ("[0-9]");
            Regex hasSpecialRegex = new Regex ("[~!#$^&()+=]");

            StringBuilder result = new StringBuilder ();
            Random rand = new Random ();

            string password = new string (Enumerable.Repeat (alphalowerChars, 4).Select (s => s[rand.Next (s.Length)]).ToArray ());
            password += new string (Enumerable.Repeat (alphaUpperChars, 4).Select (s => s[rand.Next (s.Length)]).ToArray ());
            password += new string (Enumerable.Repeat (NumericChars, 3).Select (s => s[rand.Next (s.Length)]).ToArray ());
            password += new string (Enumerable.Repeat (specialChars, 2).Select (s => s[rand.Next (s.Length)]).ToArray ());

            Random newRand = new Random ();

            string shuffledPassword = new string (password.ToCharArray ().OrderBy (s => (newRand.Next (2) % 2) == 0).ToArray ());
            return shuffledPassword;
        }

        public static string GenerateHash (string input) {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider ();
            byte[] value = Encoding.UTF8.GetBytes (input);
            byte[] hash = hashAlgorithm.ComputeHash (value);
            return Convert.ToBase64String (hash);
        }
    }
}