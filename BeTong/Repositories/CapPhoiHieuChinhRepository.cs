using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BeTong.Data;
using BeTong.Models;
using BeTong.Helpers;

namespace BeTong.Repositories
{
    public class CapPhoiHieuChinhRepository
    {
        private const string MainTable = "xdaCapPhoiHieuChinhs";
        private const string LogTable = "xdaCapPhoiHieuChinh_Logs";

        private static readonly string[] EntityColumns =
        {
            "Id", "IDCapPhoi", "CompanyId", "IDKhachHang", "Ngay", "LoaiCapPhoi", "IDMac",
            "LoaiXiMang", "LoaiDa", "LoaiCat", "DoSut", "YCDB", "LoaiPhuGia",
            "KLRiengXi", "KLRiengCat", "KLRiengDa", "KLRiengPG", "KLRiengTroBay", "GhiChu",
            "DMXM", "DMCat", "DMDa", "DMNuoc", "DMPG", "DMTroBay",
            "TLQDXM", "TLQDCat", "TLQDDa", "TLQDNuoc", "TLQDPG", "TLQDTroBay",
            "LuongSoi", "DoAmCat", "SHCXM", "SHCCat", "SHCDa", "SHCNuoc", "SHCPG", "SHCTroBay",
            "TTXM", "TTCat", "TTDa", "TTNuoc", "TTPG", "TTTroBay",
            "CPSXXM", "CPSXCat", "CPSXDa", "CPSXNuoc", "CPSXPG", "CPSXTroBay",
            "NguoiTao", "NgayTao", "TrangThai", "IDCongTrinh", "HangMuc", "TroBay",
            "IDMacHD", "LoaiXiMangHD", "LoaiDaHD", "LoaiCatHD", "DoSutHD", "YCDBHD",
            "LoaiNuoc", "LoaiTroBay", "IDDinhMuc", "GroupId", "Ordinarily", "CreateAt", "CreateBy",
            "IsActive", "ApprovalUserId", "DateApproval", "DepartmentId", "DepartmentOrder",
            "ApprovalOrder", "ApprovalId", "LastApprovalId", "IsStatus"
        };

        private readonly SqlConnectionFactory _connectionFactory;

        public CapPhoiHieuChinhRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public DataTable GetPage(string groupId, string companyId, string khachHangId, string macId, string congTrinhId, string macHdId, int page, int pageSize, string sortColumn, bool sortDescending, out int totalRows)
        {
            totalRows = 0;

            using (var connection = _connectionFactory.Create())
            using (var command = connection.CreateCommand())
            {
                var where = " WHERE p1.GroupId = @GroupId AND p1.IsActive <> 100 ";
                AppendFilter(ref where, command, "p1.CompanyId", "@CompanyId", companyId);
                AppendFilter(ref where, command, "p1.IDKhachHang", "@IDKhachHang", khachHangId);
                AppendFilter(ref where, command, "p1.IDMac", "@IDMac", macId);
                AppendFilter(ref where, command, "p1.IDCongTrinh", "@IDCongTrinh", congTrinhId);
                AppendFilter(ref where, command, "p1.IDMacHD", "@IDMacHD", macHdId);

                command.Parameters.AddWithValue("@GroupId", groupId ?? "");
                int startRow = Math.Max(1, ((page - 1) * pageSize) + 1);
                int endRow = Math.Max(startRow, page * pageSize);
                command.Parameters.AddWithValue("@StartRow", startRow);
                command.Parameters.AddWithValue("@EndRow", endRow);

                string orderBy = GetSafeOrderBy(sortColumn, sortDescending);
                command.CommandText = @"
                    SELECT COUNT(1)
                    FROM xdaCapPhoiHieuChinhs p1" + where + @";

                    SELECT *
                    FROM
                    (
                    SELECT
                        ROW_NUMBER() OVER (ORDER BY " + orderBy + @") AS RowNumber,
                        p1.Id,
                        p1.IDCapPhoi,
                        ISNULL(cn.TenChiNhanh, p1.CompanyId) AS CompanyId,
                        ISNULL(kh.TenKhachHang, p1.IDKhachHang) AS IDKhachHang,
                        p1.Ngay,
                        p1.LoaiCapPhoi,
                        ISNULL(mac.TenLoaiSanPham, p1.IDMac) AS IDMac,
                        ISNULL(xm.TenXiMang, CONVERT(nvarchar(50), p1.LoaiXiMang)) AS LoaiXiMang,
                        ISNULL(da.TenCotLieu, CONVERT(nvarchar(50), p1.LoaiDa)) AS LoaiDa,
                        ISNULL(cat.TenCotLieuMin, CONVERT(nvarchar(50), p1.LoaiCat)) AS LoaiCat,
                        ISNULL(ds.DoSut, CONVERT(nvarchar(50), p1.DoSut)) AS DoSut,
                        ISNULL(yc.TenYCDB, CONVERT(nvarchar(50), p1.YCDB)) AS YCDB,
                        ISNULL(pg.TenPhuGia, CONVERT(nvarchar(50), p1.LoaiPhuGia)) AS LoaiPhuGia,
                        p1.KLRiengXi, p1.KLRiengCat, p1.KLRiengDa, p1.KLRiengPG, p1.KLRiengTroBay, p1.GhiChu,
                        p1.DMXM, p1.DMCat, p1.DMDa, p1.DMNuoc, p1.DMPG, p1.DMTroBay,
                        p1.TLQDXM, p1.TLQDCat, p1.TLQDDa, p1.TLQDNuoc, p1.TLQDPG, p1.TLQDTroBay,
                        p1.LuongSoi, p1.DoAmCat,
                        p1.SHCXM, p1.SHCCat, p1.SHCDa, p1.SHCNuoc, p1.SHCPG, p1.SHCTroBay,
                        p1.TTXM, p1.TTCat, p1.TTDa, p1.TTNuoc, p1.TTPG, p1.TTTroBay,
                        p1.CPSXXM, p1.CPSXCat, p1.CPSXDa, p1.CPSXNuoc, p1.CPSXPG, p1.CPSXTroBay,
                        p1.NguoiTao, p1.NgayTao, p1.TrangThai,
                        ISNULL(ct.TenCongTrinh, p1.IDCongTrinh) AS IDCongTrinh,
                        p1.HangMuc, p1.TroBay,
                        ISNULL(machd.TenLoaiSanPham, p1.IDMacHD) AS IDMacHD,
                        ISNULL(xmhd.TenXiMang, CONVERT(nvarchar(50), p1.LoaiXiMangHD)) AS LoaiXiMangHD,
                        ISNULL(dahd.TenCotLieu, CONVERT(nvarchar(50), p1.LoaiDaHD)) AS LoaiDaHD,
                        ISNULL(cathd.TenCotLieuMin, CONVERT(nvarchar(50), p1.LoaiCatHD)) AS LoaiCatHD,
                        ISNULL(dshd.DoSut, CONVERT(nvarchar(50), p1.DoSutHD)) AS DoSutHD,
                        ISNULL(ychd.TenYCDB, CONVERT(nvarchar(50), p1.YCDBHD)) AS YCDBHD,
                        ISNULL(nuoc.TenLoaiNuoc, CONVERT(nvarchar(50), p1.LoaiNuoc)) AS LoaiNuoc,
                        ISNULL(tb.TenLoaiTroBay, CONVERT(nvarchar(50), p1.LoaiTroBay)) AS LoaiTroBay,
                        p1.IDDinhMuc, p1.GroupId, p1.Ordinarily, p1.CreateAt, p1.CreateBy,
                        p1.IsActive, p1.ApprovalUserId, p1.DateApproval, p1.DepartmentId,
                        p1.DepartmentOrder, p1.ApprovalOrder, p1.ApprovalId, p1.LastApprovalId, p1.IsStatus
                    FROM xdaCapPhoiHieuChinhs p1
                    LEFT JOIN ChiNhanhs cn ON p1.CompanyId = cn.Id
                    LEFT JOIN xdaKhachHangs kh ON p1.IDKhachHang = kh.Id
                    LEFT JOIN xdaLoaiSanPhams mac ON p1.IDMac = mac.Id
                    LEFT JOIN xdaLoaiXiMangs xm ON p1.LoaiXiMang = xm.IDs
                    LEFT JOIN xdaCotLieuThos da ON p1.LoaiDa = da.IDs
                    LEFT JOIN xdaCotLieuMins cat ON p1.LoaiCat = cat.IDs
                    LEFT JOIN xdaDoSuts ds ON p1.DoSut = ds.IDs
                    LEFT JOIN xdaYeuCauDacBiets yc ON p1.YCDB = yc.IDs
                    LEFT JOIN xdaPhuGias pg ON p1.LoaiPhuGia = pg.IDs
                    LEFT JOIN xdaHDBanBeTongs ct ON p1.IDCongTrinh = ct.Id
                    LEFT JOIN xdaLoaiSanPhams machd ON p1.IDMacHD = machd.Id
                    LEFT JOIN xdaLoaiXiMangs xmhd ON p1.LoaiXiMangHD = xmhd.IDs
                    LEFT JOIN xdaCotLieuThos dahd ON p1.LoaiDaHD = dahd.IDs
                    LEFT JOIN xdaCotLieuMins cathd ON p1.LoaiCatHD = cathd.IDs
                    LEFT JOIN xdaDoSuts dshd ON p1.DoSutHD = dshd.IDs
                    LEFT JOIN xdaYeuCauDacBiets ychd ON p1.YCDBHD = ychd.IDs
                    LEFT JOIN xdaLoaiNuocs nuoc ON p1.LoaiNuoc = nuoc.IDs
                    LEFT JOIN xdaLoaiTroBays tb ON p1.LoaiTroBay = tb.IDs" + where + @"
                    ) AS PageData
                    WHERE RowNumber BETWEEN @StartRow AND @EndRow
                    ORDER BY RowNumber;";

                var table = new DataTable();
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        totalRows = reader.GetInt32(0);
                    }

                    reader.NextResult();
                    table.Load(reader);
                }

