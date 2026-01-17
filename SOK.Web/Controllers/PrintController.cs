using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOK.Application.Services.Interface;
using SOK.Domain.Enums;
using SOK.Web.Filters;

namespace SOK.Web.Controllers
{
    [AuthorizeRoles(Role.Administrator, Role.Priest)]
    [ActivePage("Calendar")]
    public class PrintController : Controller
    {
        private readonly IPrintService _printService;
        private readonly IPlanService _planService;

        public PrintController(IPrintService printService, IPlanService planService)
        {
            _printService = printService;
            _planService = planService;
        }

        /// <summary>
        /// Generuje i zwraca PDF dla pojedynczej agendy.
        /// </summary>
        /// <param name="id">Identyfikator agendy.</param>
        /// <param name="index">Indeks agendy (numer księży).</param>
        /// <returns>Plik PDF.</returns>
        [HttpGet]
        [Route("Print/Agenda/{id}")]
        public async Task<IActionResult> Agenda(int id, [FromQuery] int index = 1)
        {
            try
            {
                var pdfBytes = await _printService.GenerateAgendaPdfAsync(id, index);
                
                var fileName = $"Agenda_Ksiadz_{index}.pdf";
                Response.Headers.Append("Content-Disposition", $"inline; filename=\"{fileName}\"");

                return File(pdfBytes, "application/pdf");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas generowania PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// Generuje i zwraca PDF dla wszystkich agend w danym dniu.
        /// </summary>
        /// <param name="id">Identyfikator dnia.</param>
        /// <returns>Plik PDF.</returns>
        [HttpGet]
        [Route("Print/Day/{id}")]
        public async Task<IActionResult> Day(int id)
        {
            try
            {
                var pdfBytes = await _printService.GenerateDayPdfAsync(id);

                var day = await _planService.GetDayAsync(id);
                string dateString = day?.Date.ToString("yyyyMMdd") ?? "?";
                var fileName = $"Plan_Dnia_{dateString}.pdf";
                Response.Headers.Append("Content-Disposition", $"inline; filename=\"{fileName}\"");

                return File(pdfBytes, "application/pdf");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas generowania PDF: {ex.Message}");
            }
        }
    }
}
