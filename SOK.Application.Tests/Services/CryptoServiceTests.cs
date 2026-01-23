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
            // Konfiguracja z prostym hasłem dla testów (nowy system PBKDF2)
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Crypto:Keys:1", "TestPassword123!"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _cryptoService = new CryptoService(configuration);
        }

        [Fact]
        public void Encrypt_WithPlainText_ShouldReturnVersionedBase64String()
        {
            // Arrange
            string plainText = "Test message";

            // Act
            string encrypted = _cryptoService.Encrypt(plainText);

            // Assert
            encrypted.Should().NotBeNullOrEmpty();
            encrypted.Should().NotBe(plainText);
            
            // Sprawdź czy zawiera prefix wersji
            encrypted.Should().StartWith("v");
            encrypted.Should().Contain(":");
            
            // Sprawdź czy część po dwukropku to prawidłowy Base64
            var parts = encrypted.Split(':', 2);
            parts.Should().HaveCount(2);
            var act = () => Convert.FromBase64String(parts[1]);
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
        public void GetCurrentKeyVersion_ShouldReturnHighestVersionNumber()
        {
            // Act
            int version = _cryptoService.GetCurrentKeyVersion();

            // Assert
            version.Should().Be(1);
        }

        [Fact]
        public void Encrypt_WithSpecificKeyVersion_ShouldUseCorrectVersion()
        {
            // Arrange
            string plainText = "Test with specific version";

            // Act
            string encrypted = _cryptoService.Encrypt(plainText, 1);

            // Assert
            encrypted.Should().StartWith("v1:");
        }

        [Fact]
        public void Decrypt_WithVersionPrefix_ShouldDetectCorrectVersion()
        {
            // Arrange
            string plainText = "Connection string data";
            string encrypted = _cryptoService.Encrypt(plainText);

            // Act
            string decrypted = _cryptoService.Decrypt(encrypted);

            // Assert
            decrypted.Should().Be(plainText);
        }

        [Fact]
        public void Reencrypt_ShouldMigrateDataToNewKeyVersion()
        {
            // Arrange - konfiguracja z dwoma kluczami
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Crypto:Keys:1", "OldPassword123!"},
                {"Crypto:Keys:2", "NewPassword456!"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            var cryptoService = new CryptoService(configuration);

            string plainText = "Sensitive data";
            string encryptedV1 = cryptoService.Encrypt(plainText, 1);

            // Act - re-enkryptuj do wersji 2
            string encryptedV2 = cryptoService.Reencrypt(encryptedV1, 2);

            // Assert
            encryptedV1.Should().StartWith("v1:");
            encryptedV2.Should().StartWith("v2:");
            encryptedV1.Should().NotBe(encryptedV2);
            
            // Oba powinny się poprawnie odszyfrować do tego samego tekstu
            cryptoService.Decrypt(encryptedV1).Should().Be(plainText);
            cryptoService.Decrypt(encryptedV2).Should().Be(plainText);
        }

        [Fact]
        public void Decrypt_WithMissingKeyVersion_ShouldThrowInvalidOperationException()
        {
            // Arrange - szyfruj kluczem v1
            var inMemorySettings1 = new Dictionary<string, string>
            {
                {"Crypto:Keys:1", "Password123!"}
            };
            var config1 = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings1!)
                .Build();
            var cryptoService1 = new CryptoService(config1);
            
            string plainText = "Data";
            string encrypted = cryptoService1.Encrypt(plainText);

            // Utwórz nowy serwis bez klucza v1
            var inMemorySettings2 = new Dictionary<string, string>
            {
                {"Crypto:Keys:2", "DifferentPassword456!"}
            };
            var config2 = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings2!)
                .Build();
            var cryptoService2 = new CryptoService(config2);

            // Act & Assert - próba odszyfrowania bez odpowiedniego klucza powinna rzucić wyjątek
            var act = () => cryptoService2.Decrypt(encrypted);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*key version*not found*");
        }

        [Fact]
        public void Decrypt_WithInvalidBase64_ShouldThrowFormatException()
        {
            // Arrange
            string invalidEncrypted = "v1:not-valid-base64!@#";

            // Act & Assert
            var act = () => _cryptoService.Decrypt(invalidEncrypted);
            act.Should().Throw<FormatException>();
        }
    }
}
