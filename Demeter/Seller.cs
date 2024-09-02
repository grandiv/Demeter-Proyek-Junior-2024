using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demeter
{
    internal class Seller : User
    {
        public string namaToko { get; set; }
        public int noTelp { get; set; }
        public string alamat {  get; set; }

        public void addProduk(Produk produk)
        {
            produk.namaToko = this.namaToko;
        }
        public void deleteProduk(int produkId)
        {
            // TODO
        }
        public void updateProduk(Produk produk)
        {

        }
    }
}
