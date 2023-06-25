var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();
builder.Services.AddCors((opt) => 
{
    opt.AddPolicy("AllowAllPloicy",
        (p) => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()

        );
    
    
    });
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Search}/{id?}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAllPloicy");
app.UseAuthorization();
app.MapControllers();
app.Run();
