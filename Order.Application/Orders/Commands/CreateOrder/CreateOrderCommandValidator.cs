using FluentValidation;

namespace Order.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.StoreId).GreaterThan(0);

        RuleFor(x => x.Items)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.NameSnapshot).NotEmpty().MaximumLength(256);
            item.RuleFor(i => i.UnitPriceAmount).GreaterThan(0);
            item.RuleFor(i => i.CurrencyCode).NotEmpty().Length(3);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });

        RuleFor(x => x.Items)
            .Must(items =>
                items is null || items.Select(i => i.CurrencyCode).Distinct(StringComparer.OrdinalIgnoreCase).Count() <=
                1)
            .WithMessage("All items must have the same currency.")
            .WithErrorCode("currency_mismatch");
    }
}