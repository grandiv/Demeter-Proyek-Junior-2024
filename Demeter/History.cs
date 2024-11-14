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
        public int CartID { get; set; }
        public List<Produk> DaftarBelanja { get; set; }
        public DateTime TanggalBelanja { get; set; }
        public string Status { get; set; }

        public void updateStatus(string Status)
        {
            // Implementation
        }
    }
}
