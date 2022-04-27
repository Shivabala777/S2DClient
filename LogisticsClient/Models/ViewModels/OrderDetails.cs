namespace LogisticsClient.Models.ViewModels
{
    public class OrderDetails
    {
        public string CustomerName { get; set; }
        public int OrderId { get; set; }
        public string ProductName { get; set; } = null!;
        public string Source { get; set; } = null!;
        public string Destination { get; set; } = null!;
        public int Kilometres { get; set; }
        public int Weight { get; set; }
        public int Charges { get; set; }
        public string? ApprovalStatus { get; set; }
        public string? DeliveryStatus { get; set; }
    }
}
