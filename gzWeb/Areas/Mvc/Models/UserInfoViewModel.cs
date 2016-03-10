namespace gzWeb.Areas.Mvc.Models
{
    public class UserInfoViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
        public string Initials { get; set; }
        public string Img { get; set; }
        public string Bg { get; set; }
    }
}