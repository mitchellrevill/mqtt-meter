using Application.Interfaces.Repositories;
using Application.Services.SampleService.Query.GetAllSamples;
using Domain.Entities;
using Domain.Entities.SampleEntities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ReadingService.Query.GetAllReadings
{
    public class GetAllReadingsQueryHandler : IRequestHandler<GetAllReadingsQuery, IEnumerable<Reading>>
    {
        private readonly IReadingRepository _readingRepository;

        public GetAllReadingsQueryHandler(IReadingRepository readingRepository)
        {
            _readingRepository = readingRepository;
        }

        public async Task<IEnumerable<Reading>> Handle(GetAllReadingsQuery request, CancellationToken cancellationToken)
        {
            return await _readingRepository.GetAll(); ;
        }
    }
}
