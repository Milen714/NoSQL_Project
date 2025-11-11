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
                User user = await _userService.GetUserByEmailAsync(model.Email);
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
        public async Task<IActionResult> ResetPassword(string userId, string token)
        {
            try
            {
                User user = await _userService.FindByIdAsync(userId);
                if (user == null || user.RessetToken != token || user.RessetTokenExpiry < DateTime.UtcNow)
                {
                    throw new Exception("Invalid or Expired Resset Link");
                }
                return View(new ResetPasswordViewModel { Token = token, UserId = userId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(new ResetPasswordViewModel());
                
            }
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            //Checks is the password in both pass fields match
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                //Once again validates the link with against the token and the expiry time (Prehaps the user took too much time to resset their pass)
                User user = await _userService.FindByIdAsync(model.UserId);
                if (user == null || user.RessetToken != model.Token || user.RessetTokenExpiry < DateTime.UtcNow)
                {
                    throw new Exception("Invalid or expired resset link");
                }
                user.PasswordHash = model.Password;
                user = _userService.HashUserPassword(user);
                user.RessetToken = null;
                user.RessetTokenExpiry = null;
                await _userService.UpdateUserAsync(user);

                TempData["Success"] = "Password has been reset successfully";
                return RedirectToAction("Login", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(model);
            }
        }

    }
}