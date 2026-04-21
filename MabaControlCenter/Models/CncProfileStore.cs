namespace MabaControlCenter.Models;

public class CncProfileStore
{
    public string? ActiveProfileId { get; set; }
    public List<CncMachineProfile> Profiles { get; set; } = new();
}
