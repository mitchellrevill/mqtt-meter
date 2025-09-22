using Application.Interfaces;
using Application.Services.ReadingService.Query.GetAllReadings;
using Application.Services.SampleService.Query.GetAllSamples;
using Domain.Entities;
using Domain.Entities.SampleEntities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ReadingService
{
    public class ReadingService : IReadingService
    {
        private readonly ISender _sender;

        public ReadingService(ISender sender)
        {
            _sender = sender;
        }

        public async Task<IEnumerable<Reading>> GetAll()
        {
            return await _sender.Send(new GetAllReadingsQuery());
        }
    }
}
