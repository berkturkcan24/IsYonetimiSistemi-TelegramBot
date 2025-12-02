using System.ComponentModel.DataAnnotations;

namespace IsYonetimiSistemi.Shared.Models;

public class Kullanici
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string KullaniciAdi { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string SifreHash { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string AdSoyad { get; set; } = string.Empty;
    
    public DateTime KayitTarihi { get; set; } = DateTime.Now;
    
    public bool Aktif { get; set; } = true;
    
    // Telegram bot için
    public long? TelegramChatId { get; set; }
    public bool TelegramBildirimleri { get; set; } = true;
}
