using Infrastructure.Services.Mocking;
using Infrastructure.Services;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Data.ClientTests
{
    public class ClientTests : BaseEfRepoTest
    {

        public ClientTests()
        {
            
        }

        public void GenerateClients(int count)
        {
            var mockData = new ClientGenerator(_dbContext).Generate(count);
            GeneratorHelper.AddEntities<Client>(mockData, _dbContext);
        }

        [Theory]
        [InlineData(10)]
        public void Test_Clients_Generator_Should_Not_Generate_Null(int generateCount)
        {
            // Arrange
            List<Client> mockData;
            IEnumerable<Client> clients;

            // Act
            GenerateClients(generateCount);
            clients = _dbContext.Client.Include(c => c.Readings);

            // Assert
            clients
                .Should()
                .NotBeEmpty();
        }

        [Theory]
        [InlineData(10)]
        public void Test_Clients_Generator_Should_Not_Exceed_Limit(int generateCount)
        {
            // Arrange
            List<Client> mockData;
            IEnumerable<Client> clients;

            // Act
            GenerateClients(generateCount);
            clients = _dbContext.Client.Include(c => c.Readings);

            // Assert
            clients
                .Should()
                .HaveCountLessThanOrEqualTo(generateCount);
        }

        [Theory]
        [InlineData(10)]
        public void Test_Clients_Generator_Should_Not_Generate_Empty_Readings(int generateCount)
        {
            // Arrange
            List<Client> mockData;
            IEnumerable<Client> clients;

            // Act
            GenerateClients(generateCount);
            clients = _dbContext.Client.Include(c => c.Readings);

            // Assert
            clients
                .Should()
                .AllSatisfy(client => client.Readings.Should().NotBeEmpty());
        }
    }
}
