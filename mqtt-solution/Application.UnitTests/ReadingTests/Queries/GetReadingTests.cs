using Application.Interfaces.Repositories;
using Application.Services.ReadingService.Query.GetAllReadings;
using Application.Services.ReadingService.Query.GetReadingsByUser;
using Domain.Entities;
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
        [Fact]
        public async Task Get_Readings_Should_Not_Return_Null()
        {
            // Arrange
            var mockRepo = new Mock<IReadingRepository>();
            var query = new GetAllReadingsQuery();
            var handler = new GetAllReadingsQueryHandler(mockRepo.Object);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Get_Readings_By_User_Should_Not_Return_Null_If_User_Exists()
        {
            // Arrange
            var mockRepo = new Mock<IReadingRepository>();
            string userId = "user";

            mockRepo.Setup(repo => repo.GetByUserId(userId))
                .ReturnsAsync(new List<Reading> { new Reading { UserId = userId } });
            
            var query = new GetReadingsByUserQuery(userId);
            var handler = new GetReadingsByUserQueryHandler(mockRepo.Object);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Get_Readings_By_User_Should_Be_Empty_If_User_Does_Not_Exist()
        {
            // Arrange
            var mockRepo = new Mock<IReadingRepository>();

            var query = new GetReadingsByUserQuery("User");
            var handler = new GetReadingsByUserQueryHandler(mockRepo.Object);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
