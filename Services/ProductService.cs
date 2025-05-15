using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Enums;
using Shared.Exceptions;

namespace Services
{
    public class ProductService : IProductService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<ProductService> _logger;
        private readonly IJobService _jobService;
        private readonly IUserService _userService;
        private readonly IReviewService _reviewService;
        private readonly IServiceProvider _serviceProvider;


        public ProductService(VinnareDbContext context, ILogger<ProductService> logger, IJobService jobService, IUserService userService, IReviewService reviewService, IServiceProvider serviceProvider)
        {
            _context = context;
            _logger = logger;
            _jobService = jobService;
            _userService = userService;
            _reviewService = reviewService;
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {

            /*/var products = await _context.Products
            .Include(p => p.Category)
            .ToListAsync();*/

            _logger.LogInformation("TESING");
            return await _context.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    OwnerId = p.OwnerId,
                    Title = p.Title,
                    Price = p.Price,
                    Category = p.Category,
                    Description = p.Description,
                    Image = p.Image,
                    Approved = p.Approved,
                    Quantity = p.Quantity,
                    Available = p.Available,
                    Date = p.Date
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductViewPage>> GetAvailableProductsPageAsync()
        {

            var productViewPages = await _context.Products
                .Where(p => p.Approved)
                .GroupJoin(_context.Reviews,
                    product => product.Id,
                    review => review.ProductId,
                    (product, reviews) => new { product, reviews })
                .Select(g => new ProductViewPage
                {
                    Id = g.product.Id,
                    Title = g.product.Title,
                    Price = g.product.Price,
                    Description = g.product.Description,
                    Category = g.product.Category,
                    Image = g.product.Image,
                    Rate = g.reviews.Any() ? (int)g.reviews.Average(r => r.Rate) : 0,
                    Quantity = g.product.Quantity,
                    Available = g.product.Available
                })
                .ToListAsync();


            return productViewPages;
        }


        public async Task<ProductDetail> GetProductByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            var rate = await _reviewService.GetReviewsRateByIdAsync(product.Id);

            return new ProductDetail
            {
                Id = product.Id,
                OwnerId = product.OwnerId,
                Title = product.Title,
                Price = product.Price,
                Description = product.Description,
                Category = product.Category,
                Image = product.Image,
                Rate = rate,
                Quantity = product.Quantity,
                Available = product.Available
            };
        }

        public async Task<string> GetProductNameByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            return product.Title;
        }


