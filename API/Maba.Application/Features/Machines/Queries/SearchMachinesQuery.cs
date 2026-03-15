using MediatR;
using Maba.Application.Common.Models;
using Maba.Application.Features.Machines.DTOs;

namespace Maba.Application.Features.Machines.Queries;

public class SearchMachinesQuery : IRequest<PagedResult<MachineDto>>
{
    public string? SearchTerm { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public string? Location { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } // Name, Manufacturer, PurchaseDate
    public bool SortDescending { get; set; } = false;
}

