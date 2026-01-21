using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using SOK.Application.Common.Interface;
using SOK.Application.Services.Implementation;
using SOK.Domain.Entities.Central;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using System.Linq.Expressions;
using System.Security.Claims;

namespace SOK.Application.Tests.Services
{
    public class ParishMemberServiceTests
    {
        private readonly Mock<IUnitOfWorkParish> _uowMock;
        private readonly Mock<IUnitOfWorkCentral> _uowCentralMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IParishMemberRepository> _parishMemberRepoMock;
        private readonly ParishMemberService _parishMemberService;

        public ParishMemberServiceTests()
        {
            _uowMock = new Mock<IUnitOfWorkParish>();
            _uowCentralMock = new Mock<IUnitOfWorkCentral>();
            _parishMemberRepoMock = new Mock<IParishMemberRepository>();

            // Mock UserManager (wymaga mock store)
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _uowMock.Setup(u => u.ParishMember).Returns(_parishMemberRepoMock.Object);

            _parishMemberService = new ParishMemberService(
                _uowMock.Object,
                _uowCentralMock.Object,
                _userManagerMock.Object);
        }

        [Fact]
        public async Task GetParishMemberAsync_WithValidId_ShouldReturnMember()
        {
            // Arrange
            int memberId = 1;
            var expectedMember = new ParishMember
            {
                Id = memberId,
                DisplayName = "ks. Jan Kowalski",
                CentralUserId = Guid.NewGuid().ToString()
            };

            _parishMemberRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ParishMember, bool>>>(),
                It.IsAny<Expression<Func<ParishMember, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync(expectedMember);

            // Act
            var result = await _parishMemberService.GetParishMemberAsync(memberId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(memberId);
            result.DisplayName.Should().Be("ks. Jan Kowalski");
        }

        [Fact]
        public async Task GetParishMemberAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            int memberId = 999;

            _parishMemberRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ParishMember, bool>>>(),
                It.IsAny<Expression<Func<ParishMember, object>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .ReturnsAsync((ParishMember?)null);

            // Act
            var result = await _parishMemberService.GetParishMemberAsync(memberId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetParishMemberAsync_WithValidClaim_ShouldReturnMember()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            var expectedMember = new ParishMember
            {
                Id = 1,
                DisplayName = "ks. Jan Kowalski",
                CentralUserId = userId
            };

            _userManagerMock.Setup(um => um.GetUserId(principal))
                .Returns(userId);

            _parishMemberRepoMock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ParishMember, bool>>>(),
                It.IsAny<Expression<Func<ParishMember, object>>>(),
                It.IsAny<string>(),
                true))
                .ReturnsAsync(expectedMember);

            // Act
            var result = await _parishMemberService.GetParishMemberAsync(principal);

            // Assert
            result.Should().NotBeNull();
            result!.DisplayName.Should().Be("ks. Jan Kowalski");
        }

        [Fact]
        public async Task GetParishMemberAsync_WithInvalidClaim_ShouldReturnNull()
        {
            // Arrange
            var principal = new ClaimsPrincipal();

            _userManagerMock.Setup(um => um.GetUserId(principal))
                .Returns((string?)null);

            // Act
            var result = await _parishMemberService.GetParishMemberAsync(principal);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateParishMemberAsync_WithValidMember_ShouldUpdateAndSave()
        {
            // Arrange
            var member = new ParishMember
            {
                Id = 1,
                DisplayName = "ks. Jan Nowak",
                CentralUserId = Guid.NewGuid().ToString()
            };

            // Act
            await _parishMemberService.UpdateParishMemberAsync(member);

            // Assert
            _parishMemberRepoMock.Verify(r => r.Update(member), Times.Once);
            _uowMock.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllInRoleAsync_WithPriestRole_ShouldReturnPriests()
        {
            // Arrange
            var priestId1 = Guid.NewGuid().ToString();
            var priestId2 = Guid.NewGuid().ToString();

            var users = new List<User>
            {
                new User { Id = priestId1, UserName = "priest1" },
                new User { Id = priestId2, UserName = "priest2" }
            };

            var parishMembers = new List<ParishMember>
            {
                new ParishMember { Id = 1, CentralUserId = priestId1, DisplayName = "ks. Jan" },
                new ParishMember { Id = 2, CentralUserId = priestId2, DisplayName = "ks. Piotr" }
            };

            _userManagerMock.Setup(um => um.GetUsersInRoleAsync("Priest"))
                .ReturnsAsync(users);

            _parishMemberRepoMock.Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<ParishMember, bool>>>(),
                It.IsAny<Expression<Func<ParishMember, object>>>(),
                "AssignedPlans",
                It.IsAny<bool>()))
                .ReturnsAsync(parishMembers);

            // Act
            var result = await _parishMemberService.GetAllInRoleAsync(Role.Priest);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(pm => pm.DisplayName == "ks. Jan");
            result.Should().Contain(pm => pm.DisplayName == "ks. Piotr");
        }

