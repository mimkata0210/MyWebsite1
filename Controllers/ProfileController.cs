using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyWebsite1.Models;
using System.Threading.Tasks;

namespace MyWebsite1.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var model = new ProfileViewModel
            {
                Username = user.UserName,
                Email = user.Email
            };

            return View(model);
        }

        // GET: /Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var model = new ProfileViewModel
            {
                Username = user.UserName,
                Email = user.Email
            };

            return View(model);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Обнови UserName и Email
            user.UserName = model.Username;
            user.Email = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Ако са подадени пароли - опитай да смениш паролата
            if (!string.IsNullOrEmpty(model.CurrentPassword)
                && !string.IsNullOrEmpty(model.NewPassword)
                && !string.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            // Обнови сесията
            await _signInManager.RefreshSignInAsync(user);

            TempData["StatusMessage"] = "Профилът е обновен успешно.";

            return RedirectToAction(nameof(Edit));
        }
    }
}
