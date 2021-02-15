using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using TaxiParser.Core.Enums;
using TaxiParser.Core.Extensions;
using TaxiParser.Core.Logger;
using TaxiParser.Core.Models.Licenses;

namespace TaxiParser.Core.Parsers
{
    public abstract class ParserBase<TLicense>
        where TLicense : ILicense
    {
        protected ILogger logger { get; }
        protected Uri basicUrl { get; }
        protected string filePath { get; }
        protected SaveTo saveTo { get; }

        public ParserBase(ILogger logger, SaveTo saveTo)
        {
            this.logger = logger;
            this.saveTo = saveTo;

            var region = typeof(TLicense).ToString().GetStrByRegexGroup(@".(\w+)License$");

            var folderPath = ConfigurationManager.AppSettings.Get("ResultsFolderPath");

            if (string.IsNullOrEmpty(folderPath))
            {
                throw new ConfigurationErrorsException($"В файле конфигурации не задан параметр ResultsFolderPath (Путь к папке для сохранения результата)");
            }

            if (folderPath.Intersect(Path.GetInvalidFileNameChars()).Any())
            {
                throw new ConfigurationErrorsException($"В файле конфигурации параметр ResultsFolderPath содержит недопустимые символы");
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            filePath = $"{folderPath}/{region}.json";

            var targetUriString = ConfigurationManager.AppSettings.Get($"{region}TargetUrl");

            if (string.IsNullOrEmpty(targetUriString))
            {
                throw new ConfigurationErrorsException($"В файле конфигурации не задан параметр {region}TargetUrl (нужна ссылка на ресурс для парсинга)");
            }

            if (Uri.TryCreate(targetUriString, UriKind.Absolute, out var url) == false)
            {
                throw new ConfigurationErrorsException($"В конфиге прописан некорретный url (параметр {region}TargetUrl)");
            }
            basicUrl = url;

            InitStorage();
        }

        public void Start()
        {
            while (true)
            {
                try
                {
                    logger.Log("Запуск");

                    GoParsing();

                    logger.Log("парсинк успешно завершён", LogLevel.SUCCSESS);
                }
                catch (Exception e)
                {
                    logger.Log(e.Message, LogLevel.ERROR);
                }

                logger.Log("\r\nСледующий запуск через 5 минут");
                Thread.Sleep(5 * 60 * 1000);
            }
        }

        protected abstract void GoParsing();

        protected void Save(TLicense license)
        {
            using (var file = File.AppendText(filePath))
            {
                var option = new JsonSerializerSettings();
                option.Formatting = Formatting.Indented;
                var js = JsonSerializer.Create(option);
                js.Serialize(file, license);
                file.WriteLine(",");
            }
        }

        protected void Save(IEnumerable<TLicense> licenses)
        {
            foreach (var item in licenses)
            {
                Save(item);
            }
        }

        protected void InitStorage()
        {
            switch (saveTo)
            {
                case SaveTo.File:
                    if (File.Exists(filePath))
                    {
                        logger.Log("\nУдалим существующий файл с результатами");
                        File.Delete(filePath);
                        logger.Log("Успешно удалён\r\n", LogLevel.SUCCSESS);
                    }
                    break;
                case SaveTo.Database:
                    throw new NotImplementedException();
                default: throw new ArgumentException(nameof(saveTo));
            }
        }
    }
}
