﻿using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;

namespace Discussion.Web.Tests.StartupSpecs
{
    [Collection("AppSpecs")]
    public class MiddlewareConfigureSpecs
    {

        private TestServer server;
        public MiddlewareConfigureSpecs(Application app)
        {
            server = app.Server;
        }


        [Fact]
        public void should_use_iis_platform()
        {
            var app = Application.BuildApplication("Dev", host =>
            {
                host.UseSetting("PORT", "5000");
                host.UseSetting("APPL_PATH", "/");
                host.UseSetting("TOKEN", "dummy-token");
            });

            var filters = app.Server.Host
                .Services
                .GetServices<IStartupFilter>()
                .ToList();

            filters.ShouldContain(f => f.GetType().FullName.Contains("IISSetupFilter"));

            (app as IDisposable).Dispose();
        }

        [Fact]
        public async Task should_use_mvc()
        {
            HttpContext httpContext = null;
            await server.SendAsync(ctx =>
            {
                httpContext = ctx;
                ctx.Request.Path = IntegrationTests.HomePageSpecs.HomePagePath;
            });
            

            var loggerProvider = httpContext.RequestServices.GetRequiredService<ILoggerProvider>() as StubLoggerProvider;
            loggerProvider.ShouldNotBeNull();
            loggerProvider.LogItems.ShouldContain(item => item.Category.StartsWith("Microsoft.AspNetCore.Mvc"));
        }

        [Fact]
        public async Task should_use_static_files()
        {
            var staticFile = IntegrationTests.NotFoundSpecs.NotFoundStaticFile;
            HttpContext httpContext = null;
            
            await server.SendAsync(ctx =>
            {
                httpContext = ctx;
                ctx.Request.Method = "GET";
                ctx.Request.Path = staticFile;
            });

            var loggerProvider = httpContext.RequestServices.GetRequiredService<ILoggerProvider>() as StubLoggerProvider;
            loggerProvider.ShouldNotBeNull();
            loggerProvider.LogItems.ShouldContain(item => item.Category.StartsWith("Microsoft.AspNetCore.StaticFiles"));
        }
    }

}
