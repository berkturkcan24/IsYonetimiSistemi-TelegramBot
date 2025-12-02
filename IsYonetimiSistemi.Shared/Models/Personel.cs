using System.ComponentModel.DataAnnotations;

namespace IsYonetimiSistemi.Shared.Models;

public class Personel
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string AdSoyad { get; set; } = string.Empty;
    
    public decimal KomisyonOrani { get; set; }
    
    public DateTime IseBaslamaTarihi { get; set; }
    
    public bool Aktif { get; set; } = true;
    
    public DateTime KayitTarihi { get; set; } = DateTime.Now;
}
