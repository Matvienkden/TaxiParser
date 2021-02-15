using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using TaxiParser.Core.Enums;
using TaxiParser.Core.Extensions;
using TaxiParser.Core.Logger;
using TaxiParser.Core.Models.Licenses;

namespace TaxiParser.Core.Parsers
{
    /// <summary>
    /// Парсер такси для Воронежской области
    /// </summary>
    public sealed class VoronezhParser : ParserBase<VoronezhLicense>
    {
        private int totalPage { get; set; }
        private string dtid { get; set; }
        private string uuid { get; set; }
        private string sessionId { get; set; }

        private void GetTechData()
        {
            var request = WebRequest.Create(new Uri(basicUrl, "zul/permitDocs.zul"));
            var response = request.GetResponse();
            var doc = new HtmlDocument();
            doc.Load(response.GetResponseStream());

            dtid = doc.Text.GetStrByRegexGroup("dt:'(.*)',cu");
            uuid = doc.Text.GetStrByRegexGroup("Paging','(.*)',{");
            sessionId = response.Headers.Get("Set-Cookie").Split(';').FirstOrDefault();
            totalPage = int.Parse(doc.Text.GetStrByRegexGroup(@"pageCount:(\d+)"));
        }

        public VoronezhParser(ILogger logger, SaveTo saveTo) : base(logger, saveTo) { }

        protected override void GoParsing()
        {
            logger.Log("\nОпределим кол-во страниц и полчим айдишники для парсинга");
            GetTechData();
            logger.Log($"Успешно. Всего страниц: {totalPage}", LogLevel.SUCCSESS);

            logger.Log("\r\nЗапускаем парсинг страниц\r\n");

            var resultCount = 0;
            for (int i = 1; i <= totalPage; i++)
            {
                try
                {
                    var results = GetAndParsingOnePage(i);
                    resultCount += results.Count();
                    Save(results);

                    logger.Log($"Всего получено записей записей: {resultCount}");
                }
                catch (Exception e)
                {
                    logger.Log(e.Message, LogLevel.ERROR);
                }
            }

            logger.Log("\r\nВсе страницы успешно обработаны", LogLevel.SUCCSESS);
        }

        private IEnumerable<VoronezhLicense> GetAndParsingOnePage(int pageNumber)
        {
            logger.Log($"Грузим страницу {pageNumber} из {totalPage}");

            var body = $"dtid={dtid}&cmd_0=onPaging&uuid_0={uuid}&data_0=" + HttpUtility.UrlEncode($"{{\"\":{pageNumber}}}");
            var bodyBytes = Encoding.Default.GetBytes(body);
            
            var pageRequest = (HttpWebRequest)WebRequest.Create(new Uri(basicUrl, "zkau"));
            pageRequest.Method = "POST";
            pageRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            pageRequest.Headers.Set(HttpRequestHeader.Cookie, sessionId);

            var requestStream = pageRequest.GetRequestStream();
            requestStream.Write(bodyBytes, 0, bodyBytes.Length);
            requestStream.Close();

            var responseStr = pageRequest.GetResponse()
                .GetStrFromResponseStream();

            logger.Log($"Страница {pageNumber} получена, начинам парсинг", LogLevel.SUCCSESS);

            var licenses = Regex.Split(responseStr, @"zul\.box\.Box").Skip(1);

            var tempResults = new List<VoronezhLicense>();
            foreach (var license in licenses)
            {
                var licensesValues = new Regex(@"value:'(.*)'}|,{},\[]],").Matches(license);

                if (licensesValues.Count != 12)
                {
                    throw new HttpParseException("По каждой записи должно прийти 12 полей, но что-то пошло не так.");
                }

                // если пришла запись по которой нет гос.номера, то перейдём к следующей записи
                if (licensesValues[6].Groups[1].Value == "-" || licensesValues[6].Groups[1].Value == ".")
                {
                    continue;
                }

                var vendor = licensesValues[7].Groups[1].Value.GetStrByRegexGroup(@"^(\w+)");

                var ogrn = "";
                var ogrnip = "";
                if (licensesValues[9].Groups[1].Value.Length == 13)
                {
                    ogrn = licensesValues[9].Groups[1].Value;
                }
                else
                {
                    ogrnip = licensesValues[9].Groups[1].Value;
                }

                var fio = "";
                var company = "";
                if (ogrnip != "")
                {
                    fio = licensesValues[10].Groups[1].Value.RegexReplace(@"^ип", "", RegexOptions.IgnoreCase)
                        .FioFormat();
                }
                else
                {
                    company = licensesValues[10].Groups[1].Value.DeleteExtraSpaces();
                }

                var result = new VoronezhLicense
                {
                    LicenseState = licensesValues[0].Groups[1].Value,
                    LicenseNumber = licensesValues[1].Groups[1].Value,
                    LicenseRegNumber = licensesValues[2].Groups[1].Value,
                    LicenseStart = DateTime.TryParse(licensesValues[3].Groups[1].Value, out var date) ? date : (DateTime?)null,
                    LicenseOrderNumber = licensesValues[4].Groups[1].Value,
                    LicenseEnd = DateTime.TryParse(licensesValues[5].Groups[1].Value, out date) ? date : (DateTime?)null,
                    CarNumber = licensesValues[6].Groups[1].Value.RegexReplace(@"\D+$|\s", ""),
                    CarVendor = vendor,
                    CarModel = licensesValues[7].Groups[1].Value.ModelFormat(vendor),
                    CarYear = int.Parse(licensesValues[8].Groups[1].Value),
                    OwnerOgrn = ogrn,
                    OwnerOgrnip = ogrnip,
                    OwnerFio = fio,
                    OwnerCompanyName = company,
                    OwnerAddress = licensesValues[11].Groups[1].Value.GetStrByRegexGroup(@"^(.*)\bтел\b"),
                    OwnerPhone = licensesValues[11].Groups[1].Value.GetStrByRegexGroup(@"\bтел\b([\d\s-\/]*)")
                        .RegexReplace(@"\s|-", ""),
                    OwnerContacts = licensesValues[11].Groups[1].Value
                };

                tempResults.Add(result);
            }

            logger.Log($"Успешно, получено записей со страницы: {tempResults.Count()}");
            return tempResults;
        }
    }
}
