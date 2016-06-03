namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SoldVintageCustFundShares : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.GzTrxs", "CustomerId_Mon_idx_gztransaction");
            DropIndex("dbo.GzTrxs", new[] { "TypeId" });
            DropIndex("dbo.SoldVintages", "CustomerId_Mon_idx_gzSoldVintage");
            AddColumn("dbo.CustFundShares", "SoldVintageId", c => c.Int());
            AddColumn("dbo.SoldVintages", "YearMonth", c => c.String(nullable: false, maxLength: 6));
            AddColumn("dbo.SoldVintages", "UpdatedOnUtc", c => c.DateTime(nullable: false));
            CreateIndex("dbo.CustFundShares", "SoldVintageId");
            CreateIndex("dbo.SoldVintages", new[] { "CustomerId", "YearMonth", "VintageYearMonth" }, unique: true, name: "CustomerId_Mon_idx_gzSoldVintage");
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "YearMonthCtd", "TypeId" }, name: "CustomerId_Mon_idx_gztransaction");
            AddForeignKey("dbo.CustFundShares", "SoldVintageId", "dbo.SoldVintages", "Id");
            DropColumn("dbo.CustFundShares", "SoldNewSharesNum");
            DropColumn("dbo.CustFundShares", "CashedNewSharesValue");
            DropColumn("dbo.SoldVintages", "SoldOnUtc");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SoldVintages", "SoldOnUtc", c => c.DateTime(nullable: false));
            AddColumn("dbo.CustFundShares", "CashedNewSharesValue", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.CustFundShares", "SoldNewSharesNum", c => c.Decimal(precision: 29, scale: 16));
            DropForeignKey("dbo.CustFundShares", "SoldVintageId", "dbo.SoldVintages");
            DropIndex("dbo.GzTrxs", "CustomerId_Mon_idx_gztransaction");
            DropIndex("dbo.SoldVintages", "CustomerId_Mon_idx_gzSoldVintage");
            DropIndex("dbo.CustFundShares", new[] { "SoldVintageId" });
            DropColumn("dbo.SoldVintages", "UpdatedOnUtc");
            DropColumn("dbo.SoldVintages", "YearMonth");
            DropColumn("dbo.CustFundShares", "SoldVintageId");
            CreateIndex("dbo.SoldVintages", new[] { "CustomerId", "VintageYearMonth" }, name: "CustomerId_Mon_idx_gzSoldVintage");
            CreateIndex("dbo.GzTrxs", "TypeId");
            CreateIndex("dbo.GzTrxs", new[] { "CustomerId", "YearMonthCtd" }, name: "CustomerId_Mon_idx_gztransaction");
        }
    }
}
