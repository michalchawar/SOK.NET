using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Web.Filters;
using SOK.Web.ViewModels.Parish;

namespace SOK.Web.Controllers
{
    [Authorize]
    [ActivePage("Plans")]
    public class SettingsController : Controller
    {
        private readonly IParishMemberService _parishMemberService;
        private readonly IStreetService _streetService;
        private readonly IBuildingService _buildingService;
        private readonly ICityService _cityService;
        private readonly ISubmissionService _submissionService;

        public SettingsController(
            IParishMemberService parishMemberService,
            IStreetService streetService,
            IBuildingService buildingService,
            ICityService cityService,
            ISubmissionService submissionService)
        {
            _parishMemberService = parishMemberService;
            _streetService = streetService;
            _buildingService = buildingService;
            _cityService = cityService;
            _submissionService = submissionService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _parishMemberService.GetUsersPaginatedAsync(pageSize: 20, loadRoles: true);

            ViewData["UserDtos"] = users;
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Streets()
        {
            var streets = await _streetService.GetAllStreetsAsync(buildings: true, type: true);

            ViewData["Streets"] = streets;
            return View();
        }

        [HttpGet("Settings/Streets/Create")]
        public async Task<IActionResult> CreateStreet()
        {
            StreetVM model = new()
            {
                Cities = (await _cityService.GetAllCitiesAsync()).Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    }).ToList(),
                StreetSpecifiers = (await _streetService.GetAllStreetSpecifiersAsync()).Select(s => new SelectListItem
                    {
                        Text = s.FullName,
                        Value = s.Id.ToString(),
                        Selected = s.FullName.ToLower() == "ulica"
                    }).ToList()
            };

            return View(model);
        }

