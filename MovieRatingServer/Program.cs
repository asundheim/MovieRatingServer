using Microsoft.Extensions.DependencyInjection;
using MovieRatingServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ITimeService, TimeService>();
builder.Services.AddSingleton<IMovieListService, MovieListService>();
builder.Services.AddSingleton<IPlayerScoreService, PlayerScoreService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// TODO disable
app.UseCors(options => options.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
