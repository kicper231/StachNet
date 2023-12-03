using Domain.Abstractions;
using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

// Klasa DbCustomerRepository implementuje interfejs ICustomerRepository
public class DbCustomerRepository : ICustomerRepository
{
    // Prywatne pole przechowuj�ce kontekst bazy danych
    private readonly ShopperContext context;

    // Konstruktor inicjuj�cy kontekst bazy danych
    public DbCustomerRepository(ShopperContext context)
    {
        this.context = context;
    }

    // Metoda zwracaj�ca wszystkich klient�w z bazy danych
    public List<Customer> GetAll()
    {
        // Zwraca wszystkich klient�w jako list�
        return context.Customers.ToList();
    }

    // Metoda zwracaj�ca klienta na podstawie jego ID
    public Customer GetById(int id)
    {
        // Znajduje klienta w bazie danych na podstawie ID
        return context.Customers.Find(id);
    }

    // Metoda dodaj�ca nowego klienta do bazy danych
    public void Add(Customer customer)
    {
        // Dodaje klienta do zbioru Customers w kontek�cie bazy danych
        context.Customers.Add(customer);
        // Zapisuje zmiany w bazie danych
        context.SaveChanges();
    }
}

// Klasa kontekstu bazy danych, rozszerzaj�ca DbContext z Entity Framework
public class ShopperContext : DbContext
{
    // Zbiory reprezentuj�ce tabele w bazie danych
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }

    // Konstruktor przyjmuj�cy opcje konfiguracyjne DbContext
    public ShopperContext(DbContextOptions options)
    : base(options)
    {
    }
}
