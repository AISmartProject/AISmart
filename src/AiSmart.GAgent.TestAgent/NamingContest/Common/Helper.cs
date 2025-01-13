using System.Security.Cryptography;
using System.Text;

namespace AiSmart.GAgent.TestAgent.NamingContest.Common;

public static class Helper
{
    public static Guid GetVoteCharmingGrainId(int round,int step)
    {
        return ConvertStringToGuid($"AI-Most-Charming-Naming-Contest-{round}-{step}");
    }
    
    private static Guid ConvertStringToGuid(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return new Guid(hash);
        }
    }
}