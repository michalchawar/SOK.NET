using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SOK.Application.Common.Helpers;
using SOK.Application.Services.Interface;
using SOK.Domain.Entities.Parish;
using SOK.Domain.Enums;
using SOK.Web.Filters;
using SOK.Web.ViewModels.Settings;

namespace SOK.Web.Controllers
{
    [AuthorizeRoles(Role.Administrator, Role.Priest)]
    [RequireParish]
    [ActivePage("Settings")]
    public class SettingsController : Controller
    {
        private readonly IParishMemberService _parishMemberService;
        private readonly IStreetService _streetService;
        private readonly IBuildingService _buildingService;
        private readonly ICityService _cityService;
        private readonly ISubmissionService _submissionService;
        private readonly IParishInfoService _parishInfoService;

        public SettingsController(
            IParishMemberService parishMemberService,
            IStreetService streetService,
            IBuildingService buildingService,
            ICityService cityService,
            ISubmissionService submissionService,
            IParishInfoService parishInfoService)
        {
            _parishMemberService = parishMemberService;
            _streetService = streetService;
            _buildingService = buildingService;
            _cityService = cityService;
            _submissionService = submissionService;
            _parishInfoService = parishInfoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string? parishUid = await _parishInfoService.GetValueAsync(InfoKeys.Parish.UniqueId);
            ViewData["ParishUid"] = parishUid;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _parishMemberService.GetUsersPaginatedAsync(pageSize: 100, loadRoles: true);

            ViewData["UserDtos"] = users;
            return View("Users/Index");
        }

        [HttpGet("Settings/Users/Create")]
        public IActionResult CreateUser()
        {
            var model = new CreateUserVM();
            ViewData["AllRoles"] = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            return View("Users/Create", model);
        }

        [HttpPost("Settings/Users/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["AllRoles"] = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
                return View("Users/Create", model);
            }

            var result = await _parishMemberService.CreateUserAsync(
                model.DisplayName,
                model.UserName,
                model.Email,
                model.Password,
                model.SelectedRoles);

            if (result == null)
            {
                TempData["error"] = "Nie udało się utworzyć użytkownika. Login może być już zajęty.";
                ViewData["AllRoles"] = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
                return View("Users/Create", model);
            }

            TempData["success"] = $"Użytkownik {result.DisplayName} został utworzony pomyślnie.";
            return RedirectToAction(nameof(Users));
        }

        [HttpGet("Settings/Users/Edit/{id}")]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _parishMemberService.GetUserByIdAsync(id, loadRoles: true, loadPlans: true);
            
            if (user == null)
            {
                TempData["error"] = "Nie znaleziono użytkownika.";
                return RedirectToAction(nameof(Users));
            }

            var allPlans = await _parishMemberService.GetAllPlansAsync();

            var model = new EditUserVM
            {
                CentralId = user.CentralId,
                DisplayName = user.DisplayName,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email,
                SelectedRoles = user.Roles.ToList(),
                AssignedPlanIds = user.AssignedPlans.Select(p => p.Id).ToList(),
                AvailablePlans = allPlans.ToList()
            };

            ViewData["AllRoles"] = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            return View("Users/Edit", model);
        }

