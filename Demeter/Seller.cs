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

                        string getSellerIdQuery = "SELECT sellerid, namatoko FROM seller WHERE userid = @userId";
                        int sellerId;
                        string namaToko;
                        using (var cmd = new NpgsqlCommand(getSellerIdQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("userId", userId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    throw new Exception("Seller not found");
                                }
                                sellerId = reader.GetInt32(0);
                                namaToko = reader.GetString(1);
                            }
                        }

                        string insertProdukQuery = @"
                        INSERT INTO produk (nama, deskripsi, harga, namatoko, sellerid, photourl, stok)
                        VALUES (@nama, @deskripsi, @harga, @namatoko, @sellerid, @photourl, @stok)";

                        using (var cmd = new NpgsqlCommand(insertProdukQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("nama", produk.namaProduk);
                            cmd.Parameters.AddWithValue("deskripsi", produk.deskripsiProduk);
                            cmd.Parameters.AddWithValue("harga", produk.hargaProduk);
                            cmd.Parameters.AddWithValue("namatoko", namaToko);
                            cmd.Parameters.AddWithValue("sellerid", sellerId);
                            cmd.Parameters.AddWithValue("photourl", !string.IsNullOrEmpty(produk.photoUrl) ? produk.photoUrl : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("stok", produk.stok);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Failed to add product: " + ex.Message);
                    }
                }
           }
        }

        public List<Produk> LoadSellerProducts()
        {
            List<Produk> products = new List<Produk>();

            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
            SELECT 
                p.produkid,
                p.nama,
                p.deskripsi,
                p.harga,
                p.namatoko,
                p.status,
                p.stok,
                p.photourl
            FROM produk p
            INNER JOIN seller s ON p.sellerid = s.sellerid
            INNER JOIN pengguna u ON s.userid = u.userid
            WHERE u.username = @username";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("username", User.CurrentUsername);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Produk product = new Produk
                                {
                                    produkID = reader.GetInt32(0),
                                    namaProduk = reader.GetString(1),
                                    deskripsiProduk = reader.GetString(2),
                                    hargaProduk = reader.GetDouble(3),
                                    namaToko = reader.GetString(4),
                                    status = reader.GetString(5),
                                    stok = reader.GetInt32(6),
                                    photoUrl = reader.IsDBNull(7) ? null : reader.GetString(7)
                                };
                                products.Add(product);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading seller products: " + ex.Message);
                    throw;
                }
            }

            return products;
        }

        public void deleteProduk(int produkId)
        {
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteProdukQuery = "DELETE FROM produk WHERE produkid = @produkId";
                        using (var cmd = new NpgsqlCommand(deleteProdukQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("produkId", produkId);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Failed to delete product: " + ex.Message);
                    }
                }
            }
        }

        public void updateProduk(Produk produk)
        {
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateProdukQuery = @"
                        UPDATE produk 
                        SET nama = @nama, 
                            deskripsi = @deskripsi, 
                            harga = @harga, 
                            photourl = @photoUrl, 
                            stok = @stok 
                        WHERE produkid = @produkId";
                        using (var cmd = new NpgsqlCommand(updateProdukQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("nama", produk.namaProduk);
                            cmd.Parameters.AddWithValue("deskripsi", produk.deskripsiProduk);
                            cmd.Parameters.AddWithValue("harga", produk.hargaProduk);
                            cmd.Parameters.AddWithValue("photoUrl", !string.IsNullOrEmpty(produk.photoUrl) ? produk.photoUrl : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("stok", produk.stok);
                            cmd.Parameters.AddWithValue("produkId", produk.produkID);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Failed to update product: " + ex.Message);
                    }
                }
            }
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
                    WHERE userid = @userId;
                    
                    INSERT INTO seller (userid, namatoko, notelp, alamat, photourl)
                    SELECT @userId, @nama, @noTelp, @alamat, @photoUrl
                    WHERE NOT EXISTS (SELECT 1 FROM seller WHERE userid = @userId);";

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
                INNER JOIN pengguna p ON s.userid = p.userid 
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
