using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StayEasy.Seguridad
{
    public class Criptografia
    {
        public static byte[] HashearPassword(string passwordPlana)
        {
            if (string.IsNullOrWhiteSpace(passwordPlana)) return null;

            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(passwordPlana));
            }
        }
    }
}
