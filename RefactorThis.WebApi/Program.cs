using Microsoft.Extensions.Configuration;
using RefactorThis.Mappers.Invoices;
using RefactorThis.Persistence;
using RefactorThis.Persistence.Entities;
using RefactorThis.Persistence.Interfaces;
using RefactorThis.Services;
using RefactorThis.Services.Contracts;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMvc(); //This will replace the BadRequestObjectResult instead of a ProblemDetails object
builder.Services.AddTransient<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddTransient<IInvoiceService, InvoicesService>();
builder.Services.AddSingleton<InvoicesMappings> ();
//builder.Services.AddTransient<IInvoiceRepository, InvoiceRepository>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors();

//Serilog.Log.Logger = new LoggerConfiguration()
//                                  //  .ReadFrom.Configuration(configuration)
//                                    .Enrich.FromLogContext()
//                                    .WriteTo(ILogger)
//                                    //.WriteTo.Console() //using the config from the Appsettings file instead, otherwise line will be doubled
//                                    .CreateLogger();

app.Run();
