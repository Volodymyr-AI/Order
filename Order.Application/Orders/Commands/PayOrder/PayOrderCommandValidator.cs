using FluentValidation;

namespace Order.Application.Orders.Commands.PayOrder;

public sealed class PayOrderCommandValidator : AbstractValidator<PayOrderCommand>
{
    public PayOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}