using Application.Interfaces.Repositories;
using Domain.Entities.SampleEntities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.SampleService.Query.GetAllSamples
{
    public class GetAllSamplesQueryHandler : IRequestHandler<GetAllSamplesQuery, IEnumerable<Sample>>
    {
        private readonly ISampleRepository _sampleRepository;

        public GetAllSamplesQueryHandler(ISampleRepository sampleRepository)
        {
            _sampleRepository = sampleRepository;
        }

        public async Task<IEnumerable<Sample>> Handle(GetAllSamplesQuery request, CancellationToken cancellationToken)
        {
            var AllSamples = await _sampleRepository.GetAll();
            return AllSamples;
        }
    }
}
