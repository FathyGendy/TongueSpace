using CoursePLatform.Data;
using CoursePLatform.Models;
using CoursePLatform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration or environment variables.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Events.OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync;
});

builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Console.WriteLine("🌱 Seeding database...");
        await SeedRoles(services);
        await SeedAdminUser(services);
        await SeedInstructorUser(services);
        await SeedCourses(services);
        Console.WriteLine("✅ Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.Run();

static async Task SeedRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roleNames = { "Admin", "Instructor", "Student" };
    
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
            Console.WriteLine($"✅ Role created: {roleName}");
        }
    }
}

static async Task SeedAdminUser(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    var adminEmail = configuration["AdminUser:Email"] 
        ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") 
        ?? "admin@tonguespace.com";
    
    var adminPassword = configuration["AdminUser:Password"]
        ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
        ?? "Admin123!";

    var adminFirstName = configuration["AdminUser:FirstName"]
        ?? Environment.GetEnvironmentVariable("ADMIN_FIRSTNAME")
        ?? "Admin";

    var adminLastName = configuration["AdminUser:LastName"]
        ?? Environment.GetEnvironmentVariable("ADMIN_LASTNAME")
        ?? "User";
    
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = adminFirstName,
            LastName = adminLastName,
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine($"✅ Admin user created: {adminEmail}");
        }
        else
        {
            Console.WriteLine("❌ Admin user creation failed:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($" - {error.Description}");
            }
        }
    }
    else
    {
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine($"✅ Assigned Admin role to existing user: {adminEmail}");
        }
    }
}
static async Task SeedInstructorUser(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var instructorEmail = configuration["InstructorUser:Email"]
        ?? Environment.GetEnvironmentVariable("INSTRUCTOR_EMAIL")
        ?? "instructor@tonguespace.com";

    var instructorPassword = configuration["InstructorUser:Password"]
        ?? Environment.GetEnvironmentVariable("INSTRUCTOR_PASSWORD")
        ?? "Instructor123!";

    var instructorFirstName = configuration["InstructorUser:FirstName"]
        ?? Environment.GetEnvironmentVariable("INSTRUCTOR_FIRSTNAME")
        ?? "Ahmed";

    var instructorLastName = configuration["InstructorUser:LastName"]
        ?? Environment.GetEnvironmentVariable("INSTRUCTOR_LASTNAME")
        ?? "Hassan";
    var instructorUser = await userManager.FindByEmailAsync(instructorEmail);

    if (instructorUser == null)
    {
        instructorUser = new ApplicationUser
        {
            UserName = instructorEmail,
            Email = instructorEmail,
            FirstName = instructorFirstName,
            LastName = instructorLastName,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(instructorUser, instructorPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(instructorUser, "Instructor");
            Console.WriteLine($"✅ Instructor user created: {instructorEmail}");
        }
        else
        {
            Console.WriteLine("❌ Instructor user creation failed:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($" - {error.Description}");
            }
        }
    }
    else
    {
        if (!await userManager.IsInRoleAsync(instructorUser, "Instructor"))
        {
            await userManager.AddToRoleAsync(instructorUser, "Instructor");
            Console.WriteLine($"✅ Assigned Instructor role to existing user: {instructorEmail}");
        }
    }
}
static async Task SeedCourses(IServiceProvider serviceProvider)
{
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    if (await context.Courses.AnyAsync())
    {
        Console.WriteLine("📚 Courses already exist, skipping course seeding.");
        return;
    }
    var instructorEmail = configuration["InstructorUser:Email"]
        ?? Environment.GetEnvironmentVariable("INSTRUCTOR_EMAIL")
        ?? "instructor@tonguespace.com";

    var adminEmail = configuration["AdminUser:Email"]
        ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL")
        ?? "admin@tonguespace.com";
    var instructor1 = await userManager.FindByEmailAsync(instructorEmail);
    var instructor2 = await userManager.FindByEmailAsync(adminEmail);

    if (instructor1 == null || instructor2 == null)
    {
        Console.WriteLine("❌ No instructor users found for course seeding.");
        return;
    }
    var courses = new List<Course>
    {
        new Course
        {
            Title = "Learn Arabic for Beginners",
            Description = "Complete course for learning Arabic from scratch. Learn the Arabic alphabet, basic grammar, pronunciation, and essential vocabulary used in everyday conversations.",
            Language = CourseLanguage.Arabic,
            Level = CourseLevel.Beginner,
            Price = 49.99m,
            InstructorId = instructor1.Id,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            ThumbnailUrl = "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=400&h=200&fit=crop"
        },
        new Course
        {
            Title = "German Grammar Mastery",
            Description = "Master German grammar with practical exercises. Learn cases, verb conjugations, sentence structure, and advanced grammar concepts with real-world examples.",
            Language = CourseLanguage.German,
            Level = CourseLevel.Intermediate,
            Price = 79.99m,
            InstructorId = instructor1.Id,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-25),
            ThumbnailUrl = "https://images.unsplash.com/photo-1527866959252-deab85ef7d1b?w=400&h=200&fit=crop"
        },

        new Course
        {
            Title = "Advanced English Writing",
            Description = "Perfect your English writing skills for academic and professional purposes. Learn essay structure, research methods, and advanced writing techniques.",
            Language = CourseLanguage.English,
            Level = CourseLevel.Advanced,
            Price = 89.99m,
            InstructorId = instructor1.Id,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            ThumbnailUrl = "https://images.unsplash.com/photo-1456513080510-7bf3a84b82f8?w=400&h=200&fit=crop"
        },

        new Course
        {
            Title = "French Conversation Practice",
            Description = "Improve your French speaking skills through interactive conversations. Practice everyday scenarios, business French, and cultural expressions.",
            Language = CourseLanguage.French,
            Level = CourseLevel.Intermediate,
            Price = 59.99m,
            InstructorId = instructor2.Id,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ThumbnailUrl = null // Will use CSS gradient
        },

        new Course
        {
            Title = "Spanish for Travelers",
            Description = "Essential Spanish phrases and vocabulary for travelers. Learn practical conversations for hotels, restaurants, transportation, and emergencies.",
            Language = CourseLanguage.Spanish,
            Level = CourseLevel.Beginner,
            Price = 39.99m,
            InstructorId = instructor2.Id,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            ThumbnailUrl = "https://images.unsplash.com/photo-1539037116277-4db20889f2d4?w=400&h=200&fit=crop"
        },

        new Course
        {
            Title = "Business English Communication",
            Description = "Professional English for the workplace. Master email writing, presentations, meetings, negotiations, and business vocabulary.",
            Language = CourseLanguage.English,
            Level = CourseLevel.Advanced,
            Price = 99.99m,
            InstructorId = instructor1.Id,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            ThumbnailUrl = "https://images.unsplash.com/photo-1521737604893-d14cc237f11d?w=400&h=200&fit=crop"
        }
    };
    await context.Courses.AddRangeAsync(courses);
    await context.SaveChangesAsync();

    Console.WriteLine($"✅ {courses.Count} sample courses created successfully!");
    await SeedLessons(context, courses);
}
static async Task SeedLessons(ApplicationDbContext context, List<Course> courses)
{
    var lessons = new List<Lesson>();

    foreach (var course in courses)
    {
        switch (course.Title)
        {
            case "Learn Arabic for Beginners":
                lessons.AddRange(new[]
                {
                    new Lesson { CourseId = course.Id, Title = "Arabic Alphabet - Part 1", Description = "Learn the first 14 letters of the Arabic alphabet", DurationMinutes = 30, OrderIndex = 1, IsPublished = true, VideoUrl = "https://youtu.be/NYQU0_KgWD8?si=q0eM7aW8AJZGTsmB" },
                    new Lesson { CourseId = course.Id, Title = "Arabic Alphabet - Part 2", Description = "Complete the Arabic alphabet", DurationMinutes = 30, OrderIndex = 2, IsPublished = true, VideoUrl = "https://youtu.be/mZnUOs1lyCs?si=ozfIoL0NV2kbg9fU" },
                    new Lesson { CourseId = course.Id, Title = "Basic Greetings", Description = "Essential greetings and polite expressions", DurationMinutes = 25, OrderIndex = 3, IsPublished = true, VideoUrl = "https://youtu.be/hEApCv1bIeg?si=W6wNDvPhRwM9BYCN" },
                    new Lesson { CourseId = course.Id, Title = "Numbers 1-20", Description = "Learn to count in Arabic", DurationMinutes = 20, OrderIndex = 4, IsPublished = true, VideoUrl = "https://youtu.be/R4OdbmZebdA?si=mQvjF23cenGI_Ads" }
                });
                break;

            case "German Grammar Mastery":
                lessons.AddRange(new[]
                {
                    new Lesson { CourseId = course.Id, Title = "German Cases Overview", Description = "Introduction to Nominative, Accusative, Dative, and Genitive cases", DurationMinutes = 45, OrderIndex = 1, IsPublished = true, VideoUrl = "https://youtu.be/bZL5OB61XXM?si=HdQlcC1JRlUAUqSA" },
                    new Lesson { CourseId = course.Id, Title = "Der, Die, Das - Articles", Description = "Master German definite and indefinite articles", DurationMinutes = 40, OrderIndex = 2, IsPublished = true, VideoUrl = "https://youtu.be/fYZP95y2mgM?si=_5Rj7KVi4tVLdCNv" },
                    new Lesson { CourseId = course.Id, Title = "Verb Conjugations", Description = "Present, past, and future verb conjugations", DurationMinutes = 50, OrderIndex = 3, IsPublished = true, VideoUrl = "https://youtu.be/0LRs_M_BtsI?si=IDJXrHrJvCqeiwhe" }
                });
                break;

            case "French Conversation Practice":
                lessons.AddRange(new[]
                {
                    new Lesson { CourseId = course.Id, Title = "Café Conversations", Description = "Ordering food and drinks in French", DurationMinutes = 35, OrderIndex = 1, IsPublished = true, VideoUrl = "https://youtu.be/MBK7K1Xw3Lc?si=7Sd8k5V2ZzAA-Vgr" },
                    new Lesson { CourseId = course.Id, Title = "Shopping Dialogues", Description = "Shopping vocabulary and expressions", DurationMinutes = 30, OrderIndex = 2, IsPublished = true, VideoUrl = "https://youtu.be/9MPp8XzC4rA?si=bjjWfsxGRfcQ_buU" }
                });
                break;

            case "Advanced English Writing":
                lessons.AddRange(new[]
                {
                    new Lesson { CourseId = course.Id, Title = "Essay Structure", Description = "Learn to write compelling essays", DurationMinutes = 60, OrderIndex = 1, IsPublished = true, VideoUrl = "https://youtu.be/Drv6jD8xWdw?si=KpqnR_CTXUSfWHsc" },
                    new Lesson { CourseId = course.Id, Title = "Research and Citations", Description = "Academic research and proper citation methods", DurationMinutes = 45, OrderIndex = 2, IsPublished = true, VideoUrl = "https://youtu.be/VIy4A3zmljA?si=FOm1voiIaY2cE9vJ" }
                });
                break;

            case "Spanish for Travelers":
                lessons.AddRange(new[]
                {
                    new Lesson { CourseId = course.Id, Title = "Airport and Transportation", Description = "Navigate airports and public transportation", DurationMinutes = 40, OrderIndex = 1, IsPublished = true, VideoUrl = "https://youtu.be/7iXFSyaMr_g?si=7sALHysZT8xsPNTS" },
                    new Lesson { CourseId = course.Id, Title = "Hotel Check-in", Description = "Hotel reservations and check-in procedures", DurationMinutes = 35, OrderIndex = 2, IsPublished = true, VideoUrl = "https://youtu.be/a1wUYeFRMHc?si=4nUMtGW0vwJpo0nU" }
                });
                break;

            case "Business English Communication":
                lessons.AddRange(new[]
                {
                    new Lesson { CourseId = course.Id, Title = "Professional Email Writing", Description = "Write effective business emails", DurationMinutes = 50, OrderIndex = 1, IsPublished = true, VideoUrl = "https://youtu.be/3Tu1jN65slw?si=7ErTm2JsG_cFgFJK" },
                    new Lesson { CourseId = course.Id, Title = "Meeting Vocabulary", Description = "Essential vocabulary for business meetings", DurationMinutes = 40, OrderIndex = 2, IsPublished = true, VideoUrl = "https://youtu.be/PLhAzAymMsY?si=j-TFcNs3_HUZS2eC" },
                    new Lesson { CourseId = course.Id, Title = "Presentation Skills", Description = "Deliver confident presentations in English", DurationMinutes = 55, OrderIndex = 3, IsPublished = true, VideoUrl = "https://youtu.be/pLzOA36qLQ0?si=HJ9WjIM3rYMhdomB" }
                });
                break;
        }
    }

    if (lessons.Any())
    {
        await context.Lessons.AddRangeAsync(lessons);
        await context.SaveChangesAsync();
        Console.WriteLine($"✅ {lessons.Count} sample lessons created successfully!");
    }
}