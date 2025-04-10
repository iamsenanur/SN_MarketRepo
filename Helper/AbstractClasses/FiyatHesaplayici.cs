using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SN_MarketUygulamasi.Models.AbstractClasses
{
	public abstract class FiyatHesaplayici
	{
		protected decimal MaliyetFiyati { get; set; }
	
		protected decimal CalisanPayi { get; set; }

		protected decimal KarPayi { get; set; }
        protected decimal KDVOrani { get; set; }  



        public FiyatHesaplayici(decimal maliyetFiyati, decimal calisanPayi, decimal karPayi, decimal kdvOrani)
		{
			MaliyetFiyati = maliyetFiyati;
            KDVOrani = kdvOrani;
            CalisanPayi = calisanPayi;
			KarPayi = karPayi;
        }
		
		protected decimal CalisanPayiHesapla()
		{
			return MaliyetFiyati * CalisanPayi;
		}
        protected decimal KarPayiHesapla()
        {
            return MaliyetFiyati * KarPayi;
        }
        protected decimal KDVEkle(decimal fiyat)
        {
            return fiyat + (fiyat * KDVOrani);
        }
        public abstract decimal NihaiFiyatHesapla();

	}
}