using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models {

    /// <summary>
    /// 
    /// Single row table
    /// 
    /// Biz Configuration Values accessible to console, web applications from the Db
    /// 
    /// </summary>
    public class GzConfiguration {

        /// <summary>
        /// Forced by Entity Framework to have a key 
        /// </summary>
        [Key]
        public int Id { get; set; } = 1;

        /// <summary>
        /// Number of days to enforce lock In Period before caching out portfolio funds 
        /// </summary>
        [Required]
        public int LOCK_IN_NUM_DAYS { get; set; } = 30;

        /// <summary>
        /// Early withdrawal penalty when less than 90 days (3 month cycles)
        /// </summary>
        [Required]
        public int EARLY_WITHDRAWAL_FEE_PCNT { get; set; } = 25;

        /// <summary>
        /// Investment fee for greater than 10% vitnage gain
        /// </summary>
        [Required]
        public int HURDLE_FEE_PCNT { get; set; } = 25;

        /// <summary>
        /// Vintage gain to trigger deducting the hurdle fee
        /// </summary>
        [Required]
        public int HURDLE_TRIGGER_GAIN_PCNT { get; set; } = 10;

        /// <summary>
        /// greenzorro percentage fee % i.e. 1.5 -> Amount * 0.015
        /// </summary>
        [Required]
        public float COMMISSION_PCNT { get; set; } = 1.5f;

        /// <summary>
        /// Fund fee % i.e 2.5 -> Amount * 0.025
        /// </summary>
        [Required]
        public float FUND_FEE_PCNT { get; set; } = 2.5f;

        /// <summary>
        /// Percentage to credit on loss.
        /// </summary>
        [Required]
        public float CREDIT_LOSS_PCNT { get; set; } = 50;

        /// <summary>
        /// Default Portfolio for new Customers
        /// </summary>
        public RiskToleranceEnum FIRST_PORTFOLIO_RISK_VAL { get; set; } = RiskToleranceEnum.Medium;

        public float CONSERVATIVE_RISK_ROI { get; set; } = 3f;
        public float MEDIUM_RISK_ROI { get; set; } = 6f;
        public float AGGRESSIVE_RISK_ROI { get; set; } = 10f;

    }
}
