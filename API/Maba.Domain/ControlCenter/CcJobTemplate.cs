using Maba.Domain.Common;

namespace Maba.Domain.ControlCenter;

public class CcJobTemplate : BaseEntity
{
    public Guid OrgId { get; set; }
    public Guid SiteId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string DeviceType { get; set; } = string.Empty;
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Opaque JSON definition (e.g. path plan, G-code reference, etc.).
    /// </summary>
    public string DefinitionJson { get; set; } = string.Empty;

    public string Version { get; set; } = "1.0.0";

    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}

