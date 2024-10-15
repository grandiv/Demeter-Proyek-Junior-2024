using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demeter
{
    internal class Admin : User
    {
        public int adminID { get; set; }
        public string adminName { get; set; }

        public void hapusAkun(User user)
        {
            // TO DO
        }
        public void verifikasiAkun (Seller seller) 
        { 
            // TO DO
        }
        public void verifikasiProduk (Produk produk)
        {
            // TO DO
        }
    }
}
