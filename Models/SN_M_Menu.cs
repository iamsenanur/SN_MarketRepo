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
    
    public partial class SN_M_Menu
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SN_M_Menu()
        {
            this.SN_M_SubMenu = new HashSet<SN_M_SubMenu>();
        }
    
        public int ID { get; set; }
        public string menuName { get; set; }
        public int menuSirasi { get; set; }
        public bool ısActive { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SN_M_SubMenu> SN_M_SubMenu { get; set; }
    }
}
