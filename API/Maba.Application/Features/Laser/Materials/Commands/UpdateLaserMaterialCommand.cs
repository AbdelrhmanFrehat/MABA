using MediatR;
using Maba.Application.Features.Laser.DTOs;

namespace Maba.Application.Features.Laser.Materials.Commands;

public class UpdateLaserMaterialCommand : IRequest<LaserMaterialDto?>
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Type { get; set; } = "both";
    public decimal? MinThicknessMm { get; set; }
    public decimal? MaxThicknessMm { get; set; }
    public string? NotesEn { get; set; }
    public string? NotesAr { get; set; }
    public bool IsMetal { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
