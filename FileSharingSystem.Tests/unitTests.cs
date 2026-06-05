using NUnit.Framework;
using System;
using FileSharingSystem;

namespace FileSharingSystem.Tests
{
    public class FileItemTests
    {
        private const int DefaultFileId = 1;
        private const int DefaultUserId = 10;

        private const string ValidPdfFileName = "document.pdf";
        private const string ValidPngFileName = "photo.png";
        private const string InvalidFileName = "virus.exe";
        private const string ValidFileData = "VALID_FILE_DATA";

        private FileItem CreateValidFileItem(string fileName = ValidPdfFileName)
            => new FileItem
            {
                FileId = DefaultFileId,
                UserId = DefaultUserId,
                FileName = fileName
            };
        // Покриває колишні тести 1, 2, 3.
        [TestCase(null, typeof(ArgumentNullException))]
        [TestCase("", typeof(ArgumentException))]
        [TestCase("   ", typeof(ArgumentException))]
        public void Upload_InvalidFileData_ThrowsExpectedException(
            string? fileData, Type expectedException)
        {
            // Arrange
            FileItem file = CreateValidFileItem();

            // Act & Assert
            Assert.Throws(expectedException, () => file.Upload(fileData));
        }

        // ── ТЕСТ 4 ──────────────────────────────────────────────────
        // Техніка: EP / негативний
        // Клас еквівалентності: file_name не задано → InvalidOperationException
        [Test]
        public void Upload_EmptyFileName_ThrowsInvalidOperationException()
        {
            // Arrange
            FileItem file = CreateValidFileItem(fileName: "");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => file.Upload(ValidFileData));
        }

