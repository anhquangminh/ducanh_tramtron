using System;
using BeTong.Data;
using BeTong.Models;
using BeTong.Services;

namespace BeTong.Repositories
{
    public class UserRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public UserRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public ApplicationUserInfo GetByLoginName(string loginName, string password)
        {
            if (string.IsNullOrWhiteSpace(loginName))
            {
                throw new InvalidOperationException("Vui lòng nhập tên đăng nhập.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Vui lòng nhập mật khẩu.");
            }

            using (var connection = _connectionFactory.Create())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT TOP 1
    app.Id,
    app.UserName,
    app.Email,
    app.CompanyId,
    app.GroupId, -- Đảm bảo GroupId được lấy từ ApplicationUsers
    app.CreateBy,
    asp.PasswordHash
FROM ApplicationUsers app
LEFT JOIN AspNetUsers asp ON app.UserName = asp.UserName OR app.Email = asp.Email
WHERE app.IsActive <> 100
  AND (app.UserName = @LoginName OR app.Email = @LoginName)
ORDER BY app.UserName";
                command.Parameters.AddWithValue("@LoginName", loginName.Trim());
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("Không tìm thấy người dùng theo tên đăng nhập/email này.");
                    }

                    var passwordHash = reader.GetStringOrEmpty("PasswordHash");
                    if (!IdentityPasswordVerifier.Verify(passwordHash, password))
                    {
                        throw new InvalidOperationException("Mật khẩu không đúng.");
                    }

                    return new ApplicationUserInfo
                    {
                        Id = reader.GetStringOrEmpty("Id"),
                        UserName = reader.GetStringOrEmpty("UserName"),
                        Email = reader.GetStringOrEmpty("Email"),
                        CompanyId = reader.GetStringOrEmpty("CompanyId"),
                        GroupId = reader.GetStringOrEmpty("GroupId"), // Đảm bảo GroupId được trả về
                        CreateBy = reader.GetStringOrEmpty("CreateBy")
                    };
                }
            }
        }
    }
}
