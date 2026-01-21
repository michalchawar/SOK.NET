using FluentAssertions;
using SOK.Domain.Entities.Parish;
using System.ComponentModel.DataAnnotations;

namespace SOK.Application.Tests.Domain
{
    public class AddressEntityTests
    {
        [Fact]
        public void Address_NewInstance_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var address = new Address();

            // Assert
            address.Id.Should().Be(0);
            address.BuildingId.Should().Be(0);
            address.ApartmentNumber.Should().BeNull();
            address.ApartmentLetter.Should().BeNull();
        }

        [Fact]
        public void Address_CanSetProperties()
        {
            // Arrange
            var address = new Address();
            var building = new Building { Id = 5 };

            // Act
            address.BuildingId = 5;
            address.Building = building;
            address.ApartmentNumber = 12;
            address.ApartmentLetter = "A";

            // Assert
            address.BuildingId.Should().Be(5);
            address.Building.Should().Be(building);
            address.ApartmentNumber.Should().Be(12);
            address.ApartmentLetter.Should().Be("A");
        }

        [Fact]
        public void Address_ApartmentNumber_ShouldFailValidationWhenTooHigh()
        {
            // Arrange
            var address = new Address
            {
                ApartmentNumber = 301 // przekroczenie max 300
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(address);
            var isValid = Validator.TryValidateObject(address, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains(nameof(Address.ApartmentNumber)));
        }

        [Fact]
        public void Address_ApartmentNumber_ShouldAcceptValidRange()
        {
            // Arrange
            var address = new Address
            {
                ApartmentNumber = 150
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(address);
            var isValid = Validator.TryValidateObject(address, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void Address_ApartmentLetter_ShouldFailValidationWhenTooLong()
        {
            // Arrange
            var address = new Address
            {
                ApartmentLetter = "ABCD" // przekroczenie max 3
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(address);
            var isValid = Validator.TryValidateObject(address, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void Address_ApartmentLetter_ShouldAcceptValidLength()
        {
            // Arrange
            var address = new Address
            {
                ApartmentLetter = "AB"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(address);
            var isValid = Validator.TryValidateObject(address, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            address.ApartmentLetter.Should().Be("AB");
        }
    }
}
