using SN_MarketUygulamasi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SN_MarketUygulamasi.Controllers
{
    public class HomeController : Controller
    {
        private DBRR_AIEntities2 dbContext = new DBRR_AIEntities2();

    
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CalisanSayisi()
        {
            try { 

                int calisanlar = dbContext.SN_M_calisanListesi
                 .Where(s => s.isActive == true)
                 .Count();
                 return Json(new { calisanlar });
            }

            catch(Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        public JsonResult MusteriSayisi()
        {
            try
            {
                int musteriler = dbContext.SN_M_musteriListesi
                    .Where(s => s.isActive == true)
                    .Count();
                return Json(new { musteriler });
            }
            catch(Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
     
        public JsonResult UrunStokMiktari()
        {
            try
            {
                int giren = dbContext.SN_M_islemler
                                     .Where(f => f.islemType == 1)
                                      .Sum(f => (int?)f.adet) ?? 0;

                int cikan = dbContext.SN_M_islemler
                                     .Where(f =>  f.islemType == 2)
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