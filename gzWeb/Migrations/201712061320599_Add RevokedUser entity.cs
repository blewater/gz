namespace gzWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRevokedUserentity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RevokedUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Birthday = c.DateTime(nullable: false),
                        GmCustomerId = c.Int(),
                        Currency = c.String(),
                        DisabledGzCustomer = c.Boolean(nullable: false),
                        ClosedGzAccount = c.Boolean(nullable: false),
                        LastLogin = c.DateTime(),
                        ActiveCustomerIdInPlatform = c.Boolean(nullable: false),
                        IsRegistrationFinalized = c.Boolean(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.RevokedUsers");
        }
    }
}
