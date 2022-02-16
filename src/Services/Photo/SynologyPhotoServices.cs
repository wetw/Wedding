using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MimeSharp;
using NetCoreLineBotSDK.Interfaces;
using Wedding.Data;

namespace Wedding.Services.Photo;

public class SynologyPhotoServices : IPhotoServices
{
    private static readonly int MimeSampleSize = 256;
    private static readonly string DefaultMimeType = "application/octet-stream";
    private static readonly string ContentType = "Content-Type";
    private static readonly string ContentDisposition = "Content-Disposition";
    private readonly ILineMessageUtility _lineMessageUtility;
    private readonly string _sharingId;
    private readonly HttpClient _httpClient;

    public SynologyPhotoServices(
        ILineMessageUtility lineMessageUtility,
        IOptionsMonitor<LineBotSetting> settings,
        HttpClient httpClient)
    {
        _lineMessageUtility = lineMessageUtility;
        var mathchs = new Regex(@"^(?<host>\S+)/sharing/(?<sharingId>\S+)").Match(settings.CurrentValue.UploadImage.ShareUrl);
        if (mathchs.Groups.TryGetValue("host", out var host))
        {
            httpClient.BaseAddress = new Uri(host.Value);
        }
        if (mathchs.Groups.TryGetValue("sharingId", out var sharingId))
        {
            _sharingId = sharingId.Value;
        }
        _httpClient = httpClient;
    }

    public async Task Upload(string messageId, string userId, string defaultExtension)
    {
        var username = (await _lineMessageUtility.GetUserProfile(userId).ConfigureAwait(false)).displayName;
        var sharingIdContent = new StringContent(_sharingId, Encoding.UTF8);
        sharingIdContent.Headers.Remove(ContentType);
        var usernameContent = new StringContent(username, Encoding.UTF8);
        usernameContent.Headers.Remove(ContentType);
        using var content = new MultipartFormDataContent
        {
            { sharingIdContent, "\"sharing_id\"" },
            { usernameContent, "\"uploader_name\"" }
        };
        var fileStream = await _lineMessageUtility.GetContentBytesAsync(messageId).ConfigureAwait(false);
        var streamContent = new StreamContent(fileStream, (int)fileStream.Length);
        var filename = $"{messageId}.{GetExtensionFromBytes(fileStream, defaultExtension)}";
        fileStream.Seek(0, SeekOrigin.Begin);
        streamContent.Headers.Remove(ContentDisposition);
        streamContent.Headers.TryAddWithoutValidation(ContentDisposition, $"form-data; name=\"file\"; filename=\"{filename}\"");
        streamContent.Headers.Remove(ContentType);
        streamContent.Headers.TryAddWithoutValidation(ContentType, DefaultMimeType);
        content.Add(streamContent);
        //  RFC 2046 format
        var boundaryValue = content.Headers.ContentType.Parameters.Single(p => p.Name == "boundary");
        boundaryValue.Value = boundaryValue.Value.Replace("\"", string.Empty);
        await _httpClient.GetAsync($"/sharing/{_sharingId}").ConfigureAwait(false);
        var response = await _httpClient.PostAsync(
            $"/webapi/entry.cgi?api=SYNO.FileStation.Upload&method=upload&version=2&_sharing_id={_sharingId}", content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public Task UploadImage(string messageId, string userId) => Upload(messageId, userId, "jpg");

    public Task UploadVideo(string messageId, string userId) => Upload(messageId, userId, "mp4");

    [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
    static extern int FindMimeFromData(
        IntPtr pBC,
        [MarshalAs(UnmanagedType.LPWStr)]
        string pwzUrl,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1, SizeParamIndex = 3)]
        byte[] pBuffer,
        int cbSize,
        [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
        int dwMimeFlags,
        out IntPtr ppwzMimeOut,
        int dwReserved);

    public static string GetMimeFromBytes(Stream stream)
    {
        try
        {
            var data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            stream.Seek(0, SeekOrigin.Begin);
            _ = FindMimeFromData(IntPtr.Zero, null, data, MimeSampleSize, null, 0, out var mimeTypePointer, 0);
            var mime = Marshal.PtrToStringUni(mimeTypePointer);
            return mime ?? DefaultMimeType;
        }
        catch
        {
            return DefaultMimeType;
        }
    }

    public static string GetExtensionFromBytes(Stream stream, string defaultExtension)
    {
        try
        {
            var mimeType = GetMimeFromBytes(stream);
            var extensions = new Mime().Extension(mimeType);
            return mimeType.Equals(DefaultMimeType) || !extensions.Any()
                ? defaultExtension
                : extensions.First();
        }
        catch
        {
            return defaultExtension;
        }
    }
}