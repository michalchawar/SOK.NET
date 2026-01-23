using FluentAssertions;
using Moq;
using SOK.Application.Common.DTO.Submission;
using SOK.Application.Common.Helpers.EmailTypes;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Implementation;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.Linq.Expressions;

namespace SOK.Application.Tests.Services
{
    public class SubmissionServiceTests
    {
        private readonly Mock<IUnitOfWorkParish> _uowMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IParishInfoService> _parishInfoServiceMock;
        private readonly Mock<IVisitService> _visitServiceMock;
        private readonly Mock<ISubmissionRepository> _submissionRepoMock;
        private readonly Mock<IBuildingRepository> _buildingRepoMock;
        private readonly Mock<IStreetRepository> _streetRepoMock;
        private readonly Mock<IScheduleRepository> _scheduleRepoMock;
        private readonly Mock<ISubmitterRepository> _submitterRepoMock;
        private readonly Mock<IAddressRepository> _addressRepoMock;
        private readonly Mock<IBuildingAssignmentRepository> _buildingAssignmentRepoMock;
        private readonly SubmissionService _submissionService;

        public SubmissionServiceTests()
        {
            _uowMock = new Mock<IUnitOfWorkParish>();
            _emailServiceMock = new Mock<IEmailService>();
            _parishInfoServiceMock = new Mock<IParishInfoService>();
            _visitServiceMock = new Mock<IVisitService>();
            _submissionRepoMock = new Mock<ISubmissionRepository>();
            _buildingRepoMock = new Mock<IBuildingRepository>();
            _streetRepoMock = new Mock<IStreetRepository>();
            _scheduleRepoMock = new Mock<IScheduleRepository>();
            _submitterRepoMock = new Mock<ISubmitterRepository>();
            _addressRepoMock = new Mock<IAddressRepository>();
            _buildingAssignmentRepoMock = new Mock<IBuildingAssignmentRepository>();

            _uowMock.Setup(u => u.Submission).Returns(_submissionRepoMock.Object);
            _uowMock.Setup(u => u.Building).Returns(_buildingRepoMock.Object);
            _uowMock.Setup(u => u.Street).Returns(_streetRepoMock.Object);
            _uowMock.Setup(u => u.Schedule).Returns(_scheduleRepoMock.Object);
            _uowMock.Setup(u => u.Submitter).Returns(_submitterRepoMock.Object);
            _uowMock.Setup(u => u.Address).Returns(_addressRepoMock.Object);
            _uowMock.Setup(u => u.BuildingAssignment).Returns(_buildingAssignmentRepoMock.Object);

            _submissionService = new SubmissionService(
                _uowMock.Object,
                _emailServiceMock.Object,
                _parishInfoServiceMock.Object,
                _visitServiceMock.Object
            );
        }

