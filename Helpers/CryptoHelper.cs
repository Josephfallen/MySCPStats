// -----------------------------------------------------------------------
// <copyright file="CryptoHelper.cs" company="MySCPStats">
// Copyright (c) joseph_fallen. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------


using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SCPStats
{
    public static class CryptoHelper
    {
        // Use your generated Key and IV here
        private static readonly string Key = "VnCOp6t7Jr8WbzFHVrk56IuYh9QwzI1u41Vvmbk8TzY=";  // Base64 encoded
        private static readonly string IV = "V6RbRr1Od/JxHjM0zF7+yQ==";  // Base64 encoded

        public static string Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Convert.FromBase64String(Key);
            aes.IV = Convert.FromBase64String(IV);

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter writer = new(cs))
                {
                    writer.Write(plainText);
                }
            }

            // Return the encrypted text as Base64 string
            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
