using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileSharingSystem
{
    // Клас Tag (Тег) 
    public class Tag
    {
        public int TagId { get; set; }
        public string  Label { get; set; } = string.Empty;
    }

    // Клас Report (Звіт)
    public class Report
    {
        public int ReportId { get; set; }
        public int ParentFileId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    // Клас FileItem (Файл)
    public class FileItem
    {
        private static readonly string[] AllowedExtensions =
            { ".pdf", ".png", ".jpg", ".jpeg", ".docx", ".txt" };

        private readonly string ShareBaseUrl;
        public FileItem() { }
        public FileItem(string shareBaseUrl, string fileName, string shareUrl)
        {
            ShareBaseUrl = shareBaseUrl;
            FileName = fileName;
            ShareUrl = shareUrl;
        }

        public int FileId { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; }
        public bool IsPublic { get; private set; }
        public string ShareUrl { get; private set; }

        public List<Report> Reports { get; } = new List<Report>();
        public List<Tag> Tags { get; } = new List<Tag>();

        // Метод 1: Upload
        public FileItem Upload(string fileData)
        {
            if (fileData == null)
                throw new ArgumentNullException(nameof(fileData),
                    "Дані файлу не можуть бути null");

            if (string.IsNullOrWhiteSpace(fileData))
                throw new ArgumentException(
                    "Дані файлу не можуть бути порожніми", nameof(fileData));

            if (string.IsNullOrWhiteSpace(FileName))
                throw new InvalidOperationException(
                    "Не задано ім'я файлу перед завантаженням");

            if (!IsValidExtension(FileName))
                throw new InvalidOperationException(
                    $"Формат файлу не підтримується: {FileName}");

            CreateUploadReport(fileData.Length);
            return this;
        }

        private static bool IsValidExtension(string fileName)
        {
            return AllowedExtensions
                .Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        private void CreateUploadReport(int dataLength)
        {
            Reports.Add(new Report
            {
                ReportId = Reports.Count + 1,
                ParentFileId = FileId,
                CreatedAt = DateTime.UtcNow,
                Content = $"Файл '{FileName}' завантажено. Розмір даних: {dataLength} символів."
            });
        }

        // Метод 2: TogglePublic
        public string TogglePublic(bool status)
        {
            if (string.IsNullOrWhiteSpace(FileName))
                throw new InvalidOperationException(
                    "Неможливо змінити доступ: ім'я файлу не задано");

            IsPublic = status;

            if (!status)
            {
                ShareUrl = string.Empty;
                return string.Empty;
            }

            string safeName = Regex.Replace(
                FileName.ToLower(),
                @"[^a-z0-9\-.]",
                "_");

            ShareUrl = $"{ShareBaseUrl}{FileId}/{safeName}";
            return ShareUrl;
        }

        // Метод 3: AddTag
        public void AddTag(Tag tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag),
                    "Тег не може бути null");

            if (string.IsNullOrWhiteSpace(tag.Label))
                throw new ArgumentException(
                    "Мітка тегу не може бути порожньою", nameof(tag));

            if (Tags.Any(t => t.TagId == tag.TagId))
                return;

            Tags.Add(tag);
        }
    }
}