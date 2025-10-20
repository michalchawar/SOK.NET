using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SOK.Application.Common.Interface;
using SOK.Domain.Entities.Parish;

namespace SOK.Web.Controllers
{
    [Authorize]
    public class SubmissionsController : Controller
    {
        private readonly IUnitOfWorkParish _uow;

        public SubmissionsController(IUnitOfWorkParish uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            Address? randomAddress = await _uow.Address.GetRandomAsync();
            Submitter? randomSubmitter = await _uow.Submitter.GetRandomAsync();

            if (randomAddress != null)
                ViewData["SampleAddress"] = $"{randomAddress.Building.Street.Name} {randomAddress.Building.Number}{randomAddress.Building.Letter}/" +
                    $"{randomAddress.ApartmentNumber}{randomAddress.ApartmentLetter}";
            else
                ViewData["SampleAddress"] = "Brzozowa 45/2";

            if (randomSubmitter != null)
                ViewData["SampleSurname"] = randomSubmitter.Surname;
            else
                ViewData["SampleSurname"] = "Kowalski";

            return View();
        }
    }
}
