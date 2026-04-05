using Maba.Domain.Common;

namespace Maba.Domain.Numbering;

public class DocumentSequence : BaseEntity
{
    public string DocumentType { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string Separator { get; set; } = "-";
    public bool IncludeYear { get; set; } = true;
    public int CurrentYear { get; set; }
    public int LastNumber { get; set; }
    public int PadLength { get; set; } = 4;
    public bool IsActive { get; set; } = true;
}
