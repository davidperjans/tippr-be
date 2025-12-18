using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Auth.Queries.GetMe;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace Application.Tests.Features.Auth.Queries;

public sealed class GetCurrentUserQueryHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _handler = new GetCurrentUserQueryHandler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_User_When_User_Exists()
    {
        // Arrange
        var authUserId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var lastLoginAt = DateTime.UtcNow.AddDays(-1);
        var user = new User
        {
            Id = userId,
            AuthUserId = authUserId,
            Email = "test@example.com",
            DisplayName = "Test User",
            LastLoginAt = lastLoginAt,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        _authServiceMock
            .Setup(x => x.GetByAuthUserIdAsync(authUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetCurrentUserQuery(authUserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.UserId.Should().Be(userId);
        result.Data.Email.Should().Be("test@example.com");
        result.Data.DisplayName.Should().Be("Test User");
        result.Data.LastLoginAt.Should().Be(lastLoginAt);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_User_Not_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _authServiceMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetCurrentUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be("user not synced");
        result.Error.Code.Should().Be("user.not_synced");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Update_Last_Login_When_User_Exists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var authUserId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            AuthUserId = authUserId,
            Email = "test@example.com",
            DisplayName = "Test User",
            LastLoginAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        _authServiceMock
            .Setup(x => x.GetByAuthUserIdAsync(authUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetCurrentUserQuery(authUserId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _authServiceMock.Verify(
            x => x.UpdateLastLoginAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Not_Update_Last_Login_When_User_Not_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _authServiceMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetCurrentUserQuery(userId);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _authServiceMock.Verify(
            x => x.UpdateLastLoginAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Use_CreatedAt_When_LastLoginAt_Is_Null()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var authUserId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddMonths(-1);
        var user = new User
        {
            Id = userId,
            AuthUserId = authUserId,
            Email = "test@example.com",
            DisplayName = "New User",
            LastLoginAt = null,
            CreatedAt = createdAt
        };

        _authServiceMock
            .Setup(x => x.GetByAuthUserIdAsync(authUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetCurrentUserQuery(authUserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.LastLoginAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_DisplayName_When_User_Has_No_DisplayName()
    {
        // Arrange
        var authUserId = Guid.NewGuid();
        var user = new User
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            Email = "test@example.com",
            DisplayName = null!,
            LastLoginAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        _authServiceMock
            .Setup(x => x.GetByAuthUserIdAsync(authUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetCurrentUserQuery(authUserId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.DisplayName.Should().BeNull();
    }
}
