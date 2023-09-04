using DinkToPdf.Contracts;
using DinkToPdf;
using ConvertCollectiveToPdf.Service;
using Microsoft.Extensions.DependencyInjection;
using ConvertCollectiveToPdf.Models;
using Serilog;

namespace ConvertCollectiveToPdf
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);
                ConvertService service = new ConvertService();

                builder.Host.UseSerilog((hostContext, services, Configuration) => {
                    Configuration.WriteTo.Console();
                    Configuration.WriteTo.File("C:\\Logs\\ConvertToPdfLogs.txt");
                });

                var configuration = builder.Configuration;

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
                builder.Services.AddScoped<ConvertService>();

                builder.Logging.ClearProviders();
               

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

                app.Run();
            }
            catch (Exception ex)
            {

            }
        }
    }
}