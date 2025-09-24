using Application.Interfaces;
using Application.Services.ClientService.GetAllClients;
using Application.Services.ReadingService.Query.GetAllReadings;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ClientService
{
    public class ClientService : IClientService
    {
        private readonly ISender _sender;

        public ClientService(ISender sender)
        {
            _sender = sender;
        }

        public async Task<IEnumerable<Client>> GetAll()
        {
            return await _sender.Send(new GetAllClientsQuery());
        }
    }
}
