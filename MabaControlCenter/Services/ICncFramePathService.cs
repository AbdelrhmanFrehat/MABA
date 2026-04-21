using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface ICncFramePathService
{
    CncFrameBounds CalculateBounds(IReadOnlyList<GcodeMotionCommand> motions);
    IReadOnlyList<GcodeMotionCommand> BuildFramePath(CncFrameBounds bounds);
}
