using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demeter
{
    internal class Payment
    {
        public int PaymentID { get; set; }
        public string Metode { get; set; }
        public string Status { get; set; } // Pending or Selesai

        public void ProsesPayment()
        {
            // Implementation of processing payment
        }

        public void UpdateStatusPayment(string status)
        {
            // Implementation of updating payment status
        }
    }
}
