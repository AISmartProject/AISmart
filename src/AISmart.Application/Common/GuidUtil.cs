using System;
using System.Security.Cryptography;
using System.Text;

namespace AISmart.Common;

public class GuidUtil
{
    public static Guid StringToGuid(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return new Guid(hash);
        }
    }
}