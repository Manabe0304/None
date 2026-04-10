namespace AssetManagement.Application.DTOs.Response.AuditLog
{
    public class AuditLogItemResponse
    {
        public Guid Id { get; set; }

        // Thời gian chính xác (kèm múi giờ)
        public DateTimeOffset CreatedAt { get; set; }

        // Người thực hiện hành động
        public string Username { get; set; } = string.Empty;

        // Bộ phận của người thực hiện
        public string Department { get; set; } = string.Empty;

        // Hoạt động chính
        public string Action { get; set; } = string.Empty;

        // Đối tượng bị tác động
        public string TargetName { get; set; } = string.Empty;
    }
}