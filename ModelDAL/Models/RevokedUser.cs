using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gzDAL.Models
{
    public class RevokedUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        #region IdentityUser fields

        /// <summary>
        ///     Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     True if the email is confirmed, default is false
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        ///     The salted/hashed form of the user password
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        ///     A random value that should change whenever a users credentials have changed (password changed, login removed)
        /// </summary>
        public string SecurityStamp { get; set; }

        /// <summary>
        ///     PhoneNumber for the user
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        ///     True if the phone number is confirmed, default is false
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        ///     Is two factor enabled for the user
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        ///     DateTime in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        public DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        ///     Is lockout enabled for this user
        /// </summary>
        public bool LockoutEnabled { get; set; }

        /// <summary>
        ///     Used to record failures for the purposes of lockout
        /// </summary>
        public int AccessFailedCount { get; set; }

        #endregion

        #region gz user fields

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime Birthday { get; set; }

        public int? GmCustomerId { get; set; }

        public string Currency { get; set; }

        public bool DisabledGzCustomer { get; set; } = false;

        public bool ClosedGzAccount { get; set; }

        public DateTime? LastLogin { get; set; }

        public bool ActiveCustomerIdInPlatform { get; set; }

        public bool? IsRegistrationFinalized { get; set; }

        #endregion

        public string Reason { get; set; }
    }
}
