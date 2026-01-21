using FluentAssertions;
using Moq;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Implementation;
using SOK.Domain.Entities.Parish;
using System.Linq.Expressions;

namespace SOK.Application.Tests.Services
{
    public class PlanServiceTests
    {
        private readonly Mock<IUnitOfWorkParish> _uowMock;
        private readonly Mock<IUnitOfWorkCentral> _uowCentralMock;
        private readonly Mock<IPlanRepository> _planRepoMock;
        private readonly Mock<IParishInfoRepository> _parishInfoRepoMock;
        private readonly PlanService _planService;

        public PlanServiceTests()
        {
            _uowMock = new Mock<IUnitOfWorkParish>();
            _uowCentralMock = new Mock<IUnitOfWorkCentral>();
            _planRepoMock = new Mock<IPlanRepository>();
            _parishInfoRepoMock = new Mock<IParishInfoRepository>();

            _uowMock.Setup(u => u.Plan).Returns(_planRepoMock.Object);
            _uowMock.Setup(u => u.ParishInfo).Returns(_parishInfoRepoMock.Object);

            _planService = new PlanService(_uowMock.Object, _uowCentralMock.Object);
        }

        [Fact]
        public async Task GetPlanAsync_WithValidId_ShouldReturnPlan()
        {
            // Arrange
            int planId = 1;
            var expectedPlan = new Plan
            {
                Id = planId,
                Name = "Kolęda 2026",
                Schedules = new List<Schedule>()
            };

            _planRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Plan, bool>>>(),
                It.IsAny<Expression<Func<Plan, object>>>(),
                "Schedules",
                It.IsAny<bool>()))
                .ReturnsAsync(expectedPlan);

