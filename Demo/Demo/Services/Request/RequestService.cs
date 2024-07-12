using Demo.Dtos.Requests;
using Demo.Dtos.Responses;
using Demo.Entities;
using Demo.Hubs;
using Demo.Utilities.ResponseHelper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Demo.Services.Request
{
    public class RequestService(TrungTq50demoContext trungTq50DemoContext, IHubContext<StockTableHub> stockTableHubContext) : IRequestService
    {
        private readonly TrungTq50demoContext _Context = trungTq50DemoContext;
        private readonly IHubContext<StockTableHub> hubContext = stockTableHubContext;

        public async Task<ResponseDto> CreateRequest(CreateRequestRequest request)
        {
            var newRequest = new SellBuyRequest()
            {
                Price = request.Price,
                Quantity = request.Quantity,
                RequestDate = request.Request_Date,
                RequestType = request.Request_Type,
                StockId = request.Stock_Id,
                UserId = request.User_Id
            };

            var preTop3BuyRequest = await GetTop3BuyRequests();
            var preTop3SellRequest = await GetTop3SellRequests();

            if (newRequest.RequestType == "buy")
            {
                var user = await _Context.Users.FirstOrDefaultAsync(i => i.UserId == newRequest.UserId);

                var totalPendingBuyRequestsAmount = await _Context.SellBuyRequests
                .Where(r => r.UserId == newRequest.UserId && r.RequestType == "buy")
                .SumAsync(r => r.Quantity * r.Price);

                if (user.AccountBalance - totalPendingBuyRequestsAmount - (newRequest.Quantity * newRequest.Price) < 0)
                {
                    return new ResponseDto()
                    {
                        Message = "Not enough balance.",
                        StatusCode = (int)ResponseStatusCode.BadRequest
                    };
                }
            }
            else
            {
                var userStockQuantity = await GetUserStockQuantity(request.User_Id, request.Stock_Id);
                if (userStockQuantity < request.Quantity)
                    return new ResponseDto { StatusCode = (int)ResponseStatusCode.BadRequest, Message = "Insufficient stock quantity." };
            }

            _Context.SellBuyRequests.Add(newRequest);

            await _Context.SaveChangesAsync();

            // Kiểm tra và xử lý các yêu cầu khớp nếu có
            await ProcessMatchingRequests(newRequest);

            var top3BuyRequest = await GetTop3BuyRequests();
            var top3SellRequest = await GetTop3SellRequests();

            await hubContext.Clients.All.SendAsync("ReceiveTop3BuyRequest", top3BuyRequest.Data, preTop3BuyRequest.Data, request.Stock_Id);
            await hubContext.Clients.All.SendAsync("ReceiveTop3SellRequest", top3SellRequest.Data, preTop3SellRequest.Data, request.Stock_Id);

            return new ResponseDto { StatusCode = (int)ResponseStatusCode.Success, Message = "Create Request Successfully!"};
        }

        private async Task ProcessMatchingRequests(SellBuyRequest newRequest)
        {
            var oppositeRequestType = newRequest.RequestType == "buy" ? "sell" : "buy";
            var matchingRequestsQuery = _Context.SellBuyRequests
                .Where(r => r.StockId == newRequest.StockId && r.RequestType == oppositeRequestType)
                .OrderBy(r => r.Price)
                .ThenBy(r => r.RequestDate);

            if (newRequest.RequestType == "buy")
            {
                matchingRequestsQuery = matchingRequestsQuery.OrderBy(r => r.Price).ThenBy(r => r.RequestDate);
            }
            else
            {
                matchingRequestsQuery = matchingRequestsQuery.OrderByDescending(r => r.Price).ThenBy(r => r.RequestDate);
            }

            var matchingRequests = await matchingRequestsQuery.ToListAsync();

            if (matchingRequests.Count != 0)
            {
                var matchingRequest = matchingRequests.First();

                var user1 = _Context.Users.FirstOrDefault(i => i.UserId == newRequest.UserId);
                var user2 = _Context.Users.FirstOrDefault(i => i.UserId == matchingRequest.UserId);

                decimal transactionAmount;
                if (newRequest.RequestType == "buy")
                {
                    transactionAmount = matchingRequest.Price * newRequest.Quantity;
                    user1.AccountBalance -= transactionAmount;
                    user2.AccountBalance += transactionAmount;
                }
                else
                {
                    transactionAmount = newRequest.Price * newRequest.Quantity;
                    user1.AccountBalance += transactionAmount;
                    user2.AccountBalance -= transactionAmount;
                }

                // Tạo giao dịch mới
                var transaction = new Entities.Transaction
                {
                    SenderId = newRequest.RequestType == "buy" ? matchingRequest.UserId : newRequest.UserId,
                    ReceiverId = newRequest.RequestType == "buy" ? newRequest.UserId : matchingRequest.UserId,
                    StockId = newRequest.StockId,
                    Quantity = newRequest.Quantity,
                    TransactionType = newRequest.RequestType,
                    CurrentPrice = newRequest.Price,
                    TransactionDate = DateTime.UtcNow
                };

                _Context.Transactions.Add(transaction);

                // Cập nhật số lượng cổ phiếu trong bảng stocks
                var stock = await _Context.Stocks.FindAsync(newRequest.StockId);
                if (stock != null)
                {
                    stock.Volume += newRequest.Quantity;
                    _Context.Stocks.Update(stock);

                    await hubContext.Clients.All.SendAsync("ReceiveStockUpdate", stock.StockId, stock.Volume);
                }

                // Update quantities of matching requests
                newRequest.Quantity -= matchingRequest.Quantity;
                matchingRequest.Quantity = 0;

                // Remove matching request if quantity is zero
                if (newRequest.Quantity <= 0)
                {
                    _Context.SellBuyRequests.Remove(newRequest);
                }

                _Context.SellBuyRequests.Remove(matchingRequest);

                await _Context.SaveChangesAsync();
            }
        }


        public async Task<ResponseDto<List<Top3BuyOrSellRequestsResponse>>> GetTop3BuyRequests()
        {
            var result = await _Context.SellBuyRequests
                .Where(r => r.RequestType == "buy")
                .GroupBy(r => r.StockId) // Nhóm theo Stock_Id
                .Select(group => new Top3BuyOrSellRequestsResponse
                {
                    Stock_Id = group.Key,
                    PriceVolumes = group
                        .GroupBy(r => r.Price) // Nhóm theo Price
                        .Select(priceGroup => new
                        {
                            Price = priceGroup.Key,
                            Volume = priceGroup.Sum(r => r.Quantity) // Cộng dồn Volume
                        })
                        .OrderByDescending(pv => pv.Price) // Sắp xếp giảm dần theo giá
                        .Take(3) // Lấy top 3
                        .Select(pv => new PriceVolume
                        {
                            Price = (double)pv.Price,
                            Volume = pv.Volume
                        })
                        .ToList()
                })
                .ToListAsync();

            foreach (var item in result)
            {
                while (item.PriceVolumes.Count < 3)
                {
                    item.PriceVolumes.Add(new PriceVolume
                    {
                        Price = 0,
                        Volume = 0
                    });
                }
            }

            var response = new ResponseDto<List<Top3BuyOrSellRequestsResponse>>
            {
                StatusCode = (int)ResponseStatusCode.Success,
                Message = "Top 3 Buy Requests retrieved successfully",
                Data = result
            };

            return response;
        }

        public async Task<ResponseDto<List<Top3BuyOrSellRequestsResponse>>> GetTop3SellRequests()
        {
            var result = await _Context.SellBuyRequests
                .Where(r => r.RequestType == "sell")
                .GroupBy(r => r.StockId) // Nhóm theo Stock_Id
                .Select(group => new Top3BuyOrSellRequestsResponse
                {
                    Stock_Id = group.Key,
                    PriceVolumes = group
                        .GroupBy(r => r.Price) // Nhóm theo Price
                        .Select(priceGroup => new
                        {
                            Price = priceGroup.Key,
                            Volume = priceGroup.Sum(r => r.Quantity) // Cộng dồn Volume
                        })
                        .OrderBy(pv => pv.Price) // Sắp xếp tăng dần theo giá
                        .Take(3) // Lấy top 3
                        .Select(pv => new PriceVolume
                        {
                            Price = (double)pv.Price,
                            Volume = pv.Volume
                        })
                        .ToList()
                })
                .ToListAsync();

            foreach (var item in result)
            {
                while (item.PriceVolumes.Count < 3)
                {
                    item.PriceVolumes.Add(new PriceVolume
                    {
                        Price = 0,
                        Volume = 0
                    });
                }
            }

            var response = new ResponseDto<List<Top3BuyOrSellRequestsResponse>>
            {
                StatusCode = (int)ResponseStatusCode.Success,
                Message = "Top 3 Sell Requests retrieved successfully",
                Data = result
            };

            return response;
        }

        public Task<int> GetUserStockQuantity(string userId, string stockId)
        {
            var userStock = _Context.Transactions
                .Where(us => us.SenderId == userId && us.StockId == stockId && us.TransactionType == "buy").Sum(i => i.Quantity);
            return Task.FromResult(userStock);
        }
    }
}
