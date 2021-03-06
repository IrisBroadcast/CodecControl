﻿#region copyright
/*
 * Copyright (c) 2018 Sveriges Radio AB, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CodecControl.Web.Hub;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace CodecControl.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            ApplicationSettings appSettings = new ApplicationSettings();
            Configuration.GetSection("Application").Bind(appSettings);
            services.AddSingleton(appSettings);

            services.ConfigureDepencencyInjection();

            // Localization
            services.AddLocalization(o => o.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("sv-SE")
                };
                options.DefaultRequestCulture = new RequestCulture("sv-SE", "sv-SE");

                // You must explicitly state which cultures your application supports.
                // These are the cultures the app supports for formatting
                // numbers, dates, etc.
                options.SupportedCultures = supportedCultures;

                // These are the cultures the app supports for UI strings,
                // i.e. we have localized resources for.
                options.SupportedUICultures = supportedCultures;
            });

            services.AddDirectoryBrowser();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1); // .AddMvc(option => option.EnableEndpointRouting = false);

            // Fixing some reference loop handling in .NET Core 3.1
            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            // Set up CORS
            List<string> allowedOrigins = new List<string>();
            if (!string.IsNullOrEmpty(appSettings.CcmHost))
            {
                allowedOrigins.Add(appSettings.CcmHost);
            }
            if (appSettings.AllowedOrigins != null)
            {
                var additionalOrigins = appSettings.AllowedOrigins.Split(",", StringSplitOptions.RemoveEmptyEntries);
                allowedOrigins.AddRange(additionalOrigins);
            }

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .SetIsOriginAllowed(_ => true) // BREAKING CHANGE IN .NETCORE 2.2
                        .WithHeaders(HeaderNames.AccessControlAllowHeaders, "Content-Type")
                        .AllowAnyMethod()
                        .WithOrigins(allowedOrigins.ToArray())
                        .AllowAnyHeader()
                        .AllowCredentials();
                });

                options.AddPolicy("AllowSubdomain",
                    builder =>
                    {
                        builder.SetIsOriginAllowedToAllowWildcardSubdomains();
                    });
            });

            services.AddLazyCache();

            services.AddSignalR();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Codec Control API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ApplicationSettings applicationSettings)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            app.UseAuthentication();

            //app.UseHttpsRedirection();

            app.UseStaticFiles();

            // Serve log files as static files
            var logFolder = applicationSettings.LogFolder;
            Directory.CreateDirectory(logFolder);

            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(logFolder),
                RequestPath = "/log",
                EnableDirectoryBrowsing = true,
                StaticFileOptions =
                {
                    ContentTypeProvider = new FileExtensionContentTypeProvider { Mappings = {[".log"] = "text/plain"}},
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache");
                    }
                }
            });

            app.UseCors("CorsPolicy");

            // Localization (RFC 4646)
            app.UseRequestLocalization();

            // Swagger documentation api
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Codec Control API");
            });

            // EndpointRoutingMiddleware must be added to the request execution pipeline before EndpointMiddleware
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // endpoints.MapControllers();
                endpoints.MapHub<AudioStatusHub>("/audiostatusHub");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
