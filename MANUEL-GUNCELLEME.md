# TELEGRAM BOT DOSYA GUNCELLEME REHBERI

## Manuel Guncellemeler

### 1. IsYonetimiSistemi.Shared\Models\Islem.cs

**DURUM:** ? GUNCELLENDI

`PaylasimOrani` alani eklendi.

---

### 2. RaporCommands.cs - Emoji ve Turkce Karakter Temizleme

**DURUM:** ? GUNCELLENDI

Tum emoji ve Turkce karakterler temizlendi.

---

### 3. RaporCommands.cs - Performans Hesaplama Duzeltme

**BULMACA:**
```csharp
var komisyon = islem.Tutar * personel.KomisyonOrani / 100;
```

**DEGISTIR:**
```csharp
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
```

**KONUM:**
- Dosya: `IsYonetimiSistemi.TelegramBot\Commands\RaporCommands.cs`
- Satir: ~100-120 (tam bul)
- Fonksiyon: `ShowPersonelPerformans` icerisinde

---

### 4. AppDbContext.cs - Precision Ekle

**EKLE:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    modelBuilder.Entity<Islem>()
        .Property(i => i.PaylasimOrani)
        .HasPrecision(5, 2);
}
```

**KONUM:**
- Dosya: `IsYonetimiSistemi.Shared\Data\AppDbContext.cs`

---

## Visual Studio ile Manuel Duzeltme

1. Visual Studio'da telegram bot projesini ac
2. `RaporCommands.cs` dosyasini ac
3. CTRL+F ile ara: `var komisyon = islem.Tutar * personel.KomisyonOrani / 100;`
4. Yukardaki kodu degistir
5. Dosyayi kaydet
6. Build yap
7. Test et

---

## Kontrol Listesi

- [x] Islem.cs - PaylasimOrani eklendi
- [x] RaporCommands.cs - Emoji temizlendi
- [x] RaporCommands.cs - Turkce karakter temizlendi
- [ ] RaporCommands.cs - Performans hesaplama (MANUEL)
- [ ] AppDbContext.cs - Precision ekle (MANUEL)

---

## Test

```
1. Bot'u baslat
2. /personel_rapor komutunu calistir
3. Paylasilmis islem olan personel sec
4. Komisyon dogru hesaplansin
5. Emoji yok
6. Turkce karakter yok
```
