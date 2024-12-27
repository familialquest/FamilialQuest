using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

using NLog.Web;
using TaskService.Services;

namespace TaskService
{
    /// <summary>
    /// Base class
    /// </summary>
    public class Startup
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        /// <summary>
        /// Startup.
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Get configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.Configure<CookiePolicyOptions>(options =>
                {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });


                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


                #region Swagger
                // Register the Swagger generator, defining 1 or more Swagger documents
                services.AddSwaggerGen(c =>
                {
                    try
                    {
                        // load info from file
                        var openApiInfoSerialized = File.ReadAllText("Properties/openApiInfo.json");
                        var openApiInfoDeserialized = JsonConvert.DeserializeObject<OpenApiInfo>(openApiInfoSerialized);
                        c.SwaggerDoc(Assembly.GetEntryAssembly().GetName().Name, openApiInfoDeserialized);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Can't load openApiInfo from setting file");
                        var openApiInfo = new OpenApiInfo()
                        {
                            Title = "Default API info",
                            Description = "Please contact devs"
                        };
                        c.SwaggerDoc(Assembly.GetEntryAssembly().GetName().Name, openApiInfo);
                    }

                    try
                    {
                        // Support XML comments
                        // Set the comments path for the Swagger JSON and UI.
                        var xmlFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                        c.IncludeXmlComments(xmlPath);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Can't load XML-comments");
                        throw;
                    }
                });
                #endregion

                services.AddHttpContextAccessor();

                services.AddSingleton<ITaskService, TaskService.Services.TaskService>();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            try
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


                #region Swagger
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/{Assembly.GetEntryAssembly().GetName().Name}/swagger.json", Assembly.GetEntryAssembly().GetName().Name);
                });
                #endregion
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
        }
    }
}
