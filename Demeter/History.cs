using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demeter
{
    internal class History
    {
        public int HistoryID { get; set; }
        public int CustID { get; set; }
        public List<Produk> DaftarBelanja { get; set; }
        public DateTime TanggalBelanja { get; set; }
        public double TotalHarga { get; set; }

        public void LihatDetail()
        {
            // Implementation of viewing detail
        }
    }
}
