using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ReadingService.Command.InsertBatchReadings
{
    public record InsertBatchReadingsCommand(string UserId, List<Reading> Readings) : IRequest<Unit>;
}
