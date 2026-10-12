using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace Nobisoft.Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Trim spaces then convert string to lowercase
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToTrimLower(this string input)
        {
            return input.Trim().ToLower();
        }

        /// <summary>
        /// Trim spaces then convert string to uppercase
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToTrimUpper(this string input)
        {
            return input.Trim().ToUpper();
        }

        /// <summary>
        /// Hash MD5
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Md5Hash(this string input)
        {
            var stringBuilder = new StringBuilder();
            foreach (var num in MD5.Create().ComputeHash(new UTF8Encoding().GetBytes(input)))
                stringBuilder.Append(num.ToString("x2"));
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Encrypt string by AES algorithm
        /// </summary>
        /// <param name="text"></param>
        /// <param name="keyString"></param>
        /// <returns></returns>
        public static string EncryptString(this string text, string keyString)
        {
            Debug.Assert(keyString.Length == (256 / 8));
            var bytes = Encoding.ASCII.GetBytes(keyString);
            Debug.Assert(bytes.Length == (256 / 8));
            Debug.Assert(bytes.Length == keyString.Length);
            using var aes = Aes.Create();
            using var encryptor = aes.CreateEncryptor(bytes, aes.IV);
            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                using var streamWriter = new StreamWriter(cryptoStream);
                streamWriter.Write(text);
            }

            var iv = aes.IV;
            var array = memoryStream.ToArray();
            var inArray = new byte[iv.Length + array.Length];
            Buffer.BlockCopy(iv, 0, inArray, 0, iv.Length);
            Buffer.BlockCopy(array, 0, inArray, iv.Length, array.Length);
            return Convert.ToBase64String(inArray);
        }

        /// <summary>
        /// Decrypt string
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="keyString"></param>
        /// <returns></returns>
        public static string DecryptString(this string cipherText, string keyString)
        {
            Debug.Assert(keyString.Length == (256 / 8));
            var numArray = Convert.FromBase64String(cipherText);
            var rgbIv = new byte[16];
            var buffer = new byte[numArray.Length - rgbIv.Length];
            Buffer.BlockCopy(numArray, 0, rgbIv, 0, rgbIv.Length);
            Buffer.BlockCopy(numArray, rgbIv.Length, buffer, 0, numArray.Length - rgbIv.Length);
            var bytes = Encoding.ASCII.GetBytes(keyString);
            Debug.Assert(bytes.Length == (256 / 8));
            Debug.Assert(bytes.Length == keyString.Length);
            using Aes aes = Aes.Create();
            using ICryptoTransform decryptor = aes.CreateDecryptor(bytes, rgbIv);
            string end;
            using (var memoryStream = new MemoryStream(buffer))
            {
                using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                using var streamReader = new StreamReader(cryptoStream);
                end = streamReader.ReadToEnd();
            }

            return end;
        }

        /// <summary>
        /// Convert string to the snake case
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToSnakeCase(this string input)
        {
            return string.IsNullOrEmpty(input)
                ? input
                : Regex.Match(input, "^_+") + Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }

        public static string ToSpaceSeparatedString(this IEnumerable<string> list)
        {
            var enumerable = list as string[] ?? list.ToArray();
            if (!enumerable.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder(100);

            foreach (var element in enumerable)
            {
                sb.Append(element + " ");
            }

            return sb.ToString().Trim();
        }


        public static IEnumerable<string> FromSpaceSeparatedString(this string input)
        {
            input = input.Trim();
            return input.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public static List<string> ParseScopesString(this string scopes)
        {
            if (scopes.IsMissing())
            {
                return new List<string>();
            }

            scopes = scopes.Trim();
            var parsedScopes = scopes.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

            if (parsedScopes.Any())
            {
                parsedScopes.Sort();
            }

            return parsedScopes;
        }


        public static bool IsMissing(this string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static string GetThumbnailImage(this string fileName)
        {
            if (fileName.IsMissing()) return string.Empty;
            return Path.GetFileNameWithoutExtension(fileName) + "_thumbnail.png";
        }

        public static bool IsMissingOrTooLong(this string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            if (value.Length > maxLength)
            {
                return true;
            }

            return false;
        }


        public static bool IsPresent(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }


        public static string EnsureLeadingSlash(this string url)
        {
            if (!url.StartsWith("/"))
            {
                return "/" + url;
            }

            return url;
        }


        public static string EnsureTrailingSlash(this string url)
        {
            if (!url.EndsWith("/"))
            {
                return url + "/";
            }

            return url;
        }


        public static string RemoveLeadingSlash(this string url)
        {
            if (url.StartsWith("/"))
            {
                url = url[1..];
            }

            return url;
        }


        public static string RemoveTrailingSlash(this string url)
        {
            if (url.EndsWith("/"))
            {
                url = url[..^1];
            }

            return url;
        }


        public static string CleanUrlPath(this string url)
        {
            if (url.IsMissing()) url = "/";

            if (url != "/" && url.EndsWith("/"))
            {
                url = url[..^1];
            }

            return url;
        }

        public static bool IsIp(this string ip)
        {
            return Regex.IsMatch(ip,
                @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
        }


        public static bool IsLocalUrl(this string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            // Allows "/" or "/foo" but not "//" or "/\".
            if (url[0] == '/')
            {
                // url is exactly "/"
                if (url.Length == 1)
                {
                    return true;
                }

                // url doesn't start with "//" or "/\"
                if (url[1] != '/' && url[1] != '\\')
                {
                    return true;
                }

                return false;
            }

            // Allows "~/" or "~/foo" but not "~//" or "~/\".
            if (url[0] == '~' && url.Length > 1 && url[1] == '/')
            {
                // url is exactly "~/"
                if (url.Length == 2)
                {
                    return true;
                }

                // url doesn't start with "~//" or "~/\"
                if (url[2] != '/' && url[2] != '\\')
                {
                    return true;
                }

                return false;
            }

            return false;
        }


        public static string AddQueryString(this string url, string query)
        {
            if (!url.Contains("?"))
            {
                url += "?";
            }
            else if (!url.EndsWith("&"))
            {
                url += "&";
            }

            return url + query;
        }


        public static string AddQueryString(this string url, string name, string value)
        {
            return url.AddQueryString(name + "=" + UrlEncoder.Default.Encode(value));
        }


        public static string AddHashFragment(this string url, string query)
        {
            if (!url.Contains("#"))
            {
                url += "#";
            }

            return url + query;
        }


        public static string GetOrigin(this string url)
        {
            if (!url.IsMissing())
            {
                Uri uri;
                try
                {
                    uri = new Uri(url);
                }
                catch (Exception)
                {
                    return string.Empty;
                }

                if (uri.Scheme is "http" or "https")
                {
                    return $"{uri.Scheme}://{uri.Authority}";
                }
            }

            return string.Empty;
        }

        public static string Obfuscate(this string value)
        {
            var last4Chars = "****";
            if (value.IsPresent() && value.Length > 4)
            {
                last4Chars = value[^4..];
            }

            return "****" + last4Chars;
        }

        public static string Masking(this string value, int len = 20)
        {
            var maskedString = value;
            if (value.IsPresent() && value.Length > len)
            {
                maskedString = "**** " + value[^len..];
            }

            return maskedString;
        }

        /// <summary>
        /// Format the string with string.Format
        /// </summary>
        /// <param name="value"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Format(this string value, params object?[] args)
        {
            return string.Format(value, args);
        }
        public static bool IsAllowedFileExtension(this string fileName, IEnumerable<string> allowedExtensions)
        {
            return allowedExtensions.Any(e => e.Equals(fileName.GetFileExtension()));
        }

        public static bool IsAllowedMimeType(this string mimeType, IEnumerable<string> allowedMimeTypes)
        {
            return allowedMimeTypes.Any(m => m.Equals(mimeType));
        }

        public static string GetFileExtension(this string fileName)
        {
            if (!fileName.Contains('.'))
            {
                return fileName;
            }

            var arr = fileName.Split('.');
            return arr.Length > 1 ? $".{arr[^1]}" : fileName;
        }
    }
}