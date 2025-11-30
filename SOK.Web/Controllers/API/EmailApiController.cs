using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOK.Application.Common.Helpers;
using SOK.Application.Common.Helpers.EmailTypes;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;

namespace SOK.Web.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/email")]
    public class EmailApiController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ISubmissionService _submissionService;
        private readonly IParishInfoService _parishInfoService;
        public EmailApiController(IEmailService emailService, ISubmissionService submissionService, IParishInfoService parishInfoService)
        {
            _emailService = emailService;
            _submissionService = submissionService;
            _parishInfoService = parishInfoService;

            _emailService.SetSMTPTimeout(5000);
        }

        [HttpPost("queue/confirmation")]
        public async Task<IActionResult> QueueEmail([FromBody] ConfirmationEmailRequest request)
        {
            return await QueueGenericEmail(request);
        }

        [HttpPost("queue/invalid_email")]
        public async Task<IActionResult> QueueEmail([FromBody] InvalidEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To))
            {
                return BadRequest(new { success = false, message = "Nie podano adresu email do wysłania." });
            }

            return await QueueGenericEmail(request);
        }

        [HttpPost("preview/confirmation")]
        public async Task<IActionResult> PreviewEmail([FromBody] ConfirmationEmailRequest request)
        {
            return await PreviewGenericEmail(request);
        }
        
        [HttpPost("preview/invalid_email")]
        public async Task<IActionResult> PreviewEmail([FromBody] InvalidEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To))
            {
                return BadRequest(new { success = false, message = "Nie podano adresu email do podglądu." });
            }

            return await PreviewGenericEmail(request);
        }

        private async Task<IActionResult> QueueGenericEmail(EmailRequest request) 
        {
            try {
                bool sent = false;

                switch (request)
                {
                    case ConfirmationEmailRequest confirmationRequest:
                        sent = await _submissionService.SendConfirmationEmailAsync(confirmationRequest.SubmissionId);
                        break;
                    case InvalidEmailRequest invalidRequest:
                        sent = await _submissionService.SendInvalidEmailAsync(invalidRequest.SubmissionId, invalidRequest.To);
                        break;
                    default:
                        return BadRequest(new { success = false, message = "Nieznany typ żądania email." });
                }

                if (sent)
                {
                    return Ok(new { success = true, message = "Email został wysłany." });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Nie udało się wysłać maila, został dodany do kolejki." });
                }
            } catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd podczas kolejkowania emaila: {ex.Message}" });
            }
        }
        
        private async Task<IActionResult> PreviewGenericEmail(EmailRequest request) 
        {
            try {
                string html = string.Empty;

                switch (request)
                {
                    case ConfirmationEmailRequest confirmationRequest:
                        html = await _submissionService.PreviewConfirmationEmailAsync(confirmationRequest.SubmissionId);
                        break;
                    case InvalidEmailRequest invalidRequest:
                        html = await _submissionService.PreviewInvalidEmailAsync(invalidRequest.SubmissionId, invalidRequest.To);
                        break;
                    default:
                        return BadRequest(new { success = false, message = "Nieznany typ żądania email." });
                }

                if (!string.IsNullOrEmpty(html))
                {
                    return Ok(new { success = true, html = html });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Nie udało się wygenerować podglądu emaila." });
                }
            } catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd podczas generowania podglądu emaila: {ex.Message}" });
            }
        }

        /// <summary>
        /// Próbuje natychmiast wysłać email z kolejki
        /// </summary>
        /// <param name="emailLogId">ID EmailLog do wysłania</param>
        /// <returns>Status wysyłki</returns>
        [HttpPost("send/{emailLogId}")]
        public async Task<IActionResult> SendEmail(int emailLogId)
        {
            try
            {
                bool sent = await _emailService.SendEmailAsync(emailLogId);

                if (sent)
                {
                    return Ok(new { success = true, message = "Email został wysłany." });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Nie udało się wysłać emaila." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd podczas wysyłania emaila: {ex.Message}" });
            }
        }
    }

    public abstract class EmailRequest {
        public int SubmissionId { get; set; }
        public int Priority { get; set; } = 5;
        public bool ForceSend { get; set; } = true;
    } 

    public class ConfirmationEmailRequest : EmailRequest
    {
    }

    public class InvalidEmailRequest : EmailRequest
    {
        public string To { get; set; } = string.Empty;
    }
}
