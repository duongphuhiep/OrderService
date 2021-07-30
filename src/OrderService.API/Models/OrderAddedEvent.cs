namespace OrderService.API.Models
{
    public class OrderAddedEvent
    {
        public string OrderId { get; set; }
        public string AddedBy { get; set; }
    }
}
