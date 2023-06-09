using Microsoft.AspNetCore.Datasync;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Datasync.NSwag;
using Server.Db;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var database = builder.Configuration.GetValue("CosmosDatabaseName", "TodoAppTestDatabase")!;

builder.Services.AddDbContext<TodoAppContext>(options => options.UseCosmos(connectionString, databaseName: database));
builder.Services.AddDatasyncControllers();
builder.Services.AddOpenApiDocument(options =>
{
    options.AddDatasyncProcessors();
});

var app = builder.Build();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TodoAppContext>();
    await context.InitializeDatabaseAsync().ConfigureAwait(false);
}

app.UseOpenApi();
app.UseSwaggerUi3();

// Configure and run the web service.
app.MapControllers();
app.Run();