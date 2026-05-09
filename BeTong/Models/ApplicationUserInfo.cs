namespace BeTong.Models
{
    public class ApplicationUserInfo
    {
        public string Id { get; set; }
        public string UserId
        {
            get => Id;
            set => Id = value;
        }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string CompanyId { get; set; }
        public string GroupId { get; set; }
        public string CreateBy { get; set; }

        public ApplicationUserInfo()
        {
            Id = "";
            UserName = "";
            Email = "";
            CompanyId = "";
            GroupId = "";
            CreateBy = "";
        }
    }
}
