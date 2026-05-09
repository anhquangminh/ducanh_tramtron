using System;
using BeTong.Data;
using BeTong.Models;

namespace BeTong.Services
{
    public class ApprovalService
    {
        public const string MajorIdCapPhoiHieuChinh = "dbcd285b-4379-4bce-ab53-92d4b42e766e";
        public const string PermissionAdd = "9623f2b0-7cf9-0578-ea01-b2ba3ddbec85";
        public const string PermissionEdit = "3a8ac9b3-cb7c-1f22-7487-93be72bdae86";
        public const string PermissionDelete = "ddb90ffb-7846-ae74-7b6c-1b1981c9054b";

        private readonly SqlConnectionFactory _connectionFactory;

        public ApprovalService(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public ApprovalStep GetFirstApprovalStep(string companyId, string permissionId)
        {
            using (var connection = _connectionFactory.Create())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT TOP 1 p.Id, p.DeptId, p.Content, p.ApprovalStep, p.DeptOrder AS DepartmentOrder, p.ApprovalOrder
                    FROM ApprovalStepSettings p
                    INNER JOIN ApprovalDeptSettings q ON p.MajorId = q.MajorId AND p.CompanyId = q.CompanyId AND p.DeptId = q.DeptId
                    WHERE p.CompanyId = @CompanyId
                      AND p.MajorId = @MajorId
                      AND p.PermissionId = @PermissionId
                      AND p.ApprovalStep = 1
                      AND q.ApprovalStep = 1
                      AND p.IsActive <> 100
                    ORDER BY p.ApprovalStep";
                command.Parameters.AddWithValue("@CompanyId", companyId);
                command.Parameters.AddWithValue("@MajorId", MajorIdCapPhoiHieuChinh);
                command.Parameters.AddWithValue("@PermissionId", permissionId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("Bạn chưa cài đặt bước duyệt đầu tiên cho nghiệp vụ này.");
                    }

                    return ReadStep(reader);
                }
            }
        }

        public ApprovalStep GetLastApprovalStep(string companyId, string permissionId)
        {
            using (var connection = _connectionFactory.Create())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP 1 p.Id, p.DeptId, p.Content, p.ApprovalStep, p.DeptOrder AS DepartmentOrder, p.ApprovalOrder
FROM ApprovalStepSettings p
WHERE p.CompanyId = @CompanyId
  AND p.MajorId = @MajorId
  AND p.PermissionId = @PermissionId
  AND p.IsActive <> 100
ORDER BY p.DeptOrder DESC, p.ApprovalStep DESC";
                command.Parameters.AddWithValue("@CompanyId", companyId);
                command.Parameters.AddWithValue("@MajorId", MajorIdCapPhoiHieuChinh);
                command.Parameters.AddWithValue("@PermissionId", permissionId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("Bạn chưa cài đặt bước duyệt cuối cho nghiệp vụ này.");
                    }

                    return ReadStep(reader);
                }
            }
        }

        public ApprovalStep GetNextApprovalStep(string companyId, string permissionId, string departmentId, int departmentOrder, int approvalOrder)
        {
            using (var connection = _connectionFactory.Create())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
SELECT TOP 1 Id, DeptId, Content, ApprovalStep, DeptOrder AS DepartmentOrder, ApprovalOrder
FROM ApprovalStepSettings
WHERE CompanyId = @CompanyId
  AND MajorId = @MajorId
  AND PermissionId = @PermissionId
  AND DeptId = @DeptId
  AND ApprovalStep = @NextApprovalOrder
  AND IsActive <> 100";
                    command.Parameters.AddWithValue("@CompanyId", companyId);
                    command.Parameters.AddWithValue("@MajorId", MajorIdCapPhoiHieuChinh);
                    command.Parameters.AddWithValue("@PermissionId", permissionId);
                    command.Parameters.AddWithValue("@DeptId", departmentId);
                    command.Parameters.AddWithValue("@NextApprovalOrder", approvalOrder + 1);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadStep(reader);
                        }
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
SELECT TOP 1 p.Id, p.DeptId, p.Content, p.ApprovalStep, p.DeptOrder AS DepartmentOrder, p.ApprovalOrder
FROM ApprovalDeptSettings d
INNER JOIN ApprovalStepSettings p ON d.CompanyId = p.CompanyId AND d.MajorId = p.MajorId AND d.DeptId = p.DeptId
WHERE d.CompanyId = @CompanyId
  AND d.MajorId = @MajorId
  AND d.ApprovalStep = @NextDepartmentOrder
  AND p.PermissionId = @PermissionId
  AND p.ApprovalStep = 1
  AND p.IsActive <> 100
ORDER BY p.ApprovalStep";
                    command.Parameters.AddWithValue("@CompanyId", companyId);
                    command.Parameters.AddWithValue("@MajorId", MajorIdCapPhoiHieuChinh);
                    command.Parameters.AddWithValue("@PermissionId", permissionId);
                    command.Parameters.AddWithValue("@NextDepartmentOrder", departmentOrder + 1);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadStep(reader);
                        }
                    }
                }
            }

            throw new InvalidOperationException("Bạn chưa cài đặt bước duyệt tiếp theo cho nghiệp vụ này.");
        }

        public static string PermissionForState(int isActive)
        {
            if (isActive == 0) return PermissionAdd;
            if (isActive == 1) return PermissionEdit;
            if (isActive == 2) return PermissionDelete;
            return "";
        }

        private static ApprovalStep ReadStep(System.Data.IDataRecord reader)
        {
            return new ApprovalStep
            {
                Id = reader.GetStringOrEmpty("Id"),
                DepartmentId = reader.GetStringOrEmpty("DeptId"),
                DeptId = reader.GetStringOrEmpty("DeptId"),
                Content = reader.GetStringOrEmpty("Content"),
                ApprovalStepNo = reader.GetIntOrDefault("ApprovalStep"),
                DepartmentOrder = reader.GetIntOrDefault("DepartmentOrder"),
                ApprovalOrder = reader.GetIntOrDefault("ApprovalOrder")
            };
        }
    }
}