            // Act
            var result = await _planService.GetPlanAsync(planId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(planId);
            result.Name.Should().Be("Kolęda 2026");
            result.Schedules.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPlanAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            int planId = 999;

            _planRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Plan, bool>>>(),
                It.IsAny<Expression<Func<Plan, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync((Plan?)null);

            // Act
            var result = await _planService.GetPlanAsync(planId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreatePlanAsync_WithValidPlan_ShouldAddAndSave()
        {
            // Arrange
            var plan = new Plan
            {
                Name = "Kolęda 2027"
            };

            // Act
            await _planService.CreatePlanAsync(plan);

            // Assert
            _planRepoMock.Verify(r => r.Add(plan), Times.Once);
            _uowMock.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeletePlanAsync_WithExistingPlan_ShouldRemoveAndReturnTrue()
        {
            // Arrange
            int planId = 1;
            var plan = new Plan
            {
                Id = planId,
                Name = "Plan do usunięcia"
            };

            _planRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Plan, bool>>>(),
                It.IsAny<Expression<Func<Plan, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(plan);

            // Act
            var result = await _planService.DeletePlanAsync(planId);

            // Assert
            result.Should().BeTrue();
            _planRepoMock.Verify(r => r.Remove(plan), Times.Once);
            _uowMock.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeletePlanAsync_WithNonExistentPlan_ShouldReturnTrue()
        {
            // Arrange
            int planId = 999;

            _planRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Plan, bool>>>(),
                It.IsAny<Expression<Func<Plan, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync((Plan?)null);

            // Act
            var result = await _planService.DeletePlanAsync(planId);

            // Assert
            result.Should().BeTrue();
            _planRepoMock.Verify(r => r.Remove(It.IsAny<Plan>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePlanAsync_WithValidPlan_ShouldUpdateAndSave()
        {
            // Arrange
            var plan = new Plan
            {
                Id = 1,
                Name = "Zaktualizowana nazwa"
            };

            // Act
            await _planService.UpdatePlanAsync(plan);

            // Assert
            _planRepoMock.Verify(r => r.Update(plan), Times.Once);
            _uowMock.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task SetActivePlanAsync_WithExistingPlan_ShouldSetActiveAndSave()
        {
            // Arrange
            var plan = new Plan
            {
                Id = 5,
                Name = "Aktywny plan",
                Schedules = new List<Schedule>()
            };

            _planRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Plan, bool>>>(),
                It.IsAny<Expression<Func<Plan, object>>>(),
                "Schedules",
                It.IsAny<bool>()))
                .ReturnsAsync(plan);

            // Act
            await _planService.SetActivePlanAsync(plan);

            // Assert
            _parishInfoRepoMock.Verify(r => r.SetValueAsync("ActivePlanId", "5"), Times.Once);
            _uowMock.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task SetActivePlanAsync_WithNonExistentPlan_ShouldThrowArgumentException()
        {
            // Arrange
            var plan = new Plan { Id = 999, Name = "Nieistniejący plan" };

            _planRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Plan, bool>>>(),
                It.IsAny<Expression<Func<Plan, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync((Plan?)null);

            // Act
            var act = async () => await _planService.SetActivePlanAsync(plan);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Plan does not exist in the database.");
        }

        [Fact]
        public async Task GetActivePlanAsync_WhenActivePlanExists_ShouldReturnPlan()
        {
            // Arrange
            var activePlan = new Plan
            {
                Id = 3,
                Name = "Aktywny plan 2026",
                Schedules = new List<Schedule>()
            };

            _parishInfoRepoMock.Setup(r => r.GetValueAsync("ActivePlanId"))
                .ReturnsAsync("3");

            _planRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Plan, bool>>>(),
                It.IsAny<Expression<Func<Plan, object>>>(),
                "Schedules",
                It.IsAny<bool>()))
                .ReturnsAsync(activePlan);

            // Act
            var result = await _planService.GetActivePlanAsync();

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(3);
            result.Name.Should().Be("Aktywny plan 2026");
        }

        [Fact]
        public async Task GetActivePlanAsync_WhenNoActivePlan_ShouldReturnNull()
        {
            // Arrange
            _parishInfoRepoMock.Setup(r => r.GetValueAsync("ActivePlanId"))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _planService.GetActivePlanAsync();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ClearActivePlanAsync_ShouldClearValueAndSave()
        {
            // Act
            await _planService.ClearActivePlanAsync();

            // Assert
            _parishInfoRepoMock.Verify(r => r.ClearValueAsync("ActivePlanId"), Times.Once);
            _uowMock.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetPlansPaginatedAsync_WithInvalidPageSize_ShouldThrowArgumentException()
        {
            // Act
            var act = async () => await _planService.GetPlansPaginatedAsync(page: 1, pageSize: 0);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Page size must be positive.");
        }

        [Fact]
        public async Task GetPlansPaginatedAsync_WithInvalidPage_ShouldThrowArgumentException()
        {
            // Act
            var act = async () => await _planService.GetPlansPaginatedAsync(page: 0, pageSize: 10);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Page must be positive.");
        }

        [Fact]
        public async Task IsSubmissionGatheringEnabledAsync_WhenEnabled_ShouldReturnTrue()
        {
            // Arrange
            var plan = new Plan { Id = 1, Name = "Test Plan" };

            _parishInfoRepoMock.Setup(r => r.GetMetadataAsync(plan, "IsSubmissionGatheringEnabled"))
                .ReturnsAsync("true");

            // Act
            var result = await _planService.IsSubmissionGatheringEnabledAsync(plan);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsSubmissionGatheringEnabledAsync_WhenDisabled_ShouldReturnFalse()
        {
            // Arrange
            var plan = new Plan { Id = 1, Name = "Test Plan" };

            _parishInfoRepoMock.Setup(r => r.GetMetadataAsync(plan, "IsSubmissionGatheringEnabled"))
                .ReturnsAsync("false");

            // Act
            var result = await _planService.IsSubmissionGatheringEnabledAsync(plan);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsSubmissionGatheringEnabledAsync_WhenNotSet_ShouldReturnFalse()
        {
            // Arrange
            var plan = new Plan { Id = 1, Name = "Test Plan" };

            _parishInfoRepoMock.Setup(r => r.GetMetadataAsync(plan, "IsSubmissionGatheringEnabled"))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _planService.IsSubmissionGatheringEnabledAsync(plan);

            // Assert
            result.Should().BeFalse();
        }
    }
}
