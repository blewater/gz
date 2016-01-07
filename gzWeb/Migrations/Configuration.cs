namespace gzWeb.Migrations
{
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Data.Entity.Migrations;
    using Microsoft.AspNet.Identity.Owin;
    using System.Data.Entity;
    internal sealed class Configuration : DbMigrationsConfiguration<gzWeb.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(gzWeb.Models.ApplicationDbContext context) {
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

            DbContextTransaction transaction = null;
            try {
                transaction = context.Database.BeginTransaction();

                AddUpdData(context);
                transaction.Commit();

                // if succeeded call base.seed
                base.Seed(context);

            } catch (Exception ex) {

                if (transaction != null) {
                    transaction.Rollback();
                    transaction.Dispose();
                }
            }

        }

        private static void AddUpdData(ApplicationDbContext context) {
            var manager = new ApplicationUserManager(new CustomUserStore(context));

            Random rnd = new Random();
            int rndPlatformId = rnd.Next(1, int.MaxValue);

            var newUser = new ApplicationUser() {
                UserName = "joe@mymail.com",
                Email = "joe@mymail.com",
                EmailConfirmed = true,
                FirstName = "Joe",
                LastName = "Smith",
                Birthday = new DateTime(1990, 1, 1),
                PlatformCustomerId = rndPlatformId,
                PlatformBalance = new decimal(4200.54),
                LastUpdatedBalance = DateTime.Now
            };

            // Truncate tables
            http://stackoverflow.com/questions/24209220/ef6-code-first-drop-tables-not-entire-database-when-model-changes
            var fUser = manager.FindByEmail(newUser.Email);
            if (fUser == null) {
                manager.Create(newUser, "1q2w3e");
            } else {
                manager.Update(newUser);
            }

            context.Funds.AddOrUpdate(
                f => f.Symbol,
                new Fund {
                    HoldingName = "iShares MUB", Symbol = "MUB"
                },
                new Fund {
                    HoldingName = "Schwab SCHP", Symbol = "SCHP"
                },
                new Fund {
                    HoldingName = "State Street XLE", Symbol = "XLE"
                },
                new Fund {
                    HoldingName = "Vanguard VEA", Symbol = "VEA"
                },
                new Fund {
                    HoldingName = "Vanguard VIG", Symbol = "VIG"
                },
                new Fund {
                    HoldingName = "Vanguard VTI", Symbol = "VTI"
                },
                new Fund {
                    HoldingName = "Vanguard VWO", Symbol = "VWO"
                }
                );
        }
    }
}
