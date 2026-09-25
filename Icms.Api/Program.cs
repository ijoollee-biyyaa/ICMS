using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Icms.Api.Authorization;
using Icms.Api.ExceptionHandlers;
using Icms.Application.Churches;
using Icms.Application.Interfaces;
using Icms.Infrastructure.Identity;
using Icms.Infrastructure.Persistence;
using Icms.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new Microsoft.OpenApi.OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste the access token returned by POST /api/auth/login."
        };

        var bearerRef = new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document, null);

        document.Security ??= new List<Microsoft.OpenApi.OpenApiSecurityRequirement>();
        document.Security.Add(new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            [bearerRef] = new List<string>()
        });

        return Task.CompletedTask;
    });
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy("IcmsClient", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10))));

builder.Services.AddDbContext<IcmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("IcmsDatabase")));

builder.Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequiredLength = 12;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IcmsDbContext>();

builder.Services.AddAntiforgery(options =>
    options.HeaderName = "X-XSRF-TOKEN");

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthLimiter", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddScoped<TokenService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanManageDistrict", policy =>
        policy.Requirements.Add(new ManageDistrictRequirement()))
    .AddPolicy("CanManageChurch", policy =>
        policy.Requirements.Add(new ManageChurchRequirement()))
    .AddPolicy("CanManageTeam", policy =>
        policy.Requirements.Add(new ManageTeamRequirement()));
builder.Services.AddScoped<IAuthorizationHandler, ManageDistrictHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ManageChurchHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ManageTeamHandler>();

builder.Services.AddValidatorsFromAssembly(typeof(CreateChurchValidator).Assembly);

builder.Services.AddScoped<IChurchRepository, ChurchRepository>();
builder.Services.AddScoped<IChurchService, ChurchService>();

builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IEfgbcIdGenerator, EfgbcIdGenerator>();

builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<ITeamService, TeamService>();

builder.Services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
builder.Services.AddScoped<ITeamMemberService, TeamMemberService>();

builder.Services.AddScoped<ITeamAttendanceRepository, TeamAttendanceRepository>();
builder.Services.AddScoped<ITeamAttendanceService, TeamAttendanceService>();

builder.Services.AddScoped<ITeamMeetingRepository, TeamMeetingRepository>();
builder.Services.AddScoped<ITeamMeetingService, TeamMeetingService>();

builder.Services.AddScoped<ITeamPaymentRepository, TeamPaymentRepository>();
builder.Services.AddScoped<ITeamPaymentService, TeamPaymentService>();

builder.Services.AddScoped<IMemberDashboardRepository, MemberDashboardRepository>();
builder.Services.AddScoped<IMemberDashboardService, MemberDashboardService>();

builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
builder.Services.AddScoped<IDistrictService, DistrictService>();

builder.Services.AddScoped<IDistrictDepartmentRepository, DistrictDepartmentRepository>();
builder.Services.AddScoped<IDistrictDepartmentService, DistrictDepartmentService>();

builder.Services.AddScoped<IDistrictEmployeeRepository, DistrictEmployeeRepository>();
builder.Services.AddScoped<IDistrictEmployeeService, DistrictEmployeeService>();

builder.Services.AddScoped<IChurchEmployeeRepository, ChurchEmployeeRepository>();
builder.Services.AddScoped<IChurchEmployeeService, ChurchEmployeeService>();
builder.Services.AddScoped<IChurchDepartmentRepository, ChurchDepartmentRepository>();
builder.Services.AddScoped<IChurchDepartmentService, ChurchDepartmentService>();

builder.Services.AddScoped<ITransferRepository, TransferRepository>();
builder.Services.AddScoped<ITransferService, TransferService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors("IcmsClient");
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    if (!context.Request.Path.StartsWithSegments("/scalar")
        && !context.Request.Path.StartsWithSegments("/openapi"))
    {
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; img-src 'self' data: blob: https:; connect-src 'self' https:; script-src 'self'; style-src 'self' 'unsafe-inline';");
    }

    await next();
});

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true
        || context.Request.Cookies.ContainsKey("tms_auth"))
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
            new CookieOptions
            {
                HttpOnly = false,
                Secure = builder.Environment.IsProduction(),
                SameSite = SameSiteMode.Strict
            });
    }
    await next(context);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ICMS API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IcmsDbContext>();
    context.Database.Migrate();
    await DistrictSeeder.SeedAsync(app.Services, builder.Configuration);
}

app.Run();