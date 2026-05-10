using System;
using System.Security.Cryptography;
using System.Text;

#if !NETSTANDARD2_0 && !NETCOREAPP && !NET5_0_OR_GREATER
internal struct HashAlgorithmName
{
    private readonly string _name;
    public string Name => _name;
    public HashAlgorithmName(string name) { _name = name; }
    public static readonly HashAlgorithmName SHA256 = new HashAlgorithmName("SHA256");
    public static readonly HashAlgorithmName SHA512 = new HashAlgorithmName("SHA512");
}
#endif

namespace BeTong.Services
{
    public static class IdentityPasswordVerifier
    {
        public static bool Verify(string hashedPassword, string password)
        {
            if (hashedPassword == null || hashedPassword.Trim() == "" || password == null)
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

            var deriveBytes = new Rfc2898DeriveBytes(password, salt, 1000);
            try
            {
                var actualSubkey = deriveBytes.GetBytes(subkeySize);
                return ByteArraysEqual(actualSubkey, expectedSubkey);
            }
            finally
            {
                if (deriveBytes is IDisposable disposable)
                {
                    disposable.Dispose();
                }
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

            var deriveBytes = CreateDeriveBytes(password, salt, iterationCount, prf);
            try
            {
                var actualSubkey = deriveBytes.GetBytes(subkeyLength);
                return ByteArraysEqual(actualSubkey, expectedSubkey);
            }
            finally
            {
                if (deriveBytes is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        // Return type changed to DeriveBytes so we can provide a custom PBKDF2 implementation for SHA256/SHA512
        private static DeriveBytes CreateDeriveBytes(string password, byte[] salt, int iterationCount, KeyDerivationPrf prf)
        {
            if (prf == KeyDerivationPrf.HMACSHA1)
            {
                return new Rfc2898DeriveBytes(password, salt, iterationCount);
            }

            if (prf == KeyDerivationPrf.HMACSHA256)
            {
                return new Pbkdf2DerivedBytes(password, salt, iterationCount, HashAlgorithmName.SHA256);
            }

            if (prf == KeyDerivationPrf.HMACSHA512)
            {
                return new Pbkdf2DerivedBytes(password, salt, iterationCount, HashAlgorithmName.SHA512);
            }

            throw new InvalidOperationException("Unsupported PRF");
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

        // Custom PBKDF2 implementation that supports SHA256 and SHA512 on .NET Framework 3.5
        private sealed class Pbkdf2DerivedBytes : DeriveBytes
        {
            private readonly byte[] _salt;
            private readonly int _iterationCount;
            private readonly Func<byte[], HMAC> _hmacFactory;
            private readonly int _hashSize;
            private bool _disposed;

            public Pbkdf2DerivedBytes(string password, byte[] salt, int iterationCount, HashAlgorithmName hashAlgorithm)
            {
                if (password == null) throw new ArgumentNullException("password");
                if (salt == null) throw new ArgumentNullException("salt");
                if (iterationCount <= 0) throw new ArgumentOutOfRangeException("iterationCount");

                _salt = (byte[])salt.Clone();
                _iterationCount = iterationCount;

                var key = Encoding.UTF8.GetBytes(password);

                if (hashAlgorithm.Name == HashAlgorithmName.SHA256.Name)
                {
                    _hmacFactory = k => new HMACSHA256(k);
                    _hashSize = 32;
                }
                else if (hashAlgorithm.Name == HashAlgorithmName.SHA512.Name)
                {
                    _hmacFactory = k => new HMACSHA512(k);
                    _hashSize = 64;
                }
                else
                {
                    throw new NotSupportedException("Only SHA256 and SHA512 are supported by this implementation.");
                }

                // store key for factory usage by closing over it
                var closureKey = (byte[])key.Clone();
                var originalFactory = _hmacFactory;
                _hmacFactory = k => originalFactory(closureKey);
            }

            public override byte[] GetBytes(int cb)
            {
                if (_disposed) throw new ObjectDisposedException("Pbkdf2DerivedBytes");
                if (cb <= 0) throw new ArgumentOutOfRangeException("cb");

                int hashLen = _hashSize;
                int blockCount = (int)Math.Ceiling(cb / (double)hashLen);
                var output = new byte[cb];
                var buffer = new byte[hashLen];
                int outputOffset = 0;

                for (int i = 1; i <= blockCount; i++)
                {
                    // U1 = PRF(password, salt || INT(i))
                    byte[] intBlock = Int(i);
                    byte[] saltInt = new byte[_salt.Length + 4];
                    Buffer.BlockCopy(_salt, 0, saltInt, 0, _salt.Length);
                    Buffer.BlockCopy(intBlock, 0, saltInt, _salt.Length, 4);

                    using (var hmac = _hmacFactory(null))
                    {
                        hmac.Initialize();
                        byte[] u = hmac.ComputeHash(saltInt);
                        Buffer.BlockCopy(u, 0, buffer, 0, hashLen);

                        byte[] t = (byte[])u.Clone();

                        for (int j = 1; j < _iterationCount; j++)
                        {
                            u = hmac.ComputeHash(u);
                            for (int k = 0; k < hashLen; k++)
                            {
                                t[k] ^= u[k];
                            }
                        }

                        int bytesToCopy = Math.Min(hashLen, cb - outputOffset);
                        Buffer.BlockCopy(t, 0, output, outputOffset, bytesToCopy);
                        outputOffset += bytesToCopy;
                    }
                }

                return output;
            }

            public override void Reset()
            {
                // stateless implementation - nothing to reset
            }

            protected void Dispose()
            {
                if (_disposed) return;
                // zero sensitive data
                if (_salt != null)
                    Array.Clear(_salt, 0, _salt.Length);
                _disposed = true;
            }

            private static byte[] Int(int i)
            {
                // big-endian
                return new[] {
                    (byte)((i >> 24) & 0xff),
                    (byte)((i >> 16) & 0xff),
                    (byte)((i >> 8) & 0xff),
                    (byte)(i & 0xff)
                };
            }
        }
    }
}
