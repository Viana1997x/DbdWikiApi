namespace DbdWikiApi.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveProfilePictureAsync(IFormFile file, string userId);
    }
}