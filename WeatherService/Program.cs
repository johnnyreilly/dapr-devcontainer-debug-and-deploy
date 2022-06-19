using Config;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MailConfig>(builder.Configuration.GetSection("Mail"));

builder.Services.AddControllers().AddDapr();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseCloudEvents();

app.MapSubscribeHandler(); // This is the Dapr subscribe handler
app.MapControllers();


/*
app.UseRouting();

app.UseCloudEvents();

app.UseEndpoints(endpoints =>
{
    endpoints.MapSubscribeHandler(); // This is the Dapr subscribe handler
    endpoints.MapControllers();
});
*/

app.Run();
