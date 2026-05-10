using System;
using System.Data;
using System.Data.SqlClient;
using BeTong.Data;
using BeTong.Models;

namespace BeTong.Services
{
    public class PermissionService
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public PermissionService(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        /// <summary>
        /// Ensures the current user has approval permission for the given company/approval and current day of week.
        /// Throws InvalidOperationException when not permitted or user not logged in.
        /// (keeps original behaviour but delegates to internal helper)
        /// </summary>
        public void EnsureUserHasApprovalPermission(string companyId, string approvalId)
        {
            if (!CurrentUserContext.IsLoggedIn || CurrentUserContext.CurrentUser == null)
            {
                throw new InvalidOperationException("Người dùng chưa đăng nhập.");
            }

            var userId = CurrentUserContext.CurrentUser.Id;
            EnsureUserHasApprovalPermission(userId, companyId, approvalId);
        }

        /// <summary>
        /// Backing implementation that checks permission for a specific user id.
        /// </summary>
        private void EnsureUserHasApprovalPermission(string userId, string companyId, string approvalId)
        {
            if (userId == null || userId.Trim().Length == 0)
            {
                throw new InvalidOperationException("UserId không hợp lệ.");
            }

            using (var connection = _connectionFactory.Create())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT COUNT(1)
FROM SomePermissionTable p
WHERE p.UserId = @UserId
  AND p.CompanyId = @CompanyId
  AND p.ApprovalId = @ApprovalId
  AND p.DayinWeek = @DayInWeek";

                command.CommandType = CommandType.Text;

                // DayOfWeek: Sunday = 0 ... Saturday = 6
                var dayInWeek = (int)DateTime.Now.DayOfWeek;

                var paramUserId = command.CreateParameter();
                paramUserId.ParameterName = "@UserId";
                paramUserId.Value = userId;
                command.Parameters.Add(paramUserId);

                var paramCompanyId = command.CreateParameter();
                paramCompanyId.ParameterName = "@CompanyId";
                paramCompanyId.Value = companyId ?? string.Empty;
                command.Parameters.Add(paramCompanyId);

                var paramApprovalId = command.CreateParameter();
                paramApprovalId.ParameterName = "@ApprovalId";
                paramApprovalId.Value = approvalId ?? string.Empty;
                command.Parameters.Add(paramApprovalId);

                var paramDay = command.CreateParameter();
                paramDay.ParameterName = "@DayInWeek";
                paramDay.Value = dayInWeek;
                command.Parameters.Add(paramDay);

                connection.Open();
                var result = Convert.ToInt32(command.ExecuteScalar());
                if (result <= 0)
                {
                    throw new InvalidOperationException("Bạn không có quyền duyệt trong thời gian này.");
                }
            }
        }

        /// <summary>
        /// Compatibility method used by existing UI code:
        /// checks that the provided user has the given permission id (uses user's CompanyId).
        /// </summary>
        public void CheckPermission(ApplicationUserInfo user, string permissionId)
        {
            if (user == null)
            {
                throw new InvalidOperationException("Người dùng chưa đăng nhập.");
            }

            EnsureUserHasApprovalPermission(user.Id, user.CompanyId, permissionId);
        }

        /// <summary>
        /// Compatibility method used by existing UI code:
        /// checks that the provided user has approval permission for specific company/approvalId.
        /// </summary>
        public void CheckApproval(ApplicationUserInfo user, string companyId, string approvalId)
        {
            if (user == null)
            {
                throw new InvalidOperationException("Người dùng chưa đăng nhập.");
            }

            EnsureUserHasApprovalPermission(user.Id, companyId, approvalId);
        }
    }
}
