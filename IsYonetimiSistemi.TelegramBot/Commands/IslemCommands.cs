using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using IsYonetimiSistemi.Shared.Data;
using IsYonetimiSistemi.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IsYonetimiSistemi.TelegramBot.Commands;

public class IslemCommands
{
    private readonly ILogger<IslemCommands> _logger;
    private readonly AppDbContext _context;

    public IslemCommands(ILogger<IslemCommands> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // Bu sýnýf gelecekte gerekirse iþlem görüntüleme komutlarý için kullanýlabilir
    // Þu anda tüm iþlem ekleme özellikleri kaldýrýlmýþtýr - sadece masaüstü uygulamadan eklenebilir
}


