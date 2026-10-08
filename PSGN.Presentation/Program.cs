using PSGN.CrossCutting;
using PSGN.Infra.Integracoes.Moodle;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddInfraData(builder.Configuration);
builder.Services
    .AddOptions<ConfigMoodleWebApi>()
    .Bind(builder.Configuration.GetSection("ConfigMoodleWebApi"))
    .ValidateDataAnnotations()
    .ValidateOnStart(); // a aplicação não sobe se faltar BaseUrl ou Token


var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
