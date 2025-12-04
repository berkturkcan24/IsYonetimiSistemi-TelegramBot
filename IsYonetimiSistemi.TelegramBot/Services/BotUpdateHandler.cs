using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using IsYonetimiSistemi.TelegramBot.Commands;
using Microsoft.Extensions.Logging;

namespace IsYonetimiSistemi.TelegramBot.Services;

public class BotUpdateHandler : IUpdateHandler
{
    private readonly ILogger<BotUpdateHandler> _logger;
    private readonly RaporCommands _raporCommands;
    private readonly AuthorizationService _authorizationService;

    public BotUpdateHandler(
        ILogger<BotUpdateHandler> logger,
        RaporCommands raporCommands,
        AuthorizationService authorizationService)
    {
        _logger = logger;
        _raporCommands = raporCommands;
        _authorizationService = authorizationService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            // Chat ID kontrolü
            long chatId = 0;
            if (update.Type == UpdateType.Message && update.Message != null)
            {
                chatId = update.Message.Chat.Id;
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Message != null)
            {
                chatId = update.CallbackQuery.Message.Chat.Id;
            }

            // Chat ID'yi logla (debug için)
            if (chatId != 0)
            {
                _logger.LogInformation($"Incoming request from Chat ID: {chatId}");
            }

            // Yetkilendirme kontrolü
            if (chatId != 0 && !_authorizationService.IsAuthorized(chatId))
            {
                await SendUnauthorizedMessage(botClient, chatId, cancellationToken);
                return;
            }

            // Yetkili kullanýcý - iþlemleri devam ettir
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await HandleMessageAsync(botClient, update.Message, cancellationToken);
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                await HandleCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update");
        }
    }

    private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var messageText = message.Text!;

        _logger.LogInformation($"Message: {messageText} from {chatId}");

        if (messageText == "/start")
        {
            await SendMainMenu(botClient, chatId, cancellationToken);
            return;
        }

        // Varsayýlan: Ana menü göster
        await SendMainMenu(botClient, chatId, cancellationToken);
    }

    private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var chatId = callbackQuery.Message!.Chat.Id;
        var messageId = callbackQuery.Message.MessageId;
        var data = callbackQuery.Data;

        _logger.LogInformation($"Callback: {data}");
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);

        try
        {
            switch (data)
            {
                case "main_menu":
                    await EditMainMenu(botClient, chatId, messageId, cancellationToken);
                    break;
                case "menu_personeller":
                    await _raporCommands.ShowPersonellerList(botClient, chatId, messageId, cancellationToken);
                    break;
                case "menu_durum":
                    await _raporCommands.ShowDurumRaporu(botClient, chatId, messageId, cancellationToken);
                    break;
                case "menu_muhasebe":
                    await ShowMuhasebeMenu(botClient, chatId, messageId, cancellationToken);
                    break;
                case "menu_personel_islemler":
                    await ShowPersonelIslemlerMenu(botClient, chatId, messageId, cancellationToken);
                    break;
                case "menu_raporlar":
                    await ShowRaporlarMenu(botClient, chatId, messageId, cancellationToken);
                    break;
                case "muhasebe_gelir":
                    await _raporCommands.ShowMuhasebeGelirler(botClient, chatId, messageId, cancellationToken);
                    break;
                case "muhasebe_gider":
                    await _raporCommands.ShowMuhasebeGiderler(botClient, chatId, messageId, cancellationToken);
                    break;
                case "personel_islem_1gun":
                    await _raporCommands.ShowPersonelIslemler(botClient, chatId, messageId, 1, cancellationToken);
                    break;
                case "personel_islem_1hafta":
                    await _raporCommands.ShowPersonelIslemler(botClient, chatId, messageId, 7, cancellationToken);
                    break;
                case "personel_islem_1ay":
                    await _raporCommands.ShowPersonelIslemler(botClient, chatId, messageId, 30, cancellationToken);
                    break;
                case "rapor_performans":
                    await _raporCommands.ShowPerformansRaporu(botClient, chatId, messageId, cancellationToken);
                    break;
                case "rapor_personel_zaman_sec":
                    await ShowPersonelRaporZamanMenu(botClient, chatId, messageId, cancellationToken);
                    break;
                case "yardim":
                    await ShowYardimMenu(botClient, chatId, messageId, cancellationToken);
                    break;
                default:
                    if (data.StartsWith("personel_rapor_gun_"))
                    {
                        var parts = data.Replace("personel_rapor_gun_", "").Split('_');
                        var gunSayisi = int.Parse(parts[0]);
                        await _raporCommands.ShowPersonelSecimMenu(botClient, chatId, messageId, gunSayisi, cancellationToken);
                    }
                    else if (data.StartsWith("personel_detay_"))
                    {
                        var parts = data.Replace("personel_detay_", "").Split('_');
                        var personelId = int.Parse(parts[0]);
                        var gunSayisi = int.Parse(parts[1]);
                        await _raporCommands.ShowPersonelDetayRapor(botClient, chatId, messageId, personelId, gunSayisi, cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning($"Unknown callback: {data}");
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling callback: {data}");
        }
    }

    private async Task SendMainMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Personeller", "menu_personeller"),
                InlineKeyboardButton.WithCallbackData("Durum", "menu_durum")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Gelir-Gider", "menu_muhasebe"),
                InlineKeyboardButton.WithCallbackData("Personel Islemler", "menu_personel_islemler")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Raporlar", "menu_raporlar"),
                InlineKeyboardButton.WithCallbackData("Yardim", "yardim")
            }
        });

        await botClient.SendTextMessageAsync(
            chatId,
            "ANA MENU\n\nBilgi Görüntüleme ve Raporlama\n\n" +
            "Not: islem ekleme sadece masaüstü uygulamadan yapilabilir.",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }

    private async Task EditMainMenu(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Personeller", "menu_personeller"),
                InlineKeyboardButton.WithCallbackData("Durum", "menu_durum")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Gelir-Gider", "menu_muhasebe"),
                InlineKeyboardButton.WithCallbackData("Personel Islemler", "menu_personel_islemler")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Raporlar", "menu_raporlar"),
                InlineKeyboardButton.WithCallbackData("Yardim", "yardim")
            }
        });

        await botClient.EditMessageTextAsync(
            chatId,
            messageId,
            "ANA MENU\n\nBilgi Görüntüleme ve Raporlama\n\n" +
            "Not: islem ekleme sadece masaüstü uygulamadan yapilabilir.",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }

    private async Task ShowMuhasebeMenu(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📖 Gelirler Listesi", "muhasebe_gelir")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📖 Giderler Listesi", "muhasebe_gider")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu")
            }
        });

        await botClient.EditMessageTextAsync(
            chatId,
            messageId,
            "MUHASEBE\n\n" +
            "Muhasebe kayitlarini görüntüleyin:\n" +
            "📖 Gelirler listesi\n" +
            "📖 Giderler listesi\n\n" +
            "Islem ekleme için masaüstü uygulamayi kullanýn.",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }

    private async Task ShowPersonelIslemlerMenu(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Son 1 Gün", "personel_islem_1gun")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Son 1 Hafta", "personel_islem_1hafta")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Son 1 Ay", "personel_islem_1ay")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu")
            }
        });

        await botClient.EditMessageTextAsync(
            chatId,
            messageId,
            "PERSONEL ISLEMLERI\n\nZaman araligi seçin:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }

    private async Task ShowRaporlarMenu(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Genel Performans", "rapor_performans")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Personel Raporu", "rapor_personel_zaman_sec")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu")
            }
        });

        await botClient.EditMessageTextAsync(
            chatId,
            messageId,
            "RAPORLAR\n\nDetayli raporlari görüntüleyebilirsiniz:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }

    private async Task ShowPersonelRaporZamanMenu(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Son 1 Gün", "personel_rapor_gun_1_")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Son 1 Hafta", "personel_rapor_gun_7_")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Son 1 Ay", "personel_rapor_gun_30_")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Raporlar", "menu_raporlar"),
                InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu")
            }
        });

        await botClient.EditMessageTextAsync(
            chatId,
            messageId,
            "PERSONEL RAPORU\n\nZaman araligi seçin:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }

    private async Task ShowYardimMenu(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu")
            }
        });

        var helpText = "YARDIM\n\n" +
                      "Bu bot sadece bilgi görüntüleme amaçlidir.\n\n" +
                      "YAPILABILENLER:\n" +
                      "👤 Personel listesi görüntüleme\n" +
                      "💰 Mali durum kontrolü\n" +
                      "📖 Muhasebe kayitlari inceleme\n" +
                      "👤 Personel islem geçmisi\n" +
                      "📈 Performans raporlari\n\n" +
                      "YAPILAM AYANLAR:\n" +
                      " Islem ekleme\n" +
                      " Gelir/Gider ekleme\n" +
                      " Kayit düzenleme\n" +
                      " Kayit silme\n\n" +
                      "Bu islemler için masaüstü uygulamayi kullanin.";

        await botClient.EditMessageTextAsync(
            chatId,
            messageId,
            helpText,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }

    private async Task SendUnauthorizedMessage(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var message = "?? <b>YETKISIZ ERISIM</b>\n\n" +
                     "? Bu botu kullanma yetkiniz bulunmamaktadir.\n\n" +
                     $"?? Chat ID: <code>{chatId}</code>\n\n" +
                     "?? Erisim için sistem yöneticisi ile iletisime geçiniz.";

        await botClient.SendTextMessageAsync(
            chatId,
            message,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
            cancellationToken: cancellationToken
        );

        _logger.LogWarning($"Unauthorized access denied for chat ID: {chatId}");
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Polling error");
        return Task.CompletedTask;
    }
}
