using System.ComponentModel.DataAnnotations;

namespace BankingAppBackend.Models
{
    public class RegisterNewAccountModel
    {

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateLastUpdated { get; set; }
        //cummulative
        [Required]
        [RegularExpression(@"^[0-9]{4}$")]
        public string Pin { get; set; }
        [Required]
        [Compare("Pin", ErrorMessage = "Pins do not match")]
        public string ConfirmPin { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Initial deposit must be greater than zero")]
        public decimal InitialDeposit { get; set; }

    }
}
