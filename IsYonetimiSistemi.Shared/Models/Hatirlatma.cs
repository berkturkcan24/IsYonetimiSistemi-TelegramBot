using System.ComponentModel.DataAnnotations;

namespace IsYonetimiSistemi.Shared.Models;

public class Hatirlatma
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Baslik { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Aciklama { get; set; } = string.Empty;
    
    public DateTime HatirlatmaTarihi { get; set; }
    
    public bool Tamamlandi { get; set; } = false;
    
    [MaxLength(50)]
    public string Oncelik { get; set; } = "Normal"; // D???k, Normal, Y?ksek
    
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    
    // Telegram bildirim durumu
    public bool TelegramBildirimGonderildi { get; set; } = false;
}
