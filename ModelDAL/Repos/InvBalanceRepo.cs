using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Runtime.Caching;
using System.Threading.Tasks;
using gzDAL.Conf;
using gzDAL.DTO;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using gzDAL.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NLog;
using Z.EntityFramework.Plus;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace gzDAL.Repos
{

    /// <summary>
    /// Return type of investment balance values
    /// </summary>
    public class InvBalAmountsRow
    {
        public decimal Balance { get; set; }
        public decimal CashInvestment { get; set; }
        // Total player losses for vintages that have not been sold
        public decimal TotalCashInvInHold { get; set; }
        public decimal LowRiskShares { get; set; }
        public decimal MediumRiskShares { get; set; }
        public decimal HighRiskShares { get; set; }
        //-- New monthly gaming amounts imported by the reports
        public decimal BegGmBalance { get; set; }
        public decimal Deposits { get; set; }
        public decimal Withdrawals { get; set; }
        public decimal GmGainLoss { get; set; }
        public decimal EndGmBalance { get; set; }
        public decimal CashBonus { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }

    /// <summary>
    /// 
    /// Return type for user vintages along with past cashed withdrawn vintages
    /// 
    /// </summary>
    public class VintagesWithSellingValues
    {
        public List<VintageDto> VintagesDtos { get; set; }
        public SoldVintagesAmounts SoldVintagesAmounts { get; set; }
    }

    /// <summary>
    /// 
    /// Return type for past vintages cashed withdrawn amounts
    /// 
    /// </summary>
    public class SoldVintagesAmounts
    {
        public decimal TotalSoldVintagesNetProceeds { get; set; }
        public decimal TotalSoldVintagesFeesAmount { get; set; }
    }

    /// <summary>
    /// 
    /// Main repository class of this file for the InvestmentBalances entity
    /// 
    /// </summary>
    public class InvBalanceRepo : IInvBalanceRepo
    {
        private readonly ApplicationDbContext db;
        private readonly IUserPortfolioSharesRepo userPortfolioSharesRepo;
        private readonly IGzTransactionRepo gzTransactionRepo;
        private readonly IUserPortfolioRepo custPortfolioRepo;
        private readonly IConfRepo confRepo;
        private readonly IUserRepo userRepo;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public InvBalanceRepo(
            ApplicationDbContext db,
            IUserPortfolioSharesRepo userPortfolioSharesRepo,
            IGzTransactionRepo gzTransactionRepo,
            IUserPortfolioRepo custPortfolioRepo,
            IConfRepo confRepo,
            IUserRepo userRepo)
        {

            this.db = db;
            this.userPortfolioSharesRepo = userPortfolioSharesRepo;
            this.gzTransactionRepo = gzTransactionRepo;
            this.custPortfolioRepo = custPortfolioRepo;
            this.confRepo = confRepo;
            this.userRepo = userRepo;
        }

        #region InvestmentSummaryQueries

        /// <summary>
        /// 
        /// CacheBalance specifying the month and ask it asynchronously.
        /// 
        /// Meant to be used with GetCachedLatestBalanceTimestamp() if possible after a short time delay.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yyyyMm"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task<InvBalance> GetCachedLatestBalanceAsyncByMonth(int customerId, string yyyyMm)
        {
            var invBalanceRow =
                await db.InvBalances
                    .Where(i => i.CustomerId == customerId
                                && i.YearMonth == yyyyMm)
                    .DeferredSingleOrDefault()
                    // Cache 4 hours
                    .FromCacheAsync(DateTime.UtcNow.AddHours(4));

            return invBalanceRow;
        }

        /// <summary>
        /// 
        /// CacheBalance and ask it asynchronously.
        /// 
        /// Meant to be used with GetCachedLatestBalanceTimestamp() if possible after a short time delay.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public Task<InvBalance> GetCachedLatestBalanceAsync(int customerId)
        {
            var now = DateTime.UtcNow;
            string monthToAskInvBalance = 
                now.Day == 1 
                ? 
                now.AddMonths(-1).ToStringYearMonth() 
                : 
                now.ToStringYearMonth();

            return GetCachedLatestBalanceAsyncByMonth(customerId, monthToAskInvBalance);
        }

        /// <summary>
        /// 
        /// Call this after CacheLatestBalance() to get the results.
        /// 
        /// </summary>
        /// <param name="lastBalanceRow"></param>
        /// <returns>
        /// 1. Balance Amount of last month
        /// 2. Last updated timestamp of invBalance.
        /// </returns>
        public InvBalAmountsRow GetLatestBalanceDto(InvBalance lastBalanceRow)
        {
            var cachedBalanceRow = new InvBalAmountsRow
            {
                Balance = lastBalanceRow?.Balance ?? 0,
                CashInvestment = lastBalanceRow?.CashInvestment ?? 0,
                TotalCashInvInHold = lastBalanceRow?.TotalCashInvInHold ?? 0,
                LowRiskShares = lastBalanceRow?.LowRiskShares ?? 0,
                MediumRiskShares = lastBalanceRow?.MediumRiskShares ?? 0,
                HighRiskShares = lastBalanceRow?.HighRiskShares ?? 0,
                BegGmBalance = lastBalanceRow?.BegGmBalance ?? 0,
                Deposits = lastBalanceRow?.Deposits ?? 0,
                Withdrawals = lastBalanceRow?.Withdrawals ?? 0,
                GmGainLoss = lastBalanceRow?.GmGainLoss ?? 0,
                EndGmBalance = lastBalanceRow?.EndGmBalance ?? 0,
                CashBonus = lastBalanceRow?.CashBonusAmount ?? 0,
                UpdatedOnUtc = lastBalanceRow?.UpdatedOnUtc ?? DateTime.MinValue
            };

            return cachedBalanceRow;
        }

        /// <summary>
        /// 
        /// Perform all the Summary SQL Queries in a optimized fashion to fill up the summary DTO object.
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<UserSummaryDTO, ApplicationUser>> GetSummaryDataAsync(int userId)
        {
            ApplicationUser userRet = null;
            UserSummaryDTO summaryDtoRet = null;

            try
            {
                var invBalanceRes =
                    GetLatestBalanceDto(
                        await GetCachedLatestBalanceAsync(userId)
                        );

                userRet = await userRepo.GetCachedUserAsync(userId);

                var withdrawalEligibility = GetWithdrawEligibilityData(userId);

                //---------------- Execute SQL Function
                var vintages = await GetCustomerVintagesAsync(userId);

                var lastInvestmentAmount =
                    DbExpressions.RoundCustomerBalanceAmount(gzTransactionRepo.LastInvestmentAmount(userId,
                        DateTime.UtcNow
                            .ToStringYearMonth
                            ()));
                //-------------- Retrieve previously executed async query results

                // user
                if (userRet == null)
                {
                    _logger.Error("User with id {0} is null in GetSummaryData()", userId);
                }

                // Package all the results
                summaryDtoRet = new UserSummaryDTO()
                {

                    Vintages = vintages,

                    Currency = CurrencyHelper.GetSymbol(userRet.Currency),
                    // current balance should not include this month's loss amount to be invested
                    InvestmentsBalance = invBalanceRes.Balance - lastInvestmentAmount, 

                    // Monthly gaming amounts
                    BegGmBalance = invBalanceRes.BegGmBalance,
                    Deposits = invBalanceRes.Deposits,
                    Withdrawals = invBalanceRes.Withdrawals,
                    GmGainLoss = invBalanceRes.GmGainLoss,
                    EndGmBalance = invBalanceRes.EndGmBalance,
                    CashBonus = invBalanceRes.CashBonus,

                    TotalInvestments = invBalanceRes.TotalCashInvInHold,
                    TotalInvestmentsReturns = invBalanceRes.Balance - invBalanceRes.TotalCashInvInHold,

                    NextInvestmentOn = DbExpressions.GetNextMonthsFirstWeekday(),
                    LastInvestmentAmount = lastInvestmentAmount,

                    //latestBalanceUpdateDatetime
                    StatusAsOf = GetLastUpdatedMidnight(invBalanceRes.UpdatedOnUtc),

                    // Withdrawal eligibility
                    LockInDays = withdrawalEligibility.LockInDays,
                    EligibleWithdrawDate = withdrawalEligibility.EligibleWithdrawDate,
                    OkToWithdraw = withdrawalEligibility.OkToWithdraw,
                    Prompt = withdrawalEligibility.Prompt
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in GetSummaryData()");
            }

            return Tuple.Create(summaryDtoRet, userRet);
        }

        /// <summary>
        /// 
        /// Get last updated timestamp reflecting the excel everymatrix numbers update.
        /// -1 Day @ 23:59 of invBalance UpdatedOn timestamp.
        /// 
        /// </summary>
        /// <param name="invBalanceUpdatedTimestamp"></param>
        /// <returns></returns>
        private static DateTime GetLastUpdatedMidnight(DateTime invBalanceUpdatedTimestamp)
        {

            DateTime lastUpdateDate;
            if (invBalanceUpdatedTimestamp > DateTime.MinValue)
            {
                lastUpdateDate = invBalanceUpdatedTimestamp.AddDays(-1);
                lastUpdateDate = new DateTime(lastUpdateDate.Year, lastUpdateDate.Month, lastUpdateDate.Day, 23, 59, 59);
            }
            else
            {
                lastUpdateDate = DateTime.Today;
            }
            return lastUpdateDate;
        }

        #endregion

        #region Vintages

        /// <summary>
        /// 
        /// Data method to enforce the 6? month lock-in period before allowed withdrawals.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public WithdrawEligibilityDTO GetWithdrawEligibilityData(int customerId)
        {

            string prompt = "First available withdrawal on: ";

            var tuple = IsWithdrawalEligible(customerId);
            var eligibleWithdrawDate = tuple.Item2;
            var lockInDays = tuple.Item3;
            bool okToWithdraw = tuple.Item1;

            var retValues = new WithdrawEligibilityDTO()
            {
                LockInDays = lockInDays,
                EligibleWithdrawDate = eligibleWithdrawDate,
                OkToWithdraw = okToWithdraw,
                Prompt = prompt
            };

            return retValues;
        }

        /// <summary>
        /// 
        /// Slack Q and A:
        /// "Post 3 months to sell a vintage" ->  On the 4th month Regardless of the number of days.
        /// 
        /// Biz logic for withdrawal eligibility
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        private Tuple<bool, DateTime, int> IsWithdrawalEligible(int customerId)
        {
            var lockInDays = (confRepo.GetConfRow().Result).LOCK_IN_NUM_DAYS;

            var nowUtc = DateTime.UtcNow;
            var monthsLockCnt = lockInDays / 30;

            var oldestVintageYm = GetAndCacheOldestVintageYm(customerId);

            var oldestVintage = oldestVintageYm != null ? DbExpressions.GetDtYearMonthStrTo1StOfMonth(oldestVintageYm) : nowUtc;
            var eligibleWithdrawDate = oldestVintage.AddMonths(monthsLockCnt + 1); // the 1st of the 4th month
            var okToWithdraw = eligibleWithdrawDate <= nowUtc;

            return Tuple.Create(okToWithdraw, eligibleWithdrawDate, lockInDays);
        }

        private string GetAndCacheOldestVintageYm(int customerId)
        {
            string key = "oldestVintage" + customerId;
            var oldestVintageYm = (string)
                MemoryCache
                    .Default
                    .Get(key) ?? db.InvBalances
                        .Where(i => i.CustomerId == customerId)
                        .Select(i => i.YearMonth)
                        .OrderBy(ym => ym)
                        .FirstOrDefault();

            if (oldestVintageYm != null)
            {
                MemoryCache
                    .Default
                    .Set(key, oldestVintageYm, DateTimeOffset.UtcNow.AddDays(1));
            }
            return oldestVintageYm;
        }

        /// <summary>
        /// 
        /// Get User Vintages from InvBalance.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<List<VintageDto>> GetCustomerVintagesAsync(int customerId)
        {
            var monthsLockPeriod = (await confRepo.GetConfRow()).LOCK_IN_NUM_DAYS / 30;

            var vintagesList = db.Database
                .SqlQuery<VintageDto>(
                    "SELECT InvBalanceId, YearMonthStr, InvestmentAmount, Sold, SellingValue - SoldFees As SellingValue," +
                    " SoldFees, SoldYearMonth FROM dbo.GetVintages(@CustomerId)",
                    new SqlParameter("@CustomerId", customerId))
                .ToList();

            // Remove current month as a vintage
            if (vintagesList.Count > 0 && vintagesList[vintagesList.Count - 1].YearMonthStr == DateTime.UtcNow.ToStringYearMonth()) {
                vintagesList.RemoveAt(vintagesList.Count - 1);
            }

            // Calcluate earliest locking date, locked status
            foreach (var dto in vintagesList)
            {
                var firstOfMonthUnlocked = DbExpressions.GetDtYearMonthStrTo1StOfMonth(dto.YearMonthStr).AddMonths(monthsLockPeriod);
                // Unlock at Utc noon of 1st month day
                dto.Locked = firstOfMonthUnlocked >= DateTime.UtcNow.AddHours(12);
            }

            return vintagesList;
        }

        /// <summary>
        /// 
        /// Unit Test Supporting verson
        /// 
        /// Calculate the vintage in portfolio market value for a given month.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintageCashInvestment"></param>
        /// <param name="vintageYearMonthStr"></param>
        /// <param name="sellOnThisYearMonth"></param>
        /// <param name="vintageSharesDto"></param>
        /// <param name="fees"></param>
        /// <returns></returns>
        private decimal GetVintageValuePricedOn(
            int customerId,
            decimal vintageCashInvestment,
            string vintageYearMonthStr,
            string sellOnThisYearMonth,
            out VintageSharesDto vintageSharesDto,
            out FeesDto fees)
        {

            vintageSharesDto = userPortfolioSharesRepo.GetVintageSharesMarketValueOn(
                customerId,
                vintageYearMonthStr,
                sellOnThisYearMonth);

            fees = gzTransactionRepo.GetWithdrawnFees(vintageCashInvestment, vintageYearMonthStr, vintageSharesDto.PresentMarketPrice);

            return vintageSharesDto.PresentMarketPrice;
        }

        /// <summary>
        /// 
        /// Calculate the vintage in latest portfolio market value
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintageToBeLiquidated"></param>
        /// <param name="portfolioPrices"></param>
        /// <param name="vintageSharesDto"></param>
        /// <param name="fees"></param>
        /// <returns></returns>
        private decimal GetVintageValuePricedNow(
            int customerId,
            VintageDto vintageToBeLiquidated,
            PortfolioPricesDto portfolioPrices,
            out VintageSharesDto vintageSharesDto,
            out FeesDto fees)
        {
            var presentMonth = DateTime.UtcNow.ToStringYearMonth();
            // Sales after the completion of one month only, have earned no interest
            var sellingInFollowingMonth = DbExpressions.AddMonth(vintageToBeLiquidated.YearMonthStr);
            if (sellingInFollowingMonth == presentMonth) {
                vintageSharesDto = new VintageSharesDto() {
                    HighRiskShares = 0m,
                    MediumRiskShares = 0m,
                    LowRiskShares = 0m,
                    PresentMarketPrice = vintageToBeLiquidated.MarketPrice = vintageToBeLiquidated.InvestmentAmount
                };
            }
            // Sales post 2 or more months
            else {

                vintageSharesDto =
                    userPortfolioSharesRepo.GetVintageSharesMarketValue(
                        customerId,
                        vintageToBeLiquidated.YearMonthStr, 
                        portfolioPrices);

            }

            // Obsolete Special case: SALE within same month (ref alaa)
            // if vintageToBeSold month = current month (early cashout in same month)
            // value of vintage is what's not withdrawn from the month's investment cash
            if (vintageToBeLiquidated.YearMonthStr == DateTime.UtcNow.ToStringYearMonth()) {

                vintageSharesDto.PresentMarketPrice =
                    vintageToBeLiquidated.InvestmentAmount
                    - vintageToBeLiquidated.SellingValue
                    - vintageToBeLiquidated.SoldFees;
            }

            fees = gzTransactionRepo.GetWithdrawnFees(
                    vintageToBeLiquidated.InvestmentAmount,
                    vintageToBeLiquidated.YearMonthStr,
                    vintageSharesDto.PresentMarketPrice);

            return vintageSharesDto.PresentMarketPrice;
        }

        /// <summary>
        /// 
        /// ** Unit Test Helper **
        /// 
        /// Set the vintages market selling value on a given month
        /// 
        /// *Does not* checks before attempting to return the present selling value.
        /// 
        /// De-selects a Vintage for selling if sold already (slipped through error validation cracks).
        /// 
        /// Throws an exception if vintage is already sold or not available for selling.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        /// <param name="sellOnThisYearMonth"></param>
        /// <returns>Number of vintages marked for selling</returns>
        public int SetAllSelectedVintagesMarketValueOn(int customerId, IEnumerable<VintageDto> vintages, string sellOnThisYearMonth)
        {

            var numOfVintagesSold = 0;
            foreach (var vintageDto in vintages)
            {
                if (vintageDto.Selected)
                {
                    FeesDto fees;
                    VintageSharesDto vintageShares;
                    vintageDto.MarketPrice = GetVintageValuePricedOn(
                        customerId,
                        vintageDto.InvestmentAmount,
                        vintageDto.YearMonthStr,
                        //-- On this month
                        sellOnThisYearMonth,
                        out vintageShares,
                        out fees);

                    vintageDto.VintageShares = vintageShares;
                    vintageDto.SoldFees = fees.Total;
                    vintageDto.SoldFeesDto = fees;

                    numOfVintagesSold++;
                }
            }

            return numOfVintagesSold;
        }

        /// <summary>
        /// 
        /// Set the vintages market selling value at present market value.
        /// 
        /// Checks for selling preconditions before attempting to return the present selling value.
        /// 
        /// De-selects a Vintage for selling if sold already or locked (slipped through error validation cracks).
        /// 
        /// Throws an exception if vintage is already sold or not available for selling.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        /// <returns>Number of vintages marked for selling</returns>
        public int SetAllSelectedVintagesPresentMarketValue(int customerId, IEnumerable<VintageDto> vintages)
        {

            var numOfVintagesSold = 0;

            var portfolioPrices = userPortfolioSharesRepo.GetCachedLatestPortfolioSharePrice();

            foreach (var vintageDto in vintages)
            {
                if (vintageDto.Selected)
                {
                    FeesDto fees;
                    VintageSharesDto vintageShares;
                    vintageDto.MarketPrice = 
                        GetVintageValuePricedNow(
                            customerId,
                            vintageDto,
                            portfolioPrices,
                            out vintageShares,
                            out fees);

                    vintageDto.VintageShares = vintageShares;
                    vintageDto.SoldFees = fees.Total;
                    vintageDto.SoldFeesDto = fees;

                    numOfVintagesSold++;
                }
            }

            return numOfVintagesSold;
        }

        /// <summary>
        /// 
        /// ** Unit Test Helper **
        /// 
        /// Get the vintages with the selling value calculated if not sold.
        /// 
        /// Calculate the vintage in the market value and deduct fees. The amount the customer
        /// would receive.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customerVintages"></param>
        /// <param name="sellOnThisYearMonth"></param>
        /// <returns></returns>
        public ICollection<VintageDto> GetUserVintagesSellingValueOn(int customerId, List<VintageDto> customerVintages, string sellOnThisYearMonth)
        {

            foreach (var dto in customerVintages
                .Where(v => v.SellingValue == 0 && !v.Locked))
            {
                // out var declarations
                VintageSharesDto vintageShares;
                FeesDto fees;

                // Call to calculate latest selling price
                decimal vintageMarketPrice = GetVintageValuePricedOn(
                        customerId,
                        dto.InvestmentAmount,
                        dto.YearMonthStr,
                        sellOnThisYearMonth,
                        out vintageShares,
                        out fees);

                // Save the selling price and shares
                dto.VintageShares = vintageShares;
                dto.SellingValue = vintageMarketPrice - fees.Total;
                dto.SoldFees = fees.Total;
                dto.SoldFeesDto = fees;
            }

            return customerVintages;
        }

        /// <summary>
        /// 
        /// Get the vintages with the selling value calculated if not sold.
        /// 
        /// Calculate the vintage in latest portfolio market value and deduct fees. The amount the customer
        /// would receive.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customerVintages"></param>
        /// <returns></returns>
        public VintagesWithSellingValues GetCustomerVintagesSellingValueNow(int customerId, List<VintageDto> customerVintages)
        {
            var portfolioPrices = userPortfolioSharesRepo.GetCachedLatestPortfolioSharePrice();

            foreach (var dto in customerVintages)
            {
                if (!dto.Sold && dto.InvestmentAmount != 0 && !dto.Locked)
                {
                    // out var declarations
                    VintageSharesDto vintageShares;
                    FeesDto fees;

                    // Call to calculate latest selling price
                    decimal vintageMarketPrice = 
                        GetVintageValuePricedNow(
                            customerId,
                            dto,
                            portfolioPrices,
                            out vintageShares,
                            out fees);

                    // Save the selling price and shares
                    dto.VintageShares = vintageShares;
                    dto.SellingValue = vintageMarketPrice - fees.Total;
                    dto.SoldFees = fees.Total;
                    dto.SoldFeesDto = fees;
                }
            }
            var soldAmounts = GetSoldVintagesAmounts(customerId);

            return new VintagesWithSellingValues() {
                VintagesDtos = customerVintages,
                SoldVintagesAmounts = soldAmounts
            };
        }

        /// <summary>
        /// 
        /// Query the net proceeds and fees from past "cashed" withdrawn vintages
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public SoldVintagesAmounts GetSoldVintagesAmounts(int userId) {

            var soldAmounts =
                db.InvBalances
                    .Where(i => i.CustomerId == 8 && i.Sold)
                    .Select(i =>
                        new
                        {
                            SoldAmount = i.SoldAmount.Value,
                            SoldFees = i.SoldFees.Value
                        }
                    )
                    .AsEnumerable()
                    .Aggregate(
                        Tuple.Create(0m, 0m), (acc, nextAmounts) =>
                            Tuple.Create(acc.Item1 + (nextAmounts.SoldAmount - nextAmounts.SoldFees) // Total Sold Amount - Fees = Net Proceeds
                            , acc.Item2 + nextAmounts.SoldFees));

            var soldAmountsRet = new SoldVintagesAmounts() {
                TotalSoldVintagesNetProceeds = soldAmounts.Item1,
                TotalSoldVintagesFeesAmount = soldAmounts.Item2
            };

            return soldAmountsRet;
        }

        /// <summary>
        /// 
        /// ** Unit Test Helper **
        /// 
        /// Asks both the user vintages and calculates their selling value.
        /// 
        /// Get the vintages with the selling value calculated if not sold
        /// 
        /// -- or
        /// 
        /// their selling value when they were sold.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<List<VintageDto>> GetCustomerVintagesSellingValueUnitTestHelper(int customerId)
        {

            var customerVintages = await GetCustomerVintagesAsync(customerId);

            GetCustomerVintagesSellingValueNow(customerId, customerVintages);

            return customerVintages;
        }

        /// <summary>
        /// 
        /// Update InvBalances with sold values for a sold vintage
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintage"></param>
        /// <param name="soldOnUtc"></param>
        /// <param name="soldOnYearMonth"></param>
        private void SaveDbSellOneVintageSetInvBalance(int customerId, VintageDto vintage, DateTime soldOnUtc, string soldOnYearMonth = "")
        {
            try
            {
                var vintageToBeSold = db.InvBalances
                    .Where(b => b.Id == vintage.InvBalanceId)
                    .Select(b => b)
                    .Single();

                // Side effect to in-parameter vintage.Sold = true
                vintage.Sold = vintageToBeSold.Sold = true;

                var presentMonth = DateTime.UtcNow.ToStringYearMonth();

                // within the current month multiple withdrawals may occur
                if (soldOnYearMonth == presentMonth) {
                    vintageToBeSold.SoldAmount += vintage.MarketPrice;
                    vintageToBeSold.SoldFees += vintage.SoldFees;
                    vintageToBeSold.EarlyCashoutFee += vintage.SoldFeesDto.EarlyCashoutFee;
                }
                else {
                    vintageToBeSold.SoldAmount = vintage.MarketPrice;
                    vintageToBeSold.SoldFees = vintage.SoldFees;
                    vintageToBeSold.EarlyCashoutFee = vintage.SoldFeesDto.EarlyCashoutFee;
                }
                vintageToBeSold.SoldOnUtc = soldOnUtc.Truncate(TimeSpan.FromSeconds(1));
                vintageToBeSold.HurdleFee = vintage.SoldFeesDto.HurdleFee;
                vintageToBeSold.UpdatedOnUtc = vintageToBeSold.SoldOnUtc.Value;
                // this is set to the month sold
                vintageToBeSold.SoldYearMonth =
                    soldOnYearMonth.Length == 0
                    ? soldOnUtc.ToStringYearMonth()
                    : soldOnYearMonth;

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error("While selling vintage: {0}, for customerId: {1}, updating invBalance: Exception {2}", vintage.YearMonthStr, customerId, ex);
            }
        }

        /// <summary>
        /// 
        /// PreCondition Requirements:
        ///     *** This method assumes it's called within transaction.
        ///     *** This method assumes that vintage marketsPrices are up to date.
        /// 
        /// Perform all database operations for selling a vintage.
        /// 
        /// Sell all vintages marked for selling them.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        /// <param name="soldOnYearMonth"></param>
        private void SaveDbSellAllSelectedVintages(int customerId, ICollection<VintageDto> vintages, string soldOnYearMonth = "")
        {
            var soldOnUtc = DateTime.UtcNow;

            foreach (var vintage in vintages)
            {
                if (vintage.Selected)
                {

                    SaveDbSellOneVintageSetInvBalance(customerId, vintage, soldOnUtc, soldOnYearMonth);
                }
            }
        }

        /// <summary>
        /// 
        /// Sell any vintages marked for selling. 
        /// 
        /// This method will update the vintages selling values before selling them.
        /// The vintages are sold at the current fund prices.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        /// <param name="gmailPwd"></param>
        /// <param name="sellOnThisYearMonth"></param>
        /// <param name="sendEmail2Admin"></param>
        /// <param name="queueAzureConnString"></param>
        /// <param name="queueName"></param>
        /// <param name="emailAdmins"></param>
        /// <param name="gmailUser"></param>
        /// <returns></returns>
        public ICollection<VintageDto> SaveDbSellAllSelectedVintagesInTransRetry(
            int customerId, 
            ICollection<VintageDto> vintages,
            bool sendEmail2Admin,
            string queueAzureConnString,
            string queueName,
            string emailAdmins, 
            string gmailUser, 
            string gmailPwd, 
            string sellOnThisYearMonth = "")
        {

            // Update market price on it.. they may have left the browser window open 
            // for a long time before hitting withdraw

            var vintagesToBeSold =
                sellOnThisYearMonth.Length == 0
                    ? SetAllSelectedVintagesPresentMarketValue(customerId, vintages)
                    : SetAllSelectedVintagesMarketValueOn(customerId, vintages, sellOnThisYearMonth);

            if (vintagesToBeSold > 0)
            {
                ConnRetryConf.TransactWithRetryStrategy(db,

                    () =>
                    {
                        SaveDbSellAllSelectedVintages(customerId, vintages, sellOnThisYearMonth);

                    });

                if (sendEmail2Admin) {

                    var user = GetEmailVintageLiquidationUser(customerId);

                    //https://stackoverflow.com/questions/29383116/asp-net-mvc-5-asynchronous-action-method
                    Task.Run(() => 
                        SendVintageWithdrawalReq(user, customerId, vintages, queueAzureConnString, queueName, emailAdmins, gmailUser, gmailPwd)
                    );
                }
            }

            return vintages;
        }

        /// <summary>
        /// 
        /// Cache essential email user properties
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private VintageLiquidationUser GetEmailVintageLiquidationUser(int userId)
        {
            var user =
                db.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new VintageLiquidationUser()
                    {
                        UserId = userId,
                        GmUserId = u.GmCustomerId.Value,
                        UserName = u.UserName,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Currency = u.Currency
                    })
                    .Single();
            return user;
        }

        /// <summary>
        /// 
        /// Vintage Liquidation User for Email Type
        /// 
        /// </summary>
        private class VintageLiquidationUser
        {
            public int UserId { get; set; }
            public int GmUserId { get; set; }
            public string Email { get; set; }
            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Currency { get; set; }
            public decimal NetProceeds { get; set; }
            public decimal Fees { get; set; }
        }

        /// <summary>
        /// 
        /// Entry point for email receipt sending
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userId"></param>
        /// <param name="vintages"></param>
        /// <param name="queueAzureConnString"></param>
        /// <param name="queueName"></param>
        /// <param name="emailAdmins"></param>
        /// <param name="gmailUser"></param>
        /// <param name="gmailPwd"></param>
        private static void SendVintageWithdrawalReq(
            VintageLiquidationUser user, 
            int userId, 
            ICollection<VintageDto> vintages, 
            string queueAzureConnString,
            string queueName,
            string emailAdmins, 
            string gmailUser, 
            string gmailPwd)
        {
            Logger logger = LogManager.GetCurrentClassLogger();

            var soldVintages = vintages.Where(v => v.Selected).ToList();
            foreach (var vintageDto in soldVintages)
            {
                user.NetProceeds += DbExpressions.RoundCustomerBalanceAmount(vintageDto.SellingValue);
                user.Fees += DbExpressions.RoundGzFeesAmount(vintageDto.SoldFees);
            }

            SendQueueBonusReqMsg(user, queueAzureConnString, queueName, emailAdmins, soldVintages, logger);

            SendEmailBonusReq(user, emailAdmins, gmailUser, gmailPwd, logger);
        }

#region Queue
        private static void SendQueueBonusReqMsg(VintageLiquidationUser user, string queueAzureConnString, string queueName,
            string emailAdmins, List<VintageDto> soldVintages, Logger logger)
        {

            try
            {

                // Parse the connection string and return a reference to the queue.
                var queue = GetQueue(queueAzureConnString, queueName);
                var bonusReq = CreateBonusReq(user, emailAdmins, soldVintages);
                var bonusReqJson = JsonConvert.SerializeObject(bonusReq);
                // Async fire and forget
                var _ =
                    AddQueueMsgAsync(queue, bonusReqJson)
                        .ConfigureAwait(false);

            }
            catch (Exception ex)
            {

                logger.Error(ex, "Exception while sendina a queue message");
            }
        }

        /// <summary>
        /// 
        /// Init az queue
        /// 
        /// </summary>
        /// <param name="queueAzureConnString"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private static CloudQueue GetQueue(string queueAzureConnString, string queueName)
        {

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(queueAzureConnString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            return queueClient.GetQueueReference(queueName);
        }

        /// <summary>
        /// 
        /// Create & init the bonus req object
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="emailAdmins"></param>
        /// <param name="soldVintageDtos"></param>
        /// <returns></returns>
        private static BonusReq CreateBonusReq(VintageLiquidationUser user, string emailAdmins, ICollection<VintageDto> soldVintageDtos)
        {

            var adminsArr = emailAdmins.Split(';');
            var newBonusReq = new BonusReq()
            {
                AdminEmailRecipients = adminsArr,
                Currency = user.Currency,
                Amount = user.NetProceeds,
                Fees = user.Fees,
                GmUserId = user.GmUserId,
                InvBalIds = soldVintageDtos.Select(v => v.InvBalanceId).ToArray(),
                UserEmail = user.Email,
                UserFirstName = user.FirstName
            };
            return newBonusReq;
        }

        /// <summary>
        /// 
        /// Create and send a queue json string (msg)
        /// 
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="bonusJson"></param>
        /// <returns></returns>
        private static async Task AddQueueMsgAsync(CloudQueue queue, string bonusJson)
        {
            CloudQueueMessage qmsg = new CloudQueueMessage(bonusJson);
            await 
                queue
                .AddMessageAsync(qmsg)
                .ConfigureAwait(false);
        }

        #endregion Queue

        #region EmailVintageProceeds

        private static void SendEmailBonusReq(VintageLiquidationUser user, string emailAdmins, string gmailUser,
            string gmailPwd, Logger logger)
        {

            try
            {
                // Async fire and forget
                var _ =
                    EmailSendMimeVintagesProceedsAsync(user, emailAdmins, gmailUser, gmailPwd)
                        .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Error(e, "Exception in SendVintageWithdrawalReq");
            }
        }

        /// <summary>
        /// 
        /// Set up a mime message for sending to admins.
        /// 
        /// </summary>
        /// <param name="vintageLiquidationUser"></param>
        /// <param name="emailAdmins"></param>
        /// <param name="gmailUser"></param>
        /// <param name="gmailPwd"></param>
        /// <returns></returns>
        private static async Task EmailSendMimeVintagesProceedsAsync(VintageLiquidationUser vintageLiquidationUser, string emailAdmins, string gmailUser, string gmailPwd)
        {
            var message = GetMimeMessage(vintageLiquidationUser, emailAdmins);

            await SendEmailAsync(gmailUser, gmailPwd, message).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// Create a mime message for vintage admins
        /// 
        /// </summary>
        /// <param name="vintageLiquidationUser"></param>
        /// <param name="emailAdmins"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        private static MimeMessage GetMimeMessage(VintageLiquidationUser vintageLiquidationUser, string emailAdmins) {

            MimeMessage message = null;
            try {
                if (vintageLiquidationUser.GmUserId == 4300962) {
                    emailAdmins = "mario@greenzorro.com";
                }
                message = GetMessage(vintageLiquidationUser, emailAdmins);

                message.Body = GetMessageBody(vintageLiquidationUser).ToMessageBody();
            }
            catch (Exception e) {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Error(e, "Exception in GetMimeMessage()");
            }

            return message;
        }

        /// <summary>
        /// 
        /// Create the mime message To, Subject
        /// 
        /// </summary>
        /// <param name="vintageLiquidationUser"></param>
        /// <param name="emailAdmins"></param>
        /// <returns></returns>
        private static MimeMessage GetMessage(VintageLiquidationUser vintageLiquidationUser, string emailAdmins) {

            MimeMessage message = null;
            try
            {
                message = new MimeMessage();
                message.From.Add(new MailboxAddress("Admin Greenzorro", "admin@greenzorro.com"));
                var admins = emailAdmins.Split(';');
                message.To.Add(new MailboxAddress(admins[0]));
                if (admins.Length > 1) {
                    message.Cc.Add(new MailboxAddress(admins[1]));
                }
                message.Subject =
                    $@"User id {vintageLiquidationUser.GmUserId} withdrew vintage(s) on {
                            DateTime.UtcNow.ToString("ddd d MMM yyyy")
                        }";
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Error(e, "Exception in GetMessage()");
            }
            return message;
        }

        /// <summary>
        /// 
        /// Create the message text body.
        /// 
        /// </summary>
        /// <param name="vintageLiquidationUser"></param>
        /// <param name="builder"></param>
        private static BodyBuilder GetMessageBody(VintageLiquidationUser vintageLiquidationUser) {

            BodyBuilder builder = null;
            try {
                builder = new BodyBuilder {
                    TextBody = $@"UserName {vintageLiquidationUser.UserName} 
with email {vintageLiquidationUser.Email} withdrew vintage(s)

To userID {vintageLiquidationUser.GmUserId}
Amount {vintageLiquidationUser.NetProceeds} {vintageLiquidationUser.Currency}

Thanks
Your friendly neighborhood admin"
                };
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Error(e, "Exception in GetMessageBody()");
            }

            return builder;
        }

        /// <summary>
        /// 
        /// Slow method.
        /// 
        /// Connect to gmail and send message.
        /// 
        /// </summary>
        /// <param name="gmailUser"></param>
        /// <param name="gmailPwd"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static async Task SendEmailAsync(string gmailUser, string gmailPwd, MimeMessage message) {

            try {

                using (var client = new SmtpClient()) {
                    await client.ConnectAsync("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect)
                        .ConfigureAwait(false);

                    await client.AuthenticateAsync(gmailUser, gmailPwd).ConfigureAwait(false);

                    await client.SendAsync(message).ConfigureAwait(false);

                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Logger logger = LogManager.GetCurrentClassLogger();
                logger.Error(e, "Exception in SendEmailAsync()");
            }
        }

        #endregion EmailVintageProceeds

        #endregion Vintages

        /// <summary>
        /// 
        /// ** Support Seed() only **
        /// 
        /// Upsert an investment balance monthly row
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="userPortfolioRiskSelection"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <param name="cashToInvest"></param>
        /// <param name="monthlyBalance"></param>
        /// <param name="invGainLoss"></param>
        /// <param name="lowRiskShares"></param>
        /// <param name="mediumRiskShares"></param>
        /// <param name="highRiskShares"></param>
        /// <param name="begGmBalance"></param>
        /// <param name="deposits"></param>
        /// <param name="withdrawals"></param>
        /// <param name="gamingGainLoss"></param>
        /// <param name="endGmBalance"></param>
        /// <param name="totalCashInvInHold"></param>
        /// <param name="totalCashInvestments"></param>
        /// <param name="totalSoldVintagesValue"></param>
        /// <param name="updatedOnUtc"></param>
        public void UpsInvBalance(
                int customerId,
                RiskToleranceEnum userPortfolioRiskSelection,
                int yearCurrent,
                int monthCurrent,
                decimal cashToInvest,
                decimal monthlyBalance,
                decimal invGainLoss,
                decimal lowRiskShares,
                decimal mediumRiskShares,
                decimal highRiskShares,
                decimal begGmBalance,
                decimal deposits,
                decimal withdrawals,
                decimal gamingGainLoss,
                decimal endGmBalance,
                decimal totalCashInvInHold,
                decimal totalCashInvestments,
                decimal totalSoldVintagesValue,
                DateTime updatedOnUtc)
        {

            db.InvBalances.AddOrUpdate(i => new { i.CustomerId, i.YearMonth },
                new InvBalance
                {
                    YearMonth = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent),
                    CustomerId = customerId,
                    PortfolioId = (int)userPortfolioRiskSelection,
                    Balance = monthlyBalance,
                    InvGainLoss = invGainLoss,
                    CashInvestment = cashToInvest,
                    LowRiskShares = lowRiskShares,
                    MediumRiskShares = mediumRiskShares,
                    HighRiskShares = highRiskShares,
                    BegGmBalance = begGmBalance,
                    Deposits = deposits,
                    Withdrawals = withdrawals,
                    GmGainLoss = gamingGainLoss,
                    EndGmBalance = endGmBalance,
                    TotalCashInvInHold = totalCashInvInHold,
                    TotalCashInvestments = totalCashInvestments,
                    TotalSoldVintagesValue = totalSoldVintagesValue,
                    UpdatedOnUtc = updatedOnUtc
                });
        }
    }
}