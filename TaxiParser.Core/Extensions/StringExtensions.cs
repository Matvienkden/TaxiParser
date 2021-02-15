using System.Linq;
using System.Text.RegularExpressions;

namespace TaxiParser.Core.Extensions
{
    public static class StringExtensions
    {
        public static string FioFormat(this string[] fio, bool needDecline = false)
        {
            fio = fio.Select(w => w.Substring(0, 1).ToUpper() + w.Substring(1, w.Length - 1).ToLower()).ToArray();
            if (needDecline)
            {
                return string.Join(" ", fio.Select(w => w.DativusToNominativus()));
            }
            return string.Join(" ", fio);
        }

        public static string FioFormat(this string fio, bool needDecline = false)
        {
            return FioFormat(fio.DeleteExtraSpaces().Split(' '));
        }

        public static string ModelFormat(this string model, string vendor)
        {
            var modelArr = model.DeleteExtraSpaces()
                .Split(' ')
                .Distinct()
                .Where(w => w.ToLower() != vendor.ToLower() && w != "VAZ" && w != "GAZ");
            return string.Join(" ", modelArr);
        }

        /// <summary>
        /// Склонение имени/фамилии/отчества из дательного падежа в именительный
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DativusToNominativus(this string str)
        {
            switch (str)
            {
                case var e when e.EndsWith("ко") || e.EndsWith("ук") || e.EndsWith("дзе") || e.EndsWith("ян"):
                    return e;
                case "Павлу": return "Павел";
                case "Любови": return "Любовь";
                case var e when e.EndsWith("кой"):
                    return e.RegexReplace("ой$", "ая");
                case var e when e.EndsWith("жей"):
                    return e.RegexReplace("ей$", "ая");
                case var e when e.EndsWith("ой"):
                    return e.RegexReplace("ой$", "а");
                case var e when e.EndsWith("кому"):
                    return e.RegexReplace("ому", "ий");
                case var e when e.EndsWith("ому"):
                    return e.RegexReplace("ому", "ый");
                case var e when e.EndsWith("у"):
                    return e.Remove(e.Length - 1);
                case var e when e.RegexIsMatch("[ауоыиэяюёе]ю$"):
                    return e.RegexReplace("ю$", "й");
                case var e when e.EndsWith("ю"):
                    return e.RegexReplace("ю$", "ь");
                case var e when e.EndsWith("ье"):
                    return e.RegexReplace("ье$", "я");
                case var e when e.EndsWith("е"):
                    return e.RegexReplace("е$", "а");
                case var e when e.EndsWith("ии"):
                    return e.RegexReplace("ии$", "ия");
                default: return str;
            }
        }

        public static string RegexReplace(this string str, string a, string b, RegexOptions options = RegexOptions.None)
        {
            return Regex.Replace(str, a, b, options);
        }

        public static string DeleteExtraSpaces(this string str)
        {
            return str.RegexReplace(@"\s+", " ").Trim();
        }

        public static bool RegexIsMatch(this string str, string pattern)
        {
            return new Regex(pattern).IsMatch(str);
        }

        public static string GetStrByRegexGroup(this string str, string pattern, int groupIndex = 1)
        {
            return new Regex(pattern)
                .Match(str).Groups[groupIndex]
                .ToString();
        }

        public static GroupCollection GetRegexGroups(this string str, string pattern)
        {
            return new Regex(pattern)
                .Match(str).Groups;
        }
    }
}
