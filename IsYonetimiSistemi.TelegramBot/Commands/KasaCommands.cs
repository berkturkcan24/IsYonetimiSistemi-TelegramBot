using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace IsYonetimiSistemi.TelegramBot.Commands;

public class KasaCommands
{
    private readonly ILogger<KasaCommands> _logger;
    private static readonly HttpClient _httpClient = new HttpClient();
    
    // Cuzdan adresleri
    private const string LTC_ADDRESS = "LhqaeRQJmQjoEfjkrXyXQgMaU3LiTzqQ41";
    private const string TRON_ADDRESS = "TVhcDC9LmmLW4ddxGwutjhGn8m8D6xLuqs";
    private const string TRON_ADDRESS_HEX = "41d86f3bfec93eca6368b9fa36a465253fa25f2e1f"; // Hex format
    private const string USDT_TRC20_ADDRESS = "TVhcDC9LmmLW4ddxGwutjhGn8m8D6xLuqs";
    private const string USDT_ERC20_ADDRESS = "0xDD2B447A95c348CfaF97C76e35DE4F8617a961a6";
    
    // API Endpoints
    private const string LITECOIN_API = "https://api.blockcypher.com/v1/ltc/main/addrs/";
    private const string TRON_API = "https://api.trongrid.io/v1/accounts/";
    private const string ETHEREUM_API = "https://api.etherscan.io/api";
    private const string COINGECKO_API = "https://api.coingecko.com/api/v3/simple/price";

    public KasaCommands(ILogger<KasaCommands> logger)
    {
        _logger = logger;
    }

