namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtraVintageWithdrawalFees : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.InvBalances", "EarlyCashoutFee", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.InvBalances", "HurdleFee", c => c.Decimal(precision: 29, scale: 16));
            AddColumn("dbo.GzConfigurations", "EARLY_WITHDRAWAL_FEE_PCNT", c => c.Int(nullable: false, defaultValue: 25, defaultValueSql: "25"));
            AddColumn("dbo.GzConfigurations", "HURDLE_FEE_PCNT", c => c.Int(nullable: false, defaultValue: 25, defaultValueSql: "25"));
            AddColumn("dbo.GzConfigurations", "HURDLE_TRIGGER_GAIN_PCNT", c => c.Int(nullable: false, defaultValue: 10, defaultValueSql: "10"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GzConfigurations", "HURDLE_TRIGGER_GAIN_PCNT");
            DropColumn("dbo.GzConfigurations", "HURDLE_FEE_PCNT");
            DropColumn("dbo.GzConfigurations", "EARLY_WITHDRAWAL_FEE_PCNT");
            DropColumn("dbo.InvBalances", "HurdleFee");
            DropColumn("dbo.InvBalances", "EarlyCashoutFee");
        }
    }
}