        public async Task<ProductDto?> GetProductForCartWishByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            return new ProductDto
            {

                Approved = product.Approved,
                Available = product.Available,
                Category = product.Category,
                Description = product.Description,
                Id = product.Id,
                Image = product.Image,
                OwnerId = product.OwnerId,
                Price = product.Price,
                Quantity = product.Quantity,
                Title = product.Title,
                Date = product.Date
            };
        }

        public async Task<IEnumerable<ProductViewPage>> GetProductsByCategoryAsync(int id)
        {
            var products = await _context.Products
                .Where(p => p.Available > 0 && p.Approved == true && p.CategoryId == id)
                .ToListAsync();

            var productViewPages = new List<ProductViewPage>();

            foreach (var product in products)
            {
                var rate = await _reviewService.GetReviewsRateByIdAsync(product.Id);
                productViewPages.Add(new ProductViewPage
                {
                    Id = product.Id,
                    Title = product.Title,
                    Price = product.Price,
                    Description = product.Description,
                    Category = product.Category,
                    Image = product.Image,
                    Rate = rate
                });
            }

            return productViewPages;
        }

        public async Task<Product> CreateProductAsync(ProductRequest productDto)
        {

            // Verify if the user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == productDto.OwnerId);
            if (!userExists)
            {
                throw new NotFoundException("The owner doesn't exists.");
            }

            //Verify if the category exists
            var categoryExists = await _serviceProvider.GetRequiredService<ICategoryService>().CheckAvailableCategory(productDto.Category);

            //Verify if the category it's approved
            if (categoryExists.Approved == false)
            {
                throw new Exception("You can't use this category because it's not approved'.");
            }

            //Verify if we have a product with the same name
            var productExists = await _context.Products.AnyAsync(p => p.Title == productDto.Title && p.OwnerId == productDto.OwnerId);
            if (productExists)
            {
                throw new Exception("You already have a product with the same name.");
            }

            var product = new Product
            {
                OwnerId = productDto.OwnerId,
                Title = productDto.Title,
                Price = productDto.Price,
                Category = productDto.Category,
                CategoryId = categoryExists.Id,
                Approved = true,
                Description = productDto.Description,
                Image = productDto.Image,
                Quantity = productDto.Quantity,
                Available = productDto.Available,
                Date = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<Product> CreateProductByEmployeeAsync(ProductRequest productDto)
        {

            // Verify if the user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == productDto.OwnerId);
            if (!userExists)
            {
                throw new Exception("The owner doesn't exists.");
            }

            //Verify if the category exists
            var categoryExists = await _context.Categories.FirstOrDefaultAsync(c => c.Name == productDto.Category);
            if (categoryExists == null)
            {
                throw new Exception("The category doesn't exists.");
            }

            //Verify if the category it's approved
            if (categoryExists.Approved == false)
            {
                throw new Exception("You can't use this category because it's not approved'.");
            }

            //Verify if we have a product with the same name
            var productExists = await _context.Products.AnyAsync(p => p.Title == productDto.Title && p.OwnerId == productDto.OwnerId);
            if (productExists)
            {
                throw new Exception("You already have a product with the same name.");
            }

            Guid userId = (Guid)await _userService.GetIdByUsername(productDto.Username);

            if (userId == Guid.Empty)
            {
                throw new NotFoundException("User not found.");
                //Console.WriteLine($"User ID: {userId}");
            }

            var product = new Product
            {
                OwnerId = productDto.OwnerId,
                Title = productDto.Title,
                Price = productDto.Price,
                Category = productDto.Category,
                CategoryId = categoryExists.Id,
                Approved = false,
                Description = productDto.Description,
                Image = productDto.Image,
                Quantity = productDto.Quantity,
                Available = productDto.Available,
                Date = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();


            await _jobService.CreateJobAsync(new JobDto
            {
                Type = JobType.Product,
                Operation = OperationType.Create,
                CreatorId = userId,
                ProductId = product.Id
            });
            //await _context.SaveChangesAsync();

            return product;
        }

        public async Task<Product> UpdateProductAsync(int id, ProductUpdate productDto)
        {

            //Verify if the category exists
            var categoryExists = await _context.Categories.FirstOrDefaultAsync(c => c.Name == productDto.Category);
            if (categoryExists == null)
            {
                throw new Exception("The category doesn't exists.");
            }

            //Verify if the product exists
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                throw new Exception("The product doesn't exists.");
            }

            product.OwnerId = productDto.OwnerId ?? product.OwnerId;
            product.Title = productDto.Title ?? product.Title;
            product.Price = productDto.Price ?? product.Price;
            //product.Approved = productDto.Approved ?? product.Approved;
            product.Description = productDto.Description ?? product.Description;
            product.Image = productDto.Image ?? product.Image;
            product.Quantity = productDto.Quantity ?? product.Quantity;
            product.Available = productDto.Available ?? product.Available;
            product.Category = productDto.Category ?? product.Category;
            product.CategoryId = categoryExists.Id;

            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<string> DeleteProductAsync(int id)
        {
            string message = "Product deleted successfully";
            // Verify if the product exists
            var productExists = await _context.Products.AnyAsync(u => u.Id == id);
            if (!productExists)
            {
                throw new NotFoundException("The product doesn't exists.");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null) return "null";

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<string> DeleteProductByEmployeeAsync(int id, string username)
        {
            string message = "You can't delete a product";
            // Verify if the product exists
            var productExists = await _context.Products.AnyAsync(u => u.Id == id);
            if (!productExists)
            {
                throw new NotFoundException("The product doesn't exists.");
            }

            Guid userId = (Guid)await _userService.GetIdByUsername(username);

            if (userId == Guid.Empty)
            {
                throw new NotFoundException("User not found.");
                //Console.WriteLine($"User ID: {userId}");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null) return "null";

            await _context.SaveChangesAsync();

            await _jobService.CreateJobAsync(new JobDto
            {
                Type = JobType.Product,
                Operation = OperationType.Delete,
                CreatorId = userId,
                ProductId = product.Id
            });

            //_context.Products.Remove(product);
            //await _context.SaveChangesAsync();
            return message;
        }

        public async Task ApproveProduct(int productId, bool approve)
        {
            var product = await _context.Products.FindAsync(productId);
            product!.Approved = approve;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductViewPage>> SearchProductsByNameAsync(string name)
        {
            var productViewPages = await _context.Products
                .Where(p => p.Available > 0 && p.Approved && p.Title.ToLower().Contains(name.ToLower()))
                .GroupJoin(_context.Reviews,
                    product => product.Id,
                    review => review.ProductId,
                    (product, reviews) => new { product, reviews })
                .Select(g => new ProductViewPage
                {
                    Id = g.product.Id,
                    Title = g.product.Title,
                    Price = g.product.Price,
                    Description = g.product.Description,
                    Category = g.product.Category,
                    Image = g.product.Image,
                    Rate = g.reviews.Any() ? (int)g.reviews.Average(r => r.Rate) : 0
                })
                .ToListAsync();

            return productViewPages;
        }

        public async Task<IEnumerable<ProductViewPage>> GetLatestProductsAsync(int count = 3)
        {
            var productViewPages = await _context.Products
                .Where(p => p.Available > 0 && p.Approved)
                .OrderByDescending(p => p.Date)
                .Take(count)
                .GroupJoin(_context.Reviews,
                    product => product.Id,
                    review => review.ProductId,
                    (product, reviews) => new { product, reviews })
                .Select(g => new ProductViewPage
                {
                    Id = g.product.Id,
                    Title = g.product.Title,
                    Price = g.product.Price,
                    Description = g.product.Description,
                    Category = g.product.Category,
                    Image = g.product.Image,
                    Rate = g.reviews.Any() ? (int)g.reviews.Average(r => r.Rate) : 0
                })
                .ToListAsync();

            return productViewPages;
        }

        public async Task<IEnumerable<ProductViewPage>> GetTopSellingProductsAsync(int count = 3)
        {
            var productViewPages = await _context.Products
                .Where(p => p.Available > 0 && p.Approved)
                .Select(p => new
                {
                    Product = p,
                    TotalSold = _context.Purchases
                        .Where(pur => pur.Products.Contains(p.Id))
                        .Sum(pur => pur.Quantities[pur.Products.IndexOf(p.Id)])
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(count)
                .Select(x => x.Product)
                .GroupJoin(_context.Reviews,
                    product => product.Id,
                    review => review.ProductId,
                    (product, reviews) => new { product, reviews })
                .Select(g => new ProductViewPage
                {
                    Id = g.product.Id,
                    Title = g.product.Title,
                    Price = g.product.Price,
                    Description = g.product.Description,
                    Category = g.product.Category,
                    Image = g.product.Image,
                    Rate = g.reviews.Any() ? (int)g.reviews.Average(r => r.Rate) : 0
                })
                .ToListAsync();

            return productViewPages;
        }
    }
}