        [HttpPost("Settings/Users/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, EditUserVM model)
        {
            if (!ModelState.IsValid)
            {
                var allPlans = await _parishMemberService.GetAllPlansAsync();
                model.AvailablePlans = allPlans.ToList();
                ViewData["AllRoles"] = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
                return View("Users/Edit", model);
            }

            var result = await _parishMemberService.UpdateUserAsync(
                id,
                model.DisplayName,
                model.UserName,
                model.Email,
                model.SelectedRoles,
                model.AssignedPlanIds);

            if (!result)
            {
                TempData["error"] = "Nie udało się zaktualizować użytkownika.";
                var allPlans = await _parishMemberService.GetAllPlansAsync();
                model.AvailablePlans = allPlans.ToList();
                ViewData["AllRoles"] = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
                return View("Users/Edit", model);
            }

            TempData["success"] = "Użytkownik został zaktualizowany pomyślnie.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost("Settings/Users/ResetPassword/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var newPassword = await _parishMemberService.ResetPasswordAsync(id);

            if (newPassword == null)
            {
                return Json(new { success = false, message = "Nie udało się zresetować hasła." });
            }

            return Json(new { success = true, password = newPassword });
        }

        [HttpGet]
        public async Task<IActionResult> General()
        {
            var settingsDict = await _parishInfoService.GetDictionaryAsync();

            SettingsListVM model = new()
            {
                Sections =
                [
                    new()
                    {
                        Name = "Dane ogólne",
                        Settings =
                        [
                            new StringSettingVM(InfoKeys.Parish.FullName, settingsDict) {
                                Name = "Pełna nazwa parafii",
                                Description = "Nazwa parafii wyświetlana w dokładnych danych oraz dokumentach.",
                                Hint = "Np. Parafia Rzymskokatolicka pw. św. Anny w Brzegu",
                            },
                            new StringSettingVM(InfoKeys.Parish.ShortName, settingsDict) {
                                Name = "Nazwa parafii (krótka)",
                                Description = "Nazwa parafii wyświetlana w nagłówkach i krótszych formatach.",
                                Hint = "Np. Parafia pw. św. Anny",
                            },
                            new StringSettingVM(InfoKeys.Parish.ShortNameAppendix, settingsDict) {
                                Name = "Nazwa parafii (dodatek)",
                                Description = "Krótkie dookreślenie parafii, wyświetlane np. w mailu pod krótką nazwą.",
                                Hint = "Np. Mokotów, Warszawa",
                            },
                            new StringSettingVM(InfoKeys.Parish.UniqueId, settingsDict) {
                                Name = "UID parafii",
                                Description = "Unikatowy identyfikator parafii, który przypisany jest do niej w systemie.",
                                Readonly = true,
                            },
                            new StringSettingVM(InfoKeys.Parish.Diocese, settingsDict) {
                                Name = "Diecezja",
                                Description = "Nazwa diecezji, do której parafia należy. Wyświetlana jest w dokładnych danych.",
                                Hint = "Np. Archidiecezja Łódzka",
                            },
                        ]
                    },
                    new()
                    {
                        Name = "Kontakt",
                        Settings =
                        [
                            new StringSettingVM(InfoKeys.Contact.Email, settingsDict) {
                                Name = "Adres e-mail",
                                Description = "Parafialny adres e-mail, przeznaczony do korespondencji w temacie organizacji kolędy. Jeśli nie ma takiego, powinien to być główny adres mailowy parafii.",
                                Hint = "Np. koleda@parafia-sw-mikolaja.pl",
                                Type = InputType.Email,
                            },
                            new StringSettingVM(InfoKeys.Contact.Website, settingsDict) {
                                Name = "Adres strony internetowej",
                                Description = "Oficjalna strona internetowa parafii.",
                                Hint = "Np. https://www.parafia-sw-mikolaja.pl",
                                Type = InputType.Url,
                            },
                            new StringSettingVM(InfoKeys.Contact.MainPhone, settingsDict) {
                                Name = "Numer telefonu",
                                Description = "Główny parafialny numer telefonu. Wyświetlany w wielu miejscach.",
                                Hint = "Np. +48 489 291 425",
                                Type = InputType.Tel,
                            },
                            new StringSettingVM(InfoKeys.Contact.SecondaryPhone, settingsDict) {
                                Name = "Numer telefonu (dodatkowy)",
                                Description = "Drugi parafialny numer telefonu (opcjonalnie). Wyświetlany w wielu miejscach.",
                                Hint = "Np. +48 976 424 685",
                                Type = InputType.Tel,
                            },
                        ]
                    },
                    new()
                    {
                        Name = "Adres parafii",
                        Settings =
                        [
                            new StringSettingVM(InfoKeys.Contact.StreetAndBuilding, settingsDict) {
                                Name = "Ulica i numer budynku",
                                Description = "Te dane będą się wyświetlać bezpośrednio, gdy będzie taka potrzeba.",
                                Hint = "Np. pl. Kościelny 4a",
                            },
                            new StringSettingVM(InfoKeys.Contact.CityName, settingsDict) {
                                Name = "Miasto",
                                Description = "Nazwa miasta, wyświetlana w dokładnym adresie.",
                                Hint = "Np. Gdańsk",
                            },
                            new StringSettingVM(InfoKeys.Contact.PostalCode, settingsDict) {
                                Name = "Kod pocztowy",
                                Description = "Kod będzie pokazany przy wyświetlaniu pełnego adresu parafii.",
                                Hint = "Np. 42-512",
                            },
                            new StringSettingVM(InfoKeys.Contact.RegionAndCountry, settingsDict) {
                                Name = "Województwo i państwo",
                                Description = "Te dane będą pokazane przy wyświetlaniu pełnego adresu parafii.",
                                Hint = "Np. Województwo mazowieckie, Polska",
                            },
                        ]
                    },
                    new()
                    {
                        Name = "Ustawienia udostępniania",
                        Settings =
                        [
                            new StringSettingVM(InfoKeys.EmbeddedApplication.FormUrl, settingsDict) {
                                Name = "Adres URL formularza zgłoszeniowego",
                                Description = "Pełny adres podstrony na stronie parafii, na której znajduje się formularz zgłoszeniowy.",
                                Hint = "Np. https://www.parafia-sw-mikolaja.pl/koleda",
                            },
                            new StringSettingVM(InfoKeys.EmbeddedApplication.ControlPanelBaseUrl, settingsDict) {
                                Name = "Adres URL panelu zgłoszenia",
                                Description = "Pełny adres podstrony na stronie parafii, na której znajduje się panel do zarządzania zgłoszeniami.",
                                Hint = "Np. https://www.parafia-sw-mikolaja.pl/koleda/panel",
                            },
                        ]
                    },
                    new() 
                    {
                        Name = "Poczta e-mail",
                        Settings = [
                            new CheckSettingVM(InfoKeys.Email.EnableEmailSending, settingsDict) {
                                Name = "Automatycznie rozsyłaj e-maile",
                                Description = "Automatycznie wysyłaj maile ma wysyłać e-maile do zgłaszających (np. z powiadomieniami o przyjęciu zgłoszenia).",
                            },
                            new CheckSettingVM(InfoKeys.Email.PrependPlanNameToSubject, settingsDict) {
                                Name = "Dodaj nazwę planu do tematu e-maila",
                                Description = "Dodaj nazwę planu, do którego należy zgłoszenie, do tematu wysyłanych e-maili (np. 'Kolęda 2024/25 - Potwierdzenie przyjęcia zgłoszenia').",
                            },
                            new StringSettingVM(InfoKeys.Email.SmtpServer, settingsDict) {
                                Name = "Serwer SMTP",
                                Description = "Adres serwera SMTP, który będzie używany do wysyłania poczty e-mail z systemu.",
                                Hint = "Np. smtp.parafia-sw-mikolaja.pl",
                            },
                            new CheckSettingVM(InfoKeys.Email.SmtpRequireAuth, settingsDict) {
                                Name = "Wymagaj uwierzytelniania SMTP",
                                Description = "Czy serwer SMTP obsługuje uwierzytelnianie użytkowników przed wysłaniem poczty e-mail?",
                            },
                            new IntSettingVM(InfoKeys.Email.SmtpPort, settingsDict) {
                                Name = "Port SMTP",
                                Description = "Port serwera SMTP, który będzie używany do wysyłania poczty e-mail z systemu.",
                                Hint = "Dla ruchu szyfrowanego zwykle jest to 465, dla nieszyfrowanego 25 lub 587.",
                                MinValue = 1,
                                MaxValue = 65535,
                            },
                            new StringSettingVM(InfoKeys.Email.SmtpUserName, settingsDict) {
                                Name = "Nazwa użytkownika SMTP",
                                Description = "Nazwa użytkownika do uwierzytelniania na serwerze SMTP.",
                                Hint = "Np. koleda@sw-antoni-szczecin.pl",
                            },
                            new StringSettingVM(InfoKeys.Email.SmtpPassword, settingsDict) {
                                Name = "Hasło użytkownika SMTP",
                                Description = "Hasło do konta użytkownika na serwerze SMTP.",
                                Hint = "Wprowadź hasło do konta.",
                                Type = InputType.Password,
                            },
                            new CheckSettingVM(InfoKeys.Email.SmtpEnableSsl, settingsDict) {
                                Name = "Używaj szyfrowania SSL/TLS",
                                Description = "Czy połączenie SMTP powinno używać szyfrowania SSL/TLS?",
                            },
                            new StringSettingVM(InfoKeys.Email.SenderEmail, settingsDict) {
                                Name = "Adres email nadawcy",
                                Description = "Adres email, który będzie wyświetlany jako nadawca wiadomości.",
                                Hint = "Np. koleda@parafia-sw-mikolaja.pl",
                                Type = InputType.Email,
                            },
                            new StringSettingVM(InfoKeys.Email.SenderName, settingsDict) {
                                Name = "Nazwa nadawcy",
                                Description = "Nazwa wyświetlana jako nadawca wiadomości.",
                                Hint = "Np. Parafia pw. św. Mikołaja - Kolęda",
                            },
                            new StringSettingVM(InfoKeys.Email.BccRecipients, settingsDict) {
                                Name = "Adresy email do ukrytego DW (BCC)",
                                Description = "Adresy email, które będą otrzymywać kopie wiadomości w ukrytej kopii (BCC), rozdzielone średnikiem.",
                                Hint = "Np. sekretariat@parafia-sw-mikolaja.pl; koleda@parafia-sw-mikolaja.pl",
                            },
                        ]
                    }
                ]
            };

            return View("GeneralSettings/Index", model);
        }


        [HttpGet]
        public async Task<IActionResult> Streets()
        {
            var streets = await _streetService.GetAllStreetsAsync(buildings: true, type: true);

            ViewData["Streets"] = streets;
            return View("Streets/Index");
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

            return View("Streets/CreateStreet", model);
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
            return View("Streets/EditStreet", model);
        }

        [HttpPost("Settings/Streets/Create")]
        public async Task<IActionResult> CreateStreet(StreetVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Popraw błędy w formularzu.";
                return View("Streets/CreateStreet", model);
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
                return View("Streets/CreateStreet", model);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił błąd podczas tworzenia ulicy. Spróbuj ponownie.";
                return View("Streets/CreateStreet", model);
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
                return View("Streets/EditStreet", model);
            }

            Street? street = await _streetService.GetStreetAsync(id);

            if (street is null)
            {
                TempData["error"] = "Nie znaleziono ulicy, którą próbujesz edytować.";
                return View("Streets/EditStreet", model);
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
                return View("Streets/EditStreet", model);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił błąd podczas aktualizacji ulicy.";
                return View("Streets/EditStreet", model);
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

            return View("Streets/DeleteStreet", street);
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
                return View("Streets/DeleteStreet", street);
            }

            try
            {
                await _streetService.DeleteStreetAsync(id);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił błąd podczas usuwania ulicy.";
                return View("Streets/DeleteStreet", street);
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

            return View("Streets/CreateBuilding", model);
        }

        [HttpPost("Settings/Buildings/Create")]
        public async Task<IActionResult> CreateBuilding(BuildingVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Popraw błędy w formularzu.";
                return View("Streets/CreateBuilding", model);
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
                return View("Streets/CreateBuilding", model);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił błąd podczas tworzenia budynku.";
                return View("Streets/CreateBuilding", model);
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

            return View("Streets/CreateBuildingSeries", model);
        }


        [HttpPost("Settings/Buildings/Create/Series")]
        public async Task<IActionResult> CreateBuildingSeries(BuildingSeriesVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Popraw błędy w formularzu.";
                return View("Streets/CreateBuildingSeries", model);
            }

            if (model.From > model.To)
            {
                TempData["error"] = "Numer początkowy nie może być większy niż numer końcowy.";
                return View("Streets/CreateBuildingSeries", model);
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
                    return View("Streets/CreateBuildingSeries", model);
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

            return View("Streets/EditBuilding", model);
        }

        [HttpPost("Settings/Buildings/Edit/{id}")]
        public async Task<IActionResult> EditBuilding(int id, BuildingVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Popraw błędy w formularzu.";
                return View("Streets/EditBuilding", model);
            }

            Building? building = await _buildingService.GetBuildingAsync(id);

            if (building is null)
            {
                TempData["error"] = "Nie znaleziono budynku, który próbujesz edytować.";
                return View("Streets/EditBuilding", model);
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
                return View("Streets/EditBuilding", model);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił błąd podczas aktualizacji budynku.";
                return View("Streets/EditBuilding", model);
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

            return View("Streets/DeleteBuilding", building);
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
                return View("Streets/DeleteBuilding", building);
            }

            try
            {
                await _buildingService.DeleteBuildingAsync(id);
            }
            catch (Exception)
            {
                TempData["error"] = "Wystąpił błąd podczas usuwania budynku.";
                return View("Streets/DeleteBuilding", building);
            }

            TempData["success"] = "Pomyślnie usunięto budynek.";
            return RedirectToAction("Streets");
        }

    }
}