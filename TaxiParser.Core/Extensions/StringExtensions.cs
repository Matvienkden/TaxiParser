using System.Linq;
using System.Text.RegularExpressions;

namespace TaxiParser.Core.Extensions
{
    public static class StringExtensions
    {
        public static string FioFormat(this string[] fio)
        {
            //  var a = new CyrName().DeclineSurnameDative(fio[0], (int)CasesEnum.Nominative, false);

            fio = fio.Select(w => w.Substring(0, 1).ToUpper() + w.Substring(1, w.Length - 1).ToLower()).ToArray();
            var f = fio.Select(w => w.DativusToNominativus());
            return string.Join(" ", f);
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
                    return e.ReplaceRegex("ой$", "ая");
                case var e when e.EndsWith("жей"):
                    return e.ReplaceRegex("ей$", "ая");
                case var e when e.EndsWith("ой"):
                    return e.ReplaceRegex("ой$", "а");
                case var e when e.EndsWith("кому"):
                    return e.ReplaceRegex("ому", "ий");
                case var e when e.EndsWith("ому"):
                    return e.ReplaceRegex("ому", "ый");
                case var e when e.EndsWith("у"):
                    return e.Remove(e.Length - 1);
                case var e when e.RegexIsMatch("[ауоыиэяюёе]ю$"):
                    return e.ReplaceRegex("ю$", "й");
                case var e when e.EndsWith("ю"):
                    return e.ReplaceRegex("ю$", "ь");
                case var e when e.EndsWith("ье"):
                    return e.ReplaceRegex("ье$", "я");
                case var e when e.EndsWith("е"):
                    return e.ReplaceRegex("е$", "а");
                case var e when e.EndsWith("ии"):
                    return e.ReplaceRegex("ии$", "ия");
                default: return str;
            }
        }

        public static string ReplaceRegex(this string str, string a, string b)
        {
            return Regex.Replace(str, a, b);
        }

        public static string DeleteExtraSpaces(this string str)
        {
            return str.ReplaceRegex(@"\s+", " ").Trim();
        }

        public static bool RegexIsMatch(this string str, string pattern)
        {
            return new Regex(pattern).IsMatch(str);
        }
    }
}
