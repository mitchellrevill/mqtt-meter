using Domain.Entities.SampleEntities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.SampleService.Query.GetAllSamples
{
    public record GetAllSamplesQuery() : IRequest<IEnumerable<Sample>>;
}
