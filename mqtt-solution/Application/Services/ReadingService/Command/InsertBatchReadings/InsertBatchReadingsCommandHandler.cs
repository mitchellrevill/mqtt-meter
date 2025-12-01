using Application.Interfaces.Repositories;
using Application.Services.ReadingService.Command.CreateReading;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ReadingService.Command.InsertBatchReadings
{
    public class InsertBatchReadingsCommandHandler : IRequestHandler<InsertBatchReadingsCommand, Unit>
    {
        private readonly IReadingRepository _readingRepository;

        public InsertBatchReadingsCommandHandler(IReadingRepository readingRepository)
        {
            _readingRepository = readingRepository;
        }

        public async Task<Unit> Handle(InsertBatchReadingsCommand request, CancellationToken cancellationToken)
        {
            // Save to database
            await _readingRepository.InsertBatchAsync(request.Readings);
            return Unit.Value;
        }
    }
}
