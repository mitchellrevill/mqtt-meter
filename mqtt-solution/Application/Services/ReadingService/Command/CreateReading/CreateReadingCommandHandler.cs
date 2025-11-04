using Domain.Entities;
using MediatR;

namespace Application.Services.ReadingService.Command.CreateReading
{
    public class CreateReadingCommandHandler : IRequestHandler<CreateReadingCommand, Reading>
    {
        public Task<Reading> Handle(CreateReadingCommand request, CancellationToken cancellationToken)
        {
            // Create new Reading domain entity
            var reading = new Reading
            {
                TimeStamp = DateTime.UtcNow,
                Value = request.Value
            };

            // TODO: Add repository call to persist the reading when repository is implemented
            // await _readingRepository.AddAsync(reading);

            return Task.FromResult(reading);
        }
    }
}