using Microsoft.AspNetCore.SignalR;

namespace Demo.Hubs
{
    public class StockTableHub : Hub
    {
        public async Task SendStockUpdate(int stockId, int volume)
        {
            await Clients.All.SendAsync("ReceiveStockUpdate", stockId, volume);
        }
    }
}
