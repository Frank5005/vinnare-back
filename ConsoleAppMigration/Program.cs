using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

//Enum Roletype
public enum RoleType
{
    Admin,
    Seller,
    Customer
}

public class UserDto 
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public RoleType Role { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public bool Approved { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int Quantity { get; set; }
    public int Available { get; set; }
}

//User and Product Model
public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } //= string.Empty;

    [Required]
    public string Username { get; set; } //= string.Empty;

    [Required]
    public string Password { get; set; } //= string.Empty;

    [Required]
    public RoleType Role { get; set; } = RoleType.Customer;
}
public class Product
{
    [Key]
    //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //[Column(TypeName = "serial")]
    public int Id { get; set; }

    [Required]
    public Guid OwnerId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "money")]
    public decimal Price { get; set; }

    public string? Description { get; set; }

    [Required]
    public string Category { get; set; } = string.Empty;

    public string? Image { get; set; }

    public bool Approved { get; set; } = false;

    public int Quantity { get; set; } = 0;

    public int Available { get; set; } = 0;

    // Navigation Property
    [ForeignKey("OwnerId")]
    public User Owner { get; set; }
}

//Context of the database
public class VinnareDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql("Host=localhost;Port=5433;Database=vinnare;Username=postgres;Password=password;");
    }
}

class Program
{
    private static readonly HttpClient _httpClient = new();

    //Hash password methd
    public static string HashPassword(string password)
    {
        string _pepper = "4F420DFC4C4BB3C6";
        byte[] saltBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        string salt = Convert.ToBase64String(saltBytes);

        string saltedPassword = password + salt + _pepper;

        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

        return $"{salt}.{Convert.ToBase64String(hashBytes)}"; // Store as "salt.hash"
    }

    static async Task Main()
    {
        Console.WriteLine("Fetching users and products from FakeStoreAPI...");

        try
        {
            await SaveUsersToDatabase();

            Console.WriteLine("¡Users inserted!");

            await SaveProductsToDatabase();

            Console.WriteLine("¡Products inserted!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    //Method to obtain the users

    private static async Task<List<UserDto>> FetchUsersFromAPI()
    {
        var url = "https://fakestoreapi.com/users";
        var users = await _httpClient.GetFromJsonAsync<List<UserDto>>(url, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return users ?? new List<UserDto>();
    }

    //Method to insert the users

    private static async Task SaveUsersToDatabase()
    {
        using var db = new VinnareDbContext();

        var usersDto = await FetchUsersFromAPI();

        // Apply migrations
        await db.Database.MigrateAsync();
        
        foreach (var dto in usersDto)
        {
            string hashedPassword = HashPassword(dto.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                Username = dto.Username,
                Password = hashedPassword,
                Role = dto.Role
            };

            db.Users.Add(user);
        }

        await db.SaveChangesAsync();
    }

    //Method to obtain the products
    private static async Task<List<ProductDto>> FetchProductsFromAPI()
    {
        var url = "https://fakestoreapi.com/products";
        var products = await _httpClient.GetFromJsonAsync<List<ProductDto>>(url, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return products ?? new List<ProductDto>();
    }

    //Method to insert the products
    private static async Task SaveProductsToDatabase()
    {
        using var db = new VinnareDbContext();

        var userIds = await db.Users.Select(u => u.Id).ToListAsync();

        if (!userIds.Any())
        {
            Console.WriteLine("❌ No hay usuarios en la base de datos. No se pueden asignar productos.");
            return;
        }

        var productsDto = await FetchProductsFromAPI();

        // Apply migrations
        await db.Database.MigrateAsync();

        foreach (var dto in productsDto)
        {
            var randomUserId = userIds[new Random().Next(userIds.Count)];

            var product = new Product
            {
                Id = dto.Id,
                OwnerId = randomUserId,
                Title = dto.Title,
                Price = dto.Price,
                Category = dto.Category,
                Approved = false,
                Description = dto.Description,
                Image = dto.Image,
                Quantity = 10,
                Available = 10
            };

            db.Products.Add(product);
        }

        await db.SaveChangesAsync();
    }
}
