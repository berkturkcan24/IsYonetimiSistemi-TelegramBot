# TELEGRAM BOT - KASA MODÜLÜ ENTEGRASYONU

## Adýmlar:

### 1. BotUpdateHandler.cs Güncellemeleri

#### Constructor'a KasaCommands ekle:
```csharp
private readonly KasaCommands _kasaCommands;

public BotUpdateHandler(
    ILogger<BotUpdateHandler> logger,
    RaporCommands raporCommands,
    KasaCommands kasaCommands, // YENÝ
    AuthorizationService authorizationService)
{
    _logger = logger;
    _raporCommands = raporCommands;
    _kasaCommands = kasaCommands; // YENÝ
    _authorizationService = authorizationService;
}
```

#### SendMainMenu metoduna Kasa butonu ekle:
```csharp
private async Task SendMainMenu(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
{
    var keyboard = new InlineKeyboardMarkup(new[]
    {
        new[] { InlineKeyboardButton.WithCallbackData("?? Personeller", "menu_personeller") },
        new[] { InlineKeyboardButton.WithCallbackData("?? Mali Durum", "menu_durum") },
        new[] { InlineKeyboardButton.WithCallbackData("?? Muhasebe", "menu_muhasebe") },
        new[] { InlineKeyboardButton.WithCallbackData("?? Kasa", "menu_kasa") } // YENÝ
    });

    var text = "?? IS YONETIM SISTEMI\n\n" +
               "Hos geldiniz! Lutfen bir secim yapin:";

    await botClient.SendTextMessageAsync(
        chatId,
        text,
        replyMarkup: keyboard,
        cancellationToken: cancellationToken);
}
```

#### HandleCallbackQueryAsync metoduna case'ler ekle:
```csharp
switch (data)
{
    // ... mevcut case'ler ...
    
    // KASA MENU
    case "menu_kasa":
        await _kasaCommands.ShowKasaMenu(botClient, chatId, messageId, cancellationToken);
        break;
    case "kasa_ozet":
        await _kasaCommands.ShowKasaOzet(botClient, chatId, messageId, cancellationToken);
        break;
    case "kasa_ltc":
        await _kasaCommands.ShowLitecoinDetay(botClient, chatId, messageId, cancellationToken);
        break;
    case "kasa_trx":
        await _kasaCommands.ShowTronDetay(botClient, chatId, messageId, cancellationToken);
        break;
    case "kasa_usdt_trc20":
        await _kasaCommands.ShowUsdtTrc20Detay(botClient, chatId, messageId, cancellationToken);
        break;
    case "kasa_usdt_erc20":
        await _kasaCommands.ShowUsdtErc20Detay(botClient, chatId, messageId, cancellationToken);
        break;
        
    // ... diðer case'ler ...
}
```

### 2. Program.cs Güncellemeleri

#### KasaCommands DI ekle:
```csharp
builder.Services.AddSingleton<RaporCommands>();
builder.Services.AddSingleton<KasaCommands>(); // YENÝ
builder.Services.AddSingleton<AuthorizationService>();
```

---

## KULLANIM SENARYOSU

### 1. Ana Menü:
```
?? IS YONETIM SISTEMI

Hos geldiniz! Lutfen bir secim yapin:

[?? Personeller] [?? Mali Durum]
[?? Muhasebe]   [?? Kasa]
```

### 2. Kasa Menü (menu_kasa):
```
?? KASA MENU

Kripto para cuzdan bakiyelerinizi goruntuleyin.

• Ozet: Tum cuzdan bakiyeleri
• Detay: Son 5 islem geçmiþi

[?? KASA OZET]
[?? Litecoin (LTC)] [? Tron (TRX)]
[?? USDT (TRC20)]   [?? USDT (ERC20)]
[?? Ana Menu]
```

