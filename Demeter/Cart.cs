using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Demeter
{
    internal class Cart
    {
        public int CartID { get; set; }
        public int CustID { get; set; }
        public List<CartItem> DaftarBelanja { get; set; } = new List<CartItem>();
        public double TotalHarga { get; set; }

        public class CartItem
        {
            public Produk Produk { get; set; }
            public int Quantity { get; set; }
            public int CartId { get; set; }
        }

        public void LoadCartData()
        {
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                string query = @"
                    SELECT p.produkid, p.nama, p.deskripsi, p.harga, p.photourl, 
                           c.cartid, COUNT(*) as quantity
                    FROM cart c
                    INNER JOIN cartproduk cp ON c.cartid = cp.cartid
                    INNER JOIN produk p ON cp.produkid = p.produkid
                    INNER JOIN customer cu ON c.custid = cu.custid
                    INNER JOIN pengguna pe ON cu.userid = pe.userid
                    WHERE pe.username = @username
                    GROUP BY p.produkid, p.nama, p.deskripsi, p.harga, p.photourl, c.cartid";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("username", User.CurrentUsername);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var produk = new Produk
                            {
                                produkID = reader.GetInt32(0),
                                namaProduk = reader.GetString(1),
                                deskripsiProduk = reader.GetString(2),
                                hargaProduk = reader.GetDouble(3),
                                photoUrl = reader.GetString(4)
                            };

                            var cartItem = new CartItem
                            {
                                Produk = produk,
                                CartId = reader.GetInt32(5),
                                Quantity = reader.GetInt32(6)
                            };

                            DaftarBelanja.Add(cartItem);
                        }
                    }
                }
            }
            CalcTotalHarga();
        }

        public void CalcTotalHarga()
        {
            TotalHarga = DaftarBelanja.Sum(item => item.Produk.hargaProduk * item.Quantity);
        }

        public void IncreaseQuantity(CartItem item)
        {
            if (!CanIncreaseQuantity(item))
            {
                throw new Exception("Stock limit reached");
            }

            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Insert new cart product
                        string query = "INSERT INTO cartproduk (cartid, produkid) VALUES (@cartid, @produkid)";
                        using (var cmd = new NpgsqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("cartid", item.CartId);
                            cmd.Parameters.AddWithValue("produkid", item.Produk.produkID);
                            cmd.ExecuteNonQuery();
                        }

                        // Update cart total
                        string updateCartQuery = "UPDATE cart SET totalharga = totalharga + @harga WHERE cartid = @cartid";
                        using (var cmd = new NpgsqlCommand(updateCartQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("harga", item.Produk.hargaProduk);
                            cmd.Parameters.AddWithValue("cartid", item.CartId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void DecreaseQuantity(CartItem item)
        {
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Delete one cart product - PostgreSQL doesn't support LIMIT in DELETE
                        string query = @"
                    DELETE FROM cartproduk 
                    WHERE ctid IN (
                        SELECT ctid 
                        FROM cartproduk 
                        WHERE cartid = @cartid AND produkid = @produkid 
                        LIMIT 1
                    )";
                        using (var cmd = new NpgsqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("cartid", item.CartId);
                            cmd.Parameters.AddWithValue("produkid", item.Produk.produkID);
                            cmd.ExecuteNonQuery();
                        }

                        // Update cart total
                        string updateCartQuery = "UPDATE cart SET totalharga = totalharga - @harga WHERE cartid = @cartid";
                        using (var cmd = new NpgsqlCommand(updateCartQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("harga", item.Produk.hargaProduk);
                            cmd.Parameters.AddWithValue("cartid", item.CartId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void DeleteFromCart(CartItem item)
        {
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Delete all instances of the product from cartproduk
                        string query = "DELETE FROM cartproduk WHERE cartid = @cartid AND produkid = @produkid";
                        using (var cmd = new NpgsqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("cartid", item.CartId);
                            cmd.Parameters.AddWithValue("produkid", item.Produk.produkID);
                            cmd.ExecuteNonQuery();
                        }

                        // Update cart total price
                        string updateCartQuery = "UPDATE cart SET totalharga = totalharga - @harga WHERE cartid = @cartid";
                        using (var cmd = new NpgsqlCommand(updateCartQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("harga", item.Produk.hargaProduk * item.Quantity);
                            cmd.Parameters.AddWithValue("cartid", item.CartId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool CanIncreaseQuantity(CartItem item)
        {
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                string query = "SELECT stok FROM produk WHERE produkid = @produkid";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("produkid", item.Produk.produkID);
                    var availableStock = (int)cmd.ExecuteScalar();
                    return item.Quantity < availableStock;
                }
            }
        }

        public int GetCurrentCartQuantity(int produkId)
        {
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                string query = @"
            SELECT COUNT(*) 
            FROM cartproduk cp
            INNER JOIN cart c ON cp.cartid = c.cartid
            INNER JOIN customer cu ON c.custid = cu.custid
            INNER JOIN pengguna p ON cu.userid = p.userid
            WHERE p.username = @username AND cp.produkid = @produkid";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("username", User.CurrentUsername);
                    cmd.Parameters.AddWithValue("produkid", produkId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
    }
}