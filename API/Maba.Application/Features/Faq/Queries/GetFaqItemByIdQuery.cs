using MediatR;
using Maba.Application.Features.Faq.DTOs;

namespace Maba.Application.Features.Faq.Queries;

public class GetFaqItemByIdQuery : IRequest<FaqItemDto?>
{
    public Guid Id { get; set; }
}
