using Domain.Entities;
using MediatR;

namespace Application.Services.ReadingService.Command.CreateReading
{
    public record CreateReadingCommand(string UserId, float Value) : IRequest<Reading>;
}