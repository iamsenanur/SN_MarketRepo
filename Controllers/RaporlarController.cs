using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SN_MarketUygulamasi.Logs;
using System.Web.Configuration;
using Newtonsoft.Json;
using System.Threading.Tasks;
using SN_MarketUygulamasi.Models.AbstractClasses;

//ürün fiyatları abstract sınıf yaptık
//using Microsoft.AspNetCore.Mvc;
using SN_MarketUygulamasi.Models;  // Normal modelleri içeri al


namespace SN_MarketUygulamasi.Controllers
{
    public class RaporlarController : Controller

    {
        private DBRR_AIEntities2 dbContext = new DBRR_AIEntities2();
        // GET: Yonetim


        public ActionResult satisRaporlari()
        {
            return View();
        }
         public ActionResult stokRaporlari()
        {
            return View();
        }

        //ürüne göre satış listesini getirme
        public JsonResult GetUrunSatisBilgileri(int urunBarkod)
        {
            var satisBilgileri = (from s in dbContext.SN_M_islemler
                                  join c in dbContext.SN_M_calisanListesi on s.calisanID equals c.calisanID
                                  join m in dbContext.SN_M_musteriListesi on s.musteriID equals m.musteriID
                                  where s.urunID == urunBarkod
                                  orderby s.islemDate descending // Yeni satışları önce göster
                                  select new
                                  {
                                      SatisTarihi = s.islemDate,
                                      CalisanAdi = c.calisanAdi + " " + c.calisanSoyadi,
                                      MusteriAdi = m.musteriAdi + " " + m.musteriSoyadi,
                                      Adet = s.adet
                                  }).ToList();

            

            return Json(satisBilgileri, JsonRequestBehavior.AllowGet);
        }

        //Ürün Listesi, Ürün Fiyat Listesi ve Ürün Kategori Listesi tablolarını joinledik ( Seçilen ürüne ait bilgiler getirildi)
        public JsonResult GetUrunVeFiyatListesi()
        {
            var urunVeFiyatListesi = dbContext.SN_M_urunListesi
                .Where(u => u.isActive)
                .GroupJoin(
                    dbContext.SN_M_urunFiyatListesi,
                    u => u.urunBarkod,
                    f => f.urunID,
                    (u, urunFiyatJoin) => new { u, urunFiyatJoin }
                )
                .SelectMany(
                    x => x.urunFiyatJoin.DefaultIfEmpty(),
                    (x, urunFiyat) => new { x.u, urunFiyat }
                )
                .Join(
                    dbContext.SN_M_kategoriListesi, //  Kategori tablosunu joinle
                    urun => urun.u.urunKategoriID,
                    kategori => kategori.kategoriID,
                    (urun, kategori) => new
                    {
                        urun.u.urunBarkod,
                        urun.u.urunAdi,
                        urun.u.birim,
                        urun.u.urunKategoriID,
                        urun.u.urunResimURL,
                        KategoriAciklamasi = kategori.kategoriAciklamasi,
                        KategoriIcon = kategori.kategoriIcon, // Kategori ikonunu ekledik
                        UrunFiyat = urun.urunFiyat != null ? urun.urunFiyat.urunFiyati : (decimal?)null,
                        FiyatTarihi = urun.urunFiyat != null ? urun.urunFiyat.fiyatTarihi : (DateTime?)null
                    }
                )
                .AsEnumerable()
                .Select(x => new
                {
                    x.urunBarkod,
                    x.urunAdi,
                    x.birim,
                    x.urunKategoriID,
                    x.urunResimURL,
                    x.KategoriAciklamasi,
                    x.KategoriIcon, //Kategori ikonunu frontend'e gönderiyoruz
                    x.UrunFiyat,
                    FiyatTarihi = x.FiyatTarihi?.ToString("o") // ISO formatında tarih
                })
                .ToList();

            return Json(urunVeFiyatListesi, JsonRequestBehavior.AllowGet);
        }



        public JsonResult UrunStokMiktari()
        {
            try
            {
                int giren = dbContext.SN_M_islemler
                                     .Where(f => f.islemType == 1)
                                      .Sum(f => (int?)f.adet) ?? 0;

                int cikan = dbContext.SN_M_islemler
                                     .Where(f => f.islemType == 2)
                                     .Sum(f => (int?)f.adet) ?? 0;

                int stok = giren - cikan;

                return Json(new { giren, cikan, stok });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Stok bilgisi alınırken hata oluştu.", detay = ex.Message });
            }
        }


    


     


    }
}