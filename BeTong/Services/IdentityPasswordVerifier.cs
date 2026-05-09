using System;
using System.Security.Cryptography;

namespace BeTong.Services
{
    public static class IdentityPasswordVerifier
    {
        public static bool Verify(string hashedPassword, string password)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword) || password == null)
            {
                return false;
            }

            byte[] decodedHash;
            try
            {
                decodedHash = Convert.FromBase64String(hashedPassword);
            }
            catch (FormatException)
            {
                return false;
            }

            if (decodedHash.Length == 0)
            {
                return false;
            }

            if (decodedHash[0] == 0x00)
            {
                return VerifyVersion2(decodedHash, password);
            }

            if (decodedHash[0] == 0x01)
            {
                return VerifyVersion3(decodedHash, password);
            }

            return false;
        }

        private static bool VerifyVersion2(byte[] hashedPassword, string password)
        {
            const int saltSize = 16;
            const int subkeySize = 32;

            if (hashedPassword.Length != 1 + saltSize + subkeySize)
            {
                return false;
            }

            var salt = new byte[saltSize];
            Buffer.BlockCopy(hashedPassword, 1, salt, 0, salt.Length);

            var expectedSubkey = new byte[subkeySize];
            Buffer.BlockCopy(hashedPassword, 1 + saltSize, expectedSubkey, 0, expectedSubkey.Length);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 1000))
            {
                var actualSubkey = deriveBytes.GetBytes(subkeySize);
                return ByteArraysEqual(actualSubkey, expectedSubkey);
            }
        }

        private static bool VerifyVersion3(byte[] hashedPassword, string password)
        {
            if (hashedPassword.Length < 13)
            {
                return false;
            }

            var prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
            var iterationCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
            var saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);

            if (saltLength < 16 || hashedPassword.Length < 13 + saltLength)
            {
                return false;
            }

            var salt = new byte[saltLength];
            Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);

            var subkeyLength = hashedPassword.Length - 13 - salt.Length;
            if (subkeyLength < 16)
            {
                return false;
            }

            var expectedSubkey = new byte[subkeyLength];
            Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

            using (var deriveBytes = CreateDeriveBytes(password, salt, iterationCount, prf))
            {
                var actualSubkey = deriveBytes.GetBytes(subkeyLength);
                return ByteArraysEqual(actualSubkey, expectedSubkey);
            }
        }

        private static Rfc2898DeriveBytes CreateDeriveBytes(string password, byte[] salt, int iterationCount, KeyDerivationPrf prf)
        {
            if (prf == KeyDerivationPrf.HMACSHA1)
            {
                return new Rfc2898DeriveBytes(password, salt, iterationCount);
            }

            if (prf == KeyDerivationPrf.HMACSHA256)
            {
                return new Rfc2898DeriveBytes(password, salt, iterationCount, HashAlgorithmName.SHA256);
            }

            if (prf == KeyDerivationPrf.HMACSHA512)
            {
                return new Rfc2898DeriveBytes(password, salt, iterationCount, HashAlgorithmName.SHA512);
            }

            throw new InvalidOperationException("Định dạng mật khẩu mã hóa không được hỗ trợ.");
        }

        private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
        {
            return ((uint)buffer[offset] << 24)
                | ((uint)buffer[offset + 1] << 16)
                | ((uint)buffer[offset + 2] << 8)
                | buffer[offset + 3];
        }

        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }

        private enum KeyDerivationPrf : uint
        {
            HMACSHA1 = 0,
            HMACSHA256 = 1,
            HMACSHA512 = 2
        }
    }
}
