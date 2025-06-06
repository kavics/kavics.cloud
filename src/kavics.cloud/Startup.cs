﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SenseNet.Configuration;
using SenseNet.ContentRepository.Security.ApiKeys;
using SenseNet.Diagnostics;
using SenseNet.Extensions.DependencyInjection;
using SenseNet.Services.Core.Authentication;
using SnWebApplication.Api.Sql.TokenAuth.TokenValidator;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using SenseNet.ContentRepository;
using SenseNet.Search.Lucene29;

namespace kavics.cloud
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // [sensenet]: Authentication
            var authOptions = new AuthenticationOptions();
            Configuration.GetSection("sensenet:authentication").Bind(authOptions);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    if (authOptions.AuthServerType == AuthenticationServerType.SNAuth)
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = false
                        };

                        options.SecurityTokenValidators.Clear();
                        options.SecurityTokenValidators.Add(new SenseNetJwtSecurityTokenHandler(
                            $"{authOptions.Authority}/api/auth/validate-token"));
                    }
                    else
                    {
                        options.Audience = "sensenet";

                        options.Authority = authOptions.Authority;
                        if (!string.IsNullOrWhiteSpace(authOptions.MetadataHost))
                            options.MetadataAddress =
                        $"{authOptions.MetadataHost.AddUrlSchema().TrimEnd('/')}/.well-known/openid-configuration";
                    }
                });

            // [sensenet]: Set options for ApiKeys
            services.Configure<ApiKeysOptions>(Configuration.GetSection("sensenet:ApiKeys"));

            // [sensenet]: Set options for EFCSecurityDataProvider
            services.AddOptions<SenseNet.Security.EFCSecurityStore.Configuration.DataOptions>()
                .Configure<IOptions<ConnectionStringOptions>>((securityOptions, systemConnections) =>
                    securityOptions.ConnectionString = systemConnections.Value.Security);

            // [sensenet]: add sensenet services
            services
                .AddSenseNetInstallPackage()
                .AddSenseNet(Configuration, (repositoryBuilder, provider) =>
                {
                    var searchEngineLogger = repositoryBuilder.Services.GetService<ILogger<Lucene29SearchEngine>>();
                    repositoryBuilder
                        .UseLogger(provider)
                        .UseLucene29LocalSearchEngine(searchEngineLogger,
                            Path.Combine(Environment.CurrentDirectory, "App_Data", "LocalIndex"));
                })
                .AddEFCSecurityDataProvider()
                .AddSenseNetMsSqlProviders(configureInstallation: installOptions =>
                {
                    Configuration.Bind("sensenet:install:mssql", installOptions);
                })
                .AddSenseNetOData()
                .AddSenseNetWebHooks()
                .AddSenseNetWopi()
                //.AddSenseNetSemanticKernel(options =>
                //{
                //    Configuration.Bind("sensenet:ai:SemanticKernel", options);
                //})
                //.AddSenseNetAzureVision(options =>
                //{
                //    Configuration.Bind("sensenet:ai:AzureVision", options);
                //})

                .AddSingleton<IFileService, FileService>()
                ;

            // [sensenet]: statistics overrides
            var statOptions = new StatisticsOptions();
            Configuration.GetSection("sensenet:statistics").Bind(statOptions);
            if (!statOptions.Enabled)
            {
                // reset to default/null services
                services
                    .AddDefaultStatisticalDataProvider()
                    .AddDefaultStatisticalDataCollector();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // [sensenet]: custom CORS policy
            app.UseSenseNetCors();
            // [sensenet]: use Authentication and set User.Current
            app.UseSenseNetAuthentication();

            // [sensenet]: MembershipExtender middleware
            app.UseSenseNetMembershipExtenders();

            app.UseAuthorization();

            // [sensenet] Add the sensenet binary handler
            app.UseSenseNetFiles();

            // [sensenet]: Health middleware
            app.UseSenseNetHealth();
            // [sensenet]: OData middleware
            app.UseSenseNetOdata();
            // [sensenet]: WOPI middleware
            app.UseSenseNetWopi();

            // add WOPI middleware if the request contains a prefix
            app.MapMiddlewareWhen<DataRecorderMiddleware>("/recorder", null, null, true);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                endpoints.MapGet("/", async context =>
                {
                    var hostVersion = Assembly.GetExecutingAssembly().GetName().Version;
                    var sn = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == "SenseNet.Services.Core");
                    var snVersion = sn?.GetName().Version ?? new Version(0, 0, 0);
                    await context.Response.WriteAsync($"sensenet is listening." +
                                                      $" sensenet version: {snVersion}, host version: {hostVersion}. ");
                });

                endpoints.MapGet("/recorder", async context =>
                {
                    await context.Response.WriteAsync("recorder");
                });
            });
            app.Use(next => context =>
            {
                context.RequestServices.GetRequiredService<IFileService>().ServeFileAsync(context).GetAwaiter().GetResult();
                return next(context);
            });
        }
    }
}
