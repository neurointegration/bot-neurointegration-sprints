using Microsoft.AspNetCore.Authorization;
using Neurointegration.Api.Authorizations;
using Neurointegration.Api.DI;
using Neurointegration.Api.Excpetions;
using Neurointegration.Api.Extensions;
using Neurointegration.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInternalDependencies(ApiSecretSettings.FromJson(""));

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAuthorizationHandler, ApiKeyHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "ApiKey",
        policyBuilder =>
        {
            policyBuilder.AddRequirements(ApiKeyRequirement.FromConfiguration(builder.Configuration));
        });
});

builder.Services.AddControllers(options => { options.Filters.Add(typeof(ExceptionsHandler)); });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// await app.UseDb();

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();