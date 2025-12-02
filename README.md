# Ýþ Yönetimi Sistemi - Telegram Bot

Telegram bot for business management system with authorization.

## Features
- ? Chat ID based authorization
- ?? Reports and analytics
- ?? Personnel management
- ?? Task tracking

## Configuration

### appsettings.json
```json
{
  "TelegramBot": {
    "AuthorizedChatIds": "YOUR_CHAT_ID_1,YOUR_CHAT_ID_2"
  }
}
```

### Railway Environment Variables
```
TELEGRAM_BOT_TOKEN=your_bot_token
DB_CONNECTION_STRING=your_connection_string
TelegramBot__AuthorizedChatIds=YOUR_CHAT_ID_1,YOUR_CHAT_ID_2
```

## How to Get Chat ID
1. Message your bot on Telegram
2. Check bot logs for your Chat ID
3. Add your Chat ID to configuration

## Deployment
- Push to GitHub
- Railway will auto-deploy
- Set environment variables in Railway dashboard
