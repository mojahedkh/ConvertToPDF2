using DinkToPdf.Contracts;
using DinkToPdf;
using ConvertCollectiveToPdf.Service;
using Microsoft.Extensions.DependencyInjection;
using ConvertCollectiveToPdf.Models;

namespace ConvertCollectiveToPdf
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);
                var configuration = builder.Configuration;
                // Add services to the container.

                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
                builder.Services.AddScoped<ConvertService>();


                String ListOfCollectivePath = configuration.GetSection("ConvertToPdfVariable")["CollectiveFolder"];

                /* if (!Directory.Exists(ListOfCollectivePath))
                 {
                     throw new DirectoryNotFoundException("Directory Not Found");
                 }*/

                var ListOfCollective = new String[]
                {
                    "ahmad" ,
                    "mojahed",
                    "kamal" ,
                    "rami" , 
                    "ahmad"
                };

                builder.Services.AddSingleton<String[]>(ListOfCollective);

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