namespace AssetManagement.Domain.Enums
{
    public enum AssetStatus
    {
        AVAILABLE = 1,
        IN_USE = 2,
        IN_USE_SHARED = 3,
        REPORTED_BROKEN = 4,
        BROKEN = 5,
        UNDER_MAINTENANCE = 6,
        LOST = 7,
        BEYOND_REPAIR = 8,
        LIQUIDATED = 9
    }
}