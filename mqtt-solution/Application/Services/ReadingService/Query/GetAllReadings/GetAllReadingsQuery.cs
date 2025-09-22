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
    public record GetAllReadingsQuery() : IRequest<IEnumerable<Reading>>;

}
