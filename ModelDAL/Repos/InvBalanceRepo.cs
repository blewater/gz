﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using NLog;
using Z.EntityFramework.Plus;

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

            var currentMonth = DateTime.UtcNow.ToStringYearMonth();

            return GetCachedLatestBalanceAsyncByMonth(customerId, currentMonth);
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
                    InvestmentsBalance = invBalanceRes.Balance, // balance

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
                    "SELECT InvBalanceId, YearMonthStr, InvestmentAmount, Sold, COALESCE(SellingValue-SoldFees, 0) As SellingValue," +
                    " SoldFees, SoldYearMonth FROM dbo.GetVintages(@CustomerId)",
                    new SqlParameter("@CustomerId", customerId))
                .ToList();

            foreach (var dto in vintagesList)
            {
                var firstOfMonthUnlocked = DbExpressions.GetDtYearMonthStrTo1StOfMonth(dto.YearMonthStr).AddMonths(monthsLockPeriod + 1);
                dto.Locked = firstOfMonthUnlocked > DateTime.UtcNow;
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
        /// <param name="vintageYearMonthStr"></param>
        /// <param name="sellOnThisYearMonth"></param>
        /// <param name="vintageSharesDto"></param>
        /// <param name="fees"></param>
        /// <returns></returns>
        private decimal GetVintageValuePricedOn(
            int customerId,
            string vintageYearMonthStr,
            string sellOnThisYearMonth,
            out VintageSharesDto vintageSharesDto,
            out decimal fees)
        {

            vintageSharesDto = userPortfolioSharesRepo.GetVintageSharesMarketValueOn(
                customerId,
                vintageYearMonthStr,
                sellOnThisYearMonth);

            fees = gzTransactionRepo.GetWithdrawnFees(vintageSharesDto.MarketPrice);

            return vintageSharesDto.MarketPrice;
        }

        /// <summary>
        /// 
        /// Calculate the vintage in latest portfolio market value
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintageYearMonthStr"></param>
        /// <param name="vintageSharesDto"></param>
        /// <param name="fees"></param>
        /// <returns></returns>
        private decimal GetVintageValuePricedNow(
            int customerId,
            string vintageYearMonthStr,
            out VintageSharesDto vintageSharesDto,
            out decimal fees)
        {

            vintageSharesDto = userPortfolioSharesRepo.GetVintageSharesMarketValue(
                customerId,
                vintageYearMonthStr);

            fees = gzTransactionRepo.GetWithdrawnFees(vintageSharesDto.MarketPrice);

            return vintageSharesDto.MarketPrice;
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
                    decimal fees;
                    VintageSharesDto vintageShares;
                    vintageDto.MarketPrice = GetVintageValuePricedOn(
                        customerId,
                        vintageDto.YearMonthStr,
                        //-- On this month
                        sellOnThisYearMonth,
                        out vintageShares,
                        out fees);

                    vintageDto.VintageShares = vintageShares;
                    vintageDto.Fees = fees;

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
            foreach (var vintageDto in vintages)
            {
                if (vintageDto.Selected)
                {
                    decimal fees;
                    VintageSharesDto vintageShares;
                    vintageDto.MarketPrice = GetVintageValuePricedNow(
                        customerId,
                        vintageDto.YearMonthStr,
                        out vintageShares,
                        out fees);

                    vintageDto.VintageShares = vintageShares;
                    vintageDto.Fees = fees;

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
                decimal fees;

                // Call to calculate latest selling price
                decimal vintageMarketPrice = GetVintageValuePricedOn(
                        customerId,
                        dto.YearMonthStr,
                        sellOnThisYearMonth,
                        out vintageShares,
                        out fees);

                // Save the selling price and shares
                dto.VintageShares = vintageShares;
                dto.SellingValue = vintageMarketPrice - fees;
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
        public (List<VintageDto> vintagesDtos, (decimal totalSoldVintagesNetProceeds, decimal totalSoldVintagesFeesAmount) totalVintagesSoldAmounts) 
            GetCustomerVintagesSellingValueNow(int customerId, List<VintageDto> customerVintages)
        {

            foreach (var dto in customerVintages)
            {
                if (dto.SellingValue == 0 && dto.InvestmentAmount != 0 && !dto.Locked)
                {
                    // out var declarations
                    VintageSharesDto vintageShares;
                    decimal fees;

                    // Call to calculate latest selling price
                    decimal vintageMarketPrice = GetVintageValuePricedNow(
                        customerId,
                        dto.YearMonthStr,
                        out vintageShares,
                        out fees);

                    // Save the selling price and shares
                    dto.VintageShares = vintageShares;
                    dto.SellingValue = vintageMarketPrice - fees;
                }
            }
            var soldAmounts = GetSoldVintagesAmounts(customerId);

            return (customerVintages, soldAmounts);
        }

        public (decimal totalSoldVintagesNetProceeds, decimal totalSoldVintagesFeesAmount) GetSoldVintagesAmounts(int userId) {

            var soldAmounts =

                db.InvBalances
                    .Where(i => i.CustomerId == 9 && i.Sold)
                    .Select(i =>
                        Tuple.Create(
                            i.SoldAmount.Value,
                            i.SoldFees.Value
                        )
                    )
                    .AsEnumerable()
                    .Aggregate((0m, 0m), (acc, nextAmounts) => 
                        (acc.Item1 
                        + (nextAmounts.Item1 - nextAmounts.Item2) // Total Sold Amount - Fees = Net Proceeds
                        , acc.Item2 + nextAmounts.Item2)); // Fees

            return soldAmounts;
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
                vintage.Sold =
                    vintageToBeSold.Sold = true;

                vintageToBeSold.SoldAmount = vintage.MarketPrice;
                vintageToBeSold.SoldFees = vintage.Fees;
                vintageToBeSold.SoldOnUtc = soldOnUtc.Truncate(TimeSpan.FromSeconds(1));
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
        /// The vintages are sold at the current fund prices as of this method call.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        /// <param name="sellOnThisYearMonth"></param>
        /// <returns></returns>
        public void SaveDbSellAllSelectedVintagesInTransRetry(int customerId, ICollection<VintageDto> vintages, string sellOnThisYearMonth = "")
        {

            // Set market price on it.. they may have left the browser window open 
            // for a long time (stock market-wise) before hitting withdraw

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

            }
        }

        #endregion Vintages
        #region Portfolio Selling

        /// <summary>
        /// 
        /// Save the liquidated Customer portfolio funds.
        /// 
        /// Enclosed in a transaction.
        /// 
        /// </summary>
        /// <param name="portfolioFunds"></param>
        /// <param name="customerId"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <param name="newMonthlyBalance"></param>
        /// <param name="invGainLoss"></param>
        /// <param name="monthsPortfolioRisk"></param>
        /// <param name="updatedDateTimeUtc">Set the desired datetime stamp of the db operations</param>
        [Obsolete]
        private void SaveDbLiquidateCustomerPortfolio(
            Dictionary<int, PortfolioFundDTO> portfolioFunds,
            int customerId,
            int yearCurrent,
            int monthCurrent,
            decimal newMonthlyBalance,
            decimal invGainLoss,
            RiskToleranceEnum monthsPortfolioRisk,
            DateTime updatedDateTimeUtc)
        {

            /****************** Liquidate a month ****************/
            var currentYearMonthStr = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent);

            ConnRetryConf.TransactWithRetryStrategy(db,

            () =>
            {

                // Save the portfolio for the month
                custPortfolioRepo.SetDbUserMonthsPortfolioMix(
                    customerId,
                    monthsPortfolioRisk,
                    yearCurrent,
                    monthCurrent,
                    updatedDateTimeUtc);

                // Save fees transactions first and continue with reduced cash amount
                decimal lastInvestmentCredit;
                var remainingCashAmount =
                        gzTransactionRepo.SaveDbLiquidatedPortfolioWithFees(
                            customerId,
                            newMonthlyBalance,
                            GzTransactionTypeEnum.FullCustomerFundsLiquidation,
                            currentYearMonthStr,
                            updatedDateTimeUtc,
                            out lastInvestmentCredit);

                db.InvBalances.AddOrUpdate(i => new { i.CustomerId, i.YearMonth },
                    new InvBalance
                    {
                        YearMonth = currentYearMonthStr,
                        CustomerId = customerId,
                        Balance = 0,
                        //CashBalance = remainingCashAmount,
                        InvGainLoss = invGainLoss,
                        // Save cash from sale trx and last month's credit
                        CashInvestment = -(remainingCashAmount + lastInvestmentCredit),
                        UpdatedOnUtc = updatedDateTimeUtc
                    });

                //userPortfolioSharesRepo.SetDbMonthlyUserPortfolioShares(
                //    boughtShares: false, 
                //    customerId: customerId,
                //    fundsShares: portfolioFunds,
                //    year: yearCurrent,
                //    month: monthCurrent,
                //    updatedOnUtc: updatedDateTimeUtc);

                db.Database.Log = null;

            });
        }

        #endregion Portfolio Selling

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