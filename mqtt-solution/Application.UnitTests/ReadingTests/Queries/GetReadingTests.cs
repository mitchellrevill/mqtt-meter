using Application.Interfaces.Repositories;
using Application.Services.ReadingService.Query.GetAllReadings;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UnitTests.ReadingTests.Queries
{
    public class GetReadingTests
    {
        private readonly Mock<IReadingRepository> _readingRepository;

        public GetReadingTests()
        {
            _readingRepository = new();
        }

        [Fact]
        public async Task foo()
        {
            // Arrange
            var query = new GetAllReadingsQuery();
            var handler = new GetAllReadingsQueryHandler(_readingRepository.Object);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.Should().NotBeNull();
        }
    }
}
