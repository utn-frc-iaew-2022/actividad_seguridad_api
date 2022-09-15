using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TodoApi", Version = "v1" });
});
builder.Services.AddAuthorization(options =>
{
        options.AddPolicy("read:todoitems", policy => policy.Requirements.Add(new HasScopeRequirement("read:todoitems", "https://dev-qhyd4h4k.us.auth0.com/")));
        options.AddPolicy("write:todoitems", policy => policy.Requirements.Add(new HasScopeRequirement("write:todoitems", "https://dev-qhyd4h4k.us.auth0.com/")));
});
builder.Services.AddAuthentication(options  =>
{
	options.DefaultAuthenticateScheme  =  JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme  =  JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options  =>
{
	// Agregar URL de su cuenta de Auth0 de cada uno.
	options.Authority  =  "https://dev-qhyd4h4k.us.auth0.com/";
	// Agregar audiencia de la API.
	options.Audience  =  "https://api.example.com";
});
builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoApi v1"));
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

