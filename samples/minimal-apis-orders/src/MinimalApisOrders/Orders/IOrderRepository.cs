namespace MinimalApisOrders.Orders;

internal interface IOrderRepository
{
    IReadOnlyList<Order> List(int limit);

    Order? Find(Guid id);

    Order Add(CreateOrderRequest request);

    bool Delete(Guid id);
}
