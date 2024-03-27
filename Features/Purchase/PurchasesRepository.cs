using Dapper;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Data;

namespace FromScratchAssessment2024
{
    public class PurchasesRepository
    {
        private readonly IDbConnection _connection;

        public PurchasesRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<Purchase>> GetAllAsync()
        {
            return await _connection.QueryAsync<Purchase>("SELECT * FROM Purchases");
        }

        public async Task<Purchase?> GetByIdAsync(Guid id)
        {
            return await _connection.QueryFirstOrDefaultAsync<Purchase>("SELECT * FROM Purchases WHERE Id = @Id", new { Id = id });
        }

        public async Task<Purchase> AddAsync(Purchase purchase)
        {
            var newId = await _connection.QuerySingleAsync<Guid>("INSERT INTO Purchases (CustomerId, TotalAmount) " +
                "OUTPUT INSERTED.Id " +
                "VALUES (@CustomerId, 0)", purchase);
            purchase.Id = newId;
 

            var newPurchaseItems = String.Join(",", purchase.PurchaseItems.Select(pi => $"('{newId}', '{pi.ProductId}', {pi.Quantity} )"));
            (int, decimal) insertedPurchaseItemsResult = await _connection.QuerySingleAsync<(int, decimal)>(
                "INSERT INTO PurchaseItems (PurchaseId, ProductId, Quantity) " +
                "SELECT npi.PurchaseId, npi.ProductId, npi.Quantity " +
                "FROM (VALUES " +
                   newPurchaseItems +
                ") AS npi (PurchaseId, ProductId, Quantity) " +
                "INNER JOIN Products pr ON pr.Id = npi.ProductId; " +
                "DECLARE @InsertedCount AS int = @@ROWCOUNT; " +

                "DECLARE @TotalAmount AS decimal(14, 2) = ( " +
                "SELECT " +
                "SUM(SmallTotal) " +
                "FROM( " +
                "SELECT PurchaseId, ProductId, Quantity, (Quantity * pr.Price) AS SmallTotal " +
                "FROM PurchaseItems pi " +
                "INNER JOIN Products pr ON pi.ProductId = pr.Id " +
                "WHERE PurchaseId = @PurchaseId " +
                ") AS T) " +
                "SELECT @InsertedCount, @TotalAmount "
                , new { PurchaseId = newId });

            if (insertedPurchaseItemsResult.Item1 == 0)
            {
                await DeleteAsync(newId);
                return null;
            }
            purchase.TotalAmount = insertedPurchaseItemsResult.Item2;
            await UpdateAsync(purchase);
            return await GetByIdAsync(newId);
        }

        public async Task UpdateAsync(Purchase purchase)
        {
            await _connection.ExecuteAsync("UPDATE Purchases SET CustomerId = @CustomerId, TotalAmount = @TotalAmount WHERE Id = @Id", purchase);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _connection.ExecuteAsync("DELETE FROM Purchases WHERE Id = @Id", new { Id = id });
        }
    }
}
