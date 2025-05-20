namespace CarRentalSystem.Models
{

    public enum UserRole
    {
        Customer,
        Manager,
        Administrator
    }

    public enum VehicleStatus
    {
        Available,
        Reserved,
        Maintenance,
        OutOfService
    }

    public enum BookingStatus
    {
        Requested,
        Approved,
        Cancelled,
        Completed
    }

    public enum PaymentMethod
    {
        PayNow,
        PayAtPickup
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Refunded
    }

    public enum NotificationType
    {
        Email,
        System
    }
}