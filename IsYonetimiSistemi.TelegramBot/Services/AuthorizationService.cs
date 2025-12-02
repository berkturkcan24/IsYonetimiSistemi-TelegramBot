using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IsYonetimiSistemi.TelegramBot.Services;

public class AuthorizationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthorizationService> _logger;
    private readonly HashSet<long> _authorizedChatIds;

    public AuthorizationService(IConfiguration configuration, ILogger<AuthorizationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _authorizedChatIds = new HashSet<long>();

        LoadAuthorizedChatIds();
    }

    private void LoadAuthorizedChatIds()
    {
        var chatIdsString = _configuration["TelegramBot:AuthorizedChatIds"];
        
        _logger.LogWarning($"?? CONFIG OKUNAN DEGER: '{chatIdsString}'");
        
        if (string.IsNullOrWhiteSpace(chatIdsString))
        {
            _logger.LogError("??? YETKÝLÝ CHAT ID LÝSTESÝ BOÞ - HÝÇBÝR KULLANICI ERÝÞEMEZ! ???");
            return;
        }

        var chatIds = chatIdsString.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var chatId in chatIds)
        {
            if (long.TryParse(chatId.Trim(), out var id))
            {
                _authorizedChatIds.Add(id);
                _logger.LogWarning($"? Yetkili chat ID eklendi: {id}");
            }
            else
            {
                _logger.LogError($"? Geçersiz chat ID formatý: {chatId}");
            }
        }

        _logger.LogWarning($"?? TOPLAM YETKÝLÝ KULLANICI: {_authorizedChatIds.Count}");
        _logger.LogWarning($"?? YETKÝLÝ LISTESI: [{string.Join(", ", _authorizedChatIds)}]");
    }

    public bool IsAuthorized(long chatId)
    {
        var isAuthorized = _authorizedChatIds.Contains(chatId);
        
        if (isAuthorized)
        {
            _logger.LogWarning($"? YETKÝLÝ ERÝÞÝM - Chat ID: {chatId}");
        }
        else
        {
            _logger.LogError($"??? YETKÝSÝZ ERÝÞÝM ENGELLENDÝ - Chat ID: {chatId} ???");
            _logger.LogError($"? Yetkili liste: [{string.Join(", ", _authorizedChatIds)}]");
        }

        return isAuthorized;
    }

    public int GetAuthorizedUserCount()
    {
        return _authorizedChatIds.Count;
    }

    public IReadOnlySet<long> GetAuthorizedChatIds()
    {
        return _authorizedChatIds;
    }
}
