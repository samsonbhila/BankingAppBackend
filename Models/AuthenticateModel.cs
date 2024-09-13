using System.ComponentModel.DataAnnotations;

namespace BankingAppBackend.Models
{
    public class AuthenticateModel
    {
        [Required]
        [RegularExpression(@"^[0-9]{4}$", ErrorMessage = "Pin must be 4-digit")]
        public string Pin { get; set; }
    }
}
