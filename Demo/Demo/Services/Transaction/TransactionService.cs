using Demo.Dtos.Requests;
using Demo.Dtos.Responses;
using Demo.Entities;
using Demo.Hubs;
using Demo.Utilities.ResponseHelper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Demo.Services.Transaction
{
    public class TransactionService(TrungTq50demoContext context, IHubContext<StockTableHub> stockTableHubContext) : ITransactionService
    {
        private readonly TrungTq50demoContext _context = context;
        private readonly IHubContext<StockTableHub> _stockTableHubContext = stockTableHubContext;

        public async Task<ResponseDto> BuyStock(BuyOrSellStock request)
        {
            if (!await UserExists(request.User_Id))
                return new ResponseDto { StatusCode = (int)ResponseStatusCode.BadRequest, Message = "User does not exist." };

            if (!await StockExists(request.Stock_Id))
                return new ResponseDto { StatusCode = (int)ResponseStatusCode.BadRequest, Message = "Stock does not exist." };

            if (request.Quantity <= 0)
                return new ResponseDto { StatusCode = (int)ResponseStatusCode.BadRequest, Message = "Quantity must be greater than zero." };

            var currentPrice = await GetCurrentStockPrice(request.Stock_Id);
            var user = _context.Users.FirstOrDefault(i => i.UserId == request.User_Id);

            if (user.AccountBalance - request.Quantity * currentPrice < 0)
            {
                return new ResponseDto { StatusCode = (int)ResponseStatusCode.BadRequest, Message = "Not enough balance." };
            }

            user.AccountBalance -= request.Quantity * currentPrice;

            var transaction = new Demo.Entities.Transaction
            {
                SenderId = request.User_Id,
                StockId = request.Stock_Id,
                Quantity = request.Quantity,
                TransactionType = "buy",
                CurrentPrice = currentPrice,
                TransactionDate = DateTime.Now
            };

            _context.Transactions.Add(transaction);

            var stock = _context.Stocks.FirstOrDefault(i => i.StockId == request.Stock_Id);
            stock.Volume += request.Quantity;

            await _context.SaveChangesAsync();

            await _stockTableHubContext.Clients.All.SendAsync("ReceiveStockUpdate", stock.StockId, stock.Volume);

            return new ResponseDto { StatusCode = (int)ResponseStatusCode.Success, Message = "Stock bought successfully." };
        }

        public async Task<ResponseDto> SellStock(BuyOrSellStock request)
        {
            if (!await UserExists(request.User_Id))
                return new ResponseDto { StatusCode = (int)ResponseStatusCode.NotFound, Message = "User does not exist." };

            if (!await StockExists(request.Stock_Id))
                return new ResponseDto { StatusCode = (int)ResponseStatusCode.NotFound, Message = "Stock does not exist." };

            if (request.Quantity <= 0)
                return new ResponseDto { StatusCode = (int)ResponseStatusCode.BadRequest, Message = "Quantity must be greater than zero." };

            var currentPrice = await GetCurrentStockPrice(request.Stock_Id);

            var userStockQuantity = await GetUserStockQuantity(request.User_Id, request.Stock_Id);
            if (userStockQuantity < request.Quantity)
                return new ResponseDto { StatusCode = (int)ResponseStatusCode.BadRequest, Message = "Insufficient stock quantity." };

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Step 1: Add the new sell transaction
                    var newTransaction = new Demo.Entities.Transaction
                    {
                        SenderId = request.User_Id,
                        StockId = request.Stock_Id,
                        Quantity = request.Quantity,
                        TransactionType = "sell",
                        CurrentPrice = currentPrice,
                        TransactionDate = DateTime.Now
                    };
                    _context.Transactions.Add(newTransaction);
                    await _context.SaveChangesAsync();

                    // Step 2: Adjust quantities of previous buy transactions
                    var remainingQuantityToMatch = request.Quantity;
                    var buyTransactions = await _context.Transactions
                        .Where(t => t.SenderId == request.User_Id
                                    && t.StockId == request.Stock_Id
                                    && t.TransactionType == "buy"
                                    && t.Quantity > 0)
                        .OrderBy(t => t.TransactionDate)  // Ensure we process the oldest buys first
                        .ToListAsync();

                    foreach (var buyTransaction in buyTransactions)
                    {
                        if (remainingQuantityToMatch <= 0)
                            break;

                        if (buyTransaction.Quantity <= remainingQuantityToMatch)
                        {
                            remainingQuantityToMatch -= buyTransaction.Quantity;
                            buyTransaction.Quantity = 0;  // Fully used this buy transaction
                        }
                        else
                        {
                            buyTransaction.Quantity -= remainingQuantityToMatch;
                            remainingQuantityToMatch = 0;
                        }

                        _context.Transactions.Update(buyTransaction);
                        await _context.SaveChangesAsync();
                    }

                    if (remainingQuantityToMatch > 0)
                        throw new InvalidOperationException("Not enough buy transactions to cover the sell quantity");

                    var user = _context.Users.FirstOrDefault(i => i.UserId == request.User_Id);

                    user.AccountBalance += request.Quantity * currentPrice;

                    var stock = _context.Stocks.FirstOrDefault(i => i.StockId == request.Stock_Id);
                    stock.Volume += request.Quantity;

                    await transaction.CommitAsync();

                    await _context.SaveChangesAsync();

                    await _stockTableHubContext.Clients.All.SendAsync("ReceiveStockUpdate", stock.StockId, stock.Volume);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ResponseDto { StatusCode = (int)ResponseStatusCode.InternalServerError, Message = "An error occurred while processing the transaction: " + ex.Message };
                }
            }

            return new ResponseDto { StatusCode = (int)ResponseStatusCode.Success, Message = "Stock sold successfully." };
        }

        public Task<int> GetUserStockQuantity(string userId, string stockId)
        {
            var userStock = _context.Transactions
                .Where(us => us.SenderId == userId && us.StockId == stockId && us.TransactionType == "buy").Sum(i => i.Quantity);
            return Task.FromResult(userStock);
        }

        public async Task<bool> UserExists(string userId)
        {
            return await _context.Users.AnyAsync(u => u.UserId == userId);
        }

        public async Task<bool> StockExists(string stockId)
        {
            return await _context.Stocks.AnyAsync(s => s.StockId == stockId);
        }

        public async Task<decimal> GetCurrentStockPrice(string stockId)
        {
            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.StockId == stockId);
            return stock?.CurrentPrice ?? 0;
        }
    }
}
