using Application.Interfaces;
using Domain.Entities.SampleEntities;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DemoWeb.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleController : ControllerBase
    {
        private readonly ISampleService _sampleService;

        public SampleController(ISampleService sampleService)
        {
            _sampleService = sampleService;
        }

        [HttpGet]
        [Route("V1/Samples")]
        public async Task<IEnumerable<Sample>> Samples()
        {
            return await _sampleService.GetAll();
        }
    }
}
