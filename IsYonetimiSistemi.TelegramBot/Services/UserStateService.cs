using System.Collections.Concurrent;

namespace IsYonetimiSistemi.TelegramBot.Services;

public class UserStateService
{
    private readonly ConcurrentDictionary<long, UserState> _userStates = new();

    public UserState? GetState(long chatId)
    {
        _userStates.TryGetValue(chatId, out var state);
        return state;
    }

    public void SetState(long chatId, string action, Dictionary<string, object>? data = null)
    {
        _userStates[chatId] = new UserState
        {
            Action = action,
            Data = data ?? new Dictionary<string, object>(),
            CreatedAt = DateTime.Now
        };
    }

    public void ClearState(long chatId)
    {
        _userStates.TryRemove(chatId, out _);
    }

    public void UpdateStateData(long chatId, string key, object value)
    {
        if (_userStates.TryGetValue(chatId, out var state))
        {
            state.Data[key] = value;
        }
    }
}

public class UserState
{
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
