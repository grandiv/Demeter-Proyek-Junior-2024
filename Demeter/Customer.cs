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

        public void editProfile(string newNama, int newNoTelp, string newAlamatPengiriman)
        {
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Get userid from pengguna table using CurrentUsername
                        string getUserIdQuery = "SELECT userid FROM pengguna WHERE username = @username";
                        int userId;

                        using (var cmd = new NpgsqlCommand(getUserIdQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("username", User.CurrentUsername);
                            var result = cmd.ExecuteScalar();
                            if (result == null)
                            {
                                throw new Exception("User not found");
                            }
                            userId = (int)result;
                        }

                        // Try to update first, if no rows affected then insert
                        string updateCustomerQuery = @"
                    UPDATE customer 
                    SET nama = @nama, notelp = @noTelp, alamatpengiriman = @alamat
                    WHERE custid = @userId;
                    
                    INSERT INTO customer (custid, nama, notelp, alamatpengiriman)
                    SELECT @userId, @nama, @noTelp, @alamat
                    WHERE NOT EXISTS (SELECT 1 FROM customer WHERE custid = @userId);";

                        using (var cmd = new NpgsqlCommand(updateCustomerQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("userId", userId);
                            cmd.Parameters.AddWithValue("nama", newNama);
                            cmd.Parameters.AddWithValue("noTelp", newNoTelp);
                            cmd.Parameters.AddWithValue("alamat", newAlamatPengiriman);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        // Update local properties
                        this.nama = newNama;
                        this.noTelp = newNoTelp;
                        this.alamatPengiriman = newAlamatPengiriman;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Failed to update profile: " + ex.Message);
                    }
                }
            }
        }

        public void LoadCustomerData(string username)
        {
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                SELECT 
                    c.nama,
                    c.notelp,
                    c.alamatpengiriman
                FROM customer c 
                INNER JOIN pengguna p ON c.custid = p.userid 
                WHERE p.username = @username";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("username", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                this.nama = !reader.IsDBNull(0) ? reader.GetString(0) : "";
                                // Parse notelp from string to int, with error handling
                                string noTelpStr = !reader.IsDBNull(1) ? reader.GetString(1) : "0";
                                if (int.TryParse(noTelpStr, out int noTelpValue))
                                {
                                    this.noTelp = noTelpValue;
                                }
                                else
                                {
                                    this.noTelp = 0;
                                }
                                this.alamatPengiriman = !reader.IsDBNull(2) ? reader.GetString(2) : "";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading customer data: " + ex.Message);
                    throw;
                }
            }
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
