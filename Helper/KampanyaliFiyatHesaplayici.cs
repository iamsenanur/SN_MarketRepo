using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SN_MarketUygulamasi.Models.AbstractClasses;

namespace SN_MarketUygulamasi.Models
{
    public class KampanyaliFiyatHesaplayici : FiyatHesaplayici
    {
        private decimal IndirimOrani { get; set; }

        public KampanyaliFiyatHesaplayici(decimal maliyetFiyati, decimal kdvOrani, decimal calisanPayi, decimal karPayi, decimal indirimOrani)
            : base(maliyetFiyati, calisanPayi, karPayi, kdvOrani)
        {
            IndirimOrani = indirimOrani;
        }

        public override decimal NihaiFiyatHesapla()
        {
            decimal temelFiyat = MaliyetFiyati + CalisanPayiHesapla() + KarPayiHesapla();
            decimal indirimliFiyat = temelFiyat - (temelFiyat * IndirimOrani);
            return KDVEkle(indirimliFiyat); // KDV'yi indirimli fiyat üzerinden ekliyoruz.
        }
    }
}
