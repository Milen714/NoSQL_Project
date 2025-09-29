using System.ComponentModel.DataAnnotations;

namespace NoSQL_Project.Models.PasswordResset
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

}
