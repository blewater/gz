﻿using System.Data.Entity;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using gzDAL.ModelUtil;

namespace gzDAL.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        // Mario added extra profile
        [Required, StringLength(30)]
        public string FirstName { get; set; }

        [Required, StringLength(30)]
        public string LastName { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        [NotMapped]
        public decimal InvBalance {
            get {
                return InvBalances
                    .OrderByDescending(b => b.YearMonth)
                    .Select(b => b.Balance)
                    .FirstOrDefault();
            }
        }
        public virtual ICollection<InvBalance> InvBalances { get; set; }
        public virtual ICollection<GzTrx> GzTrxs { get; set; }

        public virtual ICollection<CustPortfolio> CustPortfolios { get; set; }

        //http://stackoverflow.com/questions/24361518/ef-6-1-unique-nullable-index
        [Index]
        public int? GmCustomerId { get; set; }

        [Required]
        public string Currency { get; set; }

        /// <summary>
        /// Whether the account is disabled temporarily for any reason
        /// </summary>
        private bool _disabledGzCustomer = false;
        [Required]
        public bool DisabledGzCustomer {
            get { return _disabledGzCustomer; }
            set { _disabledGzCustomer = value; }
        }

        /// <summary>
        /// Whether the account is closed and no longer a Gz customer.
        /// </summary>
        private bool _closedGzAccount;
        [Required]
        public bool ClosedGzAccount {
            get { return _closedGzAccount; }
            set { _closedGzAccount = value; }
        }



        /// <summary>
        /// Must get from Casino Operator
        /// </summary>
        [Required]
        public bool ActiveCustomerIdInPlatform { get; set; }

#region Calculated fields

        [NotMapped]
        public decimal LastInvestmentAmount {
            get {
                return GzTrxs
                    .Where(t => t.Type.Code == GzTransactionTypeEnum.CreditedPlayingLoss
                                && t.YearMonthCtd == DateTime.UtcNow.ToStringYearMonth())
                    .Select(t => t.Amount)
                    .Sum();
            }
        }
        [NotMapped]
        public decimal TotalInvestmentReturns {
            get {
                return 
                    InvBalances
                    .Select(b => b.InvGainLoss)
                    .Sum();
            }
        }
        [NotMapped]
        public decimal TotalInvestments {
            get {
                return
                    GzTrxs
                    .Where(t => t.Type.Code == GzTransactionTypeEnum.CreditedPlayingLoss)
                    .Select(t => t.Amount)
                    .Sum();
            }
        }
        [NotMapped]
        public decimal TotalWithdrawals {
            get {
                return 
                    GzTrxs
                    .Where(t => t.Type.Code == GzTransactionTypeEnum.TransferToGaming)
                    .Select(t => t.Amount)
                    .Sum();
            }
        }
        
#endregion

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    /// <summary>
    /// Introduced them to change Id to Int identity
    /// </summary>
    public class CustomUserRole : IdentityUserRole<int> { }
    public class CustomUserClaim : IdentityUserClaim<int> { }
    public class CustomUserLogin : IdentityUserLogin<int> { }

    public class CustomRole : IdentityRole<int, CustomUserRole> {
        public CustomRole() { }
        public CustomRole(string name) { Name = name; }
    }

    public class CustomUserStore : UserStore<ApplicationUser, CustomRole, int,
        CustomUserLogin, CustomUserRole, CustomUserClaim> {
        public CustomUserStore(ApplicationDbContext context)
            : base(context) {
        }
    }

    public class CustomRoleStore : RoleStore<CustomRole, int, CustomUserRole> {
        public CustomRoleStore(ApplicationDbContext context)
            : base(context) {
        }
    }

    // End of custom classes for int Id

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, CustomRole, int,
        CustomUserLogin, CustomUserRole, CustomUserClaim>
    {

        /// <summary>
        /// 
        /// Default Constructor to support Update-Database migration script
        /// 
        /// </summary>
        public ApplicationDbContext()

            : this(null) 
        {
        }


        /// <summary>
        /// 
        /// Injection friendly constructor allowing the passing of an explicit connection string or allow the compiled mode to 
        /// dictate it.
        /// 
        /// </summary>
        /// <param name="connectionString">
        /// 
        ///     Null is allowed and means:
        /// 
        ///         In Debug mode --> Construct using the "gzDevDb" connection string.
        /// 
        ///         If in Release mode --> Construct using "gzPrdDb" connection string.
        /// 
        /// </param>
        public ApplicationDbContext(string connectionString)

            : base(GetCompileModeConnString(connectionString)) 
        {
        }


        /// <summary>
        /// 
        /// When a connectionString is null the compile mode
        ///     (DEBUG||Release) mode dictates it:
        /// 
        /// </summary>
        /// <returns>
        ///     Same value if not null
        /// -- or
        ///     DEBUG -> "gzDevDb"
        /// -- or
        ///     Release -> "gzPrdDb"
        /// </returns>
        public static string GetCompileModeConnString(string connectionString) {

            if (connectionString == null)
#if DEBUG
                connectionString = "gzDevDb";
#else
                connectionString = "gzPrdDb";
#endif

            return connectionString;
        }

        public DbSet<CurrencyListX> CurrenciesListX { get; set; }
        public DbSet<CurrencyRate> CurrencyRates { get; set; }
        public DbSet<CustFundShare> CustFundShares { get; set; }
        public DbSet<CustPortfolio> CustPortfolios { get; set; }
        public DbSet<Fund> Funds { get; set; }
        public DbSet<FundPrice> FundPrices { get; set; }
        public DbSet<GzConfiguration> GzConfigurations { get; set; }
        public DbSet<GmTrx> GmTrxs { get; set; }
        public DbSet<GmTrxType> GmTrxTypes { get; set; }
        public DbSet<GzTrx> GzTrxs { get; set; }
        public DbSet<GzTrxType> GzTrxTypes { get; set; }
        public DbSet<InvBalance> InvBalances { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<PortFund> PortFunds { get; set; }
        public DbSet<SoldVintage> SoldVintages { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            // do fluent API stuff below

            // Adjust Sql Server 18,2 decimal to -->
            modelBuilder.Properties<decimal>().Configure(c => c.HasPrecision(29, 16));
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}