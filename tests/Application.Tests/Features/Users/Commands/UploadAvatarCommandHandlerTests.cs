using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Users.Commands.UploadAvatar;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Users.Commands
{
    public sealed class UploadAvatarCommandHandlerTests
    {
        private static IFormFile CreateFakeImageFile(string contentType = "image/jpeg")
        {
            var bytes = new byte[] { 1, 2, 3, 4, 5 };
            var stream = new MemoryStream(bytes);

            return new FormFile(stream, 0, bytes.Length, "file", "avatar.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        [Fact]
        public async Task Handle_Should_Update_AvatarUrl_And_Return_Url_When_User_Exists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var file = CreateFakeImageFile("image/jpeg");
            var expectedUrl = "https://slviwtmhekjzyuzkgnkp.supabase.co/storage/v1/object/public/avatars/" +
                              $"{userId}/avatar.jpg";

            var users = new List<User>
            {
                new() { Id = userId, AvatarUrl = null }
            };

            var usersDbSetMock = users.BuildMockDbSet();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Users).Returns(usersDbSetMock.Object);
            dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var storageMock = new Mock<IAvatarStorage>();
            storageMock
                .Setup(x => x.UploadUserAvatarAsync(
                    userId,
                    It.IsAny<Stream>(),
                    "image/jpeg",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<string>.Success(expectedUrl));

            var handler = new UploadAvatarCommandHandler(dbMock.Object, storageMock.Object);

            var cmd = new UploadAvatarCommand(userId, file);

            // Act
            var result = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(expectedUrl);

            users[0].AvatarUrl.Should().Be(expectedUrl);

            storageMock.Verify(x => x.UploadUserAvatarAsync(
                userId,
                It.IsAny<Stream>(),
                "image/jpeg",
                It.IsAny<CancellationToken>()), Times.Once);

            dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Return_NotFound_When_User_Does_Not_Exist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var file = CreateFakeImageFile("image/jpeg");

            var users = new List<User>(); // tom DB
            var usersDbSetMock = users.BuildMockDbSet();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Users).Returns(usersDbSetMock.Object);

            var storageMock = new Mock<IAvatarStorage>();

            var handler = new UploadAvatarCommandHandler(dbMock.Object, storageMock.Object);

            var cmd = new UploadAvatarCommand(userId, file);

            // Act
            var result = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Code.Should().Be("user.not_found");
            result.Error.Type.Should().Be(ErrorType.NotFound);

            storageMock.Verify(x => x.UploadUserAvatarAsync(
                It.IsAny<Guid>(),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);

            dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_Return_Failure_And_Not_Save_When_Upload_Fails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var file = CreateFakeImageFile("image/jpeg");

            var users = new List<User>
            {
                new() { Id = userId, AvatarUrl = null }
            };

            var usersDbSetMock = users.BuildMockDbSet();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Users).Returns(usersDbSetMock.Object);

            var storageMock = new Mock<IAvatarStorage>();
            storageMock
                .Setup(x => x.UploadUserAvatarAsync(
                    userId,
                    It.IsAny<Stream>(),
                    "image/jpeg",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<string>.Failure("upload failed", "avatar.upload_failed"));

            var handler = new UploadAvatarCommandHandler(dbMock.Object, storageMock.Object);
            var cmd = new UploadAvatarCommand(userId, file);

            // Act
            var result = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Code.Should().Be("avatar.upload_failed");
            result.Error.Type.Should().Be(ErrorType.Failure);

            // Avatar ska inte uppdateras
            users[0].AvatarUrl.Should().BeNull();

            // DB ska inte spara när upload failar
            dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

            storageMock.Verify(x => x.UploadUserAvatarAsync(
                userId,
                It.IsAny<Stream>(),
                "image/jpeg",
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
