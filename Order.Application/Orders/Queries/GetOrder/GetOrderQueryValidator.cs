using FluentValidation;

namespace Order.Application.Orders.Queries.GetOrder;

public sealed class GetOrderQueryValidator : AbstractValidator<GetOrderQuery>
{
    public GetOrderQueryValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();   
    }
}