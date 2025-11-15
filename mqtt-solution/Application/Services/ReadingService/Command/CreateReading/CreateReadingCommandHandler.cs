using Domain.Entities;
using MediatR;
using Application.Interfaces.Repositories;

namespace Application.Services.ReadingService.Command.CreateReading
{
    public class CreateReadingCommandHandler : IRequestHandler<CreateReadingCommand, Reading>
    {
        private readonly IReadingRepository _readingRepository;

        public CreateReadingCommandHandler(IReadingRepository readingRepository)
        {
            _readingRepository = readingRepository;
        }

        public async Task<Reading> Handle(CreateReadingCommand request, CancellationToken cancellationToken)
        {
            // Create new Reading domain entity
            var reading = new Reading
            {
                TimeStamp = DateTime.UtcNow,
                Value = request.Value
            };

            // Save to database
            return await _readingRepository.AddAsync(reading);
        }
    }
}