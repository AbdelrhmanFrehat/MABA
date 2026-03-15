using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Orders.Commands;
using Maba.Application.Features.Orders.DTOs;
using Maba.Application.Features.Orders.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult> GetAllOrders(
        [FromQuery] Guid? userId, 
        [FromQuery] Guid? orderStatusId,
        [FromQuery] string? paymentStatus,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        // If pagination parameters are provided, use paged query
        if (pageNumber > 0 && pageSize > 0)
        {
            var pagedQuery = new GetOrdersPagedQuery
            {
                UserId = userId,
                OrderStatusId = orderStatusId,
                PaymentStatus = paymentStatus,
                DateFrom = dateFrom,
                DateTo = dateTo,
                SearchTerm = searchTerm,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending
            };
            var result = await _mediator.Send(pagedQuery);
            return Ok(result);
        }
        
        // Otherwise, use the original non-paged query for backward compatibility
        var query = new GetAllOrdersQuery { UserId = userId, OrderStatusId = orderStatusId };
        var listResult = await _mediator.Send(query);
        return Ok(listResult);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAllOrders), new { }, result);
    }

    [HttpPost("invoices")]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromBody] CreateInvoiceCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("invoices")]
    public async Task<ActionResult<List<InvoiceDto>>> GetAllInvoices([FromQuery] Guid? orderId, [FromQuery] Guid? invoiceStatusId)
    {
        var query = new GetAllInvoicesQuery { OrderId = orderId, InvoiceStatusId = invoiceStatusId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("payments")]
    public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("payments")]
    public async Task<ActionResult<List<PaymentDto>>> GetAllPayments([FromQuery] Guid? orderId, [FromQuery] Guid? invoiceId)
    {
        var query = new GetAllPaymentsQuery { OrderId = orderId, InvoiceId = invoiceId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("payment-plans")]
    public async Task<ActionResult<PaymentPlanDto>> CreatePaymentPlan([FromBody] CreatePaymentPlanCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
    {
        var query = new GetOrderDetailQuery { OrderId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}/detail")]
    public async Task<ActionResult<OrderDto>> GetOrderDetail(Guid id)
    {
        var query = new GetOrderDetailQuery { OrderId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("number/{orderNumber}")]
    public async Task<ActionResult<OrderDto>> GetOrderByNumber(string orderNumber)
    {
        var query = new GetOrderByNumberQuery { OrderNumber = orderNumber };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<OrderDto>>> GetOrdersByUser(Guid userId)
    {
        var query = new GetOrdersByUserQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusCommand command)
    {
        command.OrderId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<OrderDto>> CancelOrder(Guid id, [FromBody] CancelOrderCommand? command = null)
    {
        if (command == null)
        {
            command = new CancelOrderCommand { OrderId = id };
        }
        else
        {
            command.OrderId = id;
        }
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/notes")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> AddOrderNote(Guid id, [FromBody] AddOrderNoteCommand command)
    {
        command.OrderId = id;
        await _mediator.Send(command);
        return Ok(new { message = "Note added successfully." });
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<OrderDto>>> SearchOrders(
        [FromQuery] string? orderNumber,
        [FromQuery] Guid? userId,
        [FromQuery] Guid? orderStatusId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var query = new SearchOrdersQuery
        {
            OrderNumber = orderNumber,
            UserId = userId,
            OrderStatusId = orderStatusId,
            FromDate = fromDate,
            ToDate = toDate
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("statuses")]
    public async Task<ActionResult<List<OrderStatusDto>>> GetOrderStatuses()
    {
        var query = new GetAllOrderStatusesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

