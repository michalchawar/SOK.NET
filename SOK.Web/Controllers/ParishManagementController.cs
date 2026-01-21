using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOK.Application.Services.Interface;
using SOK.Domain.Enums;
using SOK.Infrastructure.Provisioning;
using SOK.Web.Filters;
using SOK.Web.ViewModels.ParishManagement;

namespace SOK.Web.Controllers
{
    /// <summary>
    /// Kontroler zarządzania parafiami dla użytkowników z rolą SuperAdmin.
    /// </summary>
    [AuthorizeRoles(Role.SuperAdmin)]
    [ActivePage("ParishManagement")]
    public class ParishManagementController : Controller
    {
        private readonly ISuperAdminService _superAdminService;
        private readonly IParishProvisioningService _parishProvisioningService;

        public ParishManagementController(
            ISuperAdminService superAdminService,
            IParishProvisioningService parishProvisioningService)
        {
            _superAdminService = superAdminService;
            _parishProvisioningService = parishProvisioningService;
        }

        /// <summary>
        /// Wyświetla listę wszystkich parafii w systemie.
        /// </summary>
        /// <returns>Widok z listą parafii.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var parishes = await _superAdminService.GetAllParishesAsync();
            
            var viewModel = new ParishListViewModel
            {
                Parishes = parishes.Select(p => new ParishItemViewModel
                {
                    Id = p.Id,
                    UniqueId = p.UniqueId.ToString(),
                    Name = p.ParishName
                }).ToList()
            };

            // Sprawdź, czy jest wybrana parafia
            if (Request.Cookies.TryGetValue("SelectedParishUid", out var selectedParishUid))
            {
                viewModel.SelectedParishUid = selectedParishUid;
            }

            return View(viewModel);
        }

        /// <summary>
        /// Ustawia wybraną parafię dla superadministratora.
        /// </summary>
        /// <param name="parishUid">Unikalny identyfikator parafii.</param>
        /// <returns>Przekierowanie do listy parafii.</returns>
        [HttpPost]
        public IActionResult SelectParish(string parishUid)
        {
            if (string.IsNullOrEmpty(parishUid))
            {
                return BadRequest("Parish UID is required.");
            }

            // Ustaw ciasteczko z wybraną parafią
            Response.Cookies.Append("SelectedParishUid", parishUid, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            TempData["success"] = "Pomyślnie przełączono do wybranej parafii.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Usuwa wybraną parafię (wylogowuje superadmina z parafii).
        /// </summary>
        /// <returns>Przekierowanie do listy parafii.</returns>
        [HttpPost]
        public IActionResult ClearSelection()
        {
            Response.Cookies.Delete("SelectedParishUid");
            TempData["info"] = "Wyczyszczono wybór parafii.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Wyświetla formularz tworzenia nowej parafii.
        /// </summary>
        /// <returns>Widok formularza tworzenia.</returns>
        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateParishViewModel
            {
                CreateAdmin = true,
                SeedExampleData = false
            };
            
            return View(model);
        }

        /// <summary>
        /// Tworzy nową parafię w systemie.
        /// </summary>
        /// <param name="model">Model z danymi nowej parafii.</param>
        /// <returns>Przekierowanie do listy parafii lub widok z błędami walidacji.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateParishViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Generuj unikalny UID dla parafii
                var parishUid = Guid.NewGuid().ToString();

                // Utwórz parafię (w tym bazę danych, użytkownika DB, migracje)
                var parish = await _parishProvisioningService.CreateParishAsync(
                    parishUid, 
                    model.ParishName,
                    model.CreateAdmin,
                    model.SeedExampleData);

                TempData["success"] = $"Pomyślnie utworzono parafię \"{model.ParishName}\".";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Błąd podczas tworzenia parafii: {ex.Message}";
                return View(model);
            }
        }

        /// <summary>
        /// Wyświetla formularz edycji parafii.
        /// </summary>
        /// <param name="id">Identyfikator parafii do edycji.</param>
        /// <returns>Widok formularza edycji lub błąd 404.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var parish = await _superAdminService.GetParishByIdAsync(id);
            
            if (parish == null)
            {
                TempData["error"] = "Nie znaleziono parafii.";
                return RedirectToAction(nameof(Index));
            }

            var model = new EditParishViewModel
            {
                Id = parish.Id,
                UniqueId = parish.UniqueId.ToString(),
                ParishName = parish.ParishName
            };

            return View(model);
        }

        /// <summary>
        /// Aktualizuje dane parafii.
        /// </summary>
        /// <param name="model">Model z zaktualizowanymi danymi.</param>
        /// <returns>Przekierowanie do listy parafii lub widok z błędami walidacji.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditParishViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                bool success = await _superAdminService.UpdateParishNameAsync(model.Id, model.ParishName);
                
                if (!success)
                {
                    TempData["error"] = "Nie znaleziono parafii.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["success"] = $"Pomyślnie zaktualizowano parafię \"{model.ParishName}\".";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Błąd podczas aktualizacji parafii: {ex.Message}";
                return View(model);
            }
        }
    }
}
