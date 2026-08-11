namespace CSharpLanguageWorkbench.Domain;

public static class OrderExtensions
{
    extension(Order order)
    {
        public Money Total
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfZero(order.Lines.Count);
                return order.Lines
                    .Select(line => line.LineTotal)
                    .Aggregate((total, next) => total + next);
            }
        }

        public bool HasPromotion => !string.IsNullOrWhiteSpace(order.PromotionCode);
    }
}
