using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Tools.Utils;
using YandexDiskAuth.Configs;
using YandexDiskAuth.Services;

namespace YandexDiskAuth {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers().AddJsonOptions(opt => {
                opt.JsonSerializerOptions.UpdateJsonSerializerOptions();
            });
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "YandexDiskAuth", Version = "v1" });
            });

            services.Configure<YandexAppSettings>(Configuration.GetSection(nameof(YandexAppSettings)));
            services.Configure<TokenStorageSettings>(Configuration.GetSection(nameof(TokenStorageSettings)));
            services.AddTransient<AuthYandexService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "YandexDiskAuth v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                //https://docs.microsoft.com/ru-ru/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#set-up-conventional-route
                //endpoints.MapControllerRoute()
            });
        }
    }
}