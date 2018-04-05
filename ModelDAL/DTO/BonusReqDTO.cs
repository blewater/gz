using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gzDAL.Models;
using gzDAL.ModelUtil;

namespace gzDAL.DTO {

    /// <summary>
    /// A serializable investement bonus withdrawal request
    /// </summary>
    public class BonusReq
    {
        /// <summary>
        /// Everymatrix customer id
        /// </summary>
        public int GmUserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserEmail { get; set; }
        public string[] AdminEmailRecipients { get; set; } = new[] { "mario@greenzorro.com" };
        /// <summary>
        /// Bonus currency = User Currency
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Total Net Bonus amount for all withdrawn vintages to credit to the user's account in their currency.
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Fees already deducted from the withdrawn vintage.
        /// </summary>
        public decimal Fees { get; set; }
        public string YearMonthSold { get; set; } = DateTime.UtcNow.ToStringYearMonth();
        /// <summary>
        /// Investement balance row ids for all the withdrawn user bonus requests
        /// </summary>
        public int[] InvBalIds { get; set; }
        /// <summary>
        /// How many times it's been picked up to be awarded to the user
        /// </summary>
        public int ProcessedCnt { get; set; } = 0;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime LastProcessedTime { get; set; } = DateTime.UtcNow;
    }
}
