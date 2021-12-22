using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.LineObject;
using NetCoreLineBotSDK.Models.Message;
using Newtonsoft.Json;

namespace Wedding.Data.ReplyIntent
{
    public abstract class AbstractReplyIntent : IReplyIntent
    {
        private static readonly Regex _templateNamePattern = new(@".+_(?<templateName>[\w]+)\.json");
        private protected static ILineMessageUtility LineMessageUtility;
        private readonly IFileProvider _fileProvider;

        protected AbstractReplyIntent(ILineMessageUtility lineMessageUtility, IFileProvider fileProvider)
        {
            LineMessageUtility = lineMessageUtility;
            _fileProvider = fileProvider;
        }

        public abstract Task ReplyAsync(LineEvent ev);

        /// <summary>
        /// 當未傳入 filterName 時，抓取資料夾下的所有訊息樣板
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filterName"></param>
        /// <returns></returns>
        protected async Task<List<IRequestMessage>> GetTemplateMessages(string path, string filterName = null)
        {
            using var intents = _fileProvider.GetDirectoryContents(path)
                .Where(x => string.IsNullOrWhiteSpace(filterName) || filterName.Equals(x.Name))
                .GetEnumerator();
            var messages = new List<IRequestMessage>();

            while (intents.MoveNext() && intents.Current is { IsDirectory: false })
            {
                var json = await File.ReadAllTextAsync(intents.Current.PhysicalPath).ConfigureAwait(false);
                switch (_templateNamePattern.Match(intents.Current.Name).Groups["templateName"].Value)
                {
                    case nameof(AudioMessage):
                        messages.Add(JsonConvert.DeserializeObject<AudioMessage>(json));
                        break;
                    case nameof(FlexMessage):
                        var template = JsonDocument.Parse(json).RootElement.EnumerateObject();
                        var altText = template.FirstOrDefault(c => c.Name.Equals("altText", StringComparison.OrdinalIgnoreCase)).Value.ToString();
                        var contents = template.FirstOrDefault(c => c.Name.Equals("contents", StringComparison.OrdinalIgnoreCase)).Value.ToString();
                        var quickReplyString = template.FirstOrDefault(c => c.Name.Equals("QuickReply", StringComparison.OrdinalIgnoreCase)).Value.ToString();
                        var quickReplay = quickReplyString?.Length > 0 ? JsonConvert.DeserializeObject<QuickReply>(quickReplyString) : null;
                        messages.Add(new FlexMessage(contents, altText, quickReplay));
                        break;
                    case nameof(ImageMessage):
                        messages.Add(JsonConvert.DeserializeObject<ImageMessage>(json));
                        break;
                    case nameof(LocationMessage):
                        messages.Add(JsonConvert.DeserializeObject<LocationMessage>(json));
                        break;
                    case nameof(StickerMessage):
                        messages.Add(JsonConvert.DeserializeObject<StickerMessage>(json));
                        break;
                    case nameof(TextMessage):
                        messages.Add(JsonConvert.DeserializeObject<TextMessage>(json));
                        break;
                    case nameof(VideoMessage):
                        messages.Add(JsonConvert.DeserializeObject<VideoMessage>(json));
                        break;
                    case nameof(ButtonTemplate):
                        messages.Add(JsonConvert.DeserializeObject<ButtonTemplate>(json));
                        break;
                    case nameof(CarouselTemplate):
                        messages.Add(JsonConvert.DeserializeObject<CarouselTemplate>(json));
                        break;
                    case nameof(ConfirmTemplate):
                        messages.Add(JsonConvert.DeserializeObject<ConfirmTemplate>(json));
                        break;
                    case nameof(ImageCarouselTemplate):
                        messages.Add(JsonConvert.DeserializeObject<ImageCarouselTemplate>(json));
                        break;
                }
            }

            return messages;
        }

        /// <summary>
        /// 嘗試拿訊息樣板，沒拿到時會回傳文字
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="value">可傳入多筆樣板，藉由 ; 來分隔</param>
        /// <param name="templateFolderPath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected async Task TryGetTemplateMessageAsync(LineEvent ev, string value, string templateFolderPath)
        {
            if (ev == null)
            {
                throw new ArgumentNullException(nameof(ev));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (templateFolderPath == null)
            {
                throw new ArgumentNullException(nameof(templateFolderPath));
            }


            var messages = new List<IRequestMessage>();
            foreach (var item in value.Split(';'))
            {
                if (item.StartsWith("Intent_") && item.EndsWith(".json"))
                {
                    // Intent_*_StickerMessage.json mean try get template from folder(templateFolderPath).
                    var template = await GetTemplateMessages(templateFolderPath, item).ConfigureAwait(false);
                    if (template?.Any() == true)
                    {
                        messages.AddRange(template);
                    }
                }
            }

            // if template file not found, will return text message.
            await LineMessageUtility.ReplyMessageAsync(ev.replyToken,
                messages.Count > 0 ? messages
                    : new List<IRequestMessage> { new TextMessage(value) }).ConfigureAwait(false);
            await LineMessageUtility.ReplyMessageAsync(ev.replyToken, value).ConfigureAwait(false);
        }
    }
}