using System.ComponentModel;
namespace EasyTaskRunner.Data.Enums;

public enum RequestStatus
{
    [Description("No value selected.")]
    None = 0,

    Error = 1,

    Successful = 2,

    Abort = 3,

    DatabaseError = 4,

    ValidationError = 5
}
