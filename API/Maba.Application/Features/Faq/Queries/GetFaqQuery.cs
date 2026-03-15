using MediatR;

namespace Maba.Application.Features.Faq.Queries;

public class GetFaqQuery : IRequest<List<DTOs.FaqItemDto>>
{
    public string? Search { get; set; }
    public Domain.Faq.FaqCategory? Category { get; set; }
    public bool? IsFeaturedOnly { get; set; }
}
