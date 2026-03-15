using MediatR;
using Maba.Application.Features.Catalog.Categories.DTOs;

namespace Maba.Application.Features.Catalog.Categories.Commands;

public class CreateCategoryCommand : IRequest<CategoryDto>
{
    public Guid? ParentId { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

