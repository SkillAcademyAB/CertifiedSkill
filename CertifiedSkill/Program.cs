using CertifiedSkill.Components;
using CertifiedSkill.Components.Account;
using CertifiedSkill.Data;
using CertifiedSkill.Data.Protection;
using CertifiedSkill.Services.Participant;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CertifiedSkill
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            var authBuilder = builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                });

            authBuilder.AddIdentityCookies();

            authBuilder.AddCookie(ParticipantAuthConstants.SchemeName, options =>
            {
                options.Cookie.Name = "CertifiedSkill.Participant";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.LoginPath = "/participant/login";
                options.AccessDeniedPath = "/participant/login";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(ParticipantAuthConstants.PolicyName, policy =>
                    policy.AddAuthenticationSchemes(ParticipantAuthConstants.SchemeName)
                          .RequireAuthenticatedUser());
            });

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddSingleton<IValidateOptions<PnrProtectionOptions>, PnrProtectionOptionsValidator>();
            builder.Services.AddOptions<PnrProtectionOptions>()
                .Bind(builder.Configuration.GetSection(PnrProtectionOptions.SectionName))
                .ValidateOnStart();

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            // Participant authentication services
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddScoped<IParticipantMagicLinkService, ParticipantMagicLinkService>();
            builder.Services.AddScoped<IParticipantEmailSender, NoOpParticipantEmailSender>();
            builder.Services.AddScoped<ParticipantPersonService>();
            builder.Services.AddScoped<CertifiedSkill.Services.Pnr.IPnrProtectionService, CertifiedSkill.Services.Pnr.AesPnrProtectionService>();
            builder.Services.AddScoped<CertifiedSkill.Data.Identity.IPersonalIdentityNumberValidator, CertifiedSkill.Data.Identity.SwedishPersonalIdentityNumberValidator>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            // Participant logout endpoint
            app.MapPost("/participant/logout", async (HttpContext context) =>
            {
                await context.SignOutAsync(ParticipantAuthConstants.SchemeName);
                return Results.LocalRedirect("/participant/login");
            });

            app.Run();
        }
    }
}
