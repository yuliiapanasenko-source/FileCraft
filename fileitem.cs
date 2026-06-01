using System;
using System.Collections.Generic;

namespace FileSharingSystem
{
    // Клас Tag (Тег) 
    public class Tag
    {
        public int    TagId { get; set; }
        public string Label { get; set; }
    }

    // Клас Report (Звіт)
    public class Report
    {
        public int      ReportId     { get; set; }
        public int      ParentFileId { get; set; }
        public DateTime CreatedAt    { get; set; }
        public string   Content      { get; set; }
    }

    // Клас FileItem (Файл)
    public class FileItem
    {
        public int    FileId   { get; set; }
        public int    UserId   { get; set; }
        public string FileName { get; set; }
        public bool   IsPublic { get; private set; }
        public string ShareUrl { get; private set; }

        // Композиція: файл містить колекцію звітів (0..*)
        public List<Report> Reports { get; } = new List<Report>();

        // Теги: асоціація (0..*)
        public List<Tag> Tags { get; } = new List<Tag>();

        // Метод 1: Upload
        // upload(file_data): File
        public FileItem Upload(string fileData)
        {
            // Обробка виняткових ситуацій: null-аргумент
            if (fileData == null)
                throw new ArgumentNullException(nameof(fileData),
                    "Дані файлу не можуть бути null");

            // Умовна конструкція: порожній або пробільний вміст — відхиляємо
            if (string.IsNullOrWhiteSpace(fileData))
                throw new ArgumentException(
                    "Дані файлу не можуть бути порожніми", nameof(fileData));

            // Умовна конструкція: ім'я файлу має бути задано
            if (string.IsNullOrWhiteSpace(FileName))
                throw new InvalidOperationException(
                    "Не задано ім'я файлу перед завантаженням");

            // Умовна конструкція: перевірка допустимого розширення
            string[] allowed = { ".pdf", ".png", ".jpg", ".jpeg", ".docx", ".txt" };
            bool validExtension = false;
            foreach (string ext in allowed)
            {
                if (FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                {
                    validExtension = true;
                    break;
                }
            }

            if (!validExtension)
                throw new InvalidOperationException(
                    $"Формат файлу не підтримується: {FileName}");

            // Автоматично створюємо звіт про завантаження (композиція)
            Reports.Add(new Report
            {
                ReportId     = Reports.Count + 1,
                ParentFileId = FileId,
                CreatedAt    = DateTime.UtcNow,
                Content      = $"Файл '{FileName}' завантажено. Розмір даних: {fileData.Length} символів."
            });

            return this;
        }

        // Метод 2: TogglePublic
        // togglePublic(status): String
        public string TogglePublic(bool status)
        {
            // Умовна конструкція: ім'я файлу має бути задано
            if (string.IsNullOrWhiteSpace(FileName))
                throw new InvalidOperationException(
                    "Неможливо змінити доступ: ім'я файлу не задано");

            IsPublic = status;

            // Умовна конструкція: якщо файл стає приватним — прибираємо URL
            if (!status)
            {
                ShareUrl = null;
                return null;
            }

            // Цикл: санітизація імені файлу для побудови безпечного URL
            var safeChars = new List<char>();
            foreach (char c in FileName)
            {
                if (char.IsLetterOrDigit(c) || c == '.' || c == '-')
                    safeChars.Add(char.ToLower(c));
                else
                    safeChars.Add('_');
            }

            string safeName = new string(safeChars.ToArray());
            ShareUrl = $"https://share.example.com/files/{FileId}/{safeName}";
            return ShareUrl;
        }

        // Метод 3: AddTag
        // addTag(tag_id): void 

        public void AddTag(Tag tag)
        {
            // Обробка виняткових ситуацій: null-аргумент
            if (tag == null)
                throw new ArgumentNullException(nameof(tag),
                    "Тег не може бути null");

            // Умовна конструкція: мітка тегу не може бути порожньою
            if (string.IsNullOrWhiteSpace(tag.Label))
                throw new ArgumentException(
                    "Мітка тегу не може бути порожньою", nameof(tag));

            // Цикл: перевірка на дублікат за TagId
            foreach (Tag existing in Tags)
            {
                if (existing.TagId == tag.TagId)
                    return; // дублікат — ігноруємо без винятку
            }

            Tags.Add(tag);
        }
    }
}
