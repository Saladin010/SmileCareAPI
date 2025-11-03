
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmileCareAPI.Data;
using SmileCareAPI.Helpers;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Implementation;
using SmileCareAPI.Repositories.Interface;
using SmileCareAPI.DTOs;
using System.Security.Claims;

namespace SmileCareAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Database Configuration
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity Configuration
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // JWT Authentication
            //var jwtKey = builder.Configuration["Jwt:Key"];
            //var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            //var jwtAudience = builder.Configuration["Jwt:Audience"];

            //builder.Services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //})
            //.AddJwtBearer(options =>
            //{
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = true,
            //        ValidateLifetime = true,
            //        ValidateIssuerSigningKey = true,
            //        ValidIssuer = jwtIssuer,
            //        ValidAudience = jwtAudience,
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            //        ClockSkew = TimeSpan.Zero,
            //        RoleClaimType = ClaimTypes.Role, // ✅ مهم جدًا
            //        NameClaimType = ClaimTypes.NameIdentifier
            //    };
            //});
            // قراءة إعدادات Jwt من appsettings.json
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            // إعداد Authentication باستخدام JWT
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // في الإنتاج خليه true

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, // التأكد من الـ Issuer
                    ValidateAudience = true, // التأكد من الـ Audience
                    ValidateLifetime = true, // التأكد من عدم انتهاء الصلاحية
                    ValidateIssuerSigningKey = true, // التأكد من صحة التوقيع
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.FromMinutes(2) // فرق بسيط مسموح في الوقت
                };

                options.Events = new JwtBearerEvents
                {
                    // في حالة انتهاء صلاحية التوكن
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    // بعد التحقق الناجح
                    OnTokenValidated = context =>
                    {
                        // تقدر تضيف تحقق إضافي من المستخدم هنا لو حبيت
                        return Task.CompletedTask;
                    }
                };
            });

            // Register Services
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>(); // ADD THIS LINE
            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IPatientsRepository, PatientsRepository>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
            builder.Services.AddScoped<IDIAGDiagnosisRepository, DIAGDiagnosisRepository>();
            builder.Services.AddScoped<ITreatmentRepository, TreatmentRepository>();


            builder.Services.AddScoped<JwtHelper>();

            // CORS Configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Swagger Configuration
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "SmileCare API",
                    Version = "v1",
                    Description = "SmileCare Dental Clinic Management System API"
                });

                // Add JWT Authentication to Swagger
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your token"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // ← دي مهمة جدًا

                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    // Ensure database is created فقط في Development
                    if (app.Environment.IsDevelopment())
                    {
                        await context.Database.EnsureCreatedAsync();
                    }

                    // Seed roles if they don't exist - تحسين الأداء
                    var roles = new[] { "Admin", "Doctor", "Receptionist", "Patient" };
                    var existingRoles = roleManager.Roles.Select(r => r.Name).ToList();

                    foreach (var role in roles.Where(r => !existingRoles.Contains(r)))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                    // Seed admin user if it doesn't exist - فقط مرة واحدة
                    var adminEmail = "admin@gmail.com";
                    var adminUser = await userManager.FindByEmailAsync(adminEmail);
                    if (adminUser == null)
                    {
                        adminUser = new User
                        {
                            FirstName = "admin",
                            LastName = "admin",
                            UserName = adminEmail,
                            Email = adminEmail,
                            PhoneNumber = "01222656266",
                            Gender = Gender.Male,
                            DateOfBirth = new DateTime(1999, 8, 15),
                            City = "admin",
                            Role = UserRole.Admin,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                       

                        var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(adminUser, "Admin");
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Database seeding failed - continuing anyway");
                }
            }

            app.Run();
        }
    }
}
