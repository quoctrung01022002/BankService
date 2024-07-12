namespace Demo.Dtos.Requests
{
    public class CreateRequestRequest
    {
        public string User_Id { get; set; }
        public string Stock_Id { get; set; }
        public int Quantity {  get; set; }
        public decimal Price { get; set; }
        public DateTime Request_Date { get; set; }
        public string Request_Type { get; set; }
    }
}
