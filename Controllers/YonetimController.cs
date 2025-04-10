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
    public class YonetimController : Controller

    {
        private DBRR_AIEntities2 dbContext = new DBRR_AIEntities2();
        // GET: Yonetim
        public ActionResult calisanYonetimi()
        {
            return View();
        }
        public ActionResult musteriYonetimi()
        {
            return View();
        }
        public JsonResult musteriler()
        {
            var res = dbContext.SN_M_musteriListesi.Select(s => new
            {
                s.musteriID,
                s.musteriAdi,
                s.musteriSoyadi,
                s.isActive
            }).Where(s => s.isActive == true).ToList();
            return Json(res);
        }


        public JsonResult calisanlar()
        {            
            var res = dbContext.SN_M_calisanListesi.Select(s => new {
                s.calisanID,
                s.calisanAdi,
                s.calisanSoyadi,
                s.isActive
            }).Where(s => s.isActive == true && s.calisanID != 7).ToList(); // 7 numaralı id Sistem kullanıcısına ait

            return Json(res, JsonRequestBehavior.AllowGet);

        }


        //stok ekle/çıkart start
    
        public JsonResult StokEkle(int calisanID, int urunID, int islemType, int urunAdet)
        {
            try
            {
                if (calisanID <= 0)
                    return Json(new { success = false, message = "Lütfen bir çalışan seçiniz!" });

                if (urunID <= 0)
                    return Json(new { success = false, message = "Lütfen bir ürün seçiniz!" });

                if (urunAdet <= 0)
                    return Json(new { success = false, message = "Ürün adedi 1 veya daha büyük olmalıdır!" });

                if (islemType == 2)
                {
                    int mevcutStok = dbContext.SN_M_islemler
                        .Where(i => i.urunID == urunID)
                        .Sum(i => i.islemType == 1 ? i.adet : -i.adet); //girenleri toplar çıkanaları düşer
                    if(urunAdet> mevcutStok)
                    {
                        return Json(new { success = false, message = "Çıkarılacak miktar stoktan fazla olamaz" });
                    }
                }
                var yeniIslem = new SN_M_islemler
                {
                    calisanID = calisanID,
                    urunID = urunID,
                    islemType = islemType,   // 1 = Ekle, 2 = Çıkar
                    adet = urunAdet,
                    islemDate = DateTime.Now
                };

                dbContext.SN_M_islemler.Add(yeniIslem);
                dbContext.SaveChanges();

                return Json(new { success = true, message = "Stok işlemi başarıyla kaydedildi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }

        //stok ekle/çıkart end 

        //Çalışan Sil 
        public int deleteCalisan(int calisanID)
        {
            try
            {
                var calisan = dbContext.SN_M_calisanListesi.FirstOrDefault(c => c.calisanID == calisanID);

                if (calisan == null)
                {
                    Logger.GetInstance().Log("SN_M_calisanListesi", "ERROR", $"Çalışan silme hatası: ID {calisanID} bulunamadı!");
                    return 404;
                }
                
                string eskiVeri = $"{{\"old_calisanAdi\":\"{calisan.calisanAdi}\",\"old_isActive\":{calisan.isActive}}}";
                
                calisan.isActive = false;
                dbContext.SaveChanges();                
                
                string yeniVeri = $"{{\"new_calisanAdi\":\"{calisan.calisanAdi}\",\"new_isActive\":{calisan.isActive}}}";
                
                Logger.GetInstance().Log("SN_M_calisanListesi", "SOFT DELETE", "Çalışan pasif hale getirildi.", eskiVeri, yeniVeri);

                return 200;
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log("SN_M_calisanListesi", "ERROR", $"Çalışan silme işlemi sırasında hata oluştu: {ex.Message}");
                return 500;
            }
        }

        public int deleteMusteri(int musteriID)
        {
            try
            {
                var musteri = dbContext.SN_M_musteriListesi.FirstOrDefault(c => c.musteriID == musteriID);

                if (musteri == null)
                {
                    Logger.GetInstance().Log("SN_M_musteriListesi", "ERROR", $"Müşteri silme hatası: ID {musteriID} bulunamadı!");
                    return 404;
                }

                string eskiVeri = $"{{\"old_musteriAdi\":\"{musteri.musteriAdi}\",\"old_isActive\":{musteri.isActive}}}";

                musteri.isActive = false;
                dbContext.SaveChanges();

                string yeniVeri = $"{{\"new_musteriAdi\":\"{musteri.musteriAdi}\",\"new_isActive\":{musteri.isActive}}}";

                Logger.GetInstance().Log("SN_M_musteriListesi", "SOFT DELETE", "Müşteri pasif hale getirildi.", eskiVeri, yeniVeri);

                return 200;
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log("SN_M_musteriListesi", "ERROR", $"Müşteri silme işlemi sırasında hata oluştu: {ex.Message}");
                return 500;
            }
        }


        //Müşteri Ekle
        public int insertMusteri(string musteriAdi, string musteriSoyadi)
        {
            try
            {
                var yeniMusteri = new SN_M_musteriListesi
                {
                    musteriAdi = musteriAdi,
                    musteriSoyadi = musteriSoyadi,
                    isActive = true
                };

                dbContext.SN_M_musteriListesi.Add(yeniMusteri);
                dbContext.SaveChanges();

                string yeniVeri = $"{{\"new_musteriAdi\":\"{musteriAdi}\",\"new_musteriSoyadi\":\"{musteriSoyadi}\"}}";


                Logger.GetInstance().Log("SN_M_musteriListesi", "INSERT", "INSERT isleminden sonra yeni müşteri eklenmiştir.", null, yeniVeri);

                return 200;
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log("SN_M_musteriListesi", "ERROR", $"Müşteri ekleme hatası: {ex.Message}");
                return 500;
            }
        }

        //Çalışan Ekle
        public int insertCalisan(string calisanAdi, string calisanSoyadi)
        {
            try
            {
                var yeniCalisan = new SN_M_calisanListesi
                {
                    calisanAdi = calisanAdi,
                    calisanSoyadi = calisanSoyadi,
                    isActive = true
                };

                dbContext.SN_M_calisanListesi.Add(yeniCalisan);
                dbContext.SaveChanges();

                string yeniVeri = $"{{\"new_calisanAdi\":\"{calisanAdi}\",\"new_calisanSoyadi\":\"{calisanSoyadi}\"}}";

          
                Logger.GetInstance().Log("SN_M_calisanListesi", "INSERT", "INSERT isleminden sonra yeni çalisan eklenmiştir.", null, yeniVeri);

                return 200;
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log("SN_M_calisanListesi", "ERROR", $"Çalışan ekleme hatası: {ex.Message}");
                return 500;
            }
        }

        //Müşteri Güncelle
    
        public int updateMusteri(int musteriID, string musteriAdi, string musteriSoyadi)
        {
            try
            {
                var musteri = dbContext.SN_M_musteriListesi.FirstOrDefault(c => c.musteriID == musteriID);
                if (musteri == null)
                {
                    Logger.GetInstance().Log("SN_M_musteriListesi", "ERROR", $"Müşteri güncelleme hatası: ID {musteriID} bulunamadı!");
                    return 404;
                }

            
                string eskiVeri = $"{{\"old_musteriAdi\":\"{musteri.musteriAdi}\",\"old_musteriSoyadi\":\"{musteri.musteriSoyadi}\"}}";

                musteri.musteriAdi = musteriAdi;
                musteri.musteriSoyadi = musteriSoyadi;
                dbContext.SaveChanges();

              
                string yeniVeri = $"{{\"new_musteriAdi\":\"{musteriAdi}\",\"new_musteriSoyadi\":\"{musteriSoyadi}\"}}";

            
                Logger.GetInstance().Log("SN_M_musteriListesi", "UPDATE", "UPDATE işleminden sonra müşteri adi ve soyadi güncellenmiştir.", eskiVeri, yeniVeri);

                return 200;
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log("SN_M_musteriListesi", "ERROR", $"Müşteri güncelleme hatası: {ex.Message}");
                return 500;
            }
        }




        //Çalışan Güncelle
        public int updateCalisan(int calisanID, string calisanAdi, string calisanSoyadi)
        {
            try
            {
                var calisan = dbContext.SN_M_calisanListesi.FirstOrDefault(c => c.calisanID == calisanID);
                if (calisan == null)
                {
                    Logger.GetInstance().Log("SN_M_calisanListesi", "ERROR", $"Çalışan güncelleme hatası: ID {calisanID} bulunamadı!");
                    return 404;
                }


                string eskiVeri = $"{{\"old_calisanAdi\":\"{calisan.calisanAdi}\",\"old_calisanSoyadi\":\"{calisan.calisanSoyadi}\"}}";

                calisan.calisanAdi = calisanAdi;
                calisan.calisanSoyadi = calisanSoyadi;
                dbContext.SaveChanges();


                string yeniVeri = $"{{\"new_calisanAdi\":\"{calisanAdi}\",\"new_calisanSoyadi\":\"{calisanSoyadi}\"}}";


                Logger.GetInstance().Log("SN_M_calisanListesi", "UPDATE", "UPDATE işleminden sonra çalisan adi ve soyadi güncellenmiştir.", eskiVeri, yeniVeri);

                return 200;
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log("SN_M_calisanListesi", "ERROR", $"Çalışan güncelleme hatası: {ex.Message}");
                return 500;
            }
        }


        //Kategoriye Göre Ürün Listesini Getirme
        public JsonResult urunListesiGetir()
        {
            var urunListesi = dbContext.SN_M_kategoriListesi.Where(s=> s.isActive==true)
                .Select(s => new {
                    s.kategoriID,
                    s.kategoriAciklamasi,
                    s.kategoriIcon
                }).ToList();

            return Json(urunListesi);
        }

        public JsonResult kategoriyegoreUrunListesiGetir(int kategoriID)
        {
            var kategoriyegoreUrunListesi = dbContext.SN_M_urunListesi
                .Where(u => u.urunKategoriID == kategoriID && u.isActive == true) 
                .Select(u => new
                {
                    u.urunBarkod,
                    u.urunAdi,
                    u.birim,
                    u.urunResimURL
                
                }).ToList();

            return Json(kategoriyegoreUrunListesi);
        }


        //Kategori Ekle
        public int kategoriEkle(string kategoriAdi)
        {
            try
            {
                var yeniKategori = new SN_M_kategoriListesi
                {
                    kategoriAciklamasi = kategoriAdi,
                    kategoriIcon = "icon-plus",
                    isActive = true
                };

                dbContext.SN_M_kategoriListesi.Add(yeniKategori);
                dbContext.SaveChanges();

                // Log ekleme
                string yeniVeri = $"{{\"new_kategoriAciklamasi\":\"{kategoriAdi}\"}}";
                Logger.GetInstance().Log("SN_M_kategoriListesi", "INSERT", "Yeni kategori eklendi.", null, yeniVeri);

                return 200;
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log("SN_M_kategoriListesi", "ERROR", $"Kategori ekleme hatası: {ex.Message}");
                return 500;
            }
        }

        //Ürün Ekle
        public int urunEkle(int urunKategoriID,string urunAdi, string urunBirim)
        {


            var yeniUrun = new SN_M_urunListesi
            {

                urunAdi = urunAdi,
                urunKategoriID = urunKategoriID,
                birim = urunBirim,
                urunResimURL = "/" + urunAdi + ".jpg",
               isActive = true
            };
            dbContext.SN_M_urunListesi.Add(yeniUrun);
            try
            {

                string yeniVeri = $"{{\"new_urunAdi\":\"{urunAdi}\",\"new_urunKategoriID\":\"{urunKategoriID}\"}}";
                Logger.GetInstance().Log("SN_M_urunListesi", "INSERT", "Yeni Ürün eklendi.", null, yeniVeri);
                dbContext.SaveChanges();
                return 200;
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log("SN_M_urunListesi", "ERROR", $"Ürün ekleme hatası: {ex.Message}");
                return 500;
                
            }
        }



        //Ürün Sil
        public int deleteUrun(int urunBarkod)
        {
            try
            {
                var urun = dbContext.SN_M_urunListesi.FirstOrDefault(c => c.urunBarkod == urunBarkod);

                if (urun == null)
                {
                    Logger.GetInstance().Log("SN_M_urunListesi", "ERROR", $"Ürün silme hatası: ID {urunBarkod} bulunamadı!");
                    return 404;
                }

                string eskiVeri = $"{{\"old_urunAdi\":\"{urun.urunAdi}\",\"old_isActive\":{urun.isActive}}}";

                urun.isActive = false;
                dbContext.SaveChanges();

                string yeniVeri = $"{{\"new_isActive\":{urun.isActive}}}";
                Logger.GetInstance().Log("SN_M_urunListesi", "SOFT DELETE", "Ürün pasif hale getirildi.", eskiVeri, yeniVeri);

                return 200;
            }
            catch (Exception ex)
            {
                Logger.GetInstance().Log("SN_M_urunListesi", "ERROR", $"Ürün silme işlemi sırasında hata oluştu: {ex.Message}");
                return 500;
            }
        }

        //Kategori Sil
        public int deleteKategori(int kategoriID)
        {
            var kategori = dbContext.SN_M_kategoriListesi.Where(d => d.kategoriID == kategoriID).FirstOrDefault();

           
            kategori.isActive = false;
            var urun = dbContext.SN_M_urunListesi.Where(s => s.urunKategoriID == kategoriID).ToList();

            if (urun.Count > 0)
            {
                foreach (var item in urun)
                {
                    item.isActive = false;
                }
                
                dbContext.SaveChanges();
                return 200;
            }
           else {
                dbContext.SaveChanges();
                return 200;
            }
        }

        //Kategoriler arası ürün taşıma
        public int tasiUrun(int urunBarkod,int urunKategoriID)
        {
            var urun = dbContext.SN_M_urunListesi.Where(s => s.urunBarkod == urunBarkod).FirstOrDefault();
            urun.urunKategoriID = urunKategoriID;
            try
            {
                dbContext.SaveChanges();
                return 200; // Güncelleme başarılı
            }
            catch (Exception)
            {
                return 500; // Sunucu hatası
            }

        }

        //Ürün Güncelle
    public int updateUrun(int urunBarkod, string urunAdi, string birim)
{
    try
    {
        var urun = dbContext.SN_M_urunListesi.FirstOrDefault(u => u.urunBarkod == urunBarkod);

        if (urun == null)
        {
            Logger.GetInstance().Log("SN_M_urunListesi", "ERROR", $"Ürün güncelleme hatası: ID {urunBarkod} bulunamadı!");
            return 404;
        }

        string eskiVeri = $"{{\"old_urunAdi\":\"{urun.urunAdi}\",\"old_birim\":\"{urun.birim}\"}}";

        urun.urunAdi = urunAdi;
        urun.birim = birim;
        dbContext.SaveChanges();

        string yeniVeri = $"{{\"new_urunAdi\":\"{urunAdi}\",\"new_birim\":\"{birim}\"}}";
        Logger.GetInstance().Log("SN_M_urunListesi", "UPDATE", "Ürün bilgileri güncellendi.", eskiVeri, yeniVeri);

        return 200;
    }
    catch (Exception ex)
    {
        Logger.GetInstance().Log("SN_M_urunListesi", "ERROR", $"Ürün güncelleme hatası: {ex.Message}");
        return 500;
    }
}

        public JsonResult getUrunKategorileri()
        {
            var urunKategorileri = dbContext.SN_M_kategoriListesi.Where(s => s.isActive == true).Select(s => new { 
            
            s.kategoriID,
            s.kategoriAciklamasi
            }).ToList();

            return Json(urunKategorileri);
        }



       
        public ActionResult urunListesi()
        {
            return View();
        }
        
        //ürün yönetimi için ürün listesi
 

        public ActionResult urunYonetimi()
        {
            if (Request.IsAjaxRequest()) // Eğer AJAX isteği geliyorsa
            {
                return GetUrunVeFiyatListesi(); // JSON verisini döndür
            }

            return View(); // Normal sayfa isteği için View döndür
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







        //Yeni ürün fiyatı ekleme abstract class kullanmadan
        //public JsonResult AddUrunFiyat(int urunBarkod, decimal toplamFiyat)
        //{
        //   try
        //    {
        //        if (urunBarkod <= 0)
        //        {
        //            return Json(new { success = false, message = "Ürün barkodu geçersiz!" });
        //        }

        //        var yeniFiyat = new SN_M_urunFiyatListesi
        //        {
        //            urunID = urunBarkod,
        //           urunFiyati = toplamFiyat,
        //            fiyatTarihi = DateTime.Now
        //        };

        //        dbContext.SN_M_urunFiyatListesi.Add(yeniFiyat);
        //        dbContext.SaveChanges();

        //        return Json(new { success = true, message = "Fiyat başarıyla eklendi!" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Fiyat eklenirken hata oluştu!", error = ex.Message });
        //    }
        //}






        //Yeni ürün fiyatı ekleme
        public JsonResult AddUrunFiyat(int urunBarkod, string maliyetFiyati, string kdvOrani, string calisanPayi, string karPayi, string indirimOrani)
        {
            try
            {
                if (urunBarkod <= 0)
                {
                    return Json(new { success = false, message = "Ürün barkodu geçersiz!" });
                }

                // Kültürel dönüşümü düzelt
                var culture = System.Globalization.CultureInfo.InvariantCulture;

                // Stringleri güvenli bir şekilde `decimal`'a çevir
                if (!decimal.TryParse(maliyetFiyati, System.Globalization.NumberStyles.Any, culture, out decimal maliyet) ||
                    !decimal.TryParse(kdvOrani, System.Globalization.NumberStyles.Any, culture, out decimal kdv) ||
                    !decimal.TryParse(calisanPayi, System.Globalization.NumberStyles.Any, culture, out decimal calisan) ||
                    !decimal.TryParse(karPayi, System.Globalization.NumberStyles.Any, culture, out decimal kar))
                {
                    return Json(new { success = false, message = "Lütfen geçerli sayısal değerler girin!" });
                }

                // KDV, çalışan payı ve kar oranları yüzde olarak geldiği için 100'e böl
                kdv /= 100;
                calisan /= 100;
                kar /= 100;

                // İndirim oranını kontrol et, boş ise 0 olarak al
                decimal indirim = 0;
                if (!string.IsNullOrWhiteSpace(indirimOrani) && decimal.TryParse(indirimOrani, System.Globalization.NumberStyles.Any, culture, out decimal indirimParsed))
                {
                    indirim = indirimParsed / 100;
                }

                // Fiyat hesaplayıcı nesnesini oluştur
                FiyatHesaplayici fiyatHesaplayici;
                if (indirim > 0)
                {
                    fiyatHesaplayici = new KampanyaliFiyatHesaplayici(maliyet, kdv, calisan, kar, indirim);
                }
                else
                {
                    fiyatHesaplayici = new StandartFiyatHesaplayici(maliyet, kdv, calisan, kar);
                }

                decimal toplamFiyat = fiyatHesaplayici.NihaiFiyatHesapla();

                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // Eski fiyatları pasif yap
                        var eskiFiyatlar = dbContext.SN_M_urunFiyatListesi
                            .Where(f => f.urunID == urunBarkod && f.isActive)
                            .ToList();

                        foreach (var fiyat in eskiFiyatlar)
                        {
                            fiyat.isActive = false;
                        }

                        dbContext.SaveChanges();

                        // Yeni fiyatı ekle
                        var yeniFiyat = new SN_M_urunFiyatListesi
                        {
                            urunID = urunBarkod,
                            urunFiyati = toplamFiyat,
                            fiyatTarihi = DateTime.Now,
                            isActive = true
                        };

                        dbContext.SN_M_urunFiyatListesi.Add(yeniFiyat);
                        dbContext.SaveChanges();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return Json(new { success = false, message = "Fiyat eklenirken hata oluştu!", error = ex.Message });
                    }
                }

                return Json(new { success = true, message = "Fiyat başarıyla eklendi!", toplamFiyat });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Fiyat eklenirken hata oluştu!", error = ex.Message });
            }
        }



        //Seçilen ürüne göre fiyat geçmişini listeleme
        public JsonResult GetUruneGoreFiyatGecmisi(int urunBarkod)
        {
            var fiyatGecmisi = dbContext.SN_M_urunFiyatListesi
                .Where(f => f.urunID == urunBarkod)
                .OrderByDescending(f => f.fiyatTarihi) // Önce en güncel fiyatlar gelsin
                .Take(7) // Son 7 kaydı al
                .ToList();

            var fiyatGecmisiFormatted = new List<object>();
            decimal? oncekiFiyat = null;

            foreach (var fiyat in fiyatGecmisi.OrderBy(f => f.fiyatTarihi)) // Fiyat değişimini hesaplamak için eskiye doğru sırala
            {
                decimal fiyatDegeri = fiyat.urunFiyati;
                string degisim = ""; // Varsayılan boş

                if (oncekiFiyat.HasValue)
                {
                    if (fiyatDegeri > oncekiFiyat.Value)
                    {
                        degisim = "fa-arrow-up text-success";  // Fiyat artışı
                    }
                    else if (fiyatDegeri < oncekiFiyat.Value)
                    {
                        degisim = "fa-arrow-down text-danger"; // Fiyat düşüşü
                    }
                    else
                    {
                        degisim = "fa-minus text-secondary";  // Fiyat değişmemiş
                    }
                }

                fiyatGecmisiFormatted.Add(new
                {
                    FiyatTarihi = fiyat.fiyatTarihi,
                    Fiyat = fiyatDegeri,
                    Degisim = degisim
                });

                oncekiFiyat = fiyatDegeri;
            }

            // Kullanıcıya en güncel veriler en üstte olacak şekilde ters çevir
            fiyatGecmisiFormatted.Reverse();

            return Json(new { success = true, data = fiyatGecmisiFormatted }, JsonRequestBehavior.AllowGet);
        }


        //public JsonResult UrunStokmiktari(int urunBarkod)
        //{
        //    //Giren ürün miktarını hesaplama
        //    int giren = dbContext.SN_M_islemler
        //          .Where(f => f.urunID == urunBarkod &&  f.islemType == 1)
        //          .Sum(f => f.adet);
        //    //Çıkan ürün miktarını hesaplama
        //    int cikan = dbContext.SN_M_islemler
        //        .Where(f => f.urunID == urunBarkod && f.islemType == 2)
        //        .Sum(f => f.adet);

        //    //Stok hesaplama
        //    int stok = giren - cikan;

        //    //Json formatında veriyi döndür
        //    return Json(new { giren, cikan, stok });
        //}



       
        public JsonResult UrunStokMiktari(int urunBarkod)
        {
            try
            {
                if (urunBarkod <= 0)
                {
                    return Json(new { error = "Geçersiz barkod numarası!" });
                }

                int giren = dbContext.SN_M_islemler
                                     .Where(f => f.urunID == urunBarkod && f.islemType == 1)
                                     .Sum(f => (int?)f.adet) ?? 0;

                int cikan = dbContext.SN_M_islemler
                                     .Where(f => f.urunID == urunBarkod && f.islemType == 2)
                                     .Sum(f => (int?)f.adet) ?? 0;

                int stok = giren - cikan;

                return Json(new { giren, cikan, stok });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Stok bilgisi alınırken hata oluştu.", detay = ex.Message });
            }
        }


        //public int urunEkle(int urunKategoriID, string urunAdi, string urunBirim)
        //{


        //    var yeniUrun = new SN_M_urunListesi
        //    {

        //        urunAdi = urunAdi,
        //        urunKategoriID = urunKategoriID,
        //        birim = urunBirim,
        //        urunResimURL = "/" + urunAdi + ".jpg",
        //        isActive = true
        //    };
        //    dbContext.SN_M_urunListesi.Add(yeniUrun);
        //    try
        //    {

        //        string yeniVeri = $"{{\"new_urunAdi\":\"{urunAdi}\",\"new_urunKategoriID\":\"{urunKategoriID}\"}}";
        //        Logger.GetInstance().Log("SN_M_urunListesi", "INSERT", "Yeni Ürün eklendi.", null, yeniVeri);
        //        dbContext.SaveChanges();
        //        return 200;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.GetInstance().Log("SN_M_urunListesi", "ERROR", $"Ürün ekleme hatası: {ex.Message}");
        //        return 500;

        //    }
        //}


        //var satisBilgileri = (from s in dbContext.SN_M_satisListesi
        //                      join c in dbContext.SN_M_calisanListesi on s.calisanID equals c.calisanID
        //                      join m in dbContext.SN_M_musteriListesi on s.musteriID equals m.musteriID
        //                      where s.satilanUrunID == urunBarkod
        //                      orderby s.satisTarihi descending // Yeni satışları önce göster
        //                      select new
        //                      {
        //                          SatisTarihi = s.satisTarihi,
        //                          CalisanAdi = c.calisanAdi + " " + c.calisanSoyadi,
        //                          MusteriAdi = m.musteriAdi + " " + m.musteriSoyadi,
        //                          Adet = s.adet
        //                      }).ToList();

        //public JsonResult StokEkle(int urunBarkod,int islemType, int urunAdet)
        //{
        //    var urunStok = (from s in dbContext.SN_M_islemler
        //                    join c in dbContext.SN_M_calisanListesi on s.calisanID equals c.calisanID
        //                    join m in dbContext.SN_M_urunListesi on s.urunID==urunBarkod
        //                    where 
        //                    select new
        //                    {
        //                        CalisanAdi = c.calisanAdi,
        //                        IslemType = s.islemType,
        //                        UrunAdet = s.adet,
        //                        IslemTarihi = s.islemDate
        //                    }).ToList();
        //}





























    }
}