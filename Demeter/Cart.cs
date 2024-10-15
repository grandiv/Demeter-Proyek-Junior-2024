using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demeter
{
    internal class Cart
    {
        public int CartID { get; set; }
        public int CustID { get; set; }
        public List<Produk> DaftarBelanja { get; set; }
        public double TotalHarga { get; set; }

        public double CalcTotalHarga()
        {
            // Implementation of calculating total price
            return DaftarBelanja.Sum(p => p.hargaProduk);
        }
    }
}
