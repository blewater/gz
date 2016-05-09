namespace gzDAL.Models {

    /// <summary>
    /// 
    /// A unit testing Only Application DB Context set to the gzTestDb database.
    /// 
    /// Divorce using ApplicationDbContext in unit tests that may nuke an existing database.
    /// 
    /// </summary>
    public class TestDbContext : ApplicationDbContext {

        public TestDbContext() : base("gzTestDb") {
            
        }
    }
}