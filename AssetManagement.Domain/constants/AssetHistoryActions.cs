namespace AssetManagement.Domain.Constants
{
    public static class AssetHistoryActions
    {
        public const string Assigned = "ASSIGNED";
        public const string ReturnRequested = "RETURN_REQUESTED";
        public const string ReturnApproved = "RETURN_APPROVED";
        public const string ReturnInspected = "RETURN_INSPECTED";
        public const string ReportedBroken = "REPORTED_BROKEN";
        public const string BrokenReportApproved = "APPROVED";
        public const string MarkBeyondRepair = "BEYOND_REPAIR";
        public const string Liquidated = "LIQUIDATED";  
    }
}