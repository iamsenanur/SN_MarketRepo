using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SN_MarketUygulamasi.Models.AbstractClasses;

namespace SN_MarketUygulamasi.Models
{
    public class StandartFiyatHesaplayici : FiyatHesaplayici
    {
        public StandartFiyatHesaplayici(decimal maliyetFiyati, decimal kdvOrani, decimal calisanPayi, decimal karPayi)
            : base(maliyetFiyati, calisanPayi, karPayi, kdvOrani) { }

        public override decimal NihaiFiyatHesapla()
        {
            decimal temelFiyat = MaliyetFiyati + CalisanPayiHesapla() + KarPayiHesapla();
            return KDVEkle(temelFiyat); // KDV'yi temel fiyat üzerine ekliyoruz.
        }
    }
}
