using FluentAssertions;
using Moq;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Implementation;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Tests.Services
{
    public class ParishInfoServiceTests
    {
        private readonly Mock<IUnitOfWorkParish> _uowMock;
        private readonly Mock<IParishInfoRepository> _parishInfoRepoMock;
        private readonly ParishInfoService _parishInfoService;

        public ParishInfoServiceTests()
        {
            _uowMock = new Mock<IUnitOfWorkParish>();
            _parishInfoRepoMock = new Mock<IParishInfoRepository>();

            _uowMock.Setup(u => u.ParishInfo).Returns(_parishInfoRepoMock.Object);

            _parishInfoService = new ParishInfoService(_uowMock.Object);
        }

        [Fact]
        public async Task GetValueAsync_WithExistingOption_ShouldReturnValue()
        {
            // Arrange
            string optionName = "ParishName";
            string expectedValue = "Parafia św. Jana";
            var parishInfo = new ParishInfo
            {
                Id = 1,
                Name = optionName,
                Value = expectedValue
            };

            _parishInfoRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ParishInfo, bool>>>(),
                It.IsAny<Expression<Func<ParishInfo, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(parishInfo);

            // Act
            var result = await _parishInfoService.GetValueAsync(optionName);

            // Assert
            result.Should().Be(expectedValue);
        }

        [Fact]
        public async Task GetValueAsync_WithNonExistingOption_ShouldReturnNull()
        {
            // Arrange
            string optionName = "NonExistentOption";

            _parishInfoRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ParishInfo, bool>>>(),
                It.IsAny<Expression<Func<ParishInfo, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync((ParishInfo?)null);

            // Act
            var result = await _parishInfoService.GetValueAsync(optionName);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateValueAsync_WithExistingOption_ShouldUpdateValue()
        {
            // Arrange
            string optionName = "ParishName";
            string newValue = "Parafia Najświętszego Serca Pana Jezusa";
            var existingInfo = new ParishInfo
            {
                Id = 1,
                Name = optionName,
                Value = "Stara wartość"
            };

            _parishInfoRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ParishInfo, bool>>>(),
                It.IsAny<Expression<Func<ParishInfo, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(existingInfo);

            // Act
            await _parishInfoService.UpdateValueAsync(optionName, newValue);

            // Assert
            existingInfo.Value.Should().Be(newValue);
            _parishInfoRepoMock.Verify(r => r.Update(existingInfo), Times.Once);
            _uowMock.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateValueAsync_WithNonExistingOption_ShouldCreateNew()
        {
            // Arrange
            string optionName = "NewOption";
            string value = "Nowa wartość";

            _parishInfoRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ParishInfo, bool>>>(),
                It.IsAny<Expression<Func<ParishInfo, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync((ParishInfo?)null);

            ParishInfo? capturedInfo = null;
            _parishInfoRepoMock.Setup(r => r.Add(It.IsAny<ParishInfo>()))
                .Callback<ParishInfo>(pi => capturedInfo = pi);

            // Act
            await _parishInfoService.UpdateValueAsync(optionName, value);

            // Assert
            capturedInfo.Should().NotBeNull();
            capturedInfo!.Name.Should().Be(optionName);
            capturedInfo.Value.Should().Be(value);
            _parishInfoRepoMock.Verify(r => r.Add(It.IsAny<ParishInfo>()), Times.Once);
            _uowMock.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetDictionaryAsync_ShouldReturnDictionary()
        {
            // Arrange
            var expectedDictionary = new Dictionary<string, string>
            {
                { "ParishName", "Parafia św. Jana" },
                { "Diocese.Name", "Archidiecezja Warszawska" },
                { "Address.City.Name", "Warszawa" }
            };

            _parishInfoRepoMock.Setup(r => r.ToDictionaryAsync())
                .ReturnsAsync(expectedDictionary);

            // Act
            var result = await _parishInfoService.GetDictionaryAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedDictionary);
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetValuesAsync_WithSpecificOptions_ShouldReturnFilteredDictionary()
        {
            // Arrange
            var requestedOptions = new[] { "ParishName", "Diocese.Name" };
            var expectedDictionary = new Dictionary<string, string>
            {
                { "ParishName", "Parafia św. Jana" },
                { "Diocese.Name", "Archidiecezja Warszawska" }
            };

            _parishInfoRepoMock.Setup(r => r.GetValuesAsDictionaryAsync(requestedOptions))
                .ReturnsAsync(expectedDictionary);

            // Act
            var result = await _parishInfoService.GetValuesAsync(requestedOptions);

            // Assert
            result.Should().BeEquivalentTo(expectedDictionary);
            result.Should().HaveCount(2);
            result.Keys.Should().Contain("ParishName");
            result.Keys.Should().Contain("Diocese.Name");
        }
    }
}
