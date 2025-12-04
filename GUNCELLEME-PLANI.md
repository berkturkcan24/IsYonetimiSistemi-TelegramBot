# TELEGRAM BOT GUNCELLEME PLANI

## Sorunlar

### 1. Model Uyumsuzlugu
- ? `Islem.cs` modelinde `PaylasimOrani` alani YOK
- ? WPF programi ile uyumsuz

### 2. Performans Hesaplama Hatasi
- ? `RaporCommands.cs` paylasilmis islemleri dogru hesaplamiyor
- ? `islem.Tutar * personel.KomisyonOrani / 100` ? YANLIS!
- ? Paylasim orani kontrolu yok

### 3. Emoji ve Turkce Karakter
- ? Emoji kullaniliyor (?, ?, ?)
- ? Turkce karakterler (þ, ð, ý, ö, ü, ç)
- ? Telegram API ile sorun yasaniyor

## Cozumler

### 1. Islem.cs Guncelle
```csharp
// EKLE:
public decimal? PaylasimOrani { get; set; }
```

### 2. RaporCommands.cs Duzelt
```csharp
// ONCE (YANLIS):
var komisyon = islem.Tutar * personel.KomisyonOrani / 100;

// SONRA (DOGRU):
decimal komisyonOrani;
if (islem.PaylasimOrani.HasValue)
{
    // Paylasilmis islem
    komisyonOrani = islem.PaylasimOrani.Value;
}
else
{
    // Normal islem
    komisyonOrani = personel.KomisyonOrani;
}
var komisyon = islem.Tutar * komisyonOrani / 100;
```

### 3. Emoji ve Turkce Karakterleri Kaldir
```
ONCE: "? Ahmet"
SONRA: "Ahmet"

ONCE: "Komisyon Oraný"
SONRA: "Komisyon Orani"

ONCE: "Ýþlem"
SONRA: "Islem"
```

## Guncellenecek Dosyalar

1. `IsYonetimiSistemi.Shared\Models\Islem.cs`
2. `IsYonetimiSistemi.TelegramBot\Commands\RaporCommands.cs`
3. `IsYonetimiSistemi.TelegramBot\Commands\IslemCommands.cs` (varsa)
4. `IsYonetimiSistemi.Shared\Data\AppDbContext.cs`

## Test

1. Model guncelle
2. Bot'u yeniden derle
3. Personel raporu iste
4. Paylasilmis islem dogru hesaplansin
5. Emoji yok, Turkce karakter yok
