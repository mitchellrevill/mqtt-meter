using Application.Interfaces.Repositories;
using Application.Services.ReadingService.Command.ResetReadings;
using FluentAssertions;
using MediatR;
using Moq;

namespace Application.UnitTests.ReadingTests.Commands;

public class ResetReadingsTests
{
    private readonly Mock<IReadingRepository> _readingRepository;

    public ResetReadingsTests()
    {
        _readingRepository = new();
    }

    [Fact]
    public async Task ResetReadingsCommandHandler_CallsRepositoryDelete()
    {
        // Arrange
        var userId = "test-user";
        var command = new ResetReadingsCommand(userId);
        var handler = new ResetReadingsCommandHandler(_readingRepository.Object);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        _readingRepository.Verify(r => r.DeleteByUserIdAsync(userId), Times.Once);
        result.Should().Be(Unit.Value);
    }
}
