using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Orders.Commands.ConfirmOrder;
using Order.Application.Orders.Commands.CreateOrder;
using Order.Application.Orders.Queries.GetOrder;
using Orders.WebAPI.DTO;

namespace Orders.WebAPI.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class ClientOrdersController : ApiControllerBase
{
    public ClientOrdersController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request,
        CancellationToken ct)
    {
        var command = new CreateOrderCommand(
            request.CustomerId,
            request.StoreId,
            request.Items.Select(i =>
                new CustomerOrderItemDto(
                    i.ProductId,
                    i.NameSnapshot,
                    i.UnitPriceAmount,
                    i.CurrencyCode,
                    i.Quantity)).ToList()
        );

        var orderId = await Mediator.Send(command, ct);

        return CreatedAtAction(nameof(GetById),new { id = orderId }, new { orderId });
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var dto = await Mediator.Send(new GetOrderQuery(id), ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/confirm")]
    [Authorize]
    public async Task<IActionResult> Confirm([FromRoute] Guid id, CancellationToken ct)
    {
        var dto = await Mediator.Send(new ConfirmOrderCommand(id), ct);
        return Ok(dto);
    }
}