        [Fact]
        public async Task GetAllParishMembersAsync_WithNoFilters_ShouldReturnAllMembers()
        {
            // Arrange
            var members = new List<ParishMember>
            {
                new ParishMember { Id = 1, DisplayName = "ks. Jan" },
                new ParishMember { Id = 2, DisplayName = "ks. Piotr" },
                new ParishMember { Id = 3, DisplayName = "admin" }
            };

            _parishMemberRepoMock.Setup(r => r.GetAllAsync(
                null,
                It.IsAny<Expression<Func<ParishMember, object>>>(),
                "",
                It.IsAny<bool>()))
                .ReturnsAsync(members);

            // Act
            var result = await _parishMemberService.GetAllParishMembersAsync();

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAllParishMembersAsync_WithAgendasFlag_ShouldIncludeAgendas()
        {
            // Arrange
            var members = new List<ParishMember>
            {
                new ParishMember { Id = 1, DisplayName = "ks. Jan" }
            };

            _parishMemberRepoMock.Setup(r => r.GetAllAsync(
                null,
                It.IsAny<Expression<Func<ParishMember, object>>>(),
                It.Is<string>(s => s.Contains("AssignedAgendas")),
                It.IsAny<bool>()))
                .ReturnsAsync(members);

            // Act
            var result = await _parishMemberService.GetAllParishMembersAsync(agendas: true);

            // Assert
            result.Should().HaveCount(1);
            _parishMemberRepoMock.Verify(r => r.GetAllAsync(
                null,
                It.IsAny<Expression<Func<ParishMember, object>>>(),
                It.Is<string>(s => s.Contains("AssignedAgendas")),
                It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task GetAllParishMembersAsync_WithPlansFlag_ShouldIncludePlans()
        {
            // Arrange
            var members = new List<ParishMember>
            {
                new ParishMember { Id = 1, DisplayName = "ks. Jan" }
            };

            _parishMemberRepoMock.Setup(r => r.GetAllAsync(
                null,
                It.IsAny<Expression<Func<ParishMember, object>>>(),
                It.Is<string>(s => s.Contains("AssignedPlans")),
                It.IsAny<bool>()))
                .ReturnsAsync(members);

            // Act
            var result = await _parishMemberService.GetAllParishMembersAsync(plans: true);

            // Assert
            result.Should().HaveCount(1);
            _parishMemberRepoMock.Verify(r => r.GetAllAsync(
                null,
                It.IsAny<Expression<Func<ParishMember, object>>>(),
                It.Is<string>(s => s.Contains("AssignedPlans")),
                It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task GetUsersPaginatedAsync_WithInvalidPageSize_ShouldThrowArgumentException()
        {
            // Act
            var act = async () => await _parishMemberService.GetUsersPaginatedAsync(
                page: 1, 
                pageSize: 0);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Page size must be positive.");
        }

        [Fact]
        public async Task GetUsersPaginatedAsync_WithInvalidPage_ShouldThrowArgumentException()
        {
            // Act
            var act = async () => await _parishMemberService.GetUsersPaginatedAsync(
                page: 0, 
                pageSize: 10);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Page must be positive.");
        }
    }
}
