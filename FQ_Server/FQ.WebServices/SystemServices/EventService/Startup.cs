using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace EventService
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            //Конфигурация для swagger-a
            services.AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("s", new Info { Title = "Project Management API", Description = "Swagger Project Management API" });

                        var xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + @"EventService.xml";
                        c.IncludeXmlComments(xmlPath);
                    }
                );

            services.AddHttpContextAccessor();

            services.AddSingleton<IEventServices, EventServices>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
                        
            //app.UseHttpsRedirection();
            app.UseMvc();

            //Подключение swagger-a
            app.UseSwagger();
            app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/s/swagger.json", "Project Management API");
                    }                
                );
        }
    }
}
