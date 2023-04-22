namespace F1.Constants;

public class ConstantKeys
{
    public static readonly int FixedLength = sizeof(int);

    public static readonly string RequestHeader = "REQ";
    public static readonly string Operation = "OP";
    public static readonly string HeaderReceived = "Header Received";
    public static readonly string OperationReceived = "Operation received";
    public static readonly string QueryLengthReceived = "Query Length received";
    public static readonly string QueryReceived = "Query Received";
    public static readonly string ResponseHeader = "RES";
    public static readonly string AuthUnlocked = "AuthUnlocked";
    public static readonly string AuthLocked = "AuthLocked";
    public static readonly string Authenticated = "AUTHENTICATED";
    public static readonly string Logout = "LOGOUT";

    public static readonly string UsernameKey = "Username";
    public static readonly string PasswordKey = "Password";
    public static readonly string PartNameKey = "Part Name";
    public static readonly string PartSupplierKey = "Supplier";
    public static readonly string PartBrandKey = "Brand";
    public static readonly string CategoryNameKey = "Category Name";
    public static readonly string SelectPartKey = "Select Part";
    public static readonly string SelectPartCategoryKey = "Select Category";
}