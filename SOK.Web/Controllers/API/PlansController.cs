using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOK.Application.Services.Interface;
using SOK.Domain.Enums;
using SOK.Web.Filters;

namespace SOK.Web.Controllers.API
{
    [AuthorizeRoles(Role.Administrator, Role.Priest, Role.SubmitSupport)]
    [ApiController]
    [Route("api/[controller]")]
    public class PlansController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlansController(IPlanService planService)
        {
            _planService = planService;
        }

        /// <summary>
        /// Pobiera wszystkie harmonogramy z aktywnego planu
        /// </summary>
        [HttpGet("{planId}/schedules")]
        public async Task<IActionResult> GetSchedules(int planId)
        {
            try
            {
                var plan = await _planService.GetPlanAsync(planId);
                if (plan == null)
                {
                    return BadRequest(new { error = "Plan of this ID not found." });
                }

                var schedules = plan.Schedules.Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    shortName = s.ShortName,
                    color = s.Color,
                }).ToList();

                return Ok(new { schedules });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
