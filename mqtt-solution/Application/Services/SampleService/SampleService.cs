using Application.Interfaces;
using Application.Services.SampleService.Query.GetAllSamples;
using Domain.Entities.SampleEntities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.SampleService
{
    public class SampleService : ISampleService
    {
        private readonly ISender _sender;

        public SampleService(ISender sender)
        {
            _sender = sender;
        }

        public async Task<IEnumerable<Sample>> GetAll()
        {
            return await _sender.Send(new GetAllSamplesQuery());
        }
    }
}
