using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Common.EmailTemplates.Commands;
using Maba.Application.Features.Common.EmailTemplates.Queries;
using EmailTemplateDto = Maba.Application.Features.Common.EmailTemplates.DTOs.EmailTemplateDto;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class EmailTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmailTemplatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmailTemplateDto>>> GetEmailTemplates([FromQuery] bool? isActive)
    {
        var query = new GetEmailTemplatesQuery { IsActive = isActive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("key/{key}")]
    public async Task<ActionResult<EmailTemplateDto>> GetEmailTemplateByKey(string key)
    {
        var query = new GetEmailTemplateByKeyQuery { Key = key };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<EmailTemplateDto>> CreateEmailTemplate([FromBody] CreateEmailTemplateCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetEmailTemplateByKey), new { key = result.Key }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EmailTemplateDto>> UpdateEmailTemplate(Guid id, [FromBody] UpdateEmailTemplateCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

