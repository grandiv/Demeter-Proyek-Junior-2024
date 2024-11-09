using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demeter
{
    internal class Produk
    {
        public int produkID { get; set; }
        public string namaProduk { get; set; }
        public string deskripsiProduk { get; set; }
        public double hargaProduk { get; set; }
        public string namaToko { get; set; }
        public string status {  get; set; }
        public int stok { get; set; }
        public string photoUrl { get; set; }

        public void updateStatus(string status)
        {
            // TODO
        }
    }
}
