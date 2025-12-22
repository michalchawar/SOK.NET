using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SOK.Application.Services.Interface;
using SOK.Domain.Enums;
using SOK.Web.Filters;

namespace SOK.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthorizeRoles]
    public class StreetsController : ControllerBase
    {
        private readonly IStreetService _streetService;

        public StreetsController(IStreetService streetService)
        {
            _streetService = streetService;
        }

        // GET: api/streets/building-options
        [HttpGet("building-options")]
        public async Task<IActionResult> GetAllWithBuildingOptions()
        {
            var streets = await _streetService.GetAllStreetsAsync(buildings: true);

            var buildingOptions = streets.ToDictionary(
                s => s.Id,
                s => s.Buildings.OrderBy(b => b.Number).ThenBy(b => b.Letter).Select(b => new SelectListItem
                {
                    Text = b.Number + (b.Letter ?? ""),
                    Value = b.Id.ToString()
                })
            );

            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(buildingOptions));
        }

        // GET: api/streets
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var streets = await _streetService.GetAllStreetsAsync(type: true);
            
            var result = streets.Select(s => new
            {
                id = s.Id,
                name = s.Name,
                type = s.Type?.FullName ?? string.Empty
            }).OrderBy(s => s.name);

            return Ok(result);
        }

        // GET: api/streets/5/buildings
        [HttpGet("{id}/buildings")]
        public async Task<IActionResult> GetBuildings(int id)
        {
            var streets = await _streetService.GetAllStreetsAsync(
                filter: s => s.Id == id,
                buildings: true);
            
            var street = streets.FirstOrDefault();
            
            if (street == null)
            {
                return NotFound(new { message = "Ulica nie została znaleziona." });
            }

            var result = street.Buildings
                .OrderBy(b => b.Number)
                .ThenBy(b => b.Letter)
                .Select(b => new
                {
                    id = b.Id,
                    number = b.Number,
                    letter = b.Letter ?? string.Empty
                });

            return Ok(result);
        }

        // POST: api/streets/city?name=...
        [HttpPost("city")]
        public async Task<IActionResult> CreateCity(string name)
        {
            await _streetService.CreateCityAsync(name);
            return Ok();
        }
    }
}
