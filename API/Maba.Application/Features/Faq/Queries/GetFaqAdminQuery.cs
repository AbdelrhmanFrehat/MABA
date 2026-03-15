using MediatR;

namespace Maba.Application.Features.Faq.Queries;

public class GetFaqAdminQuery : IRequest<List<DTOs.FaqItemDto>>
{
    public string? Search { get; set; }
    public Domain.Faq.FaqCategory? Category { get; set; }
    public bool? IsActive { get; set; }
}
