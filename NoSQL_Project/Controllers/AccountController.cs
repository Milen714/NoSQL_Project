using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Models;
using NoSQL_Project.Models.PasswordResset;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IEmailSenderService _emailSenderService;

        public AccountController(IUserService userService, IEmailSenderService emailSenderService)
        {
            _userService = userService;
            _emailSenderService = emailSenderService;
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = _userService.GetUserByEmail(model.Email);
                //generating unique token
                var token = _userService.GeneratePasswordResetTokenAsync(user);

                //Building the resset link to be sent via email(what you click in the body of the mail)
                var callbackUrl = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { userId = user.Id, token = token.Result },
                    protocol: HttpContext.Request.Scheme);

                //Sending the email after building the link
                await _emailSenderService.SendEmailAsync(model.Email, "Reset Password",
                    $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");

                TempData["Success"] = "Password reset link has been sent to your email.";
                return RedirectToAction("Login", "Home");
            }
            return View(model);

        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            User user = _userService.FindById(userId);
            if (user == null || user.RessetToken != token || user.RessetTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired resset link");
            }
            return View(new ResetPasswordViewModel { Token = token, UserId = userId });
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            User user = _userService.FindById(model.UserId);
            if (user == null || user.RessetToken != model.Token || user.RessetTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired resset link");
            }
            user.Password = model.Password;
            user = _userService.HashUserPassword(user);
            user.RessetToken = null;
            user.RessetTokenExpiry = null;
            await _userService.UpdateUser(user);

            TempData["Success"] = "Password has been reset successfully";
            return RedirectToAction("Login", "Home");
        }

    }
}