using MediatR;

namespace Maba.Application.Features.Faq.Commands;

public class ToggleFaqItemActiveCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
