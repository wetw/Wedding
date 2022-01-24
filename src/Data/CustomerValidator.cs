using FluentValidation;

namespace Wedding.Data
{
    public class CustomerValidator : AbstractValidator<Customer>
    {
        public CustomerValidator()
        {
            RuleFor(customer => customer.RealName)
                .NotEmpty()
                .WithMessage("請填寫您的姓名");
            RuleFor(customer => customer.Relation)
                .NotNull()
                .When(customer => customer.IsAttend == true)
                .WithMessage("請填寫與新人的關係");
            RuleFor(customer => customer.Visitors)
                .GreaterThan(0)
                .When(customer => customer.IsAttend == true)
                .WithMessage("請填寫參加的大人數量");
            RuleFor(customer => customer.Address)
                .NotEmpty()
                .When(customer => customer.IsRealBook)
                .WithMessage("請填寫預收喜帖的地址");
            RuleFor(customer => customer.Email)
                .NotEmpty()
                .When(customer => customer.IsEmailBook)
                .WithMessage("請填寫預收電子喜帖的信箱");
            RuleFor(customer => customer.VegetarianCount)
                .GreaterThan(0)
                .When(customer => customer.IsVegetarian)
                .WithMessage("請填寫吃素的人數");
            RuleFor(customer => customer.VegetarianCount)
                .LessThanOrEqualTo(customer => customer.Visitors + customer.ChildrenCount)
                .When(customer => customer.IsAttend == true)
                .WithMessage("人數填錯了喔，必須小於總人數");
        }
    }
}
