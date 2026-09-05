using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StayEasy.Seguridad
{
    public static class Criptografia
    {
        public static byte[] HashearPassword(string passwordEnClaro)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytesPassword = Encoding.UTF8.GetBytes(passwordEnClaro);
                return sha256.ComputeHash(bytesPassword);
            }
        }
    }
}
