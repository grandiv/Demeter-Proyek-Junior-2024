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
        public int custid { get; set; }

        public void addToCart(Produk produk, int quantity)
        {
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Get the customer ID
                        string getCustomerIdQuery = @"
                            SELECT c.custid 
                            FROM customer c
                            INNER JOIN pengguna p ON c.userid = p.userid
                            WHERE p.username = @username";
                        int customerId;
                        using (var cmd = new NpgsqlCommand(getCustomerIdQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("username", User.CurrentUsername);
                            var result = cmd.ExecuteScalar();
                            if (result == null)
                            {
                                throw new Exception("Customer not found");
                            }
                            customerId = (int)result;
                        }

                        // Check if the customer already has a cart
                        string getCartIdQuery = "SELECT cartid FROM cart WHERE custid = @custid";
                        int cartId;
                        using (var cmd = new NpgsqlCommand(getCartIdQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("custid", customerId);
                            var result = cmd.ExecuteScalar();
                            if (result == null)
                            {
                                // Create a new cart if it doesn't exist
                                string insertCartQuery = "INSERT INTO cart (totalHarga, custid) VALUES (0, @custid) RETURNING cartid";
                                using (var insertCmd = new NpgsqlCommand(insertCartQuery, conn, transaction))
                                {
                                    insertCmd.Parameters.AddWithValue("custid", customerId);
                                    cartId = (int)insertCmd.ExecuteScalar();
                                }
                            }
                            else
                            {
                                cartId = (int)result;
                            }
                        }

                        // Insert the product into the CartProduk table
                        string insertCartProdukQuery = "INSERT INTO cartproduk (cartid, produkid) VALUES (@cartid, @produkid)";
                        using (var cmd = new NpgsqlCommand(insertCartProdukQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("cartid", cartId);
                            cmd.Parameters.AddWithValue("produkid", produk.produkID);

                            // Execute insert for each quantity
                            for (int i = 0; i < quantity; i++)
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Update the total price in the cart
                        string updateCartQuery = "UPDATE cart SET totalHarga = totalHarga + @harga WHERE cartid = @cartid";
                        using (var cmd = new NpgsqlCommand(updateCartQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("harga", produk.hargaProduk * quantity);
                            cmd.Parameters.AddWithValue("cartid", cartId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Failed to add product to cart: " + ex.Message);
                    }
                }
            }
        }

        public void deleteFromCart(Produk produk)
        {
            //Cart cart = new Cart();
            //cart.DaftarBelanja.Remove(produk);
            //cart.TotalHarga = cart.CalcTotalHarga();
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

        public void editProfile(string newNama, int newNoTelp, string newAlamatPengiriman, string newPhotoUrl)
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

                        string updateCustomerQuery = @"
                    UPDATE customer 
                    SET nama = @nama, 
                        notelp = @noTelp, 
                        alamatpengiriman = @alamat, 
                        photourl = @photoUrl
                    WHERE userid = @userId;
                    
                    INSERT INTO customer (userid, nama, notelp, alamatpengiriman, photourl)
                    SELECT @userId, @nama, @noTelp, @alamat, @photoUrl
                    WHERE NOT EXISTS (SELECT 1 FROM customer WHERE userid = @userId);";

                        using (var cmd = new NpgsqlCommand(updateCustomerQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("userId", userId);
                            cmd.Parameters.AddWithValue("nama", newNama);
                            cmd.Parameters.AddWithValue("noTelp", newNoTelp);
                            cmd.Parameters.AddWithValue("alamat", newAlamatPengiriman);
                            cmd.Parameters.AddWithValue("photoUrl", !string.IsNullOrEmpty(newPhotoUrl) ? newPhotoUrl : (object)DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        // Update local properties
                        this.nama = newNama;
                        this.noTelp = newNoTelp;
                        this.alamatPengiriman = newAlamatPengiriman;
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
                    c.alamatpengiriman,
                    c.photourl
                FROM customer c 
                INNER JOIN pengguna p ON c.userid = p.userid 
                WHERE p.username = @username";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("username", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                this.nama = !reader.IsDBNull(0) ? reader.GetString(0) : "";
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

        public void Order(int cartId)
        {
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Get customer ID
                        string getCustomerIdQuery = @"
                            SELECT c.custid 
                            FROM customer c
                            INNER JOIN pengguna p ON c.userid = p.userid
                            WHERE p.username = @username";
                        int customerId;
                        using (var cmd = new NpgsqlCommand(getCustomerIdQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("username", User.CurrentUsername);
                            var result = cmd.ExecuteScalar();
                            if (result == null)
                            {
                                throw new Exception("Customer not found");
                            }
                            customerId = (int)result;
                        }

                        // 2. Get total price from cart
                        string getCartTotalQuery = "SELECT totalharga FROM cart WHERE cartid = @cartid";
                        double totalHarga;
                        using (var cmd = new NpgsqlCommand(getCartTotalQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("cartid", cartId);
                            totalHarga = Convert.ToDouble(cmd.ExecuteScalar());
                        }

                        // 3. Create history record and get the new historyid
                        string createHistoryQuery = @"
                            INSERT INTO history (tanggalbelanja, custid, totalharga) 
                            VALUES (@tanggalbelanja, @custid, @totalharga)
                            RETURNING historyid";

                        int historyId;
                        using (var cmd = new NpgsqlCommand(createHistoryQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("tanggalbelanja", DateTime.Now);
                            cmd.Parameters.AddWithValue("custid", customerId);
                            cmd.Parameters.AddWithValue("totalharga", totalHarga);
                            historyId = (int)cmd.ExecuteScalar();
                        }

                        // 4. Copy cart items to historyproduk
                        string copyToHistoryQuery = @"
                            INSERT INTO historyproduk (historyid, produkid)
                            SELECT @historyid, produkid
                            FROM cartproduk
                            WHERE cartid = @cartid";

                        using (var cmd = new NpgsqlCommand(copyToHistoryQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("historyid", historyId);
                            cmd.Parameters.AddWithValue("cartid", cartId);
                            cmd.ExecuteNonQuery();
                        }

                        // 5. Update product quantities and status
                        string updateProductQuantitiesQuery = @"
                            UPDATE produk 
                            SET stok = stok - subquery.quantity,
                                status = CASE 
                                    WHEN (stok - subquery.quantity) <= 0 THEN 'Habis'
                                    ELSE 'Tersedia'
                                END
                            FROM (
                                SELECT produkid, COUNT(*) as quantity 
                                FROM cartproduk 
                                WHERE cartid = @cartid 
                                GROUP BY produkid
                            ) as subquery 
                            WHERE produk.produkid = subquery.produkid";

                        using (var cmd = new NpgsqlCommand(updateProductQuantitiesQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("cartid", cartId);
                            cmd.ExecuteNonQuery();
                        }

                        // 6. Clear cart
                        string clearCartQuery = "DELETE FROM cartproduk WHERE cartid = @cartid";
                        using (var cmd = new NpgsqlCommand(clearCartQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("cartid", cartId);
                            cmd.ExecuteNonQuery();
                        }

                        // 7. Reset cart total
                        string resetCartTotalQuery = "UPDATE cart SET totalharga = 0 WHERE cartid = @cartid";
                        using (var cmd = new NpgsqlCommand(resetCartTotalQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("cartid", cartId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Failed to process order: " + ex.Message);
                    }
                }
            }
        }
    }
}
