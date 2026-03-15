using MediatR;

namespace Maba.Application.Features.Faq.Commands;

public class DeleteFaqItemCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
