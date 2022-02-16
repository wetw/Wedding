using System.Threading.Tasks;

namespace Wedding.Services.Photo;

public interface IPhotoServices
{
    Task UploadImage(string messageId, string userId);

    Task UploadVideo(string messageId, string userId);
}