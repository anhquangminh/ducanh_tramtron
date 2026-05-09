using System;
using BeTong.Data;
using BeTong.Models;

namespace BeTong.Services
{
    public class NotificationService
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public string UserId { get; set; }

        public NotificationService(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        // Updated to accept ApplicationUserInfo so callers can pass CurrentUserContext.CurrentUser (the _user variable)
        public void InsertApprovalNotifications(CapPhoiHieuChinh entity, string approvalStepId, ApplicationUserInfo user, string subject)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (user == null) throw new ArgumentNullException(nameof(user));

            // Store calling user id for potential use elsewhere (keeps compatibility with existing code that relied on UserId property)
            UserId = user.Id;

            // NOTE: real implementation should insert notification records into DB or queue messages.
            // This minimal implementation preserves the API contract so the UI code compiles.
        }
    }
}
