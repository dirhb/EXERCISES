var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Adds the in-memory store for session data
builder.Services.AddDistributedMemoryCache();

// Adds session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Guest/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // serves wwwroot files (css, js, images)
app.UseRouting();
app.UseSession(); // must be before UseAuthorization
app.UseAuthorization();
app.MapStaticAssets();

// This sets the default page to GuestController → Home()
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Guest}/{action=Home}/{id?}");

app.Run();