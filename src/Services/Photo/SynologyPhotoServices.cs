using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NetCoreLineBotSDK.Interfaces;
using Wedding.Data;

namespace Wedding.Services.Photo;

public class SynologyPhotoServices : IPhotoServices
{
    private readonly ILineMessageUtility _lineMessageUtility;
    private readonly IOptionsMonitor<LineBotSetting> _settings;
    private readonly Uri _baseAddress;
    private readonly string _sharingId;

    public SynologyPhotoServices(
        ILineMessageUtility lineMessageUtility,
        IOptionsMonitor<LineBotSetting> settings)
    {
        _lineMessageUtility = lineMessageUtility;
        var mathchs = new Regex(@"^(?<host>\S+)/sharing/(?<sharingId>\S+)").Match(settings.CurrentValue.UploadImage.ShareUrl);
        if(mathchs.Groups.TryGetValue("host", out var host))
        {
            _baseAddress = new Uri(host.Value);
        }
        if (mathchs.Groups.TryGetValue("sharingId", out var sharingId))
        {
            _sharingId = sharingId.Value;
        }
    }

    public async Task UploadImage(string messageId, string userId)
    {
        var username = (await _lineMessageUtility.GetUserProfile(userId).ConfigureAwait(false)).displayName;
        using var httpClient = new HttpClient
        {
            BaseAddress = _baseAddress
        };
        await httpClient.GetAsync($"/sharing/{_sharingId}").ConfigureAwait(false);
        var sharingIdContent = new StringContent(_sharingId, Encoding.UTF8);
        sharingIdContent.Headers.Remove("Content-Type");
        var usernameContent = new StringContent(username, Encoding.UTF8);
        usernameContent.Headers.Remove("Content-Type");
        using var content = new MultipartFormDataContent
        {

            { sharingIdContent, "\"sharing_id\"" },
            { usernameContent, "\"uploader_name\"" },
        };
        var fileStream = await _lineMessageUtility.GetContentBytesAsync(messageId).ConfigureAwait(false);
        var streamContent = new StreamContent(fileStream, (int)fileStream.Length);
        fileStream.Seek(0, SeekOrigin.Begin);
        streamContent.Headers.Remove("Content-Disposition");
        streamContent.Headers.TryAddWithoutValidation("Content-Disposition", $"form-data; name=\"file\"; filename=\"{messageId}.jpg\"");
        streamContent.Headers.Remove("Content-Type");
        streamContent.Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream");
        content.Add(streamContent);
        //  RFC 2046 format
        var boundaryValue = content.Headers.ContentType.Parameters.Single(p => p.Name == "boundary");
        boundaryValue.Value = boundaryValue.Value.Replace("\"", string.Empty);
        var response = await httpClient.PostAsync(
            $"/webapi/entry.cgi?api=SYNO.FileStation.Upload&method=upload&version=2&_sharing_id={_sharingId}", content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}

