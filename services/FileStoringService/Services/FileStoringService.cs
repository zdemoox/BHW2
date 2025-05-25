using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AntiPlagiarismSystem.Shared.Interfaces;
using AntiPlagiarismSystem.Shared.Models;
using FileStoringService.Data;

namespace FileStoringService.Services
{
    public class FileStoringService : IFileStoringService
    {
        private readonly FileStoringDbContext _context;
        private readonly string _basePath;

        public FileStoringService(FileStoringDbContext context, IConfiguration configuration)
        {
            _context = context;
            _basePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Files");
            Directory.CreateDirectory(_basePath);
        }

        public async Task<Guid> StoreFileAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileContent = memoryStream.ToArray();
            
            var hash = CalculateHash(fileContent);
            var existingFile = await _context.Files.FirstOrDefaultAsync(f => f.Hash == hash);
            
            if (existingFile != null)
                return existingFile.Id;

            var fileId = Guid.NewGuid();
            var location = Path.Combine(_basePath, $"{fileId}.txt");

            await File.WriteAllBytesAsync(location, fileContent);

            var metadata = new FileMetadata
            {
                Id = fileId,
                Name = file.FileName,
                Hash = hash,
                Location = location
            };

            _context.Files.Add(metadata);
            await _context.SaveChangesAsync();

            return fileId;
        }

        public async Task<byte[]> GetFileAsync(Guid id)
        {
            var metadata = await _context.Files.FindAsync(id) 
                ?? throw new FileNotFoundException($"File with id {id} not found");

            return await File.ReadAllBytesAsync(metadata.Location);
        }

        public async Task<FileMetadata> GetFileMetadataAsync(Guid id)
        {
            var metadata = await _context.Files.FindAsync(id)
                ?? throw new FileNotFoundException($"File with id {id} not found");

            return metadata;
        }

        private string CalculateHash(byte[] content)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(content);
            return Convert.ToBase64String(hashBytes);
        }
    }
} 