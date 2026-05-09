namespace BeTong.Models
{
    public class CapPhoiHieuChinhLog : CapPhoiHieuChinh
    {
        public string IdChung { get; set; }
        public bool IsValid { get; set; }

        public CapPhoiHieuChinhLog()
        {
            IdChung = "";
        }
    }
}
