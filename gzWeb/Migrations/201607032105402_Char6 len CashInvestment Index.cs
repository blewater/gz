namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Char6lenCashInvestmentIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.CurrencyRates", "CurrRate_ftd_idx");
            DropIndex("dbo.CustFundShares", "CustFundShareId_YMD_idx");
            DropIndex("dbo.FundPrices", "FundId_YMD_idx");
            DropIndex("dbo.SoldVintages", "CustomerId_Mon_idx_gzSoldVintage");
            DropIndex("dbo.SoldVintages", "IX_Sold_Vintages_Cust_VintageYearMonth");
            DropIndex("dbo.SoldVintages", "IX_Sold_Vintages_VintageYearMonth");
            DropIndex("dbo.CustPortfolios", "CustomerId_Mon_idx_custp");
            DropIndex("dbo.GzTrxs", "IX_CustomerId_YM_TId_Amnt");
            DropIndex("dbo.GmTrxs", new[] { "YearMonthCtd" });
            DropIndex("dbo.InvBalances", "CustomerId_Mon_idx_invbal");
            DropIndex("dbo.InvBalances", "YearMonth_Only_idx_invbal");
            AlterColumn("dbo.CurrencyRates", "FromTo", c => c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false));
            AlterColumn("dbo.CustFundShares", "YearMonth", c => c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false));
            AlterColumn("dbo.FundPrices", "YearMonthDay", c => c.String(maxLength: 8, fixedLength: true, unicode: false));
            AlterColumn("dbo.SoldVintages", "VintageYearMonth", c => c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false));
            AlterColumn("dbo.SoldVintages", "YearMonth", c => c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false));
            AlterColumn("dbo.CustPortfolios", "YearMonth", c => c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false));
            AlterColumn("dbo.GzTrxs", "YearMonthCtd", c => c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false));
            AlterColumn("dbo.GmTrxs", "YearMonthCtd", c => c.String(maxLength: 6, fixedLength: true, unicode: false));
            AlterColumn("dbo.InvBalances", "YearMonth", c => c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false));
            CreateIndex("dbo.CurrencyRates", new[] { "TradeDateTime", "FromTo" }, unique: true, name: "CurrRate_ftd_idx");
            CreateIndex("dbo.CustFundShares", new[] { "CustomerId", "YearMonth", "FundId" }, unique: true, name: "CustFundShareId_YMD_idx");
            CreateIndex("dbo.FundPrices", new[] { "FundId", "YearMonthDay" }, unique: true, name: "FundId_YMD_idx");
            CreateIndex("dbo.SoldVintages", new[] { "CustomerId", "VintageYearMonth", "YearMonth" }, unique: true, name: "CustomerId_Mon_idx_gzSoldVintage");
            CreateIndex("dbo.SoldVintages", new[] { "CustomerId", "VintageYearMonth" }, unique: true, name: "IX_Sold_Vintages_Cust_VintageYearMonth");
            CreateIndex("dbo.SoldVintages", "VintageYearMonth", name: "IX_Sold_Vintages_VintageYearMonth");
            CreateIndex("dbo.CustPortfolios", new[] { "CustomerId", "YearMonth", "PortfolioId" }, unique: true, name: "CustomerId_Mon_idx_custp");
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "YearMonthCtd", "TypeId", "Amount" }, name: "IX_CustomerId_YM_TId_Amnt");
            CreateIndex("dbo.GmTrxs", "YearMonthCtd");
            CreateIndex("dbo.InvBalances", new[] { "CustomerId", "YearMonth" }, unique: true, name: "CustomerId_Mon_idx_invbal");
            CreateIndex("dbo.InvBalances", new[] { "CustomerId", "YearMonth", "CashBalance" }, unique: true, name: "IDX_InvBalance_CustomerId_YearMonth_CashInv");
            CreateIndex("dbo.InvBalances", "YearMonth", name: "YearMonth_Only_idx_invbal");
            CreateIndex("dbo.InvBalances", "CashInvestment", name: "ID_InvBalance_CashInvestment_Only");
        }
        
        public override void Down()
        {
            DropIndex("dbo.InvBalances", "ID_InvBalance_CashInvestment_Only");
            DropIndex("dbo.InvBalances", "YearMonth_Only_idx_invbal");
            DropIndex("dbo.InvBalances", "IDX_InvBalance_CustomerId_YearMonth_CashInv");
            DropIndex("dbo.InvBalances", "CustomerId_Mon_idx_invbal");
            DropIndex("dbo.GmTrxs", new[] { "YearMonthCtd" });
            DropIndex("dbo.GzTrxs", "IX_CustomerId_YM_TId_Amnt");
            DropIndex("dbo.CustPortfolios", "CustomerId_Mon_idx_custp");
            DropIndex("dbo.SoldVintages", "IX_Sold_Vintages_VintageYearMonth");
            DropIndex("dbo.SoldVintages", "IX_Sold_Vintages_Cust_VintageYearMonth");
            DropIndex("dbo.SoldVintages", "CustomerId_Mon_idx_gzSoldVintage");
            DropIndex("dbo.FundPrices", "FundId_YMD_idx");
            DropIndex("dbo.CustFundShares", "CustFundShareId_YMD_idx");
            DropIndex("dbo.CurrencyRates", "CurrRate_ftd_idx");
            AlterColumn("dbo.InvBalances", "YearMonth", c => c.String(nullable: false, maxLength: 6));
            AlterColumn("dbo.GmTrxs", "YearMonthCtd", c => c.String(maxLength: 6));
            AlterColumn("dbo.GzTrxs", "YearMonthCtd", c => c.String(nullable: false, maxLength: 6));
            AlterColumn("dbo.CustPortfolios", "YearMonth", c => c.String(nullable: false, maxLength: 6));
            AlterColumn("dbo.SoldVintages", "YearMonth", c => c.String(nullable: false, maxLength: 6));
            AlterColumn("dbo.SoldVintages", "VintageYearMonth", c => c.String(nullable: false, maxLength: 6));
            AlterColumn("dbo.FundPrices", "YearMonthDay", c => c.String(maxLength: 8));
            AlterColumn("dbo.CustFundShares", "YearMonth", c => c.String(nullable: false, maxLength: 6));
            AlterColumn("dbo.CurrencyRates", "FromTo", c => c.String(nullable: false, maxLength: 6));
            CreateIndex("dbo.InvBalances", "YearMonth", name: "YearMonth_Only_idx_invbal");
            CreateIndex("dbo.InvBalances", new[] { "CustomerId", "YearMonth" }, unique: true, name: "CustomerId_Mon_idx_invbal");
            CreateIndex("dbo.GmTrxs", "YearMonthCtd");
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "YearMonthCtd", "TypeId", "Amount" }, name: "IX_CustomerId_YM_TId_Amnt");
            CreateIndex("dbo.CustPortfolios", new[] { "CustomerId", "YearMonth", "PortfolioId" }, unique: true, name: "CustomerId_Mon_idx_custp");
            CreateIndex("dbo.SoldVintages", "VintageYearMonth", name: "IX_Sold_Vintages_VintageYearMonth");
            CreateIndex("dbo.SoldVintages", new[] { "CustomerId", "VintageYearMonth" }, unique: true, name: "IX_Sold_Vintages_Cust_VintageYearMonth");
            CreateIndex("dbo.SoldVintages", new[] { "CustomerId", "VintageYearMonth", "YearMonth" }, unique: true, name: "CustomerId_Mon_idx_gzSoldVintage");
            CreateIndex("dbo.FundPrices", new[] { "FundId", "YearMonthDay" }, unique: true, name: "FundId_YMD_idx");
            CreateIndex("dbo.CustFundShares", new[] { "CustomerId", "YearMonth", "FundId" }, unique: true, name: "CustFundShareId_YMD_idx");
            CreateIndex("dbo.CurrencyRates", new[] { "TradeDateTime", "FromTo" }, unique: true, name: "CurrRate_ftd_idx");
        }
    }
}