    public async Task ShowKasaMenu(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("?? KASA OZET", "kasa_ozet") },
            new[] 
            { 
                InlineKeyboardButton.WithCallbackData("?? Litecoin (LTC)", "kasa_ltc"),
                InlineKeyboardButton.WithCallbackData("? Tron (TRX)", "kasa_trx")
            },
            new[] 
            { 
                InlineKeyboardButton.WithCallbackData("?? USDT (TRC20)", "kasa_usdt_trc20"),
                InlineKeyboardButton.WithCallbackData("?? USDT (ERC20)", "kasa_usdt_erc20")
            },
            new[] { InlineKeyboardButton.WithCallbackData("?? Ana Menu", "main_menu") }
        });

        var text = "?? KASA MENU\n\n" +
                   "Kripto para cuzdan bakiyelerinizi goruntuleyin.\n\n" +
                   "• Ozet: Tum cuzdan bakiyeleri\n" +
                   "• Detay: Son 5 islem geçmiþi";

        await botClient.EditMessageTextAsync(
            chatId, 
            messageId, 
            text, 
            replyMarkup: keyboard, 
            cancellationToken: cancellationToken);
    }

    public async Task ShowKasaOzet(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        var loadingKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("?? Kasa Menu", "menu_kasa") }
        });

        await botClient.EditMessageTextAsync(
            chatId, 
            messageId, 
            "? Bakiyeler yükleniyor...", 
            replyMarkup: loadingKeyboard, 
            cancellationToken: cancellationToken);

        try
        {
            // Bakiyeleri cek
            var ltcBalance = await GetLitecoinBalanceAsync();
            var ltcPrice = await GetCryptoPriceAsync("litecoin");
            
            var trxBalance = await GetTronBalanceAsync();
            var trxPrice = await GetCryptoPriceAsync("tron");
            
            var usdtTrc20Balance = await GetTronUSDTBalanceAsync();
            var usdtErc20Balance = await GetEthereumUSDTBalanceAsync();
            
            // Hesaplamalar
            var ltcUsd = ltcBalance * ltcPrice;
            var trxUsd = trxBalance * trxPrice;
            var usdtTrc20Usd = usdtTrc20Balance;
            var usdtErc20Usd = usdtErc20Balance;
            var toplamUsd = ltcUsd + trxUsd + usdtTrc20Usd + usdtErc20Usd;
            
            // TRY kuru
            var usdTry = await GetUsdTryKuruAsync();
            var toplamTry = toplamUsd * usdTry;

            var text = "?? KASA OZET\n" +
                       "???????????????????\n\n" +
                       $"?? Litecoin (LTC)\n" +
                       $"   Bakiye: {ltcBalance:F8} LTC\n" +
                       $"   Fiyat: ${ltcPrice:F2}\n" +
                       $"   Toplam: ${ltcUsd:F2}\n\n" +
                       $"? Tron (TRX)\n" +
                       $"   Bakiye: {trxBalance:F8} TRX\n" +
                       $"   Fiyat: ${trxPrice:F4}\n" +
                       $"   Toplam: ${trxUsd:F2}\n\n" +
                       $"?? USDT (TRC20)\n" +
                       $"   Bakiye: {usdtTrc20Balance:F2} USDT\n" +
                       $"   Toplam: ${usdtTrc20Usd:F2}\n\n" +
                       $"?? USDT (ERC20)\n" +
                       $"   Bakiye: {usdtErc20Balance:F2} USDT\n" +
                       $"   Toplam: ${usdtErc20Usd:F2}\n\n" +
                       "???????????????????\n" +
                       $"?? TOPLAM\n" +
                       $"   ${toplamUsd:F2} USD\n" +
                       $"   ?{toplamTry:F2} TRY\n\n" +
                       $"?? Son Guncelleme: {DateTime.Now:HH:mm:ss}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] 
                { 
                    InlineKeyboardButton.WithCallbackData("?? LTC Detay", "kasa_ltc"),
                    InlineKeyboardButton.WithCallbackData("? TRX Detay", "kasa_trx")
                },
                new[] 
                { 
                    InlineKeyboardButton.WithCallbackData("?? USDT TRC20", "kasa_usdt_trc20"),
                    InlineKeyboardButton.WithCallbackData("?? USDT ERC20", "kasa_usdt_erc20")
                },
                new[] { InlineKeyboardButton.WithCallbackData("?? Yenile", "kasa_ozet") },
                new[] { InlineKeyboardButton.WithCallbackData("?? Kasa Menu", "menu_kasa") }
            });

            await botClient.EditMessageTextAsync(
                chatId, 
                messageId, 
                text, 
                replyMarkup: keyboard, 
                cancellationToken: cancellationToken);

            _logger.LogInformation("Kasa ozet gosterildi. Toplam: ${ToplamUsd:F2}", toplamUsd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kasa ozet hatasi");
            
            var errorText = "? Bakiyeler yuklenirken hata olustu.\n\n" +
                           "Lutfen daha sonra tekrar deneyin.";
            
            await botClient.EditMessageTextAsync(
                chatId, 
                messageId, 
                errorText, 
                replyMarkup: loadingKeyboard, 
                cancellationToken: cancellationToken);
        }
    }

    public async Task ShowLitecoinDetay(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        await ShowCryptoDetay(botClient, chatId, messageId, "Litecoin", "LTC", LTC_ADDRESS, "kasa_ltc", cancellationToken);
    }

    public async Task ShowTronDetay(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        await ShowCryptoDetay(botClient, chatId, messageId, "Tron", "TRX", TRON_ADDRESS, "kasa_trx", cancellationToken);
    }

    public async Task ShowUsdtTrc20Detay(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        await ShowCryptoDetay(botClient, chatId, messageId, "USDT", "USDT-TRC20", USDT_TRC20_ADDRESS, "kasa_usdt_trc20", cancellationToken);
    }

    public async Task ShowUsdtErc20Detay(ITelegramBotClient botClient, long chatId, int messageId, CancellationToken cancellationToken)
    {
        await ShowCryptoDetay(botClient, chatId, messageId, "USDT", "USDT-ERC20", USDT_ERC20_ADDRESS, "kasa_usdt_erc20", cancellationToken);
    }

    private async Task ShowCryptoDetay(
        ITelegramBotClient botClient, 
        long chatId, 
        int messageId, 
        string cryptoName, 
        string symbol, 
        string address, 
        string callbackData,
        CancellationToken cancellationToken)
    {
        var loadingKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("?? Kasa Ozet", "kasa_ozet") }
        });

        await botClient.EditMessageTextAsync(
            chatId, 
            messageId, 
            $"? {cryptoName} bilgileri yukleniyor...", 
            replyMarkup: loadingKeyboard, 
            cancellationToken: cancellationToken);

        try
        {
            decimal balance = 0;
            decimal price = 0;
            List<Transaction> transactions = new();

            // Bakiye ve islemler
            if (symbol == "LTC")
            {
                balance = await GetLitecoinBalanceAsync();
                price = await GetCryptoPriceAsync("litecoin");
                transactions = await GetLitecoinTransactionsAsync();
            }
            else if (symbol == "TRX")
            {
                balance = await GetTronBalanceAsync();
                price = await GetCryptoPriceAsync("tron");
                transactions = await GetTronTransactionsAsync();
            }
            else if (symbol == "USDT-TRC20")
            {
                balance = await GetTronUSDTBalanceAsync();
                price = 1.0m;
                transactions = await GetTronUSDTTransactionsAsync();
            }
            else if (symbol == "USDT-ERC20")
            {
                balance = await GetEthereumUSDTBalanceAsync();
                price = 1.0m;
                transactions = await GetEthereumUSDTTransactionsAsync();
            }

            var totalUsd = balance * price;
            var usdTry = await GetUsdTryKuruAsync();
            var totalTry = totalUsd * usdTry;

            var text = $"?? {cryptoName} ({symbol})\n" +
                       "???????????????????\n\n" +
                       $"?? BAKIYE\n" +
                       $"   {balance:F8} {symbol.Replace("-TRC20", "").Replace("-ERC20", "")}\n" +
                       $"   ${totalUsd:F2} USD\n" +
                       $"   ?{totalTry:F2} TRY\n\n" +
                       $"?? FIYAT\n" +
                       $"   ${price:F4} USD\n\n" +
                       $"?? ADRES\n" +
                       $"   {address.Substring(0, 10)}...{address.Substring(address.Length - 10)}\n\n";

            if (transactions.Any())
            {
                text += "?? SON 5 ISLEM\n" +
                        "???????????????????\n\n";

                foreach (var tx in transactions.Take(5))
                {
                    var icon = tx.Type == "Yatirim" ? "??" : "??";
                    text += $"{icon} {tx.Type}\n" +
                           $"   Tutar: {tx.Amount:F4} {symbol.Replace("-TRC20", "").Replace("-ERC20", "")}\n" +
                           $"   Tarih: {tx.Date:dd.MM.yyyy HH:mm}\n\n";
                }
            }
            else
            {
                text += "?? SON 5 ISLEM\n" +
                        "???????????????????\n\n" +
                        "Henuz islem yok.\n\n";
            }

            text += $"?? Son Guncelleme: {DateTime.Now:HH:mm:ss}";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("?? Yenile", callbackData) },
                new[] { InlineKeyboardButton.WithCallbackData("?? Kasa Ozet", "kasa_ozet") },
                new[] { InlineKeyboardButton.WithCallbackData("?? Ana Menu", "main_menu") }
            });

            await botClient.EditMessageTextAsync(
                chatId, 
                messageId, 
                text, 
                replyMarkup: keyboard, 
                cancellationToken: cancellationToken);

            _logger.LogInformation("{CryptoName} detay gosterildi. Bakiye: {Balance}", cryptoName, balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{CryptoName} detay hatasi", cryptoName);
            
            var errorText = $"? {cryptoName} bilgileri yuklenirken hata olustu.\n\n" +
                           "Lutfen daha sonra tekrar deneyin.";
            
            await botClient.EditMessageTextAsync(
                chatId, 
                messageId, 
                errorText, 
                replyMarkup: loadingKeyboard, 
                cancellationToken: cancellationToken);
        }
    }

    // API Metodlari
    private async Task<decimal> GetLitecoinBalanceAsync()
    {
        try
        {
            var url = $"{LITECOIN_API}{LTC_ADDRESS}/balance";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            if (json.RootElement.TryGetProperty("balance", out var balance))
            {
                var satoshi = balance.GetInt64();
                return satoshi / 100_000_000m;
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Litecoin bakiye hatasi");
            return 0;
        }
    }

    private async Task<decimal> GetTronBalanceAsync()
    {
        try
        {
            var url = $"{TRON_API}{TRON_ADDRESS}";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            if (json.RootElement.TryGetProperty("data", out var data) && data.GetArrayLength() > 0)
            {
                var account = data[0];
                if (account.TryGetProperty("balance", out var balance))
                {
                    var sun = balance.GetInt64();
                    return sun / 1_000_000m;
                }
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tron bakiye hatasi");
            return 0;
        }
    }

    private async Task<decimal> GetTronUSDTBalanceAsync()
    {
        try
        {
            // Basit bakiye kontrolu - gercek implementasyon daha karmasik
            return 0; // TODO: TronGrid API ile USDT bakiyesi
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tron USDT bakiye hatasi");
            return 0;
        }
    }

    private async Task<decimal> GetEthereumUSDTBalanceAsync()
    {
        try
        {
            // Basit bakiye kontrolu - gercek implementasyon daha karmasik
            return 0; // TODO: Etherscan API ile USDT bakiyesi
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ethereum USDT bakiye hatasi");
            return 0;
        }
    }

    private async Task<decimal> GetCryptoPriceAsync(string crypto)
    {
        try
        {
            var url = $"{COINGECKO_API}?ids={crypto}&vs_currencies=usd";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            if (json.RootElement.TryGetProperty(crypto, out var cryptoData))
            {
                if (cryptoData.TryGetProperty("usd", out var price))
                {
                    return price.GetDecimal();
                }
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fiyat hatasi: {Crypto}", crypto);
            return 0;
        }
    }

    private async Task<decimal> GetUsdTryKuruAsync()
    {
        try
        {
            var url = "https://api.exchangerate-api.com/v4/latest/USD";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            if (json.RootElement.TryGetProperty("rates", out var rates))
            {
                if (rates.TryGetProperty("TRY", out var tryRate))
                {
                    return tryRate.GetDecimal();
                }
            }
            return 42.56m; // Fallback
        }
        catch
        {
            return 42.56m; // Fallback
        }
    }

    private async Task<List<Transaction>> GetLitecoinTransactionsAsync()
    {
        try
        {
            var url = $"{LITECOIN_API}{LTC_ADDRESS}?limit=5";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            var transactions = new List<Transaction>();
            
            if (json.RootElement.TryGetProperty("txrefs", out var txrefs))
            {
                foreach (var tx in txrefs.EnumerateArray().Take(5))
                {
                    var value = tx.TryGetProperty("value", out var val) ? val.GetInt64() / 100_000_000m : 0;
                    var date = tx.TryGetProperty("confirmed", out var conf) 
                        ? DateTime.Parse(conf.GetString() ?? DateTime.Now.ToString()) 
                        : DateTime.Now;
                    var txType = tx.TryGetProperty("tx_input_n", out var input) && input.GetInt32() >= 0 
                        ? "Cekim" 
                        : "Yatirim";
                    
                    transactions.Add(new Transaction
                    {
                        Amount = value,
                        Date = date,
                        Type = txType
                    });
                }
            }
            
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Litecoin islem gecmisi hatasi");
            return new List<Transaction>();
        }
    }

    private async Task<List<Transaction>> GetTronTransactionsAsync()
    {
        try
        {
            var url = $"https://api.trongrid.io/v1/accounts/{TRON_ADDRESS}/transactions?limit=20";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            var transactions = new List<Transaction>();
            
            if (json.RootElement.TryGetProperty("data", out var data))
            {
                foreach (var tx in data.EnumerateArray().Take(20))
                {
                    // Transaction type ve value kontrolu
                    if (!tx.TryGetProperty("raw_data", out var rawData)) continue;
                    if (!rawData.TryGetProperty("contract", out var contracts)) continue;
                    
                    var contractArray = contracts.EnumerateArray().ToList();
                    if (contractArray.Count == 0) continue;
                    
                    var contract = contractArray[0];
                    if (!contract.TryGetProperty("parameter", out var parameter)) continue;
                    if (!parameter.TryGetProperty("value", out var value)) continue;
                    
                    // Transfer amount (sun to TRX)
                    var amount = value.TryGetProperty("amount", out var amountProp) 
                        ? amountProp.GetInt64() / 1_000_000m 
                        : 0;
                    
                    if (amount <= 0) continue;
                    
                    // Timestamp
                    var timestamp = tx.TryGetProperty("block_timestamp", out var ts) 
                        ? DateTimeOffset.FromUnixTimeMilliseconds(ts.GetInt64()).DateTime 
                        : DateTime.Now;
                    
                    // Transfer direction (owner_address vs to_address)
                    var ownerAddress = value.TryGetProperty("owner_address", out var owner) 
                        ? owner.GetString() ?? "" 
                        : "";
                    var toAddress = value.TryGetProperty("to_address", out var to) 
                        ? to.GetString() ?? "" 
                        : "";
                    
                    // Hex address comparison
                    var txType = toAddress.Equals(TRON_ADDRESS_HEX, StringComparison.OrdinalIgnoreCase) 
                        ? "Yatirim"  // Bize gelen (to_address = bizim adres)
                        : "Cekim";   // Bizden giden (owner_address = bizim adres)
                    
                    transactions.Add(new Transaction
                    {
                        Amount = amount,
                        Date = timestamp,
                        Type = txType
                    });
                    
                    if (transactions.Count >= 5) break;
                }
            }
            
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tron islem gecmisi hatasi");
            return new List<Transaction>();
        }
    }

    private async Task<List<Transaction>> GetTronUSDTTransactionsAsync()
    {
        try
        {
            // USDT TRC20 contract address
            var usdtContract = "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t";
            var url = $"https://api.trongrid.io/v1/accounts/{USDT_TRC20_ADDRESS}/transactions/trc20?limit=20&contract_address={usdtContract}";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            var transactions = new List<Transaction>();
            
            if (json.RootElement.TryGetProperty("data", out var data))
            {
                foreach (var tx in data.EnumerateArray().Take(20))
                {
                    // Value in smallest unit (6 decimals for USDT)
                    var value = tx.TryGetProperty("value", out var val) 
                        ? decimal.Parse(val.GetString() ?? "0") / 1_000_000m 
                        : 0;
                    
                    if (value <= 0) continue;
                    
                    // Timestamp
                    var timestamp = tx.TryGetProperty("block_timestamp", out var ts) 
                        ? DateTimeOffset.FromUnixTimeMilliseconds(ts.GetInt64()).DateTime 
                        : DateTime.Now;
                    
                    // Transfer direction
                    var from = tx.TryGetProperty("from", out var fromAddr) 
                        ? fromAddr.GetString() ?? "" 
                        : "";
                    var to = tx.TryGetProperty("to", out var toAddr) 
                        ? toAddr.GetString() ?? "" 
                        : "";
                    
                    // TRC20 uses base58 addresses, check both formats
                    var txType = to.Equals(USDT_TRC20_ADDRESS, StringComparison.OrdinalIgnoreCase) ||
                                 to.Equals(TRON_ADDRESS_HEX, StringComparison.OrdinalIgnoreCase)
                        ? "Yatirim" 
                        : "Cekim";
                    
                    transactions.Add(new Transaction
                    {
                        Amount = value,
                        Date = timestamp,
                        Type = txType
                    });
                    
                    if (transactions.Count >= 5) break;
                }
            }
            
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tron USDT islem gecmisi hatasi");
            return new List<Transaction>();
        }
    }

    private async Task<List<Transaction>> GetEthereumUSDTTransactionsAsync()
    {
        try
        {
            // USDT ERC20 contract address
            var usdtContract = "0xdac17f958d2ee523a2206206994597c13d831ec7";
            
            // Etherscan API - using public endpoint (rate limited)
            var url = $"{ETHEREUM_API}?module=account&action=tokentx&contractaddress={usdtContract}&address={USDT_ERC20_ADDRESS}&sort=desc&page=1&offset=20";
            
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            var transactions = new List<Transaction>();
            
            if (json.RootElement.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Array)
            {
                foreach (var tx in result.EnumerateArray().Take(20))
                {
                    // Value in smallest unit (6 decimals for USDT)
                    var value = tx.TryGetProperty("value", out var val) 
                        ? decimal.Parse(val.GetString() ?? "0") / 1_000_000m 
                        : 0;
                    
                    if (value <= 0) continue;
                    
                    // Timestamp
                    var timestamp = tx.TryGetProperty("timeStamp", out var ts) 
                        ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(ts.GetString() ?? "0")).DateTime 
                        : DateTime.Now;
                    
                    // Transfer direction
                    var from = tx.TryGetProperty("from", out var fromAddr) 
                        ? fromAddr.GetString() ?? "" 
                        : "";
                    var to = tx.TryGetProperty("to", out var toAddr) 
                        ? toAddr.GetString() ?? "" 
                        : "";
                    
                    var txType = to.Equals(USDT_ERC20_ADDRESS, StringComparison.OrdinalIgnoreCase) 
                        ? "Yatirim" 
                        : "Cekim";
                    
                    transactions.Add(new Transaction
                    {
                        Amount = value,
                        Date = timestamp,
                        Type = txType
                    });
                    
                    if (transactions.Count >= 5) break;
                }
            }
            
            return transactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ethereum USDT islem gecmisi hatasi");
            return new List<Transaction>();
        }
    }
}

public class Transaction
{
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = ""; // "Yatirim" veya "Cekim"
}
