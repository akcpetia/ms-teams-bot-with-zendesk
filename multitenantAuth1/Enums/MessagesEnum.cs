using System.ComponentModel;

namespace multitenantAuth1.Enums
{
    public enum MessagesEnum
    {
        [Description("Successfully")]
        Success = 1,
        [Description("Failed")]
        Failed = 2,
        [Description("Record Already Exist in the System")]
        AlreadyExist = 3
    }
}
