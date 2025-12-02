using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using IsYonetimiSistemi.TelegramBot.Services;

namespace IsYonetimiSistemi.TelegramBot.Services;

public class TelegramBotHostedService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelegramBotHostedService> _logger;

    public TelegramBotHostedService(
        ITelegramBotClient botClient,
        IServiceProvider serviceProvider,
        ILogger<TelegramBotHostedService> logger)
    {
        _botClient = botClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telegram Bot baslatiliyor...");

        var me = await _botClient.GetMeAsync(stoppingToken);
        _logger.LogInformation($"Bot baslatildi: @{me.Username}");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
            ThrowPendingUpdates = true
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var updateHandler = scope.ServiceProvider.GetRequiredService<BotUpdateHandler>();

                await _botClient.ReceiveAsync(
                    updateHandler,
                    receiverOptions,
                    stoppingToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bot calisirken hata olustu");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("Telegram Bot durduruluyor...");
    }
}
