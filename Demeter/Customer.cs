using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;


namespace Demeter
{
    internal class Customer : User
    {
        public string nama { get; set; }
        public int noTelp { get; set; }
        public string alamatPengiriman { get; set; }
        public string photoUrl { get; set; }

        public void addToCart(Produk produk)
        {
            Cart cart = new Cart();
            cart.DaftarBelanja.Add(produk);
            cart.TotalHarga = cart.CalcTotalHarga();
        }

        public void deleteFromCart(Produk produk)
        {
            Cart cart = new Cart();
            cart.DaftarBelanja.Remove(produk);
            cart.TotalHarga = cart.CalcTotalHarga();
        }

        public void searchProduk(string keyword)
        {
            List<Produk> produkList = new List<Produk>();
            var result = produkList.Where(p => p.namaProduk.Contains(keyword) || p.deskripsiProduk.Contains(keyword)).ToList();
        }

        public void checkOut()
        {
            Cart cart = new Cart();
            Payment payment = new Payment();
            payment.ProsesPayment();
            payment.UpdateStatusPayment("Selesai");
            cart.DaftarBelanja.Clear();
            cart.TotalHarga = 0;
        }

        public List<History> SeeHistory()
        {
            List<History> historyList = new List<History>(); 
            return historyList;
        }

        public void editProfile(string newPhotoUrl, string newNama, int newNoTelp, string newAlamatPengiriman)
        {
            this.nama = newNama;
            this.noTelp = newNoTelp;
            this.alamatPengiriman = newAlamatPengiriman;
            UpdatePhotoProfile(newPhotoUrl);
        }

        private void UpdatePhotoProfile(string newPhotoUrl)
        {
            this.photoUrl = newPhotoUrl;
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE pengguna SET photo_url = @photoUrl WHERE username = @username";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("photoUrl", newPhotoUrl);
                        cmd.Parameters.AddWithValue("username", this.username);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }
    }
}
