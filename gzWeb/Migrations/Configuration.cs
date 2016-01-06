namespace gzWeb.Migrations
{
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Data.Entity.Migrations;
    using Microsoft.AspNet.Identity.Owin;

    internal sealed class Configuration : DbMigrationsConfiguration<gzWeb.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(gzWeb.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            var manager = new ApplicationUserManager(new CustomUserStore(context));

            var user = new ApplicationUser() {
                UserName = "johnsmith@mymail.com",
                Email = "johnsmith@mymail.com",
                EmailConfirmed = true,
                FirstName = "John",
                LastName = "Smith",
                Birthday = new DateTime(1990, 1, 1)
                
            };

            manager.Create(user, "1q2w3e");

            var nfund = new Fund { HoldingName = "iShares MUB", Symbol = "MUB" };
            context.Funds.AddOrUpdate(nfund);
            nfund = new Fund { HoldingName = "Schwab SCHP", Symbol = "SCHP" };
            context.Funds.AddOrUpdate(nfund);
            nfund = new Fund { HoldingName = "State Street XLE", Symbol = "XLE" };
            context.Funds.AddOrUpdate(nfund);
            nfund = new Fund { HoldingName = "Vanguard VEA", Symbol = "VEA" };
            context.Funds.AddOrUpdate(nfund);
            nfund = new Fund { HoldingName = "Vanguard VIG", Symbol = "VIG" };
            context.Funds.AddOrUpdate(nfund);
            nfund = new Fund { HoldingName = "Vanguard VTI", Symbol = "VTI" };
            context.Funds.AddOrUpdate(nfund);
            nfund = new Fund { HoldingName = "Vanguard VWO", Symbol = "VWO" };
            context.Funds.AddOrUpdate(nfund);

            //context.Funds.AddOrUpdate(
            //    f => f,
            //    new Fund { f.HoldingName = "Schwab SCHP" },
            //);

            base.Seed(context);
        }
    }
}
