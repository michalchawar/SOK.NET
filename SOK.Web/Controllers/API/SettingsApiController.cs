using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOK.Application.Common.Helpers;
using SOK.Application.Services.Interface;
using SOK.Domain.Enums;
using SOK.Web.Filters;

namespace SOK.Web.Controllers.Api
{
    [ApiController]
    [Route("api/settings")]
    [AuthorizeRoles]
    public class SettingsApiController : ControllerBase
    {
        private readonly IParishInfoService _parishInfoService;

        public SettingsApiController(IParishInfoService parishInfoService)
        {
            _parishInfoService = parishInfoService;
        }

        [HttpGet("get/uid")]
        public async Task<IActionResult> GetParishUid()
        {
            var parishUid = await _parishInfoService.GetValueAsync(InfoKeys.Parish.UniqueId);
            return Ok(new { parishUid });
        }

        [HttpPut("update")]
        [AuthorizeRoles(Role.Administrator, Role.Priest)]
        public async Task<IActionResult> UpdateSetting([FromBody] UpdateSettingRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Key))
            {
                return BadRequest(new { success = false, message = "Klucz ustawienia jest wymagany." });
            }

            try
            {
                await _parishInfoService.UpdateValueAsync(request.Key, request.Value);
                return Ok(new { success = true, message = "Ustawienie zostało zaktualizowane." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd podczas aktualizacji: {ex.Message}" });
            }
        }

        [HttpPut("update-batch")]
        [AuthorizeRoles(Role.Administrator, Role.Priest)]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
        {
            if (request.Settings == null || request.Settings.Count == 0)
            {
                return BadRequest(new { success = false, message = "Brak ustawień do aktualizacji." });
            }

            try
            {
                foreach (var setting in request.Settings)
                {
                    await _parishInfoService.UpdateValueAsync(setting.Key, setting.Value);
                }
                
                return Ok(new { success = true, message = $"Zaktualizowano {request.Settings.Count} ustawień." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Błąd podczas aktualizacji: {ex.Message}" });
            }
        }
    }

    public class UpdateSettingRequest
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class UpdateSettingsRequest
    {
        public List<UpdateSettingRequest> Settings { get; set; } = new();
    }
}
