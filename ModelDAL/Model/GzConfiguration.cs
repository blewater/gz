using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzWeb.Models {

    /// <summary>
    /// 
    /// Single row table
    /// 
    /// Biz Configuration Values accessible to console, web apps from the Db
    /// 
    /// </summary>
    public class GzConfiguration {

        /// <summary>
        /// Forced by Entity Framework to have a key 
        /// </summary>
        private int id = 1;
        [Key]
        public int Id {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// 6 Month Lock In Period before caching out portfolio funds 
        /// </summary>
        private int lockInNumDays = 180;
        [Required]
        public int LOCK_IN_NUM_DAYS {
            get { return lockInNumDays; }
            set { lockInNumDays = value; }
        }

        /// <summary>
        /// Greenzorro percentage fee %
        /// </summary>
        private float commissionPcnt = 1.5f;
        [Required]
        public float COMMISSION_PCNT {
            get { return commissionPcnt; }
            set { commissionPcnt = value; }
        }

        /// <summary>
        /// Fund fee %
        /// </summary>
        private float fundFeePcnt = 2.5f;
        [Required]
        public float FUND_FEE_PCNT {
            get { return fundFeePcnt; }
            set { fundFeePcnt = value; }
        }

        /// <summary>
        /// Percentage to credit on loss
        /// </summary>
        private float creditLossPcnt = 50;
        [Required]
        public float CREDIT_LOSS_PCNT {
            get { return creditLossPcnt; }
            set { creditLossPcnt = value; }
        }

    }
}