        // ── ТЕСТ 5 ──────────────────────────────────────────────────
        // Техніка: EP / негативний
        // Клас еквівалентності: недопустиме розширення файлу → InvalidOperationException
        [Test]
        public void Upload_InvalidExtension_ThrowsInvalidOperationException()
        {
            // Arrange
            FileItem file = CreateValidFileItem(fileName: InvalidFileName);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => file.Upload(ValidFileData));
        }

        // ── ТЕСТ 6 ──────────────────────────────────────────────────
        // Техніка: EP / позитивний
        // Клас еквівалентності: допустиме розширення .pdf → повертає FileItem
        [Test]
        public void Upload_ValidPdfFile_ReturnsFileItemAndCreatesReport()
        {
            // Arrange
            FileItem file = CreateValidFileItem(ValidPdfFileName);

            // Act
            FileItem result = file.Upload(ValidFileData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, file.Reports.Count);
        }

        // ── ТЕСТ 7 ──────────────────────────────────────────────────
        // Техніка: EP / позитивний
        // Клас еквівалентності: допустиме розширення .png → повертає FileItem
        [Test]
        public void Upload_ValidPngFile_ReturnsFileItemAndCreatesReport()
        {
            // Arrange
            FileItem file = CreateValidFileItem(ValidPngFileName);

            // Act
            FileItem result = file.Upload(ValidFileData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, file.Reports.Count);
        }

        // ── ТЕСТ 8 ──────────────────────────────────────────────────
        // Техніка: EP / негативний
        // Клас еквівалентності: file_name не задано → InvalidOperationException
        [Test]
        public void TogglePublic_EmptyFileName_ThrowsInvalidOperationException()
        {
            // Arrange
            FileItem file = CreateValidFileItem(fileName: "");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => file.TogglePublic(true));
        }

        // ── ТЕСТ 9 ──────────────────────────────────────────────────
        // Техніка: EP / позитивний
        // Клас еквівалентності: status = true → is_public = true, share_url не null
        [Test]
        public void TogglePublic_True_SetsIsPublicAndReturnsUrl()
        {
            // Arrange
            FileItem file = CreateValidFileItem(ValidPngFileName);

            // Act
            string url = file.TogglePublic(true);

            // Assert
            Assert.IsTrue(file.IsPublic);
            Assert.IsNotNull(url);
        }

        // ── ТЕСТ 10 ─────────────────────────────────────────────────
        // Техніка: EP / позитивний
        // Клас еквівалентності: status = false → is_public = false, share_url = null
        [Test]
        public void TogglePublic_False_SetsIsPublicFalseAndReturnsNull()
        {
            // Arrange
            FileItem file = CreateValidFileItem(ValidPngFileName);

            // Act
            string url = file.TogglePublic(false);

            // Assert
            Assert.IsFalse(file.IsPublic);
            Assert.IsEmpty(url);
        }

        // ── ТЕСТ 11 ─────────────────────────────────────────────────
        // Техніка: BVA / позитивний
        // Гранична умова: file_name містить пробіли та спецсимволи → share_url санітизовано
        [Test]
        public void TogglePublic_FileNameWithSpecialChars_UrlContainsSanitizedName()
        {
            // Arrange
            FileItem file = CreateValidFileItem(fileName: "my file!.pdf");

            string expectedSanitizedName = "my_file_.pdf";

            // Act
            string url = file.TogglePublic(true);

            // Assert
            Assert.IsNotNull(url);
            Assert.IsTrue(url.Contains(expectedSanitizedName));
        }

        // ── ТЕСТ 12 ─────────────────────────────────────────────────
        // Техніка: BVA / позитивний
        // Гранична умова: file_name лише з допустимих символів → share_url без змін
        [Test]
        public void TogglePublic_FileNameWithAllowedCharsOnly_UrlContainsOriginalName()
        {
            // Arrange
            FileItem file = CreateValidFileItem(ValidPngFileName);

            // Act
            string url = file.TogglePublic(true);

            // Assert
            Assert.IsNotNull(url);
            Assert.IsTrue(url.Contains(ValidPngFileName));
        }

        // ==================== AddTag(tag_id) ====================

        // ── ТЕСТ 13 ─────────────────────────────────────────────────
        // Техніка: EP / негативний
        // Клас еквівалентності: tag = null → ArgumentNullException
        [Test]
        public void AddTag_NullTag_ThrowsArgumentNullException()
        {
            // Arrange
            FileItem file = CreateValidFileItem();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => file.AddTag(null));
        }

        // ── ТЕСТ 14 ─────────────────────────────────────────────────
        // Техніка: EP / негативний
        // Клас еквівалентності: tag.label = "" → ArgumentException
        [Test]
        public void AddTag_EmptyLabel_ThrowsArgumentException()
        {
            // Arrange
            FileItem file = CreateValidFileItem();
            Tag tag = new Tag { TagId = 1, Label = "" };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => file.AddTag(tag));
        }

        // ── ТЕСТ 15 ─────────────────────────────────────────────────
        // Техніка: BVA / позитивний
        // Гранична умова: перший тег у порожній колекції → Tags.Count = 1
        [Test]
        public void AddTag_FirstTag_TagsCountEqualsOne()
        {
            // Arrange
            FileItem file = CreateValidFileItem();
            Tag tag = new Tag { TagId = 1, Label = "Навчання" };

            // Act
            file.AddTag(tag);

            // Assert
            Assert.AreEqual(1, file.Tags.Count);
        }

        // ── ТЕСТ 16 ─────────────────────────────────────────────────
        // Техніка: EP / позитивний
        // Клас еквівалентності: дублікат за tag_id → не додається, Tags.Count = 1
        [Test]
        public void AddTag_DuplicateTagId_TagsCountRemainsOne()
        {
            // Arrange
            FileItem file = CreateValidFileItem();
            Tag tag = new Tag { TagId = 1, Label = "Навчання" };

            // Act
            file.AddTag(tag);
            file.AddTag(tag);

            // Assert
            Assert.AreEqual(1, file.Tags.Count);
        }

        // ── ТЕСТ 17 ─────────────────────────────────────────────────
        // Техніка: EP / позитивний
        // Клас еквівалентності: два різних tag_id → обидва додаються, Tags.Count = 2
        [Test]
        public void AddTag_TwoDifferentTags_TagsCountEqualsTwo()
        {
            // Arrange
            FileItem file = CreateValidFileItem();
            Tag tag1 = new Tag { TagId = 1, Label = "Навчання" };
            Tag tag2 = new Tag { TagId = 2, Label = "Робота" };

            // Act
            file.AddTag(tag1);
            file.AddTag(tag2);

            // Assert
            Assert.AreEqual(2, file.Tags.Count);
        }
    }
}