        [Fact]
        public async Task GetSubmissionAsync_WithValidId_ShouldReturnSubmission()
        {
            // Arrange
            int submissionId = 1;
            var expectedSubmission = new Submission
            {
                Id = submissionId,
                AccessToken = "test-token",
                Submitter = new Submitter(),
                Address = new Address
                {
                    Building = new Building()
                }
            };

            _submissionRepoMock.Setup(r => r.GetPaginatedAsync(
                It.IsAny<Expression<Func<Submission, bool>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<Submission> { expectedSubmission });

            // Act
            var result = await _submissionService.GetSubmissionAsync(submissionId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(submissionId);
            result.Submitter.Should().NotBeNull();
        }

        [Fact]
        public async Task GetSubmissionAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            int nonExistentId = 999;

            _submissionRepoMock.Setup(r => r.GetPaginatedAsync(
                It.IsAny<Expression<Func<Submission, bool>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<Submission>());

            // Act
            var result = await _submissionService.GetSubmissionAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSubmissionAsync_WithValidUniqueId_ShouldReturnSubmission()
        {
            // Arrange
            var uniqueId = Guid.NewGuid();
            var expectedSubmission = new Submission
            {
                Id = 1,
                UniqueId = uniqueId,
                AccessToken = "test-token",
                Submitter = new Submitter()
            };

            _submissionRepoMock.Setup(r => r.GetPaginatedAsync(
                It.IsAny<Expression<Func<Submission, bool>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<Submission> { expectedSubmission });

            // Act
            var result = await _submissionService.GetSubmissionAsync(uniqueId.ToString());

            // Assert
            result.Should().NotBeNull();
            result!.UniqueId.Should().Be(uniqueId);
        }

        [Fact]
        public void GetSubmissionAsync_WithInvalidGuidFormat_ShouldThrowFormatException()
        {
            // Arrange
            string invalidGuid = "not-a-valid-guid";

            // Act & Assert
            var act = async () => await _submissionService.GetSubmissionAsync(invalidGuid);
            act.Should().ThrowAsync<FormatException>();
        }

        #region CreateSubmissionAsync Tests

        [Fact]
        public async Task CreateSubmissionAsync_WithNonExistentBuilding_ShouldThrowArgumentException()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            
            _buildingRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Building, bool>>>(),
                It.IsAny<Expression<Func<Building, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync((Building?)null);

            // Act & Assert
            var act = async () => await _submissionService.CreateSubmissionAsync(dto);
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Cannot create submission for non-existent building*");
        }

        [Fact]
        public async Task CreateSubmissionAsync_WithNonExistentSchedule_ShouldThrowArgumentException()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            SetupValidBuilding();
            
            _scheduleRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Schedule, bool>>>(),
                It.IsAny<Expression<Func<Schedule, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync((Schedule?)null);

            // Act & Assert
            var act = async () => await _submissionService.CreateSubmissionAsync(dto);
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Cannot create submission for non-existent schedule*");
        }

        [Fact]
        public async Task CreateSubmissionAsync_WithValidData_ShouldCreateSubmissionSuccessfully()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            SetupValidBuilding();
            SetupValidSchedule();
            SetupValidStreet();
            SetupNewSubmitter(); // Submitter nie istnieje w bazie
            SetupNoExistingAddress();
            SetupNoBuildingAssignment();
            
            int expectedSubmissionId = 1;
            SetupSubmissionAfterSave(expectedSubmissionId);

            // Act
            var result = await _submissionService.CreateSubmissionAsync(dto);

            // Assert
            result.Should().Be(expectedSubmissionId);
            _submissionRepoMock.Verify(r => r.Add(It.IsAny<Submission>()), Times.Once);
            _uowMock.Verify(u => u.SaveAsync(), Times.Once); // SaveAsync is called once
        }

        [Fact]
        public async Task CreateSubmissionAsync_WithExistingSubmitter_ShouldReuseSubmitter()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            var existingSubmitter = new Submitter
            {
                Id = 10,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan@example.com",
                Phone = "123456789"
            };

            SetupValidBuilding();
            SetupValidSchedule();
            SetupValidStreet();
            SetupExistingSubmitter(existingSubmitter);
            SetupNoExistingAddress();
            SetupNoBuildingAssignment();
            SetupSubmissionAfterSave(1);

            // Act
            var result = await _submissionService.CreateSubmissionAsync(dto);

            // Assert
            result.Should().NotBeNull();
            _submitterRepoMock.Verify(r => r.GetAsync(
                It.IsAny<Expression<Func<Submitter, bool>>>(),
                It.IsAny<Expression<Func<Submitter, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task CreateSubmissionAsync_WithSameAddressInSamePlan_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            var plan = new Plan { Id = 1 };
            
            // Istniejące zgłoszenie pod tym samym adresem w tym samym planie
            var existingSubmission = new Submission(plan, DateTime.Now.AddDays(-5))
            {
                Id = 100,
                AccessToken = "existing-token"
            };

            // WAŻNE: ApartmentLetter musi być znormalizowane (lowercase) jak w SubmissionService
            var existingAddress = new Address
            {
                Id = 5,
                ApartmentNumber = dto.ApartmentNumber, // 5
                ApartmentLetter = dto.ApartmentLetter?.ToLower(), // "b" (znormalizowane)
                BuildingId = 1,
                Submissions = new List<Submission> { existingSubmission }
            };

            var building = new Building
            {
                Id = 1,
                Number = 10,
                Letter = "A",
                StreetId = 1,
                Addresses = new List<Address> { existingAddress } // Budynek już ma ten adres
            };

            SetupValidScheduleWithPlan(plan);
            SetupValidStreet();
            SetupNewSubmitter();

            _buildingRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Building, bool>>>(),
                It.IsAny<Expression<Func<Building, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(building);

            // Mock dla ponownego pobrania adresu z submissions            
            _addressRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<Expression<Func<Address, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(existingAddress);

            // Act & Assert
            var act = async () => await _submissionService.CreateSubmissionAsync(dto);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot create submission for address*Address already has a submission*");
        }

        [Fact]
        public async Task CreateSubmissionAsync_WithSameAddressInDifferentPlan_ShouldSucceed()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            var oldPlan = new Plan { Id = 1 };
            var newPlan = new Plan { Id = 2 }; // Inny plan!
            
            // Istniejące zgłoszenie pod tym samym adresem ale w INNYM planie
            var existingSubmission = new Submission(oldPlan, DateTime.Now.AddDays(-30))
            {
                Id = 100,
                AccessToken = "old-token"
            };

            var existingAddress = new Address
            {
                Id = 5,
                ApartmentNumber = dto.ApartmentNumber,
                ApartmentLetter = dto.ApartmentLetter,
                BuildingId = 1,
                Submissions = new List<Submission> { existingSubmission }
            };

            var building = new Building
            {
                Id = 1,
                Number = 10,
                Letter = "A",
                StreetId = 1,
                Addresses = new List<Address> { existingAddress }
            };

            SetupValidScheduleWithPlan(newPlan); // Harmonogram w nowym planie
            SetupValidStreet();
            SetupNewSubmitter();
            SetupNoBuildingAssignment();

            _buildingRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Building, bool>>>(),
                It.IsAny<Expression<Func<Building, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(building);

            // Mock dla ponownego pobrania adresu z submissions - nie ma zgłoszenia w nowym planie
            _addressRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<Expression<Func<Address, object>>>(),
                "Submissions",
                true))
                .ReturnsAsync(existingAddress);

            int expectedSubmissionId = 200;
            SetupSubmissionAfterSave(expectedSubmissionId);

            // Act
            var result = await _submissionService.CreateSubmissionAsync(dto);

            // Assert
            result.Should().Be(expectedSubmissionId);
            _submissionRepoMock.Verify(r => r.Add(It.IsAny<Submission>()), Times.Once);
            // Weryfikujemy że SaveAsync został wywołany (nowe zgłoszenie zostało zapisane)
            _uowMock.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateSubmissionAsync_WithSubmitterNotes_ShouldSetNotesStatusToPending()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            dto.SubmitterNotes = "Proszę zadzwonić przed wizytą";

            SetupValidBuilding();
            SetupValidSchedule();
            SetupValidStreet();
            SetupNewSubmitter();
            SetupNoExistingAddress();
            SetupNoBuildingAssignment();
            
            Submission? capturedSubmission = null;
            _submissionRepoMock.Setup(r => r.Add(It.IsAny<Submission>()))
                .Callback<Submission>(s => capturedSubmission = s);
            
            SetupSubmissionAfterSave(1);

            // Act
            await _submissionService.CreateSubmissionAsync(dto);

            // Assert
            capturedSubmission.Should().NotBeNull();
            capturedSubmission!.NotesStatus.Should().Be(NotesFulfillmentStatus.Pending);
            capturedSubmission.SubmitterNotes.Should().Be("Proszę zadzwonić przed wizytą");
        }

        [Fact]
        public async Task CreateSubmissionAsync_WithoutSubmitterNotes_ShouldSetNotesStatusToNA()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            dto.SubmitterNotes = null;

            SetupValidBuilding();
            SetupValidSchedule();
            SetupValidStreet();
            SetupNewSubmitter();
            SetupNoExistingAddress();
            SetupNoBuildingAssignment();
            
            Submission? capturedSubmission = null;
            _submissionRepoMock.Setup(r => r.Add(It.IsAny<Submission>()))
                .Callback<Submission>(s => capturedSubmission = s);
            
            SetupSubmissionAfterSave(1);

            // Act
            await _submissionService.CreateSubmissionAsync(dto);

            // Assert
            capturedSubmission.Should().NotBeNull();
            capturedSubmission!.NotesStatus.Should().Be(NotesFulfillmentStatus.NA);
        }

        [Fact]
        public async Task CreateSubmissionAsync_WithAutoAssignment_ShouldCallAssignVisitToDay()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            dto.DisableAutoAssignment = false;

            var buildingAssignment = new BuildingAssignment
            {
                BuildingId = 1,
                ScheduleId = 1,
                DayId = 5,
                EnableAutoAssign = true
            };

            SetupValidBuilding();
            SetupValidSchedule();
            SetupValidStreet();
            SetupNewSubmitter();
            SetupNoExistingAddress();
            SetupSubmissionAfterSave(1);

            _buildingAssignmentRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<BuildingAssignment, bool>>>(),
                It.IsAny<Expression<Func<BuildingAssignment, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(buildingAssignment);

            // Act
            await _submissionService.CreateSubmissionAsync(dto);

            // Assert
            _visitServiceMock.Verify(v => v.AssignVisitToDay(
                It.IsAny<int>(), 
                buildingAssignment.DayId, 
                false), Times.Once);
        }

        [Fact]
        public async Task CreateSubmissionAsync_WithDisabledAutoAssignment_ShouldNotCallAssignVisitToDay()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            dto.DisableAutoAssignment = true;

            var buildingAssignment = new BuildingAssignment
            {
                EnableAutoAssign = true,
                DayId = 5
            };

            SetupValidBuilding();
            SetupValidSchedule();
            SetupValidStreet();
            SetupNewSubmitter();
            SetupNoExistingAddress();
            SetupSubmissionAfterSave(1);

            _buildingAssignmentRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<BuildingAssignment, bool>>>(),
                It.IsAny<Expression<Func<BuildingAssignment, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(buildingAssignment);

            // Act
            await _submissionService.CreateSubmissionAsync(dto);

            // Assert
            _visitServiceMock.Verify(v => v.AssignVisitToDay(
                It.IsAny<int>(), 
                It.IsAny<int>(), 
                It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateSubmissionAsync_WithEmailSendingEnabled_ShouldQueueEmail()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            dto.SendConfirmationEmail = true;
            dto.Submitter.Email = "jan@example.com";

            SetupValidBuilding();
            SetupValidSchedule();
            SetupValidStreet();
            SetupNewSubmitter();
            SetupNoExistingAddress();
            SetupNoBuildingAssignment();
            SetupSubmissionAfterSave(1);

            _parishInfoServiceMock.Setup(p => p.GetValueAsync("Email.EnableEmailSending"))
                .ReturnsAsync("true");
            _parishInfoServiceMock.Setup(p => p.GetValueAsync("EmbeddedApplication.ControlPanelBaseUrl"))
                .ReturnsAsync("https://example.com");

            // Act
            await _submissionService.CreateSubmissionAsync(dto);

            // Assert
            _emailServiceMock.Verify(e => e.QueueEmailAsync(
                It.IsAny<EmailTypeBase>(),
                It.IsAny<Submission>(),
                null,
                null,
                true), Times.Once);
        }

        [Fact]
        public async Task CreateSubmissionAsync_WithEmailSendingDisabled_ShouldNotQueueEmail()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            dto.SendConfirmationEmail = true;
            dto.Submitter.Email = "jan@example.com";

            SetupValidBuilding();
            SetupValidSchedule();
            SetupValidStreet();
            SetupNewSubmitter();
            SetupNoExistingAddress();
            SetupNoBuildingAssignment();
            SetupSubmissionAfterSave(1);

            _parishInfoServiceMock.Setup(p => p.GetValueAsync("Email.EnableEmailSending"))
                .ReturnsAsync("false");

            // Act
            await _submissionService.CreateSubmissionAsync(dto);

            // Assert
            _emailServiceMock.Verify(e => e.QueueEmailAsync(
                It.IsAny<EmailTypeBase>(),
                It.IsAny<Submission>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>(),
                It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateSubmissionAsync_WithNoSubmitterEmail_ShouldNotQueueEmail()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            dto.SendConfirmationEmail = true;
            dto.Submitter.Email = null;

            SetupValidBuilding();
            SetupValidSchedule();
            SetupValidStreet();
            SetupNewSubmitter();
            SetupNoExistingAddress();
            SetupNoBuildingAssignment();
            SetupSubmissionAfterSave(1);

            // Act
            await _submissionService.CreateSubmissionAsync(dto);

            // Assert
            _emailServiceMock.Verify(e => e.QueueEmailAsync(
                It.IsAny<EmailTypeBase>(),
                It.IsAny<Submission>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>(),
                It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateSubmissionAsync_ShouldTrimAndFormatSubmitterData()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();
            dto.Submitter.Name = "  jan  ";
            dto.Submitter.Surname = "  KOWALSKI  ";
            dto.Submitter.Email = "  JAN@EXAMPLE.COM  ";
            dto.Submitter.Phone = "  123 456 789  ";

            SetupValidBuilding();
            SetupValidSchedule();
            SetupValidStreet();
            SetupNewSubmitter();
            SetupNoExistingAddress();
            SetupNoBuildingAssignment();
            SetupSubmissionAfterSave(1);

            // Act
            await _submissionService.CreateSubmissionAsync(dto);

            // Assert
            dto.Submitter.Name.Should().Be("Jan");
            dto.Submitter.Surname.Should().Be("Kowalski");
            dto.Submitter.Email.Should().Be("jan@example.com");
            dto.Submitter.Phone.Should().Be("123 456 789");
        }

        [Fact]
        public async Task CreateSubmissionAsync_ShouldCreateVisitWithUnplannedStatus()
        {
            // Arrange
            var dto = CreateValidSubmissionDto();

            SetupValidBuilding();
            SetupValidSchedule();
            SetupValidStreet();
            SetupNewSubmitter();
            SetupNoExistingAddress();
            SetupNoBuildingAssignment();

            Submission? capturedSubmission = null;
            _submissionRepoMock.Setup(r => r.Add(It.IsAny<Submission>()))
                .Callback<Submission>(s => capturedSubmission = s);

            SetupSubmissionAfterSave(1);

            // Act
            await _submissionService.CreateSubmissionAsync(dto);

            // Assert
            capturedSubmission.Should().NotBeNull();
            capturedSubmission!.Visit.Should().NotBeNull();
            capturedSubmission.Visit.Status.Should().Be(VisitStatus.Unplanned);
            capturedSubmission.Visit.OrdinalNumber.Should().BeNull();
            capturedSubmission.Visit.Agenda.Should().BeNull();
        }

        #endregion

        #region Helper Methods

        private SubmissionCreationRequestDto CreateValidSubmissionDto()
        {
            return new SubmissionCreationRequestDto
            {
                Building = new Building { Id = 1, Number = 10, Letter = "A" },
                ApartmentNumber = 5,
                ApartmentLetter = "B",
                Schedule = new Schedule { Id = 1, Name = "Harmonogram 2026" },
                Submitter = new Submitter
                {
                    Name = "Jan",
                    Surname = "Kowalski",
                    Email = "jan@example.com",
                    Phone = "123456789"
                },
                SubmitterNotes = null,
                AdminNotes = null,
                Created = DateTime.Now,
                Method = SubmitMethod.WebForm,
                SendConfirmationEmail = false,
                DisableAutoAssignment = false
            };
        }

        private void SetupValidBuilding()
        {
            var building = new Building
            {
                Id = 1,
                Number = 10,
                Letter = "A",
                StreetId = 1,
                Addresses = new List<Address>()
            };

            _buildingRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Building, bool>>>(),
                It.IsAny<Expression<Func<Building, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(building);
        }

        private void SetupValidSchedule()
        {
            var plan = new Plan { Id = 1 };
            var schedule = new Schedule
            {
                Id = 1,
                Name = "Harmonogram 2026",
                Plan = plan
            };

            _scheduleRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Schedule, bool>>>(),
                It.IsAny<Expression<Func<Schedule, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(schedule);
        }

        private void SetupValidScheduleWithPlan(Plan plan)
        {
            var schedule = new Schedule
            {
                Id = 1,
                Name = "Harmonogram 2026",
                Plan = plan
            };

            _scheduleRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Schedule, bool>>>(),
                It.IsAny<Expression<Func<Schedule, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(schedule);
        }

        private void SetupValidStreet()
        {
            var city = new City { Id = 1, Name = "Warszawa" };
            var streetSpecifier = new StreetSpecifier { Id = 1, FullName = "ulica" };
            var street = new Street
            {
                Id = 1,
                Name = "Główna",
                City = city,
                Type = streetSpecifier
            };

            _streetRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Street, bool>>>(),
                It.IsAny<Expression<Func<Street, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(street);
        }

        private void SetupNewSubmitter()
        {
            _submitterRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Submitter, bool>>>(),
                It.IsAny<Expression<Func<Submitter, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync((Submitter?)null);
        }

        private void SetupExistingSubmitter(Submitter submitter)
        {
            _submitterRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Submitter, bool>>>(),
                It.IsAny<Expression<Func<Submitter, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(submitter);
        }

        private void SetupNoExistingAddress()
        {
            _addressRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Address, bool>>>(),
                It.IsAny<Expression<Func<Address, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync((Address?)null);
        }

        private void SetupNoBuildingAssignment()
        {
            _buildingAssignmentRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<BuildingAssignment, bool>>>(),
                It.IsAny<Expression<Func<BuildingAssignment, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync((BuildingAssignment?)null);
        }

        private void SetupSubmissionAfterSave(int submissionId)
        {
            var savedSubmission = new Submission
            {
                Id = submissionId,
                AccessToken = "test-token",
                Submitter = new Submitter(),
                Address = new Address { Building = new Building() },
                Visit = new Visit { Id = submissionId },
                FormSubmission = new FormSubmission()
            };

            _submissionRepoMock.Setup(r => r.GetPaginatedAsync(
                It.IsAny<Expression<Func<Submission, bool>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<Submission> { savedSubmission });
        }

        #endregion
    }
}
