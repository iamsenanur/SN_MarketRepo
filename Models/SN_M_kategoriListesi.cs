//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SN_MarketUygulamasi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SN_M_kategoriListesi
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SN_M_kategoriListesi()
        {
            this.SN_M_urunListesi = new HashSet<SN_M_urunListesi>();
        }
    
        public int kategoriID { get; set; }
        public string kategoriAciklamasi { get; set; }
        public string kategoriIcon { get; set; }
        public bool isActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SN_M_urunListesi> SN_M_urunListesi { get; set; }
    }
}
