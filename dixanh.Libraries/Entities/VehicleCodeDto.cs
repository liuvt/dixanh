namespace dixanh.Libraries.Entities
{
    public class VehicleCodeDto
    {
    }
    public sealed class VehicleCodeChangeDto
    {
        public string VehicleId { get; set; } = string.Empty;
        public string OperatingArea { get; set; } = string.Empty; // RG/PQ/CT...
        public string VehicleCode { get; set; } = string.Empty;   // số hiệu mới
        public string? ChangeReason { get; set; }                 // lý do
    }

    public sealed class VehicleCodeHistoryItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;
        public string OperatingArea { get; set; } = string.Empty;
        public string VehicleCode { get; set; } = string.Empty;
        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset? ValidTo { get; set; }
        public string? ChangedBy { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
        public string? ChangeReason { get; set; }
    }
}
