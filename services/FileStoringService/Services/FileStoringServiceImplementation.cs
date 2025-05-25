using System.Security.Cryptography;
using System.Text;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;
using FileStoringService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace FileStoringService.Services;

public class FileStoringServiceImplementation : IFileStoringService
{
    private readonly FileStoringDbContext _context;
    private readonly string _fileStoragePath;

    public FileStoringServiceImplementation(FileStoringDbContext context, IConfiguration configuration)
    {
        _context = context;
        _fileStoragePath = configuration.GetValue<string>("FileStorage:Path") ?? "FileStorage";
        if (!Directory.Exists(_fileStoragePath))
        {
            Directory.CreateDirectory(_fileStoragePath);
        }
    }

    public async Task<Guid> StoreFileAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return await StoreFileAsync(file.FileName, ms.ToArray());
    }

    public async Task<Guid> StoreFileAsync(string fileName, byte[] content)
    {
        var hash = CalculateHash(content);
        var existingFile = await _context.Files.FirstOrDefaultAsync(f => f.Hash == hash);
        
        if (existingFile != null)
        {
            return existingFile.Id;
        }

        var fileId = Guid.NewGuid();
        var filePath = Path.Combine(_fileStoragePath, $"{fileId}.txt");
        
        await File.WriteAllBytesAsync(filePath, content);

        var fileMetadata = new FileMetadata
        {
            Id = fileId,
            Name = fileName,
            Hash = hash,
            Location = filePath
        };

        _context.Files.Add(fileMetadata);
        await _context.SaveChangesAsync();

        return fileId;
    }

    public async Task<byte[]> GetFileAsync(Guid fileId)
    {
        var fileMetadata = await _context.Files.FindAsync(fileId);
        if (fileMetadata == null || string.IsNullOrEmpty(fileMetadata.Location))
        {
            throw new FileNotFoundException($"File with ID {fileId} not found");
        }

        return await File.ReadAllBytesAsync(fileMetadata.Location);
    }

    public async Task<FileMetadata> GetFileMetadataAsync(Guid fileId)
    {
        var fileMetadata = await _context.Files.FindAsync(fileId);
        if (fileMetadata == null)
        {
            throw new FileNotFoundException($"File with ID {fileId} not found");
        }

        return fileMetadata;
    }

    private static string CalculateHash(byte[] content)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(content);
        return Convert.ToBase64String(hashBytes);
    }
} 