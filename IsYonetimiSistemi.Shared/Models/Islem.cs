using System.ComponentModel.DataAnnotations;

namespace IsYonetimiSistemi.Shared.Models;

public class Islem
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Aciklama { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Tip { get; set; } = string.Empty;
    
    [Required]
    public decimal Tutar { get; set; }
    
    [MaxLength(50)]
    public string Kategori { get; set; } = string.Empty;
    
    public int? PersonelId { get; set; }
    
    [MaxLength(100)]
    public string PersonelAdi { get; set; } = string.Empty;
    
    public DateTime IslemTarihi { get; set; } = DateTime.Now;
    
    [MaxLength(500)]
    public string Notlar { get; set; } = string.Empty;
    
    [MaxLength(10)]
    public string ParaBirimi { get; set; } = "TL";
    
    public decimal OrijinalTutar { get; set; }
    
    public decimal? KullanilmisKur { get; set; }
    
    [MaxLength(20)]
    public string Kaynak { get; set; } = "Muhasebe";`n`n    // Paylasim orani (paylasilmis islemler icin)`n    public decimal? PaylasimOrani { get; set; }
    
    // Yetkili kullanici bilgileri
    public int? EkleyenKullaniciId { get; set; }
    
    [MaxLength(100)]
    public string EkleyenKullaniciAdi { get; set; } = string.Empty;
    
    public DateTime EklenmeTarihi { get; set; } = DateTime.Now;

    // D?zenleme bilgileri
    public int? GuncelleyenKullaniciId { get; set; }

    [MaxLength(100)]
    public string GuncelleyenKullaniciAdi { get; set; } = string.Empty;

    public DateTime? GuncellenmeTarihi { get; set; }
}

