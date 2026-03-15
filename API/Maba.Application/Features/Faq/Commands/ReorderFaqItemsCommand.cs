using MediatR;

namespace Maba.Application.Features.Faq.Commands;

public class ReorderFaqItemsCommand : IRequest<bool>
{
    public List<FaqOrderItem> Items { get; set; } = new();
}

public class FaqOrderItem
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
}
