namespace Demo.Dtos.Requests
{
    public class BuyOrSellStock
    {
        public string User_Id { get; set; }
        public string Stock_Id { get; set; }
        public int Quantity { get; set; }
    }
}
