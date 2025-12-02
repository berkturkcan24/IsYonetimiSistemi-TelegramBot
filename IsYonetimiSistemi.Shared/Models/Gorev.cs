using System.ComponentModel.DataAnnotations;

namespace IsYonetimiSistemi.Shared.Models;

public class Gorev
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Baslik { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Aciklama { get; set; } = string.Empty;
    
    public int? PersonelId { get; set; }
    
    [MaxLength(100)]
    public string PersonelAdi { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Durum { get; set; } = "Bekliyor"; // Bekliyor, Devam Ediyor, Tamamlandi
    
    [MaxLength(50)]
    public string Oncelik { get; set; } = "Orta"; // Dusuk, Orta, Yuksek
    
    public DateTime BaslangicTarihi { get; set; } = DateTime.Now;
    
    public DateTime? BitisTarihi { get; set; }
    
    public DateTime? TamamlanmaTarihi { get; set; }
    
    [MaxLength(500)]
    public string Notlar { get; set; } = string.Empty;
    
    public DateTime KayitTarihi { get; set; } = DateTime.Now;
}
