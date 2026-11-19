namespace AssetTrackingAPI.Domain;

public enum UserRole { Administrator, SesmtManager, WarehouseResponsible, Employee }
public enum EpiStatus { Available, Delivered, Maintenance, Retired, Rejected }
public enum StockMovementType { Entry, Adjustment, Removal }
public enum EpiCondition { Good, Damaged, Expired, Rejected }

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool Active { get; set; } = true;
}

public class Employee
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public bool Active { get; set; } = true;
}

public class EpiType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ApprovalCertificate
{
    public int Id { get; set; }
    public int EpiTypeId { get; set; }
    public EpiType? EpiType { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
}

public class EpiItem
{
    public int Id { get; set; }
    public int EpiTypeId { get; set; }
    public EpiType? EpiType { get; set; }
    public string UniqueCode { get; set; } = string.Empty;
    public EpiStatus Status { get; set; } = EpiStatus.Available;
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}

public class StockMovement
{
    public int Id { get; set; }
    public int EpiTypeId { get; set; }
    public EpiType? EpiType { get; set; }
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    public int? UserId { get; set; }
    public User? User { get; set; }
}

public class Delivery
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public int? ResponsibleUserId { get; set; }
    public User? ResponsibleUser { get; set; }
    public DateTime DeliveryDate { get; set; } = DateTime.UtcNow;
    public ICollection<DeliveryItem> Items { get; set; } = new List<DeliveryItem>();
}

public class DeliveryItem
{
    public int Id { get; set; }
    public int DeliveryId { get; set; }
    public Delivery? Delivery { get; set; }
    public int EpiItemId { get; set; }
    public EpiItem? EpiItem { get; set; }
}

public class ReturnRecord
{
    public int Id { get; set; }
    public int EpiItemId { get; set; }
    public EpiItem? EpiItem { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public int? ResponsibleUserId { get; set; }
    public User? ResponsibleUser { get; set; }
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
}

public class Inspection
{
    public int Id { get; set; }
    public int EpiItemId { get; set; }
    public EpiItem? EpiItem { get; set; }
    public EpiCondition Condition { get; set; }
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;
    public int? ResponsibleUserId { get; set; }
    public User? ResponsibleUser { get; set; }
    public string? Notes { get; set; }
}

public class ComplianceAlert
{
    public int Id { get; set; }
    public int EpiItemId { get; set; }
    public EpiItem? EpiItem { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Resolved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
