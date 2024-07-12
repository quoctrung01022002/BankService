using Demo.Entities;

namespace Demo.Dtos.Responses
{
    public class Top3BuyOrSellRequestsResponse
    {
        public string Stock_Id { get; set; }
        public List<PriceVolume> PriceVolumes { get; set; }
    }
}
