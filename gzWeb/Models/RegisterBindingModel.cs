using System;
using System.ComponentModel.DataAnnotations;

namespace gzWeb.Models
{
    public class ActivationBindingModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Code{ get; set; }
    }

    public class RegisterBindingModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        // [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [StringLength(30)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(30)]
        public string LastName { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        /// <summary>
        /// Required for complete profile. 
        /// </summary>
        /// <remarks>
        /// Can be gotten from /user/account#getCurrencies reply
        /// </remarks>
        [Required]
        public string Currency { get; set; }

        /// <summary>
        /// Required for complete profile. 
        /// </summary>
        /// <remarks>
        /// The values can only be "Mr." "Ms." "Mrs." or "Miss" .
        /// </remarks>
        public string Title { get; set; }

        /// <summary>
        /// Required for complete profile. 
        /// </summary>
        /// <remarks>
        /// Can be gotten from /user/account#getCountries reply
        /// </remarks>
        public string Country { get; set; }
        
        /// <summary>
        /// Required for complete profile. 
        /// </summary>
        /// <remarks>
        /// Can be gotten from /user/account#getCountries reply
        /// </remarks>
        public string Region { get; set; }

        /// <summary>
        /// Required for complete profile. 
        /// </summary>
        /// <remarks>
        /// Can be gotten from /user/account#getPhonePrefixes reply
        /// </remarks>
        public string MobilePrefix { get; set; }

        /// <summary>
        /// Required for complete profile. 
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// Required for complete profile. 
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Required for complete profile. 
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Required for complete profile. 
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Required for complete profile. 
        /// </summary>
        public string SecurityQuestion { get; set; }

        /// <summary>
        /// Required for complete profile. 
        /// </summary>
        public string SecurityAnswer { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm password")]
        //[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        //public string ConfirmPassword { get; set; }
    }
}