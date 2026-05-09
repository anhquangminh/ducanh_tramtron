using System.Collections.Generic;
using System.Data.SqlClient;
using BeTong.Data;
using BeTong.Models;

namespace BeTong.Repositories
{
    public class LookupRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public LookupRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<LookupItem> GetChiNhanhs(string groupId, bool includeAll)
        {
            return GetLookup("ChiNhanhs", "Id", "TenChiNhanh", groupId, includeAll);
        }

        public List<LookupItem> GetKhachHangs(string groupId, bool includeAll)
        {
            return GetLookup("xdaKhachHangs", "Id", "TenKhachHang", groupId, includeAll);
        }

        public List<LookupItem> GetLoaiSanPhams(string groupId, bool includeAll)
        {
            return GetLookup("xdaLoaiSanPhams", "Id", "TenLoaiSanPham", groupId, includeAll);
        }

        public List<LookupItem> GetLoaiXiMangs(string groupId, bool includeAll)
        {
            return GetLookup("xdaLoaiXiMangs", "IDs", "TenXiMang", groupId, includeAll);
        }

        public List<LookupItem> GetCotLieuThos(string groupId, bool includeAll)
        {
            return GetLookup("xdaCotLieuThos", "IDs", "TenCotLieu", groupId, includeAll);
        }

        public List<LookupItem> GetCotLieuMins(string groupId, bool includeAll)
        {
            return GetLookup("xdaCotLieuMins", "IDs", "TenCotLieuMin", groupId, includeAll);
        }

        public List<LookupItem> GetDoSuts(string groupId, bool includeAll)
        {
            return GetLookup("xdaDoSuts", "IDs", "DoSut", groupId, includeAll);
        }

        public List<LookupItem> GetYeuCauDacBiets(string groupId, bool includeAll)
        {
            return GetLookup("xdaYeuCauDacBiets", "IDs", "TenYCDB", groupId, includeAll);
        }

        public List<LookupItem> GetPhuGias(string groupId, bool includeAll)
        {
            return GetLookup("xdaPhuGias", "IDs", "TenPhuGia", groupId, includeAll);
        }

        public List<LookupItem> GetHDBanBeTongs(string groupId, bool includeAll)
        {
            return GetLookup("xdaHDBanBeTongs", "Id", "TenCongTrinh", groupId, includeAll);
        }

        public List<LookupItem> GetLoaiNuocs(string groupId, bool includeAll)
        {
            return GetLookup("xdaLoaiNuocs", "IDs", "TenLoaiNuoc", groupId, includeAll);
        }

        public List<LookupItem> GetLoaiTroBays(string groupId, bool includeAll)
        {
            return GetLookup("xdaLoaiTroBays", "IDs", "TenLoaiTroBay", groupId, includeAll);
        }

        private List<LookupItem> GetLookup(string table, string valueColumn, string textColumn, string groupId, bool includeAll)
        {
            var result = new List<LookupItem>();
            if (includeAll)
            {
                result.Add(new LookupItem { Value = "", Text = "Tất cả" });
            }
            else
            {
                result.Add(new LookupItem { Value = "", Text = "Chọn" });
            }

            using (var connection = _connectionFactory.Create())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format(
                    "SELECT CONVERT(nvarchar(100), {0}) AS Value, CONVERT(nvarchar(500), {1}) AS Text FROM {2} WHERE IsActive <> 100 AND (@GroupId = '' OR GroupId = @GroupId) ORDER BY {1}",
                    valueColumn,
                    textColumn,
                    table);
                command.Parameters.AddWithValue("@GroupId", groupId ?? "");
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new LookupItem
                        {
                            Value = reader.GetStringOrEmpty("Value"),
                            Text = reader.GetStringOrEmpty("Text")
                        });
                    }
                }
            }

            return result;
        }
    }
}
