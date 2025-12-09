using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Polling;
using IsYonetimiSistemi.Shared.Data;
using IsYonetimiSistemi.TelegramBot.Commands;
using IsYonetimiSistemi.TelegramBot.Services;

var builder = WebApplication.CreateBuilder(args);

// Telegram Bot Token
var botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") 
    ?? throw new Exception("TELEGRAM_BOT_TOKEN environment variable is not set!");

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ??
        "Server=sql6033.site4now.net;" +
        "Database=db_ac18b3_isyonetimi;" +
        "User Id=db_ac18b3_isyonetimi_admin;" +
        "Password=IsYonetim2025!@#;" +
        "Encrypt=False;" +
        "TrustServerCertificate=True;" +
        "MultipleActiveResultSets=True;";
    
    options.UseSqlServer(connectionString);
});

// Telegram Bot
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
builder.Services.AddScoped<AuthorizationService>();
builder.Services.AddScoped<BotUpdateHandler>();
builder.Services.AddScoped<RaporCommands>();
builder.Services.AddScoped<KasaCommands>();

// Hosted Service for Telegram Bot
builder.Services.AddHostedService<TelegramBotHostedService>();

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
