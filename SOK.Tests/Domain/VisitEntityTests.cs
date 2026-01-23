using FluentAssertions;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SOK.Application.Tests.Domain
{
    public class VisitEntityTests
    {
        [Fact]
        public void Visit_NewInstance_ShouldHaveUnplannedStatus()
        {
            // Arrange & Act
            var visit = new Visit();

            // Assert
            visit.Status.Should().Be(VisitStatus.Unplanned);
        }

        [Fact]
        public void Visit_OrdinalNumber_ShouldBeWithinValidRange()
        {
            // Arrange
            var visit = new Visit { OrdinalNumber = 150 };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(visit);
            var isValid = Validator.TryValidateObject(visit, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void Visit_OrdinalNumber_ShouldFailValidationWhenTooHigh()
        {
            // Arrange
            var visit = new Visit { OrdinalNumber = 301 };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(visit);
            var isValid = Validator.TryValidateObject(visit, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().ContainSingle(vr => vr.MemberNames.Contains(nameof(Visit.OrdinalNumber)));
        }

        [Fact]
        public void Visit_OrdinalNumber_ShouldFailValidationWhenTooLow()
        {
            // Arrange
            var visit = new Visit { OrdinalNumber = 0 };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(visit);
            var isValid = Validator.TryValidateObject(visit, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().ContainSingle(vr => vr.MemberNames.Contains(nameof(Visit.OrdinalNumber)));
        }

        [Fact]
        public void Visit_CanSetStatus_ToVisited()
        {
            // Arrange
            var visit = new Visit();

            // Act
            visit.Status = VisitStatus.Visited;

            // Assert
            visit.Status.Should().Be(VisitStatus.Visited);
        }

        [Fact]
        public void Visit_CanSetPeopleCount()
        {
            // Arrange
            var visit = new Visit();

            // Act
            visit.PeopleCount = 4;

            // Assert
            visit.PeopleCount.Should().Be(4);
        }

        [Theory]
        [InlineData(VisitStatus.Unplanned)]
        [InlineData(VisitStatus.Planned)]
        [InlineData(VisitStatus.Visited)]
        [InlineData(VisitStatus.Rejected)]
        [InlineData(VisitStatus.Withdrawn)]
        public void Visit_CanSetAllValidStatuses(VisitStatus status)
        {
            // Arrange
            var visit = new Visit();

            // Act
            visit.Status = status;

            // Assert
            visit.Status.Should().Be(status);
        }
    }
}
