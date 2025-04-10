using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SN_MarketUygulamasi.Models;

namespace SN_MarketUygulamasi.Controllers
{
    public class SatisController : Controller
    {
        private DBRR_AIEntities2 dbContext = new DBRR_AIEntities2();

        // Satış İşlemleri Sayfası
        public ActionResult satisIslemleri()
        {
            return View();
        }

        // Satış Sayfası
        public ActionResult satis()
        {
            return View();
        }

        // Tüm satışları JSON olarak getir
        public JsonResult faturaGecmisi()
        {
            var res = dbContext.SN_M_faturaListesi
                .OrderByDescending(f => f.faturaTarihi)
                .Take(1)
                // En son oluşturulan faturalar üstte olsun
                .Select(f => new
                {
                    f.faturaID,
                    f.faturaTutari,
                    f.faturaTarihi
                }).ToList();

            return Json(res, JsonRequestBehavior.AllowGet);
        }


        // Aktif müşterileri getir
        public JsonResult musteriler()
        {
            var res = dbContext.SN_M_musteriListesi
                .Where(s => s.isActive == true)
                .Select(s => new
                {
                    s.musteriID,
                    s.musteriAdi
                }).ToList();
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        // Aktif çalışanları getir
        public JsonResult calisanlar()
        {
            var res = dbContext.SN_M_calisanListesi
                .Where(s => s.isActive == true && s.calisanID != 7)
                .Select(s => new
                {
                    s.calisanID,
                    s.calisanAdi
                }).ToList();
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        // Aktif ürünleri getir
        public JsonResult urunler()
        {
            var res = dbContext.SN_M_urunListesi
                .Where(s => s.isActive == true)
                .Select(s => new
                {
                    s.urunBarkod,
                    s.urunAdi,
                    s.urunKategoriID,
                    s.birim,
                    s.urunResimURL
                }).ToList();
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        // Ürünün en güncel fiyatı getiriliyor
        public JsonResult urunBilgisi(int urunBarkod)
        {
            var urun = dbContext.SN_M_urunListesi
                .Where(u => u.urunBarkod == urunBarkod)
                .Select(u => new
                {
                    u.urunAdi, 
                    u.urunBarkod,
                    fiyat = dbContext.SN_M_urunFiyatListesi
                        .Where(f => f.urunID == u.urunBarkod && f.isActive == true)
                        .OrderByDescending(f => f.fiyatTarihi)
                        .Select(f => f.urunFiyati)
                        .FirstOrDefault()
                })
                .FirstOrDefault();

            if (urun == null || urun.fiyat <= 0)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest; //  400 hatası döndür
                return Json(new { success = false, message = "Bu barkodlu ürün için aktif fiyat bulunamadı!" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, urunAdi = urun.urunAdi, urunBarkod = urun.urunBarkod, urunFiyati = urun.fiyat }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult faturaOlustur(List<UrunSepet> urunler)
        {
            if (urunler == null || urunler.Count == 0)
            {
                return Json(new { success = false, message = "Fatura oluşturulacak ürün yok!" });
            }

            decimal toplamFiyat = 0m;  //  Decimal olarak başlatıldı
            DateTime islemTarihi = DateTime.Now;
            List<object> stokHatasi = new List<object>();

            // 1- Stok Kontrolü
            foreach (var urun in urunler)
            {
                int toplamGiris = dbContext.SN_M_islemler
                    .Where(i => i.urunID == urun.urunBarkod && i.islemType == 1)
                    .Sum(i => (int?)i.adet) ?? 0;

                int toplamCikis = dbContext.SN_M_islemler
                    .Where(i => i.urunID == urun.urunBarkod && i.islemType == 2)
                    .Sum(i => (int?)i.adet) ?? 0;

                int stokMiktari = toplamGiris - toplamCikis;

                if (stokMiktari < urun.adet)
                {
                    stokHatasi.Add(new { urunID = urun.urunBarkod, stokMiktari, istenenAdet = urun.adet });
                }
            }

            if (stokHatasi.Count > 0)
            {
                return Json(new { success = false, message = "Stok yetersiz!", stokHatasi });
            }

            // 2️- Fatura Oluştur
            var yeniFatura = new SN_M_faturaListesi
            {
                faturaTarihi = islemTarihi,
                faturaTutari = 0m  //  İlk başta sıfır olarak atandı
            };

            dbContext.SN_M_faturaListesi.Add(yeniFatura);
            dbContext.SaveChanges();
            int faturaID = yeniFatura.faturaID;

            // 3️- Satış İşlemlerini Kaydet
            foreach (var urun in urunler)
            {
                var urunBilgi = dbContext.SN_M_urunListesi
                    .Where(u => u.urunBarkod == urun.urunBarkod)
                    .Select(u => new
                    {
                        u.urunAdi,
                        u.urunBarkod,
                        fiyat = dbContext.SN_M_urunFiyatListesi
                            .Where(f => f.urunID == u.urunBarkod && f.isActive == true)
                            .OrderByDescending(f => f.fiyatTarihi)
                            .Select(f => (decimal?)f.urunFiyati) //  `decimal?` olarak cast edildi
                            .FirstOrDefault()
                    })
                    .FirstOrDefault();

                if (urunBilgi == null || urunBilgi.fiyat == null || urunBilgi.fiyat <= 0)
                {
                    return Json(new { success = false, message = $"Ürün {urun.urunBarkod} için fiyat bilgisi eksik!" });
                }

                decimal birimFiyat = urunBilgi.fiyat ?? 0m; //  Null ise 0m atanır
                decimal araToplam = birimFiyat * urun.adet;
                toplamFiyat += araToplam;  //  Toplam fiyat hesaplanıyor

                var satisFiyatiID = dbContext.SN_M_urunFiyatListesi
                    .Where(f => f.urunID == urun.urunBarkod && f.isActive == true)
                    .OrderByDescending(f => f.fiyatTarihi)
                    .Select(f => f.ID)
                    .FirstOrDefault();

                dbContext.SN_M_islemler.Add(new SN_M_islemler
                {
                    calisanID = urun.calisanID,
                    urunID = urun.urunBarkod,
                    musteriID = urun.musteriID,
                    adet = urun.adet,
                    islemDate = islemTarihi,
                    islemType = 2, // **Satış İşlemi**
                    faturaID = faturaID,
                    satisFiyatiID = satisFiyatiID
                });
            }

            // 4- Fatura Toplamını Güncelle
            var guncellenenFatura = dbContext.SN_M_faturaListesi.Find(faturaID);
            if (guncellenenFatura != null)
            {
                guncellenenFatura.faturaTutari = toplamFiyat;  //  Toplam fiyat buraya kaydedildi
                dbContext.Entry(guncellenenFatura).State = EntityState.Modified;
                dbContext.SaveChanges();
            }

            return Json(new { success = true, faturaID, toplamFiyat, faturaTarihi = islemTarihi });
        }



        // Ürün fiyatlarını getir
        //public JsonResult urunFiyatlari(int urunBarkod)
        //{
        //    var fiyat = dbContext.SN_M_urunFiyatListesi
        //        .Where(f => f.urunID == urunBarkod && f.isActive == true)
        //        .OrderByDescending(f => f.fiyatTarihi)
        //        .Select(f => new
        //        {
        //            f.urunFiyati,
        //            f.fiyatTarihi
        //        })
        //        .FirstOrDefault();

        //    if (fiyat == null)
        //    {
        //        return Json(new { success = false, message = "Bu barkodlu ürün için aktif fiyat bulunamadı!" });
        //    }

        //    return Json(new { success = true, urunFiyati = fiyat.urunFiyati, fiyatTarihi = fiyat.fiyatTarihi }, JsonRequestBehavior.AllowGet);
        //}






        //  Ürün Sepeti Modeli
        public class UrunSepet
        {
            public int urunBarkod { get; set; }
            public int adet { get; set; }
            public int musteriID { get; set; }
            public int calisanID { get; set; }
            public decimal fiyat { get; set; }
        }
    }
}
