﻿using System.Runtime.Serialization;
using APITutorial.API.Database;
using APITutorial.API.DTOs.Habits;
using APITutorial.API.Entities;
using APITutorial.API.Middleware;
using APITutorial.API.Services;
using APITutorial.API.Services.Sorting;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Serialization;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace APITutorial.API;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
        {
            options.ReturnHttpNotAcceptable = true;
        })
            .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver())
            .AddXmlDataContractSerializerFormatters();

        builder.Services.Configure<MvcOptions>(options =>
        {
            NewtonsoftJsonOutputFormatter jsonOutputFormatter = options.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>().First();

            jsonOutputFormatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV1);
            jsonOutputFormatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV2);
            jsonOutputFormatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJson);
            jsonOutputFormatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV1);
            jsonOutputFormatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV2);
        });


        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1.0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            // options.ApiVersionReader = new UrlSegmentApiVersionReader();
            options.ApiVersionReader = ApiVersionReader.Combine(
                new MediaTypeApiVersionReader(),
                new MediaTypeApiVersionReaderBuilder()
                    .Template("application/vnd.apitutorial.hateoas.v{version}+json")
                    .Build());
        })
       .AddMvc();

        builder.Services.AddOpenApi();


        return builder;
    }

    public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            };
        });

        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options
            .UseNpgsql(
                builder.Configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions
                    .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
            .UseSnakeCaseNamingConvention()

        );
        return builder;
    }

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {

        builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddNpgsql())
    .WithMetrics(metrics => metrics
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation())
    .UseOtlpExporter();

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
        });


        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        builder.Services.AddHealthChecks();

        builder.Services.AddTransient<SortMappingProvider>();

        builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<HabitDto, Habit>>(_ => HabitMappings.SortMapping);

        builder.Services.AddTransient<DataShapingService>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<LinkService>();

        return builder;
    }

}
