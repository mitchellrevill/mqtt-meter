using Application.Interfaces.Repositories;
using Application.Services.ClientService.GetAllClients;
using Application.Services.ReadingService.Query.GetAllReadings;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.ClientTests.Queries;

public class GetAllClientsTests
{
    [Fact]
    public async Task Get_Clients_Should_Not_Return_Null()
    {
        // Arrange
        var mockRepo = new Mock<IClientRepository>();
        var query = new GetAllClientsQuery();
        var handler = new GetAllClientsQueryHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_Clients_Should_Not_Have_Null_Readings_For_Any_Client()
    {
        // Arrange
        var mockRepo = new Mock<IClientRepository>();

        List<Client> mockClients = new List<Client>
        {
            new Client(),
            new Client(),
            new Client(),
            new Client(),
            new Client(),
            new Client(),
        };
        mockRepo.Setup(repo => repo.GetAll())
                .ReturnsAsync(mockClients);

        var query = new GetAllClientsQuery();
        var handler = new GetAllClientsQueryHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        foreach (var r in result)
        {
            r.Readings.Should().NotBeNull();
        }
    }
}
        
