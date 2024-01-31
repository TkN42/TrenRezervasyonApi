using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace TrenRezervasyonApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RezervasyonController : Controller
    {
        [HttpPost]
        public IActionResult RezervasyonYap([FromBody] RezervasyonIstegi istek)
        {
            try
            {
                bool rezervasyonYapilabilir = RezervasyonuKontrolEt(istek);

                if (rezervasyonYapilabilir)
                {
                    List<YerlesimAyrinti> yerlesimAyrinti = YerlesimHesapla(istek);
                    return Ok(new { RezervasyonYapilabilir = true, YerlesimAyrinti = yerlesimAyrinti });
                }
                else
                {
                    return Ok(new { RezervasyonYapilabilir = false, YerlesimAyrinti = new List<YerlesimAyrinti>() });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool RezervasyonuKontrolEt(RezervasyonIstegi istek)
        {
            // Burada rezervasyonun yapılıp yapılamayacağına dair kontroller yapılabilir
            // Gerekirse vagon doluluk oranlarına, kişi sayısına ve diğer şartlara göre kontrol eklenebilir.
            // Bu örnekte basit bir algoritma kullanıldı.

            foreach (var vagon in istek.Tren.Vagonlar)
            {
                if( ((vagon.Kapasite * (0.7))-1) >= vagon.DoluKoltukAdet && istek.KisilerFarkliVagonlaraYerlestirilebilir )
                {
                    return true;
                }
                else if (((vagon.Kapasite * (0.7)) - istek.RezervasyonYapilacakKisiSayisi) >= vagon.DoluKoltukAdet && !istek.KisilerFarkliVagonlaraYerlestirilebilir)
                {
                    return true;
                }
            }

            return false;
        }

        private List<YerlesimAyrinti> YerlesimHesapla(RezervasyonIstegi istek)
        {
            // Burada rezervasyon yapılacak vagonları ve kişi sayılarını belirleyen bir algoritma oluşturabilir.
            // Bu örnekte basit bir algoritma kullanıldı.

            List<YerlesimAyrinti> yerlesimAyrinti = new List<YerlesimAyrinti>();
            int kalanKisiSayisi = istek.RezervasyonYapilacakKisiSayisi;
            int vagonSay = istek.Tren.Vagonlar.Count();
            int suankiVagon = 0;

            foreach (var vagon in istek.Tren.Vagonlar)
            {
                suankiVagon++;
                if (kalanKisiSayisi == 0)
                    break;

                double bosKoltukSay = (vagon.Kapasite * (0.7) ) - vagon.DoluKoltukAdet;
                int bosKoltukSayisi = ((int)bosKoltukSay);

                if (bosKoltukSayisi > 0 && (istek.KisilerFarkliVagonlaraYerlestirilebilir || bosKoltukSayisi >= kalanKisiSayisi))
                {
                    int yerlesimKisiSayisi = Math.Min(bosKoltukSayisi, kalanKisiSayisi);
                    yerlesimAyrinti.Add(new YerlesimAyrinti { VagonAdi = vagon.Ad, KisiSayisi = yerlesimKisiSayisi });
                    kalanKisiSayisi -= yerlesimKisiSayisi;
                }
                else if(vagonSay==suankiVagon)
                {
                    int yerlesimKisiSayisi = Math.Min(bosKoltukSayisi, kalanKisiSayisi);
                    yerlesimAyrinti.Add(new YerlesimAyrinti { VagonAdi = "Rezervasyon için yer kalmadı!", KisiSayisi = kalanKisiSayisi });
                    kalanKisiSayisi -= yerlesimKisiSayisi;
                }
            }

            return yerlesimAyrinti;
        }
    }

    public class RezervasyonIstegi
    {
        public Tren Tren { get; set; }
        public int RezervasyonYapilacakKisiSayisi { get; set; }
        public bool KisilerFarkliVagonlaraYerlestirilebilir { get; set; }
    }

    public class Tren
    {
        public string Ad { get; set; }
        public List<Vagon> Vagonlar { get; set; }
    }

    public class Vagon
    {
        public string Ad { get; set; }
        public int Kapasite { get; set; }
        public int DoluKoltukAdet { get; set; }
    }

    public class YerlesimAyrinti
    {
        public string VagonAdi { get; set; }
        public int KisiSayisi { get; set; }
    }
}
