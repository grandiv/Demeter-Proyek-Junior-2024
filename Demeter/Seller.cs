using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Demeter
{
    internal class Seller : User
    {
        public string namaToko { get; set; }
        public int noTelp { get; set; }
        public string alamat {  get; set; }
        public string photoUrl {  get; set; }

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

        public void editProfile(string newNamaToko, int newNoTelp, string newAlamat, string newPhotoUrl)
        {
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
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

                        string updateSellerQuery = @"
                    UPDATE seller 
                    SET namatoko = @nama, 
                        notelp = @noTelp, 
                        alamat = @alamat, 
                        photourl = @photoUrl
                    WHERE sellerid = @userId;
                    
                    INSERT INTO seller (sellerid, namatoko, notelp, alamat, photourl)
                    SELECT @userId, @nama, @noTelp, @alamat, @photoUrl
                    WHERE NOT EXISTS (SELECT 1 FROM seller WHERE sellerid = @userId);";

                        using (var cmd = new NpgsqlCommand(updateSellerQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("userId", userId);
                            cmd.Parameters.AddWithValue("nama", newNamaToko);
                            cmd.Parameters.AddWithValue("noTelp", newNoTelp);
                            cmd.Parameters.AddWithValue("alamat", newAlamat);
                            cmd.Parameters.AddWithValue("photoUrl", !string.IsNullOrEmpty(newPhotoUrl) ? newPhotoUrl : (object)DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        // Update local properties
                        this.namaToko = newNamaToko;
                        this.noTelp = newNoTelp;
                        this.alamat = newAlamat;
                        this.photoUrl = newPhotoUrl;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Failed to update profile: " + ex.Message);
                    }
                }
            }
        }

        public void LoadSellerData(string username)
        {
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                SELECT 
                    s.namatoko,
                    s.notelp,
                    s.alamat,
                    s.photourl
                FROM seller s
                INNER JOIN pengguna p ON s.sellerid = p.userid 
                WHERE p.username = @username";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("username", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                this.namaToko = !reader.IsDBNull(0) ? reader.GetString(0) : "";
                                string noTelpStr = !reader.IsDBNull(1) ? reader.GetString(1) : "0";
                                if (int.TryParse(noTelpStr, out int noTelpValue))
                                {
                                    this.noTelp = noTelpValue;
                                }
                                else
                                {
                                    this.noTelp = 0;
                                }
                                this.alamat = !reader.IsDBNull(2) ? reader.GetString(2) : "";
                                this.photoUrl = !reader.IsDBNull(3) ? reader.GetString(3) : "";

                                // Load profile picture if URL exists
                                if (!string.IsNullOrEmpty(this.photoUrl))
                                {
                                    Console.WriteLine($"Found photo URL: {this.photoUrl}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading seller data: " + ex.Message);
                    throw;
                }
            }
        }
    }
}
