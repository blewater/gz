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

        private int id = 1;
        /// <summary>
        /// Forced by Entity Framework to have a key 
        /// </summary>
        [Key]
        public int Id {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Number of days to enforce lock In Period before caching out portfolio funds 
        /// </summary>
        [Required]
        public int LOCK_IN_NUM_DAYS { get; set; } = 90;

        private float commissionPcnt = 1.5f;
        /// <summary>
        /// greenzorro percentage fee % i.e. 1.5 -> Amount * 0.015
        /// </summary>
        [Required]
        public float COMMISSION_PCNT {
            get { return commissionPcnt; }
            set { commissionPcnt = value; }
        }

        private float fundFeePcnt = 2.5f;
        /// <summary>
        /// Fund fee % i.e 2.5 -> Amount * 0.025
        /// </summary>
        [Required]
        public float FUND_FEE_PCNT {
            get { return fundFeePcnt; }
            set { fundFeePcnt = value; }
        }

        private float creditLossPcnt = 50;
        /// <summary>
        /// Percentage to credit on loss.
        /// </summary>
        [Required]
        public float CREDIT_LOSS_PCNT {
            get { return creditLossPcnt; }
            set { creditLossPcnt = value; }
        }

    }
}
