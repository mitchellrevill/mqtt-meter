using Application.Interfaces;
using Application.Services.ReadingService.Query.GetAllReadings;
using Application.Services.ReadingService.Command.CreateReading;
using Application.Services.ReadingService.Command.ResetReadings;
using Application.Services.ReadingService.Query.GetReadingsByUser;
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

        public async Task<IEnumerable<Reading>> GetByUserId(string userId)
        {
            return await _sender.Send(new GetReadingsByUserQuery(userId));
        }


        public async Task<Reading> CreateAsync(string userId, float value)
        {
            return await _sender.Send(new CreateReadingCommand(userId, value));
        }

        public async Task ResetForUserAsync(string userId)
        {
            await _sender.Send(new ResetReadingsCommand(userId));
        }
    }
}
