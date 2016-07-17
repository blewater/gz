namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Consolidate_SoldVintage_InvBalance : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SoldVintages", "CustomerId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CustFundShares", "SoldVintageId", "dbo.SoldVintages");
            DropIndex("dbo.CustFundShares", new[] { "SoldVintageId" });
            DropIndex("dbo.SoldVintages", "CustomerId_Mon_idx_gzSoldVintage");
            DropIndex("dbo.SoldVintages", "IX_Sold_Vintages_Cust_VintageYearMonth");
            DropIndex("dbo.SoldVintages", "IX_Sold_Vintages_VintageYearMonth");
            RenameIndex(table: "dbo.InvBalances", name: "IDX_InvBalance_CustomerId_YearMonth_CashInv", newName: "IDX_InvBalance_Cust_YM_CashInv");
            AddColumn("dbo.CustFundShares", "SoldInvBalanceId", c => c.Int());
            AddColumn("dbo.InvBalances", "Sold", c => c.Boolean(nullable: false));
            AddColumn("dbo.InvBalances", "SoldYearMonth", c => c.String(maxLength: 6, fixedLength: true, unicode: false));
            AddColumn("dbo.InvBalances", "SoldAmount", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.InvBalances", "SoldFees", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.InvBalances", "SoldOnUtc", c => c.DateTime());
            CreateIndex("dbo.CustFundShares", "SoldInvBalanceId");
            CreateIndex("dbo.InvBalances", new[] { "CustomerId", "Sold", "SoldYearMonth" }, name: "IDX_InvBalance_Cust_SoldYM_Sold");
            CreateIndex("dbo.InvBalances", new[] { "CustomerId", "YearMonth", "Sold" }, unique: true, name: "IDX_InvBalance_Cust_YM_Sold");
            AddForeignKey("dbo.CustFundShares", "SoldInvBalanceId", "dbo.InvBalances", "Id");
            DropColumn("dbo.CustFundShares", "SoldVintageId");
            DropTable("dbo.SoldVintages");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SoldVintages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        VintageYearMonth = c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false),
                        YearMonth = c.String(nullable: false, maxLength: 6, fixedLength: true, unicode: false),
                        MarketAmount = c.Decimal(nullable: false, precision: 29, scale: 16),
                        Fees = c.Decimal(nullable: false, precision: 29, scale: 16),
                        UpdatedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.CustFundShares", "SoldVintageId", c => c.Int());
            DropForeignKey("dbo.CustFundShares", "SoldInvBalanceId", "dbo.InvBalances");
            DropIndex("dbo.InvBalances", "IDX_InvBalance_Cust_YM_Sold");
            DropIndex("dbo.InvBalances", "IDX_InvBalance_Cust_SoldYM_Sold");
            DropIndex("dbo.CustFundShares", new[] { "SoldInvBalanceId" });
            DropColumn("dbo.InvBalances", "SoldOnUtc");
            DropColumn("dbo.InvBalances", "SoldFees");
            DropColumn("dbo.InvBalances", "SoldAmount");
            DropColumn("dbo.InvBalances", "SoldYearMonth");
            DropColumn("dbo.InvBalances", "Sold");
            DropColumn("dbo.CustFundShares", "SoldInvBalanceId");
            RenameIndex(table: "dbo.InvBalances", name: "IDX_InvBalance_Cust_YM_CashInv", newName: "IDX_InvBalance_CustomerId_YearMonth_CashInv");
            CreateIndex("dbo.SoldVintages", "VintageYearMonth", name: "IX_Sold_Vintages_VintageYearMonth");
            CreateIndex("dbo.SoldVintages", new[] { "CustomerId", "VintageYearMonth" }, unique: true, name: "IX_Sold_Vintages_Cust_VintageYearMonth");
            CreateIndex("dbo.SoldVintages", new[] { "CustomerId", "VintageYearMonth", "YearMonth" }, unique: true, name: "CustomerId_Mon_idx_gzSoldVintage");
            CreateIndex("dbo.CustFundShares", "SoldVintageId");
            AddForeignKey("dbo.CustFundShares", "SoldVintageId", "dbo.SoldVintages", "Id");
            AddForeignKey("dbo.SoldVintages", "CustomerId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