        [HttpGet("Settings/Streets/Edit/{id}")]
        public async Task<IActionResult> EditStreet(int id)
        {
            var street = await _streetService.GetStreetAsync(id);

            if (street is null)
            {
                TempData["error"] = "Nie znaleziono ulicy, którą próbujesz edytować.";
                return RedirectToAction("Streets");
            }

            StreetVM model = new()
            {
                Name = street.Name,
                CityId = street.CityId,
                StreetSpecifierId = street.StreetSpecifierId,
                Cities = (await _cityService.GetAllCitiesAsync()).Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = c.Id == street.CityId
                    }).ToList(),
                StreetSpecifiers = (await _streetService.GetAllStreetSpecifiersAsync()).Select(s => new SelectListItem
                    {
                        Text = s.FullName,
                        Value = s.Id.ToString(),
                        Selected = s.Id == street.StreetSpecifierId
                    }).ToList()
            };

            ViewData["streetId"] = street.Id;
            return View(model);
        }

        [HttpPost("Settings/Streets/Create")]
        public async Task<IActionResult> CreateStreet(StreetVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Popraw błędy w formularzu.";
                return View(model);
            }

            Street street = new()
            {
                Name = model.Name,
                StreetSpecifierId = model.StreetSpecifierId,
                CityId = model.CityId,
            };

            try
            {
                await _streetService.CreateStreetAsync(street);
            }
            catch (InvalidOperationException)
            {
                TempData["error"] = "Ulica o podanych danych już istnieje.";
                return View(model);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił błąd podczas tworzenia ulicy. Spróbuj ponownie.";
                return View(model);
            }

            TempData["success"] = "Pomyślnie utworzono ulicę.";
            return RedirectToAction("Streets");
        }

        [HttpPost("Settings/Streets/Edit/{id}")]
        public async Task<IActionResult> EditStreet(int id, StreetVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Popraw błędy w formularzu.";
                return View(model);
            }

            Street? street = await _streetService.GetStreetAsync(id);

            if (street is null)
            {
                TempData["error"] = "Nie znaleziono ulicy, którą próbujesz edytować.";
                return View(model);
            }

            street.Name = model.Name;
            street.StreetSpecifierId = model.StreetSpecifierId;
            street.CityId = model.CityId;

            try
            {
                await _streetService.UpdateStreetAsync(street);
            }
            catch (InvalidOperationException)
            {
                TempData["error"] = "Ulica o podanych danych już istnieje.";
                return View(model);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił błąd podczas aktualizacji ulicy.";
                return View(model);
            }

            TempData["success"] = "Pomyślnie zaktualizowano ulicę.";
            return RedirectToAction("Streets");
        }

        [HttpGet("Settings/Streets/Delete/{id}")]
        public async Task<IActionResult> DeleteStreet(int id)
        {
            Street? street = (await _streetService.GetAllStreetsAsync(s => s.Id == id, buildings: true, type: true)).FirstOrDefault();

            if (street is null)
            {
                TempData["error"] = "Nie znaleziono ulicy, którą próbujesz usunąć.";
                return RedirectToAction("Streets");
            }

            return View(street);
        }

        [HttpPost("Settings/Streets/Delete/{id}")]
        public async Task<IActionResult> DeleteStreetConfirmed(int id)
        {
            Street? street = (await _streetService.GetAllStreetsAsync(s => s.Id == id, buildings: true)).FirstOrDefault();

            if (street is null)
            {
                TempData["error"] = "Nie znaleziono ulicy, którą próbujesz usunąć.";
                return RedirectToAction("Streets");
            }
            
            if (street.Buildings.Any())
            {
                TempData["error"] = "Nie możesz usunąć ulicy, która ma przypisane bramy.";
                return View(street);
            }

            try
            {
                await _streetService.DeleteStreetAsync(id);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił błąd podczas usuwania ulicy.";
                return View(street);
            }

            TempData["success"] = "Pomyślnie usunięto ulicę.";
            return RedirectToAction("Streets");
        }

        [HttpGet("Settings/Buildings/Create")]
        public async Task<IActionResult> CreateBuilding([FromQuery] int streetId)
        {
            var street = (await _streetService.GetAllStreetsAsync(s => s.Id == streetId, type: true)).FirstOrDefault();

            if (street is null)
            {
                TempData["error"] = "Nie znaleziono ulicy, do której chcesz dodać budynek.";
                return RedirectToAction("Streets");
            }

            BuildingVM model = new()
            {
                StreetId = streetId,
                StreetName = street.Type.Abbreviation + " " + street.Name
            };

            return View(model);
        }

        [HttpPost("Settings/Buildings/Create")]
        public async Task<IActionResult> CreateBuilding(BuildingVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Popraw błędy w formularzu.";
                return View(model);
            }

            int number = int.Parse(new string(model.Signage.TakeWhile(c => char.IsDigit(c)).ToArray()));
            string letter = new string(model.Signage.SkipWhile(c => char.IsDigit(c)).ToArray());

            Building building = new()
            {
                Number = number,
                Letter = string.IsNullOrWhiteSpace(letter) ? null : letter,
                StreetId = model.StreetId,
                AllowSelection = model.IsVisible,
                FloorCount = model.FloorCount,
                ApartmentCount = model.ApartmentCount,
                HighestApartmentNumber = model.HighestApartmentNumber,
                HasElevator = model.HasElevator,
            };

            try
            {
                await _buildingService.CreateBuildingAsync(building);
            }
            catch (InvalidOperationException)
            {
                TempData["error"] = "Budynek o podanych danych już istnieje na tej ulicy.";
                return View(model);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił błąd podczas tworzenia budynku.";
                return View(model);
            }

            TempData["success"] = "Pomyślnie utworzono budynek.";
            return RedirectToAction("Streets");
        }
        
        [HttpGet("Settings/Buildings/Create/Series")]
        public async Task<IActionResult> CreateBuildingSeries([FromQuery] int streetId)
        {
            var street = (await _streetService.GetAllStreetsAsync(s => s.Id == streetId, type: true)).FirstOrDefault();

            if (street is null)
            {
                TempData["error"] = "Nie znaleziono ulicy, do której chcesz dodać budynki.";
                return RedirectToAction("Streets");
            }

            BuildingSeriesVM model = new()
            {
                From = 1,
                To = 10,
                StreetId = streetId,
                StreetName = street.Type.Abbreviation + " " + street.Name
            };

            return View(model);
        }


        [HttpPost("Settings/Buildings/Create/Series")]
        public async Task<IActionResult> CreateBuildingSeries(BuildingSeriesVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Popraw błędy w formularzu.";
                return View(model);
            }

            if (model.From > model.To)
            {
                TempData["error"] = "Numer początkowy nie może być większy niż numer końcowy.";
                return View(model);
            }

            int successCount = 0;
            int startingNumber, endingNumber;
            switch (model.InsertMode)
            {
                case 0: // All
                    startingNumber = model.From;
                    endingNumber = model.To;
                    break;
                case 1: // Even
                    startingNumber = model.From % 2 == 0 ? model.From : model.From + 1;
                    endingNumber = model.To % 2 == 0 ? model.To : model.To - 1;
                    break;
                case 2: // Odd
                    startingNumber = model.From % 2 != 0 ? model.From : model.From + 1;
                    endingNumber = model.To % 2 != 0 ? model.To : model.To - 1;
                    break;
                default:
                    TempData["error"] = "Nieprawidłowy tryb tworzenia budynków.";
                    return View(model);
            }
            
            for (int i = startingNumber; i <= endingNumber; i += model.InsertMode == 0 ? 1 : 2)
            {
                Building building = new()
                {
                    Number = i,
                    StreetId = model.StreetId,
                    HasElevator = model.HasElevator,
                };

                try
                {
                    await _buildingService.CreateBuildingAsync(building);
                    successCount++;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            TempData["success"] = "Pomyślnie utworzono budynki w liczbie: " + successCount + ".";
            return RedirectToAction("Streets");
        }
        
        [HttpGet("Settings/Buildings/Edit/{id}")]
        public async Task<IActionResult> EditBuilding(int id)
        {
            Building? building = await _buildingService.GetBuildingAsync(id);

            if (building is null)
            {
                TempData["error"] = "Nie znaleziono budynku, który próbujesz edytować.";
                return RedirectToAction("Streets");
            }

            Street? street = (await _streetService.GetAllStreetsAsync(s => s.Id == building.StreetId, type: true)).FirstOrDefault();

            BuildingVM model = new()
            {
                Signage = building.Number.ToString() + building.Letter,
                StreetId = building.StreetId,
                StreetName = street!.Type.Abbreviation + " " + street.Name,
                IsVisible = building.AllowSelection,
                HasElevator = building.HasElevator,
                FloorCount = building.FloorCount,
                ApartmentCount = building.ApartmentCount,
                HighestApartmentNumber = building.HighestApartmentNumber,
            };
            ViewData["buildingId"] = building.Id;

            return View(model);
        }

        [HttpPost("Settings/Buildings/Edit/{id}")]
        public async Task<IActionResult> EditBuilding(int id, BuildingVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Popraw błędy w formularzu.";
                return View(model);
            }

            Building? building = await _buildingService.GetBuildingAsync(id);

            if (building is null)
            {
                TempData["error"] = "Nie znaleziono budynku, który próbujesz edytować.";
                return View(model);
            }

            int number = int.Parse(new string(model.Signage.TakeWhile(c => char.IsDigit(c)).ToArray()));
            string letter = new string(model.Signage.SkipWhile(c => char.IsDigit(c)).ToArray());

            building.Number = number;
            building.Letter = string.IsNullOrWhiteSpace(letter) ? null : letter;
            building.StreetId = model.StreetId;
            building.AllowSelection = model.IsVisible;
            building.FloorCount = model.FloorCount;
            building.ApartmentCount = model.ApartmentCount;
            building.HighestApartmentNumber = model.HighestApartmentNumber;
            building.HasElevator = model.HasElevator;

            try
            {
                await _buildingService.UpdateBuildingAsync(building);
            }
            catch (InvalidOperationException)
            {
                TempData["error"] = "Budynek o podanych danych już istnieje na tej ulicy.";
                return View(model);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił błąd podczas aktualizacji budynku.";
                return View(model);
            }

            TempData["success"] = "Pomyślnie zaktualizowano budynek.";
            return RedirectToAction("Streets");
        }
    
        [HttpGet("Settings/Buildings/Delete/{id}")]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            Building? building = await _buildingService.GetBuildingAsync(id);

            if (building is null)
            {
                TempData["error"] = "Nie znaleziono budynku, który próbujesz usunąć.";
                return RedirectToAction("Streets");
            }

            int assignedSubmissionsCount = (await _submissionService.GetSubmissionsPaginated(s => s.Address.BuildingId == building.Id, pageSize: 100)).Count();
            ViewData["assignedSubmissionsCount"] = assignedSubmissionsCount;

            Street? street = (await _streetService.GetAllStreetsAsync(s => s.Id == building.StreetId, type: true)).FirstOrDefault();
            ViewData["streetName"] = street!.Type.Abbreviation + " " + street.Name;

            return View(building);
        }

        [HttpPost("Settings/Buildings/Delete/{id}")]
        public async Task<IActionResult> DeleteBuildingConfirmed(int id)
        {
            Building? building = await _buildingService.GetBuildingAsync(id);

            if (building is null)
            {
                TempData["error"] = "Nie znaleziono budynku, który próbujesz usunąć.";
                return RedirectToAction("Streets");
            }

            int assignedSubmissionsCount = (await _submissionService.GetSubmissionsPaginated(s => s.Address.BuildingId == building.Id, pageSize: 100)).Count();
            ViewData["assignedSubmissionsCount"] = assignedSubmissionsCount; 
            
            Street? street = (await _streetService.GetAllStreetsAsync(s => s.Id == building.StreetId, type: true)).FirstOrDefault();
            ViewData["streetName"] = street!.Type.Abbreviation + " " + street.Name;

            if (assignedSubmissionsCount > 0)
            {
                TempData["error"] = "Nie możesz usunąć budynku, który ma przypisane zgłoszenia.";
                return View(building);
            }

            try
            {
                await _buildingService.DeleteBuildingAsync(id);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił błąd podczas usuwania budynku.";
                return View(building);
            }

            TempData["success"] = "Pomyślnie usunięto budynek.";
            return RedirectToAction("Streets");
        }

    }
}