### 3. Kasa Özet (kasa_ozet):
```
?? KASA OZET
???????????????????

?? Litecoin (LTC)
   Bakiye: 0.05000000 LTC
   Fiyat: $85.50
   Toplam: $4.28

? Tron (TRX)
   Bakiye: 100.00000000 TRX
   Fiyat: $0.1200
   Toplam: $12.00

?? USDT (TRC20)
   Bakiye: 500.00 USDT
   Toplam: $500.00

?? USDT (ERC20)
   Bakiye: 250.00 USDT
   Toplam: $250.00

???????????????????
?? TOPLAM
   $766.28 USD
   ?32,607.43 TRY

?? Son Guncelleme: 15:30:45

[?? LTC Detay] [? TRX Detay]
[?? USDT TRC20] [?? USDT ERC20]
[?? Yenile]
[?? Kasa Menu]
```

### 4. Litecoin Detay (kasa_ltc):
```
?? Litecoin (LTC)
???????????????????

?? BAKIYE
   0.05000000 LTC
   $4.28 USD
   ?182.05 TRY

?? FIYAT
   $85.5000 USD

?? ADRES
   LhqaeRQJmQ...iTzqQ41

?? SON 5 ISLEM
???????????????????

?? Yatirim
   Tutar: 0.0100 LTC
   Tarih: 01.12.2025 10:30

?? Cekim
   Tutar: 0.0050 LTC
   Tarih: 28.11.2025 14:15

?? Yatirim
   Tutar: 0.0200 LTC
   Tarih: 25.11.2025 09:00

?? Son Guncelleme: 15:30:45

[?? Yenile]
[?? Kasa Ozet]
[?? Ana Menu]
```

---

## API ENDPOINT'LER

### BlockCypher (Litecoin):
```
https://api.blockcypher.com/v1/ltc/main/addrs/{address}/balance
https://api.blockcypher.com/v1/ltc/main/addrs/{address}?limit=5
```

### TronGrid (Tron):
```
https://api.trongrid.io/v1/accounts/{address}
https://api.trongrid.io/v1/accounts/{address}/transactions
```

### CoinGecko (Fiyat):
```
https://api.coingecko.com/api/v3/simple/price?ids=litecoin,tron&vs_currencies=usd
```

### ExchangeRate (USD/TRY):
```
https://api.exchangerate-api.com/v4/latest/USD
```

---

## DOSYA YAPISI

```
IsYonetimiSistemi-TelegramBot-main\
??? IsYonetimiSistemi.TelegramBot\
?   ??? Commands\
?   ?   ??? RaporCommands.cs
?   ?   ??? IslemCommands.cs
?   ?   ??? KasaCommands.cs (YENÝ)
?   ??? Services\
?   ?   ??? BotUpdateHandler.cs (GÜNCELLENDÝ)
?   ?   ??? AuthorizationService.cs
?   ?   ??? TelegramBotHostedService.cs
?   ??? Program.cs (GÜNCELLENDÝ)
```

---

## ÖNEMLÝ NOTLAR

### 1. API Rate Limits:
- BlockCypher: 200 istek/saat (ücretsiz)
- TronGrid: 100 istek/dakika
- CoinGecko: 50 istek/dakika
- ExchangeRate API: Sýnýrsýz

### 2. Hata Yönetimi:
```csharp
try
{
    var balance = await GetLitecoinBalanceAsync();
}
catch (Exception ex)
{
    _logger.LogError(ex, "API hatasi");
    return 0; // Fallback
}
```

### 3. Önbellekleme (Opsiyonel):
```csharp
// Ýleri seviye: Ayný bakiye 1 dakika içinde tekrar sorulursa cache'den dön
private static Dictionary<string, (decimal Balance, DateTime Time)> _cache = new();
```

---

## TEST

### Manuel Test:
```
1. Telegram'da /start komutu gönder
2. "?? Kasa" butonuna týkla
3. "?? KASA OZET" týkla
4. Bakiyeleri kontrol et
5. "?? LTC Detay" týkla
6. Son iþlemleri kontrol et
```

### Log Kontrol:
```bash
# Bot loglarýný kontrol et
tail -f /var/log/telegram-bot.log
```

---

**Hazýrlayan**: AI Assistant  
**Tarih**: 08 Aralýk 2025  
**Versiyon**: 1.4.0  
**Durum**: ? HAZIR - ENTEGRASYON BEKLÝYOR
