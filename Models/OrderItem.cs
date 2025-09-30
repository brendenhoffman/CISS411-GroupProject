namespace CISS411_GroupProject.Models
{
    public class OrderItem
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public string ItemName { get; set; } = null!;
        public int Quantity { get; set; }
        public bool DesignApproved { get; set; }

        // Nav
        public Order Order { get; set; } = null!;
    }
}
