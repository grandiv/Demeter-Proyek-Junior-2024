using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demeter
{
    internal class Customer : User
    {
        public string nama { get; set; }
        public int noTelp { get; set; }
        public string alamatPengiriman { get; set; }

        public void addToCart(Produk produk)
        {
            // TODO
        }
        public void deleteFromCart(Produk produk)
        {
            // TODO
        }
        public void searchProduk(string keyword)
        {

        }
        public void checkOut()
        {

        }
        public List<History> SeeHistory()
        {
            // Implementation of viewing history
            return new List<History>();
        }
    }
}
