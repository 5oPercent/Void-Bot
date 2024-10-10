using System.Security.Cryptography;
using System.Text;

namespace Void.Bot.Core.Extension;

public static class CryptogramExtension
{
    /// <summary>
    /// MD5加密
    /// </summary>
    /// <param name="source"></param>
    /// <param name="upperOrLower">加密: "x2" 32位, "x3" 48位, "x4" 64位</param>
    /// <returns></returns>
    public static string Md5Encrypt(this string source, string upperOrLower = "x2")
    {
        var tmpByte = Encoding.UTF8.GetBytes(source);
        var result = MD5.HashData(tmpByte);
        
        var stringBuilder = new StringBuilder();
        foreach (var t in result)
        {
            stringBuilder.Append(t.ToString(upperOrLower));
        }
        
        return stringBuilder.ToString();
    }
}