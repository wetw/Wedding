using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models;
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
        private const string TemplateFolderPath = "IntentMessages";

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
        protected async Task<IList<IRequestMessage>> GetTemplateMessages(string path, string filterName = null, UserProfile userProfile = null)
        {
            using var intents = _fileProvider.GetDirectoryContents(path)
                .Where(x => string.IsNullOrWhiteSpace(filterName) || filterName.Equals(x.Name))
                .GetEnumerator();
            var messages = new List<IRequestMessage>();

            while (intents.MoveNext() && intents.Current is { IsDirectory: false })
            {
                var jsonText = await File.ReadAllTextAsync(intents.Current.PhysicalPath).ConfigureAwait(false);
                jsonText = LineUserNameFormat(jsonText, userProfile);
                switch (_templateNamePattern.Match(intents.Current.Name).Groups["templateName"].Value)
                {
                    case nameof(AudioMessage):
                        messages.Add(JsonConvert.DeserializeObject<AudioMessage>(jsonText));
                        break;
                    case nameof(FlexMessage):
                        var template = JsonDocument.Parse(jsonText).RootElement.EnumerateObject();
                        var altText = template.FirstOrDefault(c => c.Name.Equals("altText", StringComparison.OrdinalIgnoreCase)).Value.ToString();
                        var contents = template.FirstOrDefault(c => c.Name.Equals("contents", StringComparison.OrdinalIgnoreCase)).Value.ToString();
                        var quickReplyString = template.FirstOrDefault(c => c.Name.Equals("QuickReply", StringComparison.OrdinalIgnoreCase)).Value.ToString();
                        var quickReplay = quickReplyString?.Length > 0 ? JsonConvert.DeserializeObject<QuickReply>(quickReplyString) : null;
                        messages.Add(new FlexMessage(contents, altText, quickReplay));
                        break;
                    case nameof(ImageMessage):
                        messages.Add(JsonConvert.DeserializeObject<ImageMessage>(jsonText));
                        break;
                    case nameof(LocationMessage):
                        messages.Add(JsonConvert.DeserializeObject<LocationMessage>(jsonText));
                        break;
                    case nameof(StickerMessage):
                        messages.Add(JsonConvert.DeserializeObject<StickerMessage>(jsonText));
                        break;
                    case nameof(TextMessage):
                        messages.Add(JsonConvert.DeserializeObject<TextMessage>(jsonText));
                        break;
                    case nameof(VideoMessage):
                        messages.Add(JsonConvert.DeserializeObject<VideoMessage>(jsonText));
                        break;
                    case nameof(ButtonTemplate):
                        messages.Add(JsonConvert.DeserializeObject<ButtonTemplate>(jsonText));
                        break;
                    case nameof(CarouselTemplate):
                        messages.Add(JsonConvert.DeserializeObject<CarouselTemplate>(jsonText));
                        break;
                    case nameof(ConfirmTemplate):
                        messages.Add(JsonConvert.DeserializeObject<ConfirmTemplate>(jsonText));
                        break;
                    case nameof(ImageCarouselTemplate):
                        messages.Add(JsonConvert.DeserializeObject<ImageCarouselTemplate>(jsonText));
                        break;
                }
            }

            return messages.Any() ? messages : new List<IRequestMessage> { new TextMessage(LineUserNameFormat(filterName, userProfile)) };
        }

        /// <summary>
        /// 嘗試拿訊息樣板，沒拿到時會回傳文字
        /// </summary>
        /// <param name="ev" cref="LineEvent"></param>
        /// <param name="replyObject" cref="ReplyObject"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected async Task TryGetTemplateMessageAsync(LineEvent ev, ReplyObject replyObject)
        {
            if (ev == null)
            {
                throw new ArgumentNullException(nameof(ev));
            }

            if (replyObject == null)
            {
                throw new ArgumentNullException(nameof(replyObject));
            }

            var userProfile = await LineMessageUtility.GetUserProfile(ev.source.userId).ConfigureAwait(false);

            var messages = new List<IRequestMessage>();
            switch (replyObject.Type)
            {
                case ReplyType.Continue:
                    {
                        foreach (var item in replyObject.Templates)
                        {
                            var template = await GetTemplateMessages(TemplateFolderPath, item, userProfile).ConfigureAwait(false);
                            if (template?.Any() == true && messages.Count < 6)
                            {
                                messages.AddRange(template);
                            }
                        }

                        break;
                    }
                case ReplyType.Random:
                    {
                        var templates = replyObject.Templates.GetRandomSelection(replyObject.RandomNum);
                        foreach (var item in templates)
                        {
                            var template = await GetTemplateMessages(TemplateFolderPath, item, userProfile).ConfigureAwait(false);
                            if (template?.Any() == true && messages.Count < 6)
                            {
                                messages.AddRange(template);
                            }
                        }

                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // if template file not found, will return text message.
            await LineMessageUtility.ReplyMessageAsync(ev.replyToken,
                messages.Count > 0 ? messages
                    : new List<IRequestMessage> { new TextMessage(string.Join(',', replyObject.Templates)) }).ConfigureAwait(false);
        }

        private static string LineUserNameFormat(string jsonText, UserProfile userProfile = null)
        {
            if (!string.IsNullOrWhiteSpace(jsonText) && userProfile is not null)
            {
                jsonText = jsonText.Replace("${username}", userProfile.displayName);
            }

            return jsonText;
        }
    }

    public static class Extensions
    {
        public static IEnumerable<T> GetRandomSelection<T>(this IEnumerable<T> array, int count = 1)
        {
            var maxLength = array.Count();

            var randomSelection = new List<T>();
            var random = new Random();
            for (var i = 0; i < count; i++)
            {
                var randomInterval = random.Next(0, maxLength);
                randomSelection.Add(array.ElementAt(randomInterval));
            }

            return randomSelection;
        }
    }
}