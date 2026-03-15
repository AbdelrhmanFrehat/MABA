using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.Materials.Commands;

public class CreateMaterialCommand : IRequest<MaterialDto>
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public decimal PricePerGram { get; set; }
    public decimal Density { get; set; }
    public string? Color { get; set; }
}

