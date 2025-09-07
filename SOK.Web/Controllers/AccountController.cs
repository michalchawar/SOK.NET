using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SOK.Domain.Entities.Central;
using SOK.Web.ViewModels.Central;

namespace SOK.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false
                );

                if (result.Succeeded)
                {
                    TempData["success"] = "Zostałeś zalogowany.";
                    return Redirect(returnUrl ?? "/");
                }
            }

            //ModelState.AddModelError("", "Nieprawidłowa nazwa użytkownika lub hasło.");
            TempData["error"] = "Nieprawidłowa nazwa użytkownika lub hasło.";
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            bool isLoggedIn = _signInManager.IsSignedIn(HttpContext.User);

            if (isLoggedIn)
            {
                await _signInManager.SignOutAsync();
                TempData["info"] = "Zostałeś wylogowany.";
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
