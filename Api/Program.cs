using Api;
using Domain.Abstractions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Dodaje us�ugi do kontenera.

builder.Services.AddControllers(); // Dodaje us�ugi obs�ugi kontroler�w MVC do aplikacji
builder.Services.AddEndpointsApiExplorer(); // Umo�liwia eksploracj� endpoint�w API, przydatne dla Swagger
builder.Services.AddSwaggerGen(); // Dodaje generator Swaggera, kt�ry dostarcza UI do testowania API i dokumentacji

// Dodanie us�ugi scoped dla ICustomerRepository, kt�ra b�dzie u�ywa� DbCustomerRepository do swojej implementacji
builder.Services.AddScoped<ICustomerRepository, FakeCustomerRepository>();

// Konfiguracja factory do tworzenia DbContext, konkretnie ShopperContext, u�ywaj�c SQLite jako bazy danych
builder.Services.AddDbContextFactory<ShopperContext>(options =>
    options.UseSqlite("Data Source=shopper.db"));

// Odkomentuj, aby doda� us�ug� hostowan�, kt�ra uruchomi DbCreationalService przy starcie
// builder.Services.AddHostedService<DbCreationalService>();

var app = builder.Build(); // Buduje aplikacj� webow�

// Konfiguruje pipeline ��da� HTTP.

// W �rodowisku developerskim u�yj Swaggera do dokumentacji i testowania API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // Dodaje middleware do przekierowywania ��da� HTTP na HTTPS

app.UseAuthorization(); // Dodaje middleware do autoryzacji

app.MapControllers(); // Mapuje trasy do akcji kontroler�w

// Ta niestandardowa metoda rozszerzenia (nie jest cz�ci� ASP.NET Core) prawdopodobnie s�u�y do upewnienia si�, �e baza danych jest tworzona przy starcie aplikacji.
app.CreateDatabase<ShopperContext>();

app.Run(); // Uruchamia aplikacj�
