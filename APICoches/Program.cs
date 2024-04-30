using APICoches.CarsRepository;
using APICoches.Infraestructure;
using APICoches.Models;
using APICoches.UnitOfWork;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Exceptions;
using System.Reflection;
using System.Text.Json;
using System.Text;
using HealthChecks.UI.Client;

namespace APICoches
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Fase 1: Registros de servicios
            SetupSerilog(builder);

            RegisterServices(builder);
            WebApplication app = builder.Build();

            //Fase 2: middleware
            AppDbContextSeed.Init(app.Services.CreateScope().ServiceProvider);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            MapEndpoints(app);

            app.Run();
        }

        private static void SetupSerilog(WebApplicationBuilder builder)
        {
            IWebHostEnvironment env = builder.Environment;
            builder.Configuration
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            builder.Host.UseSerilog();
            Log.Logger = CreateSerilogLogger(builder.Configuration);

            Log.Information("Starting web host ({ApplicationContext})...",
                typeof(Program).GetTypeInfo().Assembly.GetName().Name);
        }

        private static Serilog.ILogger CreateSerilogLogger(ConfigurationManager configuration)
        {
            var seqServerUrl = configuration["Serilog:SeqServerUrl"];

            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("ApplicationContext", typeof(Program).GetTypeInfo().Assembly.GetName().Name)
                .Enrich.FromLogContext()

                .WriteTo.Console()
                .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq": seqServerUrl)

                .ReadFrom.Configuration(configuration)

                .CreateLogger();
        }

        private static void RegisterServices(WebApplicationBuilder builder)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHealthChecks();

            string database = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            if (string.IsNullOrEmpty(database))
            {
                database = builder.Configuration.GetConnectionString("CarsDatabase");
            }

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseLazyLoadingProxies();
                options.UseSqlServer(database);
            }
            );

            builder.Services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>();

            builder.Services.AddScoped<IRepositoryBase<Car>, CarRepository>();
            builder.Services.AddScoped<IRepositoryBase<Sale>, SaleRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        }

        private static void MapEndpoints(WebApplication app)
        {
            app.MapHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse

            });

            app.MapHealthChecks("/liveness");

            app.MapGet("/cars/getall", GetAllCars)
            .WithName("GetAllCars")
            .WithOpenApi();

            app.MapGet("/cars/getById", GetCarById)
            .WithName("GetCarsById")
            .WithOpenApi();

            app.MapPost("/cars/post", PostCar)
            .WithName("PostCar")
            .WithOpenApi();

            app.MapDelete("cars/delete", DeleteCar)
            .WithName("DeleteCar")
            .WithOpenApi();

            app.MapPut("cars/put", PutCar)
            .WithName("PutCar")
            .WithOpenApi();

            app.MapGet("/sales/getall", GetAllSales)
            .WithName("GetAllSales")
            .WithOpenApi();

            app.MapGet("/sales/getbycarid", GetSaleByCarId)
            .WithName("GetSaleByCarId")
            .WithOpenApi();

            app.MapPost("/sales/post", PostSale)
            .WithName("PostVenta")
            .WithOpenApi();

            app.MapDelete("sales/delete", DeleteSales)
            .WithName("DeleteVenta")
            .WithOpenApi();
        }

        private static IResult GetAllCars(IUnitOfWork unitOfWork)
        {
            List<Car> cars = unitOfWork._carRepository.GetAll();

            if (cars == null)
            {
                return Results.NotFound("Error obteniendo los coches.");
            }

            return Results.Ok(cars);
        }

        private static IResult GetCarById(IUnitOfWork unitOfWork, int id)
        {
            Car car = unitOfWork._carRepository.GetById(id);

            if (car == null)
            {
                return Results.NotFound($"No se ha encontrado el coche con id={id}");
            }

            return Results.Ok(car);
        }

        private static IResult PostCar(IUnitOfWork unitOfWork, string model, string brand)
        {
            Car Car = new Car
            {
                Model = model,
                Brand = brand
            };

            if (unitOfWork._carRepository.Create(Car) && unitOfWork.Save() > 0)
            {
                return Results.Ok(unitOfWork._carRepository.GetAll());
            }

            return Results.Problem("Error insertando un nuevo coche.");
        }

        private static IResult DeleteCar(IUnitOfWork unitOfWork, int id)
        {
            Car car = unitOfWork._carRepository.GetById(id);

            if (car != null)
            {
                unitOfWork._carRepository.Delete(car);

                if (unitOfWork.Save() == 0)
                {
                    return Results.Problem("No se ha podido borrar el coche");
                }

                return Results.Ok(unitOfWork._carRepository.GetAll());
            }

            return Results.NotFound($"No existe el Coche con id={id}");
        }

        private static IResult PutCar(IUnitOfWork unitOfWork, int id, string model, string brand)
        {
            Car car = unitOfWork._carRepository.GetById(id);

            if (car != null)
            {
                car.Model = model;
                car.Brand = brand;

                unitOfWork._carRepository.Update(car);

                if (unitOfWork.Save() == 0)
                {
                    return Results.Problem("Error modificando el coche");
                }

                return Results.Ok(unitOfWork._carRepository.GetAll());
            }

            return Results.NotFound($"No existe el coche con id={id}");
        }

        private static IResult GetAllSales(IUnitOfWork unitOfWork)
        {
            List<Sale> sales = unitOfWork._saleRepository.GetAll();

            if (sales == null)
            {
                return Results.NotFound($"No se han encontrado ventas.");
            }

            return Results.Ok(sales);
        }

        private static IResult GetSaleByCarId(IUnitOfWork unitOfWork, int id)
        {
            List<Sale> sales = unitOfWork._saleRepository.GetByPredicate((sale) =>
            {
                if (sale.Car == null)
                {
                    return false;
                }

                return sale.Car.Id == id;
            });

            if (sales == null)
            {
                return Results.NotFound($"No se han encontrado ventas.");
            }

            return Results.Ok(sales);
        }

        private static IResult PostSale(IUnitOfWork unitOfWork, string seller, float price, int IdCar, int kilometers = 0)
        {
            Car car = unitOfWork._carRepository.GetById(IdCar);

            if (car != null)
            {
                Sale sale = new Sale
                {
                    Seller = seller,
                    Price = price,
                    Car = car,
                    Kilometers = kilometers
                };

                unitOfWork._saleRepository.Update(sale);

                if (unitOfWork.Save() == 0)
                {
                    Results.Problem("No se ha podido modificar la venta.");
                }

                return Results.Ok(unitOfWork._saleRepository.GetAll());
            }

            return Results.NotFound($"No existe el coche con id={IdCar}");
        }

        private static IResult DeleteSales(IUnitOfWork unitOfWork, int id)
        {
            Sale sale = unitOfWork._saleRepository.GetById(id);

            if (sale != null)
            {
                unitOfWork._saleRepository.Delete(sale);

                if (unitOfWork.Save() == 0)
                {
                    return Results.Problem("Error eliminando la venta.");
                }

                return Results.Ok(unitOfWork._saleRepository.GetAll());
            }

            return Results.NotFound($"No existe la venta con id={id}");
        }
    }
}