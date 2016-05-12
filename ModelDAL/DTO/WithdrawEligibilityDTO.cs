using System;

namespace gzDAL.DTO {

    /// <summary>
    /// 
    /// Transfer withdraw eligibility data between layers
    /// 
    /// </summary>
    public class WithdrawEligibilityDTO {

        public int LockInDays { get; set; }
        public bool OkToWithdraw { get; set; }
        public DateTime EligibleWithdrawDate { get; set; }
        public string Prompt { get; set; }
    }
}
