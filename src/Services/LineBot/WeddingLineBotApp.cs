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
        private readonly OnFollowIntent _onFollowIntent;
        private readonly OnMessageIntent _onMessageIntent;
        private readonly OnPostbackIntent _onPostbackIntent;
        private readonly ICustomerDao _customerDao;
        private readonly IPhotoServices _photoServices;
        private readonly ILineMessageUtility _lineMessageUtility;

        public WeddingLineBotApp(
            ILineMessageUtility lineMessageUtility,
            IOptionsMonitor<LineBotSetting> settings,
            OnFollowIntent onFollowIntent,
            OnMessageIntent onMessageIntent,
            OnPostbackIntent onPostbackIntent,
            ICustomerDao customerDao,
            IPhotoServices photoServices) : base(lineMessageUtility)
        {
            _lineMessageUtility = lineMessageUtility;
            _settings = settings;
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


        protected override Task OnPostbackAsync(LineEvent ev) =>
            _onPostbackIntent.ReplyAsync(ev);

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

        protected override async Task OnBeaconAsync(LineEvent ev)
        {
            if (_settings.CurrentValue.Beacon.Enabled
                && ev.beacon.type == BeaconType.Enter)
            {
                if (string.IsNullOrWhiteSpace(_settings.CurrentValue.Beacon.EnterHwid)
                    || _settings.CurrentValue.Beacon.EnterHwid.Equals(ev.beacon.hwid, StringComparison.OrdinalIgnoreCase))
                {
                    await BeaconEnterReply(ev).ConfigureAwait(false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_settings.CurrentValue.Beacon.LeaveHwid)
                       || _settings.CurrentValue.Beacon.LeaveHwid.Equals(ev.beacon.hwid, StringComparison.OrdinalIgnoreCase))
                {
                    await BeaconLeaveReply(ev).ConfigureAwait(false);
                }
            }
        }

        private async Task BeaconEnterReply(LineEvent ev)
        {
            var user = await GetUserAndTryAdd(ev).ConfigureAwait(false);
            if (user.IsSignIn)
            {
                return;
            }

            try
            {
                ev.postback = new PostBack();
                switch (user!.Table)
                {
                    case null:
                    case "":
                        ev.postback.data = "沒填寫問券的";
                        await _onPostbackIntent.ReplyAsync(ev).ConfigureAwait(false);
                        break;
                    default:
                        ev.postback.data = $"桌號{user.Table}";
                        await _onPostbackIntent.ReplyAsync(ev).ConfigureAwait(false);
                        break;
                }
            }
            finally
            {
                user.IsSignIn = true;
                await _customerDao.UpdateAsync(user, user.LineId).ConfigureAwait(false);
            }
        }

        private async Task BeaconLeaveReply(LineEvent ev)
        {
            var user = await GetUserAndTryAdd(ev).ConfigureAwait(false);
            if (user.IsLeave)
            {
                return;
            }

            try
            {
                ev.postback = new PostBack { data = "離開的感謝提醒" };
                await _onPostbackIntent.ReplyAsync(ev).ConfigureAwait(false);
            }
            finally
            {
                user.IsLeave = true;
                await _customerDao.UpdateAsync(user, user.LineId).ConfigureAwait(false);
            }
        }

        private async Task<Customer> GetUserAndTryAdd(LineEvent ev)
        {
            var user = await _customerDao.GetByLineIdAsync(ev.source.userId).ConfigureAwait(false);
            if (user == null)
            {
                var userProfile = (await _lineMessageUtility.GetUserProfile(ev.source.userId).ConfigureAwait(false))
                    .ToCustomer();
                user = await _customerDao.AddAsync(userProfile).ConfigureAwait(false);
            }
            return user;
        }
    }
}