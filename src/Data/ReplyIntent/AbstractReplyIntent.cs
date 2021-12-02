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

        protected async Task<List<IRequestMessage>> GetTemplateMessages(string path, string filterName = null)
        {
            using var intents = _fileProvider.GetDirectoryContents(path).GetEnumerator();
            var messages = new List<IRequestMessage>();
            while (intents.MoveNext())
            {
                if (intents.Current is { IsDirectory: false })
                {
                    if (!string.IsNullOrWhiteSpace(filterName)
                        && !filterName.Equals(intents.Current.Name))
                    {
                        continue;
                    }
                    var json = await File.ReadAllTextAsync(intents.Current.PhysicalPath).ConfigureAwait(false);
                    switch (_templateNamePattern.Match(intents.Current.Name).Groups["templateName"].Value)
                    {
                        case nameof(AudioMessage):
                            messages.Add(JsonConvert.DeserializeObject<AudioMessage>(json));
                            break;
                        case nameof(FlexMessage):
                            var template = JsonDocument.Parse(json).RootElement.EnumerateObject();
                            var altText= template.FirstOrDefault(c => c.Name.Equals("altText", StringComparison.OrdinalIgnoreCase)).Value.ToString();
                            var contents = template.FirstOrDefault(c => c.Name.Equals("contents", StringComparison.OrdinalIgnoreCase)).Value.ToString();
                            messages.Add(new FlexMessage(contents, altText));
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
            }

            return messages;
        }

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

            // Intent_*_StickerMessage.json mean try get template from folder(templateFolderPath).
            if (value.StartsWith("Intent_") && value.EndsWith(".json"))
            {
                var messages = await GetTemplateMessages(templateFolderPath, value).ConfigureAwait(false);
                // if template file not found, will return text message.
                await LineMessageUtility.ReplyMessageAsync(ev.replyToken,
                    messages.Count > 0 ? messages
                        : new List<IRequestMessage> { new TextMessage(value) }).ConfigureAwait(false);
            }
            await LineMessageUtility.ReplyMessageAsync(ev.replyToken, value).ConfigureAwait(false);
        }
    }
}