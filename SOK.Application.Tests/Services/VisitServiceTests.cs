using FluentAssertions;
using Moq;
using SOK.Application.Common.DTO;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Implementation;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.Linq.Expressions;

namespace SOK.Application.Tests.Services
{
    public class VisitServiceTests
    {
        private readonly Mock<IUnitOfWorkParish> _uowMock;
        private readonly Mock<IVisitRepository> _visitRepoMock;
        private readonly Mock<IDayRepository> _dayRepoMock;
        private readonly Mock<IAgendaService> _agendaServiceMock;
        private readonly VisitService _visitService;

        public VisitServiceTests()
        {
            _uowMock = new Mock<IUnitOfWorkParish>();
            _visitRepoMock = new Mock<IVisitRepository>();
            _dayRepoMock = new Mock<IDayRepository>();
            _agendaServiceMock = new Mock<IAgendaService>();
            
            _uowMock.Setup(u => u.Visit).Returns(_visitRepoMock.Object);
            _uowMock.Setup(u => u.Day).Returns(_dayRepoMock.Object);
            
            _visitService = new VisitService(_uowMock.Object, _agendaServiceMock.Object);
        }

        [Fact]
        public async Task AssignVisitToDay_WithValidData_ShouldAssignVisitToAgenda()
        {
            // Arrange
            int visitId = 1;
            int dayId = 1;
            int agendaId = 10;

            var visit = new Visit
            {
                Id = visitId,
                SubmissionId = 100,
                ScheduleId = 5,
                Submission = new Submission
                {
                    Id = 100,
                    Address = new Address
                    {
                        BuildingId = 20,
                        Building = new Building { Id = 20 }
                    }
                }
            };

            var day = new Day
            {
                Id = dayId,
                Agendas = new List<Agenda>
                {
                    new Agenda
                    {
                        Id = agendaId,
                        Visits = new List<Visit>()
                    }
                }
            };

            _visitRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Visit, bool>>>(),
                It.IsAny<Expression<Func<Visit, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(visit);

            _dayRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Day, bool>>>(),
                It.IsAny<Expression<Func<Day, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(day);

            _agendaServiceMock.Setup(a => a.AssignVisitsToAgendaAsync(It.IsAny<AssignVisitsToAgendaDto>()))
                .Returns(Task.CompletedTask);

            // Act
            await _visitService.AssignVisitToDay(visitId, dayId, sendEmail: false);

            // Assert
            _agendaServiceMock.Verify(a => a.AssignVisitsToAgendaAsync(
                It.Is<AssignVisitsToAgendaDto>(dto => 
                    dto.AgendaId == agendaId && 
                    dto.SubmissionIds.Contains(visit.SubmissionId) &&
                    dto.SendEmails == false)), 
                Times.Once);
        }

        [Fact]
        public async Task AssignVisitToDay_WithNonExistentVisit_ShouldThrowArgumentException()
        {
            // Arrange
            int visitId = 999;
            int dayId = 1;

            _visitRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Visit, bool>>>(),
                It.IsAny<Expression<Func<Visit, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync((Visit?)null);

            // Act & Assert
            var act = async () => await _visitService.AssignVisitToDay(visitId, dayId);
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Visit with ID {visitId} not found.");
        }

        [Fact]
        public async Task AssignVisitToDay_WithNonExistentDay_ShouldThrowArgumentException()
        {
            // Arrange
            int visitId = 1;
            int dayId = 999;

            var visit = new Visit
            {
                Id = visitId,
                SubmissionId = 100,
                Submission = new Submission
                {
                    Address = new Address
                    {
                        Building = new Building()
                    }
                }
            };

            _visitRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Visit, bool>>>(),
                It.IsAny<Expression<Func<Visit, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(visit);

            _dayRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Day, bool>>>(),
                It.IsAny<Expression<Func<Day, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync((Day?)null);

            // Act & Assert
            var act = async () => await _visitService.AssignVisitToDay(visitId, dayId);
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Day with ID {dayId} not found.");
        }

        [Fact]
        public async Task AssignVisitToDay_WithSendEmailTrue_ShouldPassEmailFlagToAgendaService()
        {
            // Arrange
            int visitId = 1;
            int dayId = 1;

            var visit = new Visit
            {
                Id = visitId,
                SubmissionId = 100,
                ScheduleId = 5,
                Submission = new Submission
                {
                    Address = new Address { BuildingId = 20, Building = new Building() }
                }
            };

            var day = new Day
            {
                Id = dayId,
                Agendas = new List<Agenda> { new Agenda { Id = 10, Visits = new List<Visit>() } }
            };

            _visitRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Visit, bool>>>(),
                It.IsAny<Expression<Func<Visit, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(visit);

            _dayRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Day, bool>>>(),
                It.IsAny<Expression<Func<Day, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(day);

            // Act
            await _visitService.AssignVisitToDay(visitId, dayId, sendEmail: true);

            // Assert
            _agendaServiceMock.Verify(a => a.AssignVisitsToAgendaAsync(
                It.Is<AssignVisitsToAgendaDto>(dto => dto.SendEmails == true)), 
                Times.Once);
        }
    }
}
