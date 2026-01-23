using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using SOK.Application.Services.Implementation;

namespace SOK.Application.Tests.Services
{
    public class CryptoServiceTests
    {
        private readonly CryptoService _cryptoService;

        public CryptoServiceTests()
        {
            // Generuj losowy 256-bit klucz dla testów
            var mockConfig = new Mock<IConfiguration>();
            var randomKey = new byte[32]; // 256 bits
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomKey);
            }
            mockConfig.Setup(c => c["Crypto:Key"]).Returns(Convert.ToBase64String(randomKey));

            _cryptoService = new CryptoService(mockConfig.Object);
        }

        [Fact]
        public void Encrypt_WithPlainText_ShouldReturnBase64String()
        {
            // Arrange
            string plainText = "Test message";

            // Act
            string encrypted = _cryptoService.Encrypt(plainText);

            // Assert
            encrypted.Should().NotBeNullOrEmpty();
            encrypted.Should().NotBe(plainText);
            
            // Sprawdź czy to prawidłowy Base64
            var act = () => Convert.FromBase64String(encrypted);
            act.Should().NotThrow();
        }

        [Fact]
        public void Decrypt_WithEncryptedText_ShouldReturnOriginalPlainText()
        {
            // Arrange
            string plainText = "Connection string sensitive data";
            string encrypted = _cryptoService.Encrypt(plainText);

            // Act
            string decrypted = _cryptoService.Decrypt(encrypted);

            // Assert
            decrypted.Should().Be(plainText);
        }

        [Fact]
        public void EncryptDecrypt_WithPolishCharacters_ShouldPreserveText()
        {
            // Arrange
            string plainText = "Parafia Św. Wojciecha - Łódź, Gdańsk";

            // Act
            string encrypted = _cryptoService.Encrypt(plainText);
            string decrypted = _cryptoService.Decrypt(encrypted);

            // Assert
            decrypted.Should().Be(plainText);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("Short text")]
        [InlineData("Very long text with multiple words and special chars !@#$%^&*()")]
        public void EncryptDecrypt_WithVariousLengths_ShouldWorkCorrectly(string plainText)
        {
            // Act
            string encrypted = _cryptoService.Encrypt(plainText);
            string decrypted = _cryptoService.Decrypt(encrypted);

            // Assert
            decrypted.Should().Be(plainText);
        }

        [Fact]
        public void Encrypt_CalledTwiceWithSameText_ShouldProduceDifferentCiphertext()
        {
            // Arrange
            string plainText = "Same message";

            // Act
            string encrypted1 = _cryptoService.Encrypt(plainText);
            string encrypted2 = _cryptoService.Encrypt(plainText);

            // Assert - powinny być różne z powodu losowego IV
            encrypted1.Should().NotBe(encrypted2);
            
            // Ale oba powinny się poprawnie odszyfrować
            _cryptoService.Decrypt(encrypted1).Should().Be(plainText);
            _cryptoService.Decrypt(encrypted2).Should().Be(plainText);
        }

        [Fact]
        public void Decrypt_WithInvalidBase64_ShouldThrowFormatException()
        {
            // Arrange
            string invalidEncrypted = "not-valid-base64!@#";

            // Act & Assert
            var act = () => _cryptoService.Decrypt(invalidEncrypted);
            act.Should().Throw<FormatException>();
        }
    }
}
