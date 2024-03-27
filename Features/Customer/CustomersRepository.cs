using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace FromScratchAssessment2024
{
    public class CustomersRepository
    {
        private readonly IDbConnection _connection;

        public CustomersRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _connection.QueryAsync<Customer>("SELECT * FROM Customers");
        }
        public async Task<Customer?> GetDetailsByIdAsync(Guid id)
        {
            var query = @"
                        SELECT 
                            c.Id,
                            c.CreatedAt,
                            c.UpdatedAt,
                            c.FirstName,
                            c.LastName,
                            c.Email,
                            c.Phone,
                            c.Address,
                            p.Id,
                            p.CustomerId, 
                            p.CreatedAt, 
                            p.UpdatedAt, 
                            p.TotalAmount,
                            CONCAT(pi.PurchaseId, '-', pi.ProductId) AS PurchaseItemId,
                            pi.PurchaseId,
                            pi.ProductId,
                            pi.Quantity,
                            pr.Id,
                            pr.Name,
                            pr.Price,
                            pr.Description,
                            pr.ImageUrl,
                            pr.QuantityInStock
                        FROM Customers c
                        LEFT JOIN Purchases p ON c.Id = p.CustomerId
                        LEFT JOIN PurchaseItems pi ON p.Id = pi.PurchaseId
                        LEFT JOIN Products pr ON pi.ProductId = pr.Id
                        WHERE c.Id = @CustomerId";

            var customersDictionary = new Dictionary<Guid, Customer>();

            await _connection.QueryAsync<Customer, Purchase, PurchaseItem, Product, Customer>(
                query,
                (customer, purchase, purchaseItem, product) =>
                {
                    if (!customersDictionary.TryGetValue(customer.Id, out Customer customerEntry))
                    {
                        customerEntry = customer;
                        customerEntry.Purchases = new List<Purchase>();
                        customersDictionary.Add(customerEntry.Id, customerEntry);
                    }

                    if (purchase != null)
                    {
                        purchase.CustomerId = customer.Id;
                        if (!customerEntry.Purchases.Any(p => p.Id == purchase.Id))
                        {
                            purchase.PurchaseItems = new List<PurchaseItem>();
                            customerEntry.Purchases.Add(purchase);
                        }
                        if (purchaseItem != null)
                        {
                            purchaseItem.Product = product;
                            purchase.PurchaseItems.Add(purchaseItem);
                        }
                    }

                    return customerEntry;
                },
                param: new { CustomerId = id },
                splitOn: "Id, PurchaseItemId, Id"
            );

            return customersDictionary.Values.SingleOrDefault();
        }

        public async Task<Customer> GetByIdAsync(Guid id)
        {
            return await _connection.QueryFirstOrDefaultAsync<Customer>("SELECT * FROM Customers WHERE Id = @Id", new { Id = id });
        }

        public async Task AddAsync(Customer customer)
        {
            await _connection.ExecuteAsync("INSERT INTO Customers (Id, FirstName, LastName, Email, Phone, Address, CreatedAt, UpdatedAt) VALUES (@Id, @FirstName, @LastName, @Email, @Phone, @Address, @CreatedAt, @UpdatedAt)", customer);
        }

        public async Task<bool> UpdateAsync(Customer customer)
        {
            int rows = await _connection.QuerySingleAsync<int>("UPDATE Customers SET FirstName = @FirstName, LastName = @LastName, Email = @Email, Phone = @Phone, Address = @Address WHERE Id = @Id; " +
                "SELECT @@ROWCOUNT;", customer);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            int rows = await _connection.QuerySingleAsync<int>("DELETE FROM Customers WHERE Id = @Id;" +
                "SELECT @@ROWCOUNT;", new { Id = id });
            return rows > 0;
        }
    }
}
