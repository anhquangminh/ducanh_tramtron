namespace BeTong.Models
{
    public class ApprovalStep
    {
        public string Id { get; set; }
        public string DepartmentId { get; set; }
        public string DeptId { get; set; }
        public string Content { get; set; }
        public int ApprovalStepNo { get; set; }
        public int DepartmentOrder { get; set; }
        public int ApprovalOrder { get; set; }

        public ApprovalStep()
        {
            Id = "";
            DepartmentId = "";
            DeptId = "";
            Content = "";
        }
    }
}
