using Infrastructure.Services.Mocking;
using Infrastructure.Services;
using Domain.Entities;
using FluentAssertions;

namespace Infrastructure.IntegrationTests.Data.ReadingTests
{
    public class ReadingTests : BaseEfRepoTest
    {

        public ReadingTests()
        {
            
        }

        public async Task GenerateReadings(int count)
        {
            var mockReadings = new ReadingGenerator().Generate(50);
            GeneratorHelper.AddEntities<Reading>(mockReadings, _dbContext);
        }

        [Theory]
        [InlineData(50)]
        public async Task Test_Reading_Generator_Should_Not_Generate_Null(int generateCount)
        {
            // Arrange
            List<Reading> mockReadings;

            // Act
            await GenerateReadings(generateCount);

            // Assert
            _dbContext.Reading
                .Should()
                .NotBeEmpty();
        }

        [Theory]
        [InlineData(50)]
        public async Task Test_Reading_Generator_Should_Not_Exceed_Limit(int generateCount)
        {
            // Arrange
            List<Reading> mockReadings;

            // Act
            await GenerateReadings(generateCount);

            // Assert
            _dbContext.Reading
                .Should()
                .HaveCountLessThanOrEqualTo(generateCount);
        }

        [Theory]
        [InlineData(50)]
        public async Task Test_Reading_Generator_Should_Not_Generate_Negative_Value(int generateCount)
        {
            // Arrange
            List<Reading> mockReadings;

            // Act
            await GenerateReadings(generateCount);

            // Assert
            _dbContext.Reading
                .Should()
                .AllSatisfy(reading => reading.Value.Should().BeGreaterThanOrEqualTo(0));
        }

        [Theory]
        [InlineData(50)]
        public async Task Test_Reading_Generator_Should_Not_Generate_Future_Timestamp(int generateCount)
        {
            // Arrange
            List<Reading> mockReadings;

            // Act
            await GenerateReadings(generateCount);

            // Assert
            _dbContext.Reading
                .Should()
                .AllSatisfy(reading => reading.TimeStamp.Should().BeBefore(DateTime.Now));
        }
    }
}
