# Chat ID Bulma Rehberi

## Yöntem 1: Uygulama Loglarýndan
1. Uygulamayý durdurun (Ctrl+C)
2. Uygulamayý tekrar baþlatýn: `dotnet run`
3. Telegram'dan botunuza `/start` yazýn
4. Console'da þu satýrý arayýn:
```
Message: /start from XXXXXXXX
```
veya
```
?? Yetkisiz eriþim denemesi - Chat ID: XXXXXXXX
```

## Yöntem 2: Bot API ile
1. Telegram'dan botunuza herhangi bir mesaj gönderin
2. Tarayýcýda þu URL'ye gidin:
```
https://api.telegram.org/bot<BOT_TOKEN>/getUpdates
```
3. JSON response'da `"from": {"id": 123456789}` alanýný bulun

## Yöntem 3: @userinfobot kullanýn
1. Telegram'da @userinfobot botunu bulun
2. Bota `/start` yazýn
3. Size Chat ID'nizi gösterecek

## Chat ID'yi appsettings.json'a Ekleyin
```json
{
  "TelegramBot": {
    "AuthorizedChatIds": "SÝZÝN_CHAT_ID,5987654321,1234567890"
  }
}
```

**ÖNEMLÝ:** Virgülle ayýrýn, boþluk yok!
