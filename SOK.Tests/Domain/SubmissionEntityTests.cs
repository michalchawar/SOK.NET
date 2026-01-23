using FluentAssertions;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SOK.Application.Tests.Domain
{
    public class SubmissionEntityTests
    {
        [Fact]
        public void Submission_NewInstance_ShouldHaveUniqueId()
        {
            // Arrange & Act
            var submission = new Submission();

            // Assert
            submission.UniqueId.Should().NotBeEmpty();
        }

        [Fact]
        public void Submission_NewInstance_ShouldHaveNotesStatusNA()
        {
            // Arrange & Act
            var submission = new Submission();

            // Assert
            submission.NotesStatus.Should().Be(NotesFulfillmentStatus.NA);
        }

        [Fact]
        public void Submission_TwoInstances_ShouldHaveDifferentUniqueIds()
        {
            // Arrange & Act
            var submission1 = new Submission();
            var submission2 = new Submission();

            // Assert
            submission1.UniqueId.Should().NotBe(submission2.UniqueId);
        }

        [Fact]
        public void Submission_SubmitterNotes_ShouldAcceptValidLength()
        {
            // Arrange
            var submission = new Submission
            {
                AccessToken = "test-token",
                SubmitterNotes = new string('a', 500) // 500 znaków (max 512)
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(submission);
            var isValid = Validator.TryValidateObject(submission, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void Submission_SubmitterNotes_ShouldFailValidationWhenTooLong()
        {
            // Arrange
            var submission = new Submission
            {
                AccessToken = "test-token",
                SubmitterNotes = new string('a', 513) // Przekroczenie limitu 512
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(submission);
            var isValid = Validator.TryValidateObject(submission, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains(nameof(Submission.SubmitterNotes)));
        }

        [Fact]
        public void Submission_AdminMessage_ShouldAcceptNull()
        {
            // Arrange
            var submission = new Submission
            {
                AccessToken = "test-token",
                AdminMessage = null
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(submission);
            var isValid = Validator.TryValidateObject(submission, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
            submission.AdminMessage.Should().BeNull();
        }

        [Fact]
        public void Submission_AdminNotes_ShouldFailValidationWhenTooLong()
        {
            // Arrange
            var submission = new Submission
            {
                AccessToken = "test-token",
                AdminNotes = new string('b', 600)
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(submission);
            var isValid = Validator.TryValidateObject(submission, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(NotesFulfillmentStatus.NA)]
        [InlineData(NotesFulfillmentStatus.Pending)]
        [InlineData(NotesFulfillmentStatus.Accepted)]
        [InlineData(NotesFulfillmentStatus.Rejected)]
        public void Submission_NotesStatus_CanSetAllValidStatuses(NotesFulfillmentStatus status)
        {
            // Arrange
            var submission = new Submission();

            // Act
            submission.NotesStatus = status;

            // Assert
            submission.NotesStatus.Should().Be(status);
        }

        [Fact]
        public void Submission_AccessToken_ShouldNotExceedMaxLength()
        {
            // Arrange
            var submission = new Submission
            {
                AccessToken = new string('x', 64) // Dokładnie max długość
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(submission);
            var isValid = Validator.TryValidateObject(submission, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void Submission_AccessToken_ShouldFailWhenTooLong()
        {
            // Arrange
            var submission = new Submission
            {
                AccessToken = new string('x', 65) // Przekroczenie limitu
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(submission);
            var isValid = Validator.TryValidateObject(submission, validationContext, validationResults, true);

            // Assert
            isValid.Should().BeFalse();
        }
    }
}
