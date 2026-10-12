namespace Nobisoft.Core.Constants;

public static class FieldBlackList
{
    public static List<string> BlackList = new List<string>()
    {
        "Id", "TenantId", "ConcurrencyStamp", "ExtraProperties", "CreatedTime", "CreatedBy", "ModifiedTime", "ModifiedBy", "Deleted", "DeletedBy", "DeletedTime"
    };
}