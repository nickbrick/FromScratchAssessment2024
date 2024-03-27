using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace FromScratchAssessment2024
{
    public class ProductsRepository
    {
        private readonly IDbConnection _connection;

        public ProductsRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _connection.QueryAsync<Product>("SELECT * FROM Products");
        }

        public async Task<Product> GetByIdAsync(Guid id)
        {
            return await _connection.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = id });
        }

        public async Task AddAsync(Product product)
        {
            await _connection.ExecuteAsync("INSERT INTO Products (Id, Name, Price, Description, ImageUrl, QuantityInStock, CreatedAt, UpdatedAt) VALUES (@Id, @Name, @Price, @Description, @ImageUrl, @QuantityInStock, @CreatedAt, @UpdatedAt)", product);
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            int rows = await _connection.ExecuteAsync("UPDATE Products SET Name = @Name, Price = @Price, Description = @Description, ImageUrl = @ImageUrl, QuantityInStock = @QuantityInStock, UpdatedAt = @UpdatedAt WHERE Id = @Id" +
            "SELECT @@ROWCOUNT;", product);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            int rows = await _connection.ExecuteAsync("DELETE FROM Products WHERE Id = @Id" +
                "SELECT @@ROWCOUNT;", new { Id = id });
            return rows > 0;
        }

    }
}
