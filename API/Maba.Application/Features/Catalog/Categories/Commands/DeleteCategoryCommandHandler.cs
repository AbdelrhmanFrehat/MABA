using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Categories.Commands;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Categories.Handlers;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Set<Category>()
            .Include(c => c.Children)
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        if (category.Children.Any())
        {
            throw new InvalidOperationException("Cannot delete category with child categories");
        }

        if (category.Items.Any())
        {
            throw new InvalidOperationException("Cannot delete category with associated items");
        }

        _context.Set<Category>().Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