                return table;
            }
        }

        public DataTable GetHistory(string id)
        {
            using (var connection = _connectionFactory.Create())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT
                        l.Id, l.IdChung, l.IsValid, l.IDCapPhoi,
                        ISNULL(cn.TenChiNhanh, l.CompanyId) AS CompanyId,
                        ISNULL(kh.TenKhachHang, l.IDKhachHang) AS IDKhachHang,
                        l.Ngay, l.LoaiCapPhoi,
                        ISNULL(mac.TenLoaiSanPham, l.IDMac) AS IDMac,
                        ISNULL(xm.TenXiMang, CONVERT(nvarchar(50), l.LoaiXiMang)) AS LoaiXiMang,
                        ISNULL(da.TenCotLieu, CONVERT(nvarchar(50), l.LoaiDa)) AS LoaiDa,
                        ISNULL(cat.TenCotLieuMin, CONVERT(nvarchar(50), l.LoaiCat)) AS LoaiCat,
                        ISNULL(ds.DoSut, CONVERT(nvarchar(50), l.DoSut)) AS DoSut,
                        ISNULL(yc.TenYCDB, CONVERT(nvarchar(50), l.YCDB)) AS YCDB,
                        ISNULL(pg.TenPhuGia, CONVERT(nvarchar(50), l.LoaiPhuGia)) AS LoaiPhuGia,
                        l.KLRiengXi, l.KLRiengCat, l.KLRiengDa, l.KLRiengPG, l.KLRiengTroBay, l.GhiChu,
                        l.DMXM, l.DMCat, l.DMDa, l.DMNuoc, l.DMPG, l.DMTroBay,
                        l.TLQDXM, l.TLQDCat, l.TLQDDa, l.TLQDNuoc, l.TLQDPG, l.TLQDTroBay,
                        l.LuongSoi, l.DoAmCat, l.SHCXM, l.SHCCat, l.SHCDa, l.SHCNuoc, l.SHCPG, l.SHCTroBay,
                        l.TTXM, l.TTCat, l.TTDa, l.TTNuoc, l.TTPG, l.TTTroBay,
                        l.CPSXXM, l.CPSXCat, l.CPSXDa, l.CPSXNuoc, l.CPSXPG, l.CPSXTroBay,
                        l.NguoiTao, l.NgayTao, l.TrangThai,
                        ISNULL(ct.TenCongTrinh, l.IDCongTrinh) AS IDCongTrinh,
                        l.HangMuc, l.TroBay,
                        ISNULL(machd.TenLoaiSanPham, l.IDMacHD) AS IDMacHD,
                        ISNULL(xmhd.TenXiMang, CONVERT(nvarchar(50), l.LoaiXiMangHD)) AS LoaiXiMangHD,
                        ISNULL(dahd.TenCotLieu, CONVERT(nvarchar(50), l.LoaiDaHD)) AS LoaiDaHD,
                        ISNULL(cathd.TenCotLieuMin, CONVERT(nvarchar(50), l.LoaiCatHD)) AS LoaiCatHD,
                        ISNULL(dshd.DoSut, CONVERT(nvarchar(50), l.DoSutHD)) AS DoSutHD,
                        ISNULL(ychd.TenYCDB, CONVERT(nvarchar(50), l.YCDBHD)) AS YCDBHD,
                        ISNULL(nuoc.TenLoaiNuoc, CONVERT(nvarchar(50), l.LoaiNuoc)) AS LoaiNuoc,
                        ISNULL(tb.TenLoaiTroBay, CONVERT(nvarchar(50), l.LoaiTroBay)) AS LoaiTroBay,
                        l.IDDinhMuc, l.GroupId, l.Ordinarily, l.CreateAt, l.CreateBy, l.IsActive, l.ApprovalUserId,
                        l.DateApproval, l.DepartmentId, l.DepartmentOrder, l.ApprovalOrder, l.ApprovalId, l.LastApprovalId, l.IsStatus
                    FROM xdaCapPhoiHieuChinh_Logs l
                    LEFT JOIN ChiNhanhs cn ON l.CompanyId = cn.Id
                    LEFT JOIN xdaKhachHangs kh ON l.IDKhachHang = kh.Id
                    LEFT JOIN xdaLoaiSanPhams mac ON l.IDMac = mac.Id
                    LEFT JOIN xdaLoaiXiMangs xm ON l.LoaiXiMang = xm.IDs
                    LEFT JOIN xdaCotLieuThos da ON l.LoaiDa = da.IDs
                    LEFT JOIN xdaCotLieuMins cat ON l.LoaiCat = cat.IDs
                    LEFT JOIN xdaDoSuts ds ON l.DoSut = ds.IDs
                    LEFT JOIN xdaYeuCauDacBiets yc ON l.YCDB = yc.IDs
                    LEFT JOIN xdaPhuGias pg ON l.LoaiPhuGia = pg.IDs
                    LEFT JOIN xdaHDBanBeTongs ct ON l.IDCongTrinh = ct.Id
                    LEFT JOIN xdaLoaiSanPhams machd ON l.IDMacHD = machd.Id
                    LEFT JOIN xdaLoaiXiMangs xmhd ON l.LoaiXiMangHD = xmhd.IDs
                    LEFT JOIN xdaCotLieuThos dahd ON l.LoaiDaHD = dahd.IDs
                    LEFT JOIN xdaCotLieuMins cathd ON l.LoaiCatHD = cathd.IDs
                    LEFT JOIN xdaDoSuts dshd ON l.DoSutHD = dshd.IDs
                    LEFT JOIN xdaYeuCauDacBiets ychd ON l.YCDBHD = ychd.IDs
                    LEFT JOIN xdaLoaiNuocs nuoc ON l.LoaiNuoc = nuoc.IDs
                    LEFT JOIN xdaLoaiTroBays tb ON l.LoaiTroBay = tb.IDs
                    WHERE l.IdChung = @Id
                    ORDER BY l.CreateAt ASC";
                command.Parameters.AddWithValue("@Id", id);
                var table = new DataTable();
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    table.Load(reader);
                }

                return table;
            }
        }

        public CapPhoiHieuChinh GetById(string id)
        {
            using (var connection = _connectionFactory.Create())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM " + MainTable + " WHERE Id = @Id AND IsActive <> 100";
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("Không tìm thấy cấp phối hiệu chỉnh đã chọn.");
                    }

                    return ReadEntity(reader);
                }
            }
        }

        public void Insert(CapPhoiHieuChinh entity)
        {
            entity.IDCapPhoi = entity.Id;
            using (var connection = _connectionFactory.Create())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    InsertEntity(connection, transaction, MainTable, entity, null);
                    InsertLog(connection, transaction, entity, entity.Id, true);
                    transaction.Commit();
                }
            }
        }

        public void Update(CapPhoiHieuChinh entity)
        {
            using (var connection = _connectionFactory.Create())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    UpdateEntity(connection, transaction, entity);

                    if (entity.IsActive == 3 || entity.IsActive == 100)
                    {
                        SetCurrentLogsInvalid(connection, transaction, entity.Id);
                    }
                    else
                    {
                        SetLatestLogInvalid(connection, transaction, entity.Id);
                    }

                    InsertLog(connection, transaction, entity, entity.Id, entity.IsActive != 100);
                    transaction.Commit();
                }
            }
        }

        public void UpdateCpsxValues(CapPhoiHieuChinh entity)
        {
            using (var connection = _connectionFactory.Create())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE xdaCapPhoiHieuChinhs
                    SET CPSXXM = @CPSXXM,
                        CPSXCat = @CPSXCat,
                        CPSXDa = @CPSXDa,
                        CPSXNuoc = @CPSXNuoc,
                        CPSXPG = @CPSXPG,
                        CPSXTroBay = @CPSXTroBay,
                        CreateAt = @CreateAt,
                        CreateBy = @CreateBy
                    WHERE Id = @Id
                      AND IsActive <> 100";
                command.Parameters.AddWithValue("@Id", Value(entity.Id));
                command.Parameters.AddWithValue("@CPSXXM", entity.CPSXXM);
                command.Parameters.AddWithValue("@CPSXCat", entity.CPSXCat);
                command.Parameters.AddWithValue("@CPSXDa", entity.CPSXDa);
                command.Parameters.AddWithValue("@CPSXNuoc", entity.CPSXNuoc);
                command.Parameters.AddWithValue("@CPSXPG", entity.CPSXPG);
                command.Parameters.AddWithValue("@CPSXTroBay", entity.CPSXTroBay);
                command.Parameters.AddWithValue("@CreateAt", Value(entity.CreateAt));
                command.Parameters.AddWithValue("@CreateBy", Value(entity.CreateBy));

                connection.Open();
                if (command.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException("Không tìm thấy cấp phối hiệu chỉnh để cập nhật.");
                }
            }
        }

        public void RequestDelete(CapPhoiHieuChinh entity)
        {
            Update(entity);
        }

        public void CheckDuplicate(CapPhoiHieuChinh input, bool editMode)
        {
            using (var connection = _connectionFactory.Create())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                SELECT COUNT(1)
                FROM xdaCapPhoiHieuChinh_Logs
                WHERE IdChung <> @Id
                  AND IsValid = 1
                  AND IsActive <> 100
                  AND CompanyId = @CompanyId
                  AND IDKhachHang = @IDKhachHang
                  AND IDMac = @IDMac
                  AND LoaiXiMang = @LoaiXiMang
                  AND LoaiDa = @LoaiDa
                  AND LoaiCat = @LoaiCat
                  AND DoSut = @DoSut
                  AND YCDB = @YCDB
                  AND LoaiPhuGia = @LoaiPhuGia
                  AND IDCongTrinh = @IDCongTrinh
                  AND IDMacHD = @IDMacHD
                  AND LoaiXiMangHD = @LoaiXiMangHD
                  AND LoaiDaHD = @LoaiDaHD
                  AND LoaiCatHD = @LoaiCatHD
                  AND DoSutHD = @DoSutHD
                  AND YCDBHD = @YCDBHD
                  AND LoaiNuoc = @LoaiNuoc
                  AND LoaiTroBay = @LoaiTroBay" + (editMode ? " AND Id <> @Id" : "");
                AddEntityParameters(command, input);
                connection.Open();
                var count = Convert.ToInt32(command.ExecuteScalar());
                if (count > 0)
                {
                    throw new InvalidOperationException("Thông tin bạn nhập đã tồn tại.");
                }
            }
        }

        public void CheckExclusive(string id, DateTime baseTime)
        {
            var model = GetById(id);
            if (model.CreateAt.HasValue && model.CreateAt.Value > baseTime)
            {
                throw new InvalidOperationException("Thông tin đã bị thay đổi bởi người dùng khác. Vui lòng tải lại dữ liệu.");
            }
        }

        private static void AppendFilter(ref string where, SqlCommand command, string column, string parameter, string value)
        {
            if (!TextHelper.IsNullOrWhiteSpace(value))
            {
                where += " AND " + column + " = " + parameter;
                command.Parameters.AddWithValue(parameter, value);
            }
        }

        private static string GetSafeOrderBy(string sortColumn, bool sortDescending)
        {
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "IDCapPhoi", "CompanyId", "IDKhachHang", "Ngay", "LoaiCapPhoi", "IDMac",
                "LoaiXiMang", "LoaiDa", "LoaiCat", "DoSut", "YCDB", "LoaiPhuGia",
                "KLRiengXi", "KLRiengCat", "KLRiengDa", "KLRiengPG", "KLRiengTroBay", "GhiChu",
                "DMXM", "DMCat", "DMDa", "DMNuoc", "DMPG", "DMTroBay",
                "TLQDXM", "TLQDCat", "TLQDDa", "TLQDNuoc", "TLQDPG", "TLQDTroBay",
                "LuongSoi", "DoAmCat", "SHCXM", "SHCCat", "SHCDa", "SHCNuoc", "SHCPG", "SHCTroBay",
                "TTXM", "TTCat", "TTDa", "TTNuoc", "TTPG", "TTTroBay",
                "CPSXXM", "CPSXCat", "CPSXDa", "CPSXNuoc", "CPSXPG", "CPSXTroBay",
                "NguoiTao", "NgayTao", "TrangThai", "IDCongTrinh", "HangMuc", "TroBay",
                "IDMacHD", "LoaiXiMangHD", "LoaiDaHD", "LoaiCatHD", "DoSutHD", "YCDBHD",
                "LoaiNuoc", "LoaiTroBay", "IDDinhMuc", "CreateAt", "CreateBy", "IsActive",
                "ApprovalUserId", "DateApproval", "DepartmentId", "IsStatus"
            };

            var column = allowed.Contains(sortColumn ?? "") ? sortColumn : "CreateAt";
            return "p1." + column + (sortDescending ? " DESC" : " ASC");
        }

        private static CapPhoiHieuChinh ReadEntity(IDataRecord reader)
        {
            return new CapPhoiHieuChinh
            {
                Id = reader.GetStringOrEmpty("Id"),
                IDCapPhoi = reader.GetStringOrEmpty("IDCapPhoi"),
                CompanyId = reader.GetStringOrEmpty("CompanyId"),
                IDKhachHang = reader.GetStringOrEmpty("IDKhachHang"),
                Ngay = reader.GetNullableDateTime("Ngay"),
                LoaiCapPhoi = reader.GetStringOrEmpty("LoaiCapPhoi"),
                IDMac = reader.GetStringOrEmpty("IDMac"),
                LoaiXiMang = reader.GetIntOrDefault("LoaiXiMang"),
                LoaiDa = reader.GetIntOrDefault("LoaiDa"),
                LoaiCat = reader.GetIntOrDefault("LoaiCat"),
                DoSut = reader.GetIntOrDefault("DoSut"),
                YCDB = reader.GetIntOrDefault("YCDB"),
                LoaiPhuGia = reader.GetIntOrDefault("LoaiPhuGia"),
                KLRiengXi = reader.GetDoubleOrDefault("KLRiengXi"),
                KLRiengCat = reader.GetDoubleOrDefault("KLRiengCat"),
                KLRiengDa = reader.GetDoubleOrDefault("KLRiengDa"),
                KLRiengPG = reader.GetDoubleOrDefault("KLRiengPG"),
                KLRiengTroBay = reader.GetDoubleOrDefault("KLRiengTroBay"),
                GhiChu = reader.GetStringOrEmpty("GhiChu"),
                DMXM = reader.GetDoubleOrDefault("DMXM"),
                DMCat = reader.GetDoubleOrDefault("DMCat"),
                DMDa = reader.GetDoubleOrDefault("DMDa"),
                DMNuoc = reader.GetDoubleOrDefault("DMNuoc"),
                DMPG = reader.GetDoubleOrDefault("DMPG"),
                DMTroBay = reader.GetDoubleOrDefault("DMTroBay"),
                TLQDXM = reader.GetDoubleOrDefault("TLQDXM"),
                TLQDCat = reader.GetDoubleOrDefault("TLQDCat"),
                TLQDDa = reader.GetDoubleOrDefault("TLQDDa"),
                TLQDNuoc = reader.GetDoubleOrDefault("TLQDNuoc"),
                TLQDPG = reader.GetDoubleOrDefault("TLQDPG"),
                TLQDTroBay = reader.GetDoubleOrDefault("TLQDTroBay"),
                LuongSoi = reader.GetDoubleOrDefault("LuongSoi"),
                DoAmCat = reader.GetDoubleOrDefault("DoAmCat"),
                SHCXM = reader.GetDoubleOrDefault("SHCXM"),
                SHCCat = reader.GetDoubleOrDefault("SHCCat"),
                SHCDa = reader.GetDoubleOrDefault("SHCDa"),
                SHCNuoc = reader.GetDoubleOrDefault("SHCNuoc"),
                SHCPG = reader.GetDoubleOrDefault("SHCPG"),
                SHCTroBay = reader.GetDoubleOrDefault("SHCTroBay"),
                TTXM = reader.GetDoubleOrDefault("TTXM"),
                TTCat = reader.GetDoubleOrDefault("TTCat"),
                TTDa = reader.GetDoubleOrDefault("TTDa"),
                TTNuoc = reader.GetDoubleOrDefault("TTNuoc"),
                TTPG = reader.GetDoubleOrDefault("TTPG"),
                TTTroBay = reader.GetDoubleOrDefault("TTTroBay"),
                CPSXXM = reader.GetDoubleOrDefault("CPSXXM"),
                CPSXCat = reader.GetDoubleOrDefault("CPSXCat"),
                CPSXDa = reader.GetDoubleOrDefault("CPSXDa"),
                CPSXNuoc = reader.GetDoubleOrDefault("CPSXNuoc"),
                CPSXPG = reader.GetDoubleOrDefault("CPSXPG"),
                CPSXTroBay = reader.GetDoubleOrDefault("CPSXTroBay"),
                NguoiTao = reader.GetStringOrEmpty("NguoiTao"),
                NgayTao = reader.GetNullableDateTime("NgayTao"),
                TrangThai = reader.GetIntOrDefault("TrangThai"),
                IDCongTrinh = reader.GetStringOrEmpty("IDCongTrinh"),
                HangMuc = reader.GetStringOrEmpty("HangMuc"),
                TroBay = reader.GetIntOrDefault("TroBay"),
                IDMacHD = reader.GetStringOrEmpty("IDMacHD"),
                LoaiXiMangHD = reader.GetIntOrDefault("LoaiXiMangHD"),
                LoaiDaHD = reader.GetIntOrDefault("LoaiDaHD"),
                LoaiCatHD = reader.GetIntOrDefault("LoaiCatHD"),
                DoSutHD = reader.GetIntOrDefault("DoSutHD"),
                YCDBHD = reader.GetIntOrDefault("YCDBHD"),
                LoaiNuoc = reader.GetIntOrDefault("LoaiNuoc"),
                LoaiTroBay = reader.GetIntOrDefault("LoaiTroBay"),
                IDDinhMuc = reader.GetStringOrEmpty("IDDinhMuc"),
                GroupId = reader.GetStringOrEmpty("GroupId"),
                Ordinarily = reader.GetIntOrDefault("Ordinarily"),
                CreateAt = reader.GetNullableDateTime("CreateAt"),
                CreateBy = reader.GetStringOrEmpty("CreateBy"),
                IsActive = reader.GetIntOrDefault("IsActive"),
                ApprovalUserId = reader.GetStringOrEmpty("ApprovalUserId"),
                DateApproval = reader.GetNullableDateTime("DateApproval"),
                DepartmentId = reader.GetStringOrEmpty("DepartmentId"),
                DepartmentOrder = reader.GetIntOrDefault("DepartmentOrder"),
                ApprovalOrder = reader.GetIntOrDefault("ApprovalOrder"),
                ApprovalId = reader.GetStringOrEmpty("ApprovalId"),
                LastApprovalId = reader.GetStringOrEmpty("LastApprovalId"),
                IsStatus = reader.GetStringOrEmpty("IsStatus")
            };
        }

        private static void InsertEntity(SqlConnection connection, SqlTransaction transaction, string tableName, CapPhoiHieuChinh entity, Action<SqlCommand> addExtra)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                var columns = string.Join(", ", EntityColumns);
                var parameters = "@" + string.Join(", @", EntityColumns);
                command.CommandText = "INSERT INTO " + tableName + " (" + columns + ") VALUES (" + parameters + ")";
                AddEntityParameters(command, entity);
                if (addExtra != null)
                {
                    addExtra(command);
                }
                command.ExecuteNonQuery();
            }
        }

        private static void InsertLog(SqlConnection connection, SqlTransaction transaction, CapPhoiHieuChinh entity, string idChung, bool isValid)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                var columns = string.Join(", ", EntityColumns) + ", IdChung, IsValid";
                var parameters = "@" + string.Join(", @", EntityColumns) + ", @IdChung, @IsValid";
                command.CommandText = "INSERT INTO " + LogTable + " (" + columns + ") VALUES (" + parameters + ")";
                var logEntity = CloneForLog(entity);
                logEntity.Id = Guid.NewGuid().ToString();
                AddEntityParameters(command, logEntity);
                command.Parameters.AddWithValue("@IdChung", idChung);
                command.Parameters.AddWithValue("@IsValid", isValid);
                command.ExecuteNonQuery();
            }
        }

        private static CapPhoiHieuChinh CloneForLog(CapPhoiHieuChinh entity)
        {
            return new CapPhoiHieuChinh
            {
                Id = entity.Id,
                IDCapPhoi = entity.IDCapPhoi,
                CompanyId = entity.CompanyId,
                IDKhachHang = entity.IDKhachHang,
                Ngay = entity.Ngay,
                LoaiCapPhoi = entity.LoaiCapPhoi,
                IDMac = entity.IDMac,
                LoaiXiMang = entity.LoaiXiMang,
                LoaiDa = entity.LoaiDa,
                LoaiCat = entity.LoaiCat,
                DoSut = entity.DoSut,
                YCDB = entity.YCDB,
                LoaiPhuGia = entity.LoaiPhuGia,
                KLRiengXi = entity.KLRiengXi,
                KLRiengCat = entity.KLRiengCat,
                KLRiengDa = entity.KLRiengDa,
                KLRiengPG = entity.KLRiengPG,
                KLRiengTroBay = entity.KLRiengTroBay,
                GhiChu = entity.GhiChu,
                DMXM = entity.DMXM,
                DMCat = entity.DMCat,
                DMDa = entity.DMDa,
                DMNuoc = entity.DMNuoc,
                DMPG = entity.DMPG,
                DMTroBay = entity.DMTroBay,
                TLQDXM = entity.TLQDXM,
                TLQDCat = entity.TLQDCat,
                TLQDDa = entity.TLQDDa,
                TLQDNuoc = entity.TLQDNuoc,
                TLQDPG = entity.TLQDPG,
                TLQDTroBay = entity.TLQDTroBay,
                LuongSoi = entity.LuongSoi,
                DoAmCat = entity.DoAmCat,
                SHCXM = entity.SHCXM,
                SHCCat = entity.SHCCat,
                SHCDa = entity.SHCDa,
                SHCNuoc = entity.SHCNuoc,
                SHCPG = entity.SHCPG,
                SHCTroBay = entity.SHCTroBay,
                TTXM = entity.TTXM,
                TTCat = entity.TTCat,
                TTDa = entity.TTDa,
                TTNuoc = entity.TTNuoc,
                TTPG = entity.TTPG,
                TTTroBay = entity.TTTroBay,
                CPSXXM = entity.CPSXXM,
                CPSXCat = entity.CPSXCat,
                CPSXDa = entity.CPSXDa,
                CPSXNuoc = entity.CPSXNuoc,
                CPSXPG = entity.CPSXPG,
                CPSXTroBay = entity.CPSXTroBay,
                NguoiTao = entity.NguoiTao,
                NgayTao = entity.NgayTao,
                TrangThai = entity.TrangThai,
                IDCongTrinh = entity.IDCongTrinh,
                HangMuc = entity.HangMuc,
                TroBay = entity.TroBay,
                IDMacHD = entity.IDMacHD,
                LoaiXiMangHD = entity.LoaiXiMangHD,
                LoaiDaHD = entity.LoaiDaHD,
                LoaiCatHD = entity.LoaiCatHD,
                DoSutHD = entity.DoSutHD,
                YCDBHD = entity.YCDBHD,
                LoaiNuoc = entity.LoaiNuoc,
                LoaiTroBay = entity.LoaiTroBay,
                IDDinhMuc = entity.IDDinhMuc,
                GroupId = entity.GroupId,
                Ordinarily = entity.Ordinarily,
                CreateAt = DateTime.Now,
                CreateBy = entity.CreateBy,
                IsActive = entity.IsActive,
                ApprovalUserId = entity.ApprovalUserId,
                DateApproval = entity.DateApproval,
                DepartmentId = entity.DepartmentId,
                DepartmentOrder = entity.DepartmentOrder,
                ApprovalOrder = entity.ApprovalOrder,
                ApprovalId = entity.ApprovalId,
                LastApprovalId = entity.LastApprovalId,
                IsStatus = entity.IsStatus
            };
        }

        private static void UpdateEntity(SqlConnection connection, SqlTransaction transaction, CapPhoiHieuChinh entity)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                var assignments = new List<string>();
                foreach (var column in EntityColumns)
                {
                    if (column != "Id")
                    {
                        assignments.Add(column + " = @" + column);
                    }
                }
                command.CommandText = "UPDATE " + MainTable + " SET " + string.Join(", ", assignments.ToArray()) + " WHERE Id = @Id";
                AddEntityParameters(command, entity);
                command.ExecuteNonQuery();
            }
        }

        private static void SetCurrentLogsInvalid(SqlConnection connection, SqlTransaction transaction, string id)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "UPDATE " + LogTable + " SET IsValid = 0 WHERE IdChung = @Id AND IsValid = 1";
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }

        private static void SetLatestLogInvalid(SqlConnection connection, SqlTransaction transaction, string id)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
                    UPDATE xdaCapPhoiHieuChinh_Logs
                    SET IsValid = 0
                    WHERE Id = (
                        SELECT TOP 1 Id
                        FROM xdaCapPhoiHieuChinh_Logs
                        WHERE IdChung = @Id
                        ORDER BY CreateAt DESC
                    )";
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }

        private static void AddEntityParameters(SqlCommand command, CapPhoiHieuChinh entity)
        {
            command.Parameters.AddWithValue("@Id", Value(entity.Id));
            command.Parameters.AddWithValue("@IDCapPhoi", Value(entity.IDCapPhoi));
            command.Parameters.AddWithValue("@CompanyId", Value(entity.CompanyId));
            command.Parameters.AddWithValue("@IDKhachHang", Value(entity.IDKhachHang));
            command.Parameters.AddWithValue("@Ngay", Value(entity.Ngay));
            command.Parameters.AddWithValue("@LoaiCapPhoi", Value(entity.LoaiCapPhoi));
            command.Parameters.AddWithValue("@IDMac", Value(entity.IDMac));
            command.Parameters.AddWithValue("@LoaiXiMang", entity.LoaiXiMang);
            command.Parameters.AddWithValue("@LoaiDa", entity.LoaiDa);
            command.Parameters.AddWithValue("@LoaiCat", entity.LoaiCat);
            command.Parameters.AddWithValue("@DoSut", entity.DoSut);
            command.Parameters.AddWithValue("@YCDB", entity.YCDB);
            command.Parameters.AddWithValue("@LoaiPhuGia", entity.LoaiPhuGia);
            command.Parameters.AddWithValue("@KLRiengXi", entity.KLRiengXi);
            command.Parameters.AddWithValue("@KLRiengCat", entity.KLRiengCat);
            command.Parameters.AddWithValue("@KLRiengDa", entity.KLRiengDa);
            command.Parameters.AddWithValue("@KLRiengPG", entity.KLRiengPG);
            command.Parameters.AddWithValue("@KLRiengTroBay", entity.KLRiengTroBay);
            command.Parameters.AddWithValue("@GhiChu", Value(entity.GhiChu));
            command.Parameters.AddWithValue("@DMXM", entity.DMXM);
            command.Parameters.AddWithValue("@DMCat", entity.DMCat);
            command.Parameters.AddWithValue("@DMDa", entity.DMDa);
            command.Parameters.AddWithValue("@DMNuoc", entity.DMNuoc);
            command.Parameters.AddWithValue("@DMPG", entity.DMPG);
            command.Parameters.AddWithValue("@DMTroBay", entity.DMTroBay);
            command.Parameters.AddWithValue("@TLQDXM", entity.TLQDXM);
            command.Parameters.AddWithValue("@TLQDCat", entity.TLQDCat);
            command.Parameters.AddWithValue("@TLQDDa", entity.TLQDDa);
            command.Parameters.AddWithValue("@TLQDNuoc", entity.TLQDNuoc);
            command.Parameters.AddWithValue("@TLQDPG", entity.TLQDPG);
            command.Parameters.AddWithValue("@TLQDTroBay", entity.TLQDTroBay);
            command.Parameters.AddWithValue("@LuongSoi", entity.LuongSoi);
            command.Parameters.AddWithValue("@DoAmCat", entity.DoAmCat);
            command.Parameters.AddWithValue("@SHCXM", entity.SHCXM);
            command.Parameters.AddWithValue("@SHCCat", entity.SHCCat);
            command.Parameters.AddWithValue("@SHCDa", entity.SHCDa);
            command.Parameters.AddWithValue("@SHCNuoc", entity.SHCNuoc);
            command.Parameters.AddWithValue("@SHCPG", entity.SHCPG);
            command.Parameters.AddWithValue("@SHCTroBay", entity.SHCTroBay);
            command.Parameters.AddWithValue("@TTXM", entity.TTXM);
            command.Parameters.AddWithValue("@TTCat", entity.TTCat);
            command.Parameters.AddWithValue("@TTDa", entity.TTDa);
            command.Parameters.AddWithValue("@TTNuoc", entity.TTNuoc);
            command.Parameters.AddWithValue("@TTPG", entity.TTPG);
            command.Parameters.AddWithValue("@TTTroBay", entity.TTTroBay);
            command.Parameters.AddWithValue("@CPSXXM", entity.CPSXXM);
            command.Parameters.AddWithValue("@CPSXCat", entity.CPSXCat);
            command.Parameters.AddWithValue("@CPSXDa", entity.CPSXDa);
            command.Parameters.AddWithValue("@CPSXNuoc", entity.CPSXNuoc);
            command.Parameters.AddWithValue("@CPSXPG", entity.CPSXPG);
            command.Parameters.AddWithValue("@CPSXTroBay", entity.CPSXTroBay);
            command.Parameters.AddWithValue("@NguoiTao", Value(entity.NguoiTao));
            command.Parameters.AddWithValue("@NgayTao", Value(entity.NgayTao));
            command.Parameters.AddWithValue("@TrangThai", entity.TrangThai);
            command.Parameters.AddWithValue("@IDCongTrinh", Value(entity.IDCongTrinh));
            command.Parameters.AddWithValue("@HangMuc", Value(entity.HangMuc));
            command.Parameters.AddWithValue("@TroBay", entity.TroBay);
            command.Parameters.AddWithValue("@IDMacHD", Value(entity.IDMacHD));
            command.Parameters.AddWithValue("@LoaiXiMangHD", entity.LoaiXiMangHD);
            command.Parameters.AddWithValue("@LoaiDaHD", entity.LoaiDaHD);
            command.Parameters.AddWithValue("@LoaiCatHD", entity.LoaiCatHD);
            command.Parameters.AddWithValue("@DoSutHD", entity.DoSutHD);
            command.Parameters.AddWithValue("@YCDBHD", entity.YCDBHD);
            command.Parameters.AddWithValue("@LoaiNuoc", entity.LoaiNuoc);
            command.Parameters.AddWithValue("@LoaiTroBay", entity.LoaiTroBay);
            command.Parameters.AddWithValue("@IDDinhMuc", Value(entity.IDDinhMuc));
            command.Parameters.AddWithValue("@GroupId", Value(entity.GroupId));
            command.Parameters.AddWithValue("@Ordinarily", entity.Ordinarily);
            command.Parameters.AddWithValue("@CreateAt", Value(entity.CreateAt));
            command.Parameters.AddWithValue("@CreateBy", Value(entity.CreateBy));
            command.Parameters.AddWithValue("@IsActive", entity.IsActive);
            command.Parameters.AddWithValue("@ApprovalUserId", Value(entity.ApprovalUserId));
            command.Parameters.AddWithValue("@DateApproval", Value(entity.DateApproval));
            command.Parameters.AddWithValue("@DepartmentId", Value(entity.DepartmentId));
            command.Parameters.AddWithValue("@DepartmentOrder", entity.DepartmentOrder);
            command.Parameters.AddWithValue("@ApprovalOrder", entity.ApprovalOrder);
            command.Parameters.AddWithValue("@ApprovalId", Value(entity.ApprovalId));
            command.Parameters.AddWithValue("@LastApprovalId", Value(entity.LastApprovalId));
            command.Parameters.AddWithValue("@IsStatus", Value(entity.IsStatus));
        }

        private static object Value(object value)
        {
            return value ?? DBNull.Value;
        }
    }
}
