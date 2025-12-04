using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using IsYonetimiSistemi.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IsYonetimiSistemi.TelegramBot.Commands;

public class RaporCommands
{
    private readonly ILogger<RaporCommands> _logger;
    private readonly AppDbContext _context;

    public RaporCommands(ILogger<RaporCommands> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task ShowPersonellerList(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        var personeller = await _context.Personeller
            .Where(p => p.Aktif)
            .OrderBy(p => p.AdSoyad)
            .ToListAsync(cancellationToken);

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu") }
        });

        if (!personeller.Any())
        {
            await botClient.EditMessageTextAsync(chatId, messageId, "Personel bulunamadi.", replyMarkup: keyboard, cancellationToken: cancellationToken);
            return;
        }

        var text = "PERSONELLER\n\n";
        foreach (var p in personeller)
        {
            text += $"👤 {p.AdSoyad}\n  Komisyon: %{p.KomisyonOrani:F1}\n  Baslama: {p.IseBaslamaTarihi:dd.MM.yyyy}\n\n";
        }

        await botClient.EditMessageTextAsync(chatId, messageId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    public async Task ShowDurumRaporu(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        var buAy = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        
        // Muhasebe kaynagindan bu ay
        var muhasebeIslemler = await _context.Islemler
            .Where(i => i.IslemTarihi >= buAy && i.Kaynak == "Muhasebe")
            .ToListAsync(cancellationToken);
        
        var muhasebeGelir = muhasebeIslemler.Where(i => i.Tip == "Gelir").Sum(i => i.Tutar);
        var muhasebeGider = muhasebeIslemler.Where(i => i.Tip == "Gider").Sum(i => i.Tutar);
        var muhasebeNet = muhasebeGelir - muhasebeGider;

        // Islemler kaynagindan toplam
        var islemlerKaynak = await _context.Islemler
            .Where(i => i.Kaynak == "Islemler")
            .ToListAsync(cancellationToken);
        
        var toplamIslemTL = islemlerKaynak.Sum(i => i.Tutar);
        
        // Doviz kuru
        decimal dolarKuru = await GetDolarKuruAsync();
        var toplamIslemUSD = toplamIslemTL / dolarKuru;

        var text = $"MALI DURUM\n\n" +
                   $"MUHASEBE (Bu Ay):\n" +
                   $"Gelir: {muhasebeGelir:N2} TL\n" +
                   $"Gider: {muhasebeGider:N2} TL\n" +
                   $"Net: {muhasebeNet:N2} TL\n\n" +
                   $"ISLEMLER (Toplam):\n" +
                   $"Toplam Islem TL: {toplamIslemTL:N2} TL\n" +
                   $"Toplam Islem USD: ${toplamIslemUSD:N2}";

        var keyboard = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu") } });
        await botClient.EditMessageTextAsync(chatId, messageId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    public async Task ShowMuhasebeGelirler(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        var birAyOnce = DateTime.Now.AddMonths(-1);
        var gelirler = await _context.Islemler
            .Where(i => i.IslemTarihi >= birAyOnce && i.Tip == "Gelir" && i.Kaynak == "Muhasebe")
            .OrderByDescending(i => i.IslemTarihi)
            .Take(15)
            .ToListAsync(cancellationToken);

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Muhasebe Menu", "menu_muhasebe") },
            new[] { InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu") }
        });

        if (!gelirler.Any())
        {
            await botClient.EditMessageTextAsync(chatId, messageId, "Muhasebeden gelir bulunamadi.", replyMarkup: keyboard, cancellationToken: cancellationToken);
            return;
        }

        var toplam = gelirler.Sum(i => i.Tutar);
        var text = "MUHASEBE GELIRLERI (Son 1 Ay)\n\n";
        foreach (var g in gelirler.Take(10))
        {
            text += $"• {g.Aciklama}\n  {g.Tutar:N2} TL - {g.IslemTarihi:dd.MM.yyyy}\n\n";
        }
        text += $"Toplam: {toplam:N2} TL ({gelirler.Count} islem)";

        await botClient.EditMessageTextAsync(chatId, messageId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    public async Task ShowMuhasebeGiderler(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        var birAyOnce = DateTime.Now.AddMonths(-1);
        var giderler = await _context.Islemler
            .Where(i => i.IslemTarihi >= birAyOnce && i.Tip == "Gider" && i.Kaynak == "Muhasebe")
            .OrderByDescending(i => i.IslemTarihi)
            .Take(15)
            .ToListAsync(cancellationToken);

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Muhasebe Menu", "menu_muhasebe") },
            new[] { InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu") }
        });

        if (!giderler.Any())
        {
            await botClient.EditMessageTextAsync(chatId, messageId, "Muhasebeden gider bulunamadi.", replyMarkup: keyboard, cancellationToken: cancellationToken);
            return;
        }

        var toplam = giderler.Sum(i => i.Tutar);
        var text = "MUHASEBE GIDERLERI (Son 1 Ay)\n\n";
        foreach (var g in giderler.Take(10))
        {
            text += $"• {g.Aciklama}\n  {g.Tutar:N2} TL - {g.IslemTarihi:dd.MM.yyyy}\n\n";
        }
        text += $"Toplam: {toplam:N2} TL ({giderler.Count} islem)";

        await botClient.EditMessageTextAsync(chatId, messageId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    public async Task ShowPersonelIslemler(ITelegramBotClient botClient, long chatId, int messageId, int gunSayisi, CancellationToken cancellationToken)
    {
        var baslangic = DateTime.Now.AddDays(-gunSayisi);
        var islemler = await _context.Islemler
            .Where(i => i.IslemTarihi >= baslangic && i.PersonelId != null)
            .OrderByDescending(i => i.IslemTarihi)
            .Take(20)
            .ToListAsync(cancellationToken);

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Personel Islemler", "menu_personel_islemler") },
            new[] { InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu") }
        });

        if (!islemler.Any())
        {
            await botClient.EditMessageTextAsync(chatId, messageId, $"Son {gunSayisi} gunde personel islemi bulunamadi.", replyMarkup: keyboard, cancellationToken: cancellationToken);
            return;
        }

        var zamanText = gunSayisi == 1 ? "Son 1 Gun" : gunSayisi == 7 ? "Son 1 Hafta" : "Son 1 Ay";
        var text = $"PERSONEL ISLEMLERI ({zamanText})\n\n";
        foreach (var i in islemler.Take(15))
        {
            var tip = i.Tip == "Gelir" ? "+" : "-";
            text += $"{tip} {i.PersonelAdi}\n  {i.Aciklama}\n  {i.Tutar:N2} TL - {i.IslemTarihi:dd.MM.yyyy}\n\n";
        }
        text += $"Toplam {islemler.Count} islem";

        await botClient.EditMessageTextAsync(chatId, messageId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    public async Task ShowPerformansRaporu(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        var buAy = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var performans = await _context.Islemler
            .Where(i => i.IslemTarihi >= buAy && i.PersonelId != null)
            .GroupBy(i => new { i.PersonelId, i.PersonelAdi })
            .Select(g => new { PersonelAdi = g.Key.PersonelAdi, Toplam = g.Sum(i => i.Tutar), Adet = g.Count() })
            .OrderByDescending(x => x.Toplam)
            .Take(10)
            .ToListAsync(cancellationToken);

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Raporlar", "menu_raporlar") },
            new[] { InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu") }
        });

        if (!performans.Any())
        {
            await botClient.EditMessageTextAsync(chatId, messageId, "Bu ay performans verisi yok.", replyMarkup: keyboard, cancellationToken: cancellationToken);
            return;
        }

        var text = "GENEL PERFORMANS (Bu Ay)\n\n";
        int sira = 1;
        foreach (var p in performans)
        {
            var medal = sira == 1 ? "1." : sira == 2 ? "2." : sira == 3 ? "3." : $"{sira}.";
            text += $"{medal} {p.PersonelAdi}\n   {p.Toplam:N2} TL ({p.Adet} islem)\n\n";
            sira++;
        }

        await botClient.EditMessageTextAsync(chatId, messageId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    public async Task ShowPersonelSecimMenu(ITelegramBotClient botClient, long chatId, int messageId, int gunSayisi, CancellationToken cancellationToken)
    {
        var personeller = await _context.Personeller
            .Where(p => p.Aktif)
            .OrderBy(p => p.AdSoyad)
            .ToListAsync(cancellationToken);

        if (!personeller.Any())
        {
            var emptyKeyboard = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu") } });
            await botClient.EditMessageTextAsync(chatId, messageId, "Personel bulunamadi.", replyMarkup: emptyKeyboard, cancellationToken: cancellationToken);
            return;
        }

        var zamanText = gunSayisi == 1 ? "Son 1 Gun" : gunSayisi == 7 ? "Son 1 Hafta" : "Son 1 Ay";
        var buttons = new List<InlineKeyboardButton[]>();
        foreach (var p in personeller)
        {
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(p.AdSoyad, $"personel_detay_{p.Id}_{gunSayisi}_") });
        }
        buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("Zaman Sec", "rapor_personel_zaman_sec") });
        buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("Raporlar", "menu_raporlar"), InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu") });

        var keyboard = new InlineKeyboardMarkup(buttons);
        await botClient.EditMessageTextAsync(chatId, messageId, $"PERSONEL RAPORU ({zamanText})\n\nPersonel secin:", replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    public async Task ShowPersonelDetayRapor(ITelegramBotClient botClient, long chatId, int messageId, int personelId, int gunSayisi, CancellationToken cancellationToken)
    {
        var personel = await _context.Personeller.FindAsync(personelId);
        if (personel == null)
        {
            var errorKeyboard = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu") } });
            await botClient.EditMessageTextAsync(chatId, messageId, "Personel bulunamadi.", replyMarkup: errorKeyboard, cancellationToken: cancellationToken);
            return;
        }

        var baslangicTarihi = DateTime.Now.AddDays(-gunSayisi);
        var islemler = await _context.Islemler
            .Where(i => i.PersonelId == personelId && i.IslemTarihi >= baslangicTarihi && i.Kaynak == "Islemler")
            .OrderByDescending(i => i.IslemTarihi)
            .ToListAsync(cancellationToken);

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Personel Sec", $"personel_rapor_gun_{gunSayisi}_") },
            new[] { InlineKeyboardButton.WithCallbackData("Zaman Sec", "rapor_personel_zaman_sec") },
            new[] { InlineKeyboardButton.WithCallbackData("Ana Menu", "main_menu") }
        });

        var zamanText = gunSayisi == 1 ? "Son 1 Gun" : gunSayisi == 7 ? "Son 1 Hafta" : "Son 1 Ay";

        if (!islemler.Any())
        {
            await botClient.EditMessageTextAsync(chatId, messageId, $"{personel.AdSoyad}\n\n{zamanText}'de islem yok.", replyMarkup: keyboard, cancellationToken: cancellationToken);
            return;
        }

        // Doviz kuru bilgisi
        decimal dolarKuru = await GetDolarKuruAsync();

        // Islem listesi olustur (en fazla 8 islem)
        var islemListesiText = "";
        foreach (var islem in islemler.Take(8))
        {
            // DOGRU KOMISYON HESAPLAMA - PAYLASIM ORANI KONTROLU
            decimal komisyonOrani;
            if (islem.PaylasimOrani.HasValue)
            {
                // Paylasilmis islem - Paylasim oranini kullan
                komisyonOrani = islem.PaylasimOrani.Value;
            }
            else
            {
                // Normal islem - Personelin standart oranini kullan
                komisyonOrani = personel.KomisyonOrani;
            }
            var komisyon = islem.Tutar * komisyonOrani / 100;

            islemListesiText += $"• {islem.IslemTarihi:dd.MM} - {islem.Aciklama}\n  {islem.Tutar:N2} TL (Kom: {komisyon:N2} TL)\n\n";
        }

        // Toplam hesaplamalar - DOGRU YONTEM
        var toplamIslemSayisi = islemler.Count;
        var toplamTutar = islemler.Sum(i => i.Tutar);
        var toplamTutarDolar = toplamTutar / dolarKuru;

        // DOGRU TOPLAM KOMISYON HESAPLAMA
        decimal toplamKomisyon = 0;
        foreach (var islem in islemler)
        {
            if (islem.PaylasimOrani.HasValue)
            {
                toplamKomisyon += islem.Tutar * islem.PaylasimOrani.Value / 100;
            }
            else
            {
                toplamKomisyon += islem.Tutar * personel.KomisyonOrani / 100;
            }
        }
        var toplamKomisyonDolar = toplamKomisyon / dolarKuru;

        var text = $"PERSONEL RAPORU\n\n" +
                   $"{personel.AdSoyad}\n" +
                   $"Komisyon Orani: %{personel.KomisyonOrani:F1}\n\n" +
                   $"{zamanText.ToUpper()} ISLEMLERI:\n\n" +
                   islemListesiText +
                   (islemler.Count > 8 ? $"... ve {islemler.Count - 8} islem daha\n\n" : "\n") +
                   $"OZET:\n" +
                   $"Toplam Islem: {toplamIslemSayisi} adet\n" +
                   $"Toplam Tutar: {toplamTutar:N2} TL\n" +
                   $"  (${toplamTutarDolar:N2})\n" +
                   $"Toplam Komisyon: {toplamKomisyon:N2} TL\n" +
                   $"  (${toplamKomisyonDolar:N2})";

        await botClient.EditMessageTextAsync(chatId, messageId, text, replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    private async Task<decimal> GetDolarKuruAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            var response = await httpClient.GetStringAsync("https://api.exchangerate-api.com/v4/latest/USD");
            var jsonDoc = System.Text.Json.JsonDocument.Parse(response);
            
            if (jsonDoc.RootElement.TryGetProperty("rates", out var rates))
            {
                if (rates.TryGetProperty("TRY", out var tryRate))
                {
                    return tryRate.GetDecimal();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Doviz kuru alinamadi, varsayilan kur kullaniliyor");
        }
        
        // Varsayilan kur
        return 34.50m;
    }
}

