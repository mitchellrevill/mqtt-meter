using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.SampleEntities;
using Microsoft.AspNetCore.Mvc;

namespace DemoWeb.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadingController : ControllerBase
    {
        private readonly IReadingService _readingService;

        public ReadingController(IReadingService readingService)
        {
            _readingService = readingService;
        }

        [HttpGet]
        [Route("V1/Readings")]
        public async Task<IEnumerable<Reading>> Readings()
        {
            return await _readingService.GetAll();
        }
    }
}
