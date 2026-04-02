using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.Designs;

/// <summary>Central rules for whether a design's metadata/files may be exposed.</summary>
public static class DesignVisibility
{
    public static bool CanView(Design design, Guid? requestingUserId, bool isPrivileged)
    {
        if (isPrivileged)
            return true;
        if (design.IsPublic)
            return true;
        return requestingUserId.HasValue && design.UserId == requestingUserId.Value;
    }
}
