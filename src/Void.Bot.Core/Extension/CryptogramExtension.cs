using System.Security.Cryptography;
using System.Text;

namespace Void.Bot.Core.Extension;

public static class CryptogramExtension
{
    /// <param name="source"></param>
    extension(string source)
    {
        /// <summary>
        /// MD5加密, 仅做校验文件完整性.
        /// </summary>
        /// <param name="upperOrLower">十六进制: "x2" 小写, "X2" 大写</param>
        /// <returns></returns>
        public string Md5Encrypt(string upperOrLower = "x2")
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

        /// <summary>
        /// SHA加密
        /// </summary>
        /// <param name="algorithmName">SHA变种: SHA256, SHA384, SHA512</param>
        /// <param name="upperOrLower">十六进制: "x2" 小写, "X2" 大写</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public string SHAEncrypt(string algorithmName = "SHA256", string upperOrLower = "x2")
        {
            HashAlgorithm algorithm = algorithmName switch
            {
                "SHA256" => SHA256.Create(),
                "SHA384" => SHA384.Create(),
                "SHA512" => SHA512.Create(),
                _ => throw new ArgumentException("Invalid algorithm name", nameof(algorithmName))
            };

            var bytes = Encoding.UTF8.GetBytes(source);
            var hash = algorithm.ComputeHash(bytes);

            var stringBuilder = new StringBuilder();
            foreach (var t in hash)
            {
                stringBuilder.Append(t.ToString(upperOrLower));
            }

            return stringBuilder.ToString();
        }
    }
}
