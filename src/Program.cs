using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.Text;
using WebApiUdemy.Domain.Users;
using WebApiUdemy.Endpoints.Categories;
using WebApiUdemy.Endpoints.Clients;
using WebApiUdemy.Endpoints.Employees;
using WebApiUdemy.Endpoints.Orders;
using WebApiUdemy.Endpoints.Products;
using WebApiUdemy.Endpoints.Security;
using WebApiUdemy.Infra.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["ConnectionString:WebApiTestDb"]);
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization(options => {
    options.FallbackPolicy = new AuthorizationPolicyBuilder().AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
    .RequireAuthenticatedUser().Build();

    options.AddPolicy("EmployeePolicy", p => p.RequireAuthenticatedUser().RequireClaim("EmployeeCode"));
    options.AddPolicy("CpfPolicy", p => p.RequireAuthenticatedUser().RequireClaim("Cpf"));
});

builder.Services.AddAuthentication(x => {
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters() {
        ValidateActor = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JwtBearerTokenSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtBearerTokenSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtBearerTokenSettings:SecretKey"]))
    };
});

builder.Services.AddScoped<QueryAllUsersWithClaimName>();
builder.Services.AddScoped<UserCreator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapMethods(CategoryPost.Template, CategoryPost.Methods, CategoryPost.Handle);
app.MapMethods(CategoryGetAll.Template, CategoryGetAll.Methods, CategoryGetAll.Handle);
app.MapMethods(CategoryPut.Template, CategoryPut.Methods, CategoryPut.Handle);
app.MapMethods(EmployeePost.Template, EmployeePost.Methods, EmployeePost.Handle);
app.MapMethods(EmployeeGetAll.Template, EmployeeGetAll.Methods, EmployeeGetAll.Handle);
app.MapMethods(TokenPost.Template, TokenPost.Methods, TokenPost.Handle);
app.MapMethods(ProductPost.Template, ProductPost.Methods, ProductPost.Handle);
app.MapMethods(ProductGetAll.Template, ProductGetAll.Methods, ProductGetAll.Handle);
app.MapMethods(ProductGetAllStorefront.Template, ProductGetAllStorefront.Methods, ProductGetAllStorefront.Handle);
app.MapMethods(ClientPost.Template, ClientPost.Methods, ClientPost.Handle);
app.MapMethods(ClientGet.Template, ClientGet.Methods, ClientGet.Handle);
app.MapMethods(OrderPost.Template, OrderPost.Methods, OrderPost.Handle);

app.UseExceptionHandler("/error");
app.Map("/error", (HttpContext http) => {
    var error = http.Features?.Get<IExceptionHandlerFeature>()?.Error;
    if(error != null) {
        if (error is SqlException)
            return Results.Problem(title: "Database out", statusCode: 500);
        else if (error is BadHttpRequestException)
            return Results.Problem(title: "Error to convert data to other type. See all the information sent", statusCode: 500);
    }
    
    return Results.Problem(title: "An error ocurred", statusCode: 500);
});

app.Run();
