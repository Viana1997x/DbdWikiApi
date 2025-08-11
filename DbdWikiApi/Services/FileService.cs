using DbdWikiApi.Interfaces;

namespace DbdWikiApi.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveProfilePictureAsync(IFormFile file, string userId)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Nenhum arquivo enviado.");
            }

            var uploadsFolderPath = Path.Combine(_env.WebRootPath, "images", "profiles");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                await System.IO.File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
            }

            // Retorna apenas o caminho parcial do arquivo salvo
            return $"/images/profiles/{fileName}";
        }
    }
}