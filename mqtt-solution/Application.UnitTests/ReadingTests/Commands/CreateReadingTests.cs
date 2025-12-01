using System;
using Application.Interfaces.Repositories;
using Application.Services.ReadingService.Command.CreateReading;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.ReadingTests.Commands;

public class CreateReadingTests
{
    [Fact]
    public async Task Creating_Reading_Should_Create_Accurate_Timestamp()
    {
        // Arrange
        var mockRepo = new Mock<IReadingRepository>();
        var userId = "test-user";
        float value = 1.5F;
        var command = new CreateReadingCommand(userId, value);

        Domain.Entities.Reading capturedReading = null;

        mockRepo.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Reading>()))
                .Callback<Domain.Entities.Reading>(r => capturedReading = r);

        var handler = new CreateReadingCommandHandler(mockRepo.Object);

        // Act
        await handler.Handle(command, default);

        // Assert
        capturedReading.TimeStamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(2));
    }

    [Fact]
    public async Task Creating_Reading_UserId_Should_Not_Be_Null()
    {
        // Arrange
        var mockRepo = new Mock<IReadingRepository>();
        var userId = "";
        float value = 1.5F;
        var command = new CreateReadingCommand(userId, value);

        Domain.Entities.Reading capturedReading = null;

        mockRepo.Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Reading>()))
                .Callback<Domain.Entities.Reading>(r => capturedReading = r);

        var handler = new CreateReadingCommandHandler(mockRepo.Object);

        // Act
        await handler.Handle(command, default);

        // Assert
        capturedReading.UserId.Should().NotBeNull();
    }
}
