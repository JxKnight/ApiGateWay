var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Add New Start
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddHttpContextAccessor(); // Add this line to inject IHttpContextAccessor

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15); // Set timeout value
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", builder =>
    {
        builder
            .WithOrigins("https://localhost:4200") // Replace with your Angular app's domain
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("BT-Signature")
            .WithExposedHeaders("BT-Primary-Key")
            .WithExposedHeaders("BT-Identity-Key"); // This allows cookies and credentials to be sent with the request
    });
});
var app = builder.Build();
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(c =>
  c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiGateWay v1"));
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();