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

namespace Ranger.Common
{
    public static class Crypto
    {
        private static string alphalowerChars = "abcdefghijklmnopqrstuvwxyz";
        private static string alphaUpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static string numericChars = "1234567890";
        private static string specialChars = "!@#$%^&*()|+=";

        public static string GenerateSudoRandomAlphaNumericString(int length)
        {
            StringBuilder result = new StringBuilder();
            Random rand = new Random();
            return new string(Enumerable.Repeat(alphalowerChars + alphaUpperChars + numericChars, length).Select(s => s[rand.Next(s.Length)]).ToArray());
        }

        public static string GenerateSudoRandomPasswordString()
        {
            Regex hasLowerRegex = new Regex("[a-z]");
            Regex hasUpperRegex = new Regex("[A-Z]");
            Regex hasNumberRegex = new Regex("[0-9]");
            Regex hasSpecialRegex = new Regex("[~!#$^&()+=]");

            StringBuilder result = new StringBuilder();
            Random rand = new Random();

            string password = new string(Enumerable.Repeat(alphalowerChars, 4).Select(s => s[rand.Next(s.Length)]).ToArray());
            password += new string(Enumerable.Repeat(alphaUpperChars, 4).Select(s => s[rand.Next(s.Length)]).ToArray());
            password += new string(Enumerable.Repeat(numericChars, 3).Select(s => s[rand.Next(s.Length)]).ToArray());
            password += new string(Enumerable.Repeat(specialChars, 2).Select(s => s[rand.Next(s.Length)]).ToArray());

            Random newRand = new Random();

            string shuffledPassword = new string(password.ToCharArray().OrderBy(s => (newRand.Next(2) % 2) == 0).ToArray());
            return shuffledPassword;
        }

        public static string GenerateSHA512Hash(string input)
        {
            using (SHA512 shaM = new SHA512Managed())
            {
                byte[] value = Encoding.UTF8.GetBytes(input);
                byte[] hash = shaM.ComputeHash(value);
                return Convert.ToBase64String(hash);
            }
        }
    }
}