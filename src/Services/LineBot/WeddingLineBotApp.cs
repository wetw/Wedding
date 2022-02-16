using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NetCoreLineBotSDK;
using NetCoreLineBotSDK.Enums;
using NetCoreLineBotSDK.Interfaces;
using NetCoreLineBotSDK.Models.LineObject;
using Wedding.Data;
using Wedding.Data.ReplyIntent;
using Wedding.Services.Photo;

namespace Wedding.Services.LineBot
{
    public class WeddingLineBotApp : LineBotApp
    {
        private readonly IOptionsMonitor<LineBotSetting> _settings;
        private readonly OnBeaconIntent _onBeaconIntent;
        private readonly OnFollowIntent _onFollowIntent;
        private readonly OnMessageIntent _onMessageIntent;
        private readonly OnPostbackIntent _onPostbackIntent;
        private readonly ICustomerDao _customerDao;
        private readonly IPhotoServices _photoServices;

        public WeddingLineBotApp(
            ILineMessageUtility lineMessageUtility,
            IOptionsMonitor<LineBotSetting> settings,
            OnBeaconIntent onBeaconIntent,
            OnFollowIntent onFollowIntent,
            OnMessageIntent onMessageIntent,
            OnPostbackIntent onPostbackIntent,
            ICustomerDao customerDao,
            IPhotoServices photoServices) : base(lineMessageUtility)
        {
            _settings = settings;
            _onBeaconIntent = onBeaconIntent;
            _onFollowIntent = onFollowIntent;
            _onMessageIntent = onMessageIntent;
            _onPostbackIntent = onPostbackIntent;
            _customerDao = customerDao;
            _photoServices = photoServices;
        }

        protected override async Task OnFollowAsync(LineEvent ev)
        {
            await _onFollowIntent.ReplyAsync(ev).ConfigureAwait(false);
        }

        protected override async Task OnMessageAsync(LineEvent ev)
        {
            switch (ev.message.Type)
            {
                case LineMessageType.Text:
                    // HACK: 先暫時寫死，後續改為參數方式
                    if (_settings.CurrentValue.CustomerMessage.AttendMessage.Enabled && ev.message.Text.Equals("我填好了"))
                    {
                        if (await _customerDao.GetByLineIdAsync(ev.source.userId).ConfigureAwait(false) is { } user)
                        {
                            ev.postback = new PostBack();
                            switch (user.IsAttend)
                            {
                                case null:
                                    ev.postback.data = "尚未填寫";
                                    await _onPostbackIntent.ReplyAsync(ev).ConfigureAwait(false);
                                    break;
                                default:
                                    {
                                        if (user.IsAttend.Value)
                                        {
                                            ev.postback.data = "我要參加";
                                            await _onPostbackIntent.ReplyAsync(ev).ConfigureAwait(false);
                                        }
                                        else if (!user.IsAttend.Value)
                                        {
                                            ev.postback.data = "我不能參加";
                                            await _onPostbackIntent.ReplyAsync(ev).ConfigureAwait(false);
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        await _onMessageIntent.ReplyAsync(ev).ConfigureAwait(false);
                    }
                    break;
                case LineMessageType.Image:
                    if (_settings.CurrentValue.UploadImage.Enabled && !string.IsNullOrEmpty(_settings.CurrentValue.UploadImage.ShareUrl))
                    {
                        await _photoServices.UploadImage(ev.message.Id, ev.source.userId).ConfigureAwait(false);
                    }
                    break;
                case LineMessageType.Sticker:
                case LineMessageType.Imagemap:
                case LineMessageType.Video:
                    if (_settings.CurrentValue.UploadImage.Enabled && !string.IsNullOrEmpty(_settings.CurrentValue.UploadImage.ShareUrl))
                    {
                        await _photoServices.UploadVideo(ev.message.Id, ev.source.userId).ConfigureAwait(false);
                    }
                    break;
                case LineMessageType.Audio:
                case LineMessageType.Template:
                case LineMessageType.Flex:
                case LineMessageType.Location:
                default:
                    return;
            }

        }

        protected override Task OnBeaconAsync(LineEvent ev)
        {
            if (_settings.CurrentValue.Beacon.Enabled && ev.beacon.type == BeaconType.Enter)
            {
                //var messageConfig = _settings.CurrentValue.CustomerMessage.WelcomeMessage;
                //if (messageConfig.Enabled 
                //    && DateTime.UtcNow.TimeOfDay > messageConfig.WelcomeBeforeUtcTime)
                //{
                //    if (messageConfig.EnabledDaily)
                //    {

                //    }

                //}
                return _onBeaconIntent.ReplyAsync(ev);
            }

            return Task.CompletedTask;
        }

        protected override Task OnPostbackAsync(LineEvent ev) =>
            _onPostbackIntent.ReplyAsync(ev);
    }
}