using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using BCrypt.Net;
using System.Configuration;

namespace Demeter
{
    public class User
    {
        private int Id { get; set; }
        protected string username { get; set; }
        private string passwordHash { get; set; }
        private string email { get; set; }
        public string role { get; set; }
        public static string CurrentUsername { get; set; }

        public User()
        {

        }

        public Dictionary<string, string> GetPublicData(string username)
        {
            var publicData = new Dictionary<string, string>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT username, email FROM pengguna WHERE username = @username";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            publicData["username"] = reader.GetString(0);
                            publicData["email"] = reader.GetString(1);
                        }
                    }
                }
            }

            return publicData;
        }

        public User(string username, string password, string email, string role)
        {
            this.username = username;
            this.passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            this.email = email;
            this.role = role;
        }

        string connectionString = ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString;

        public void Register()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO pengguna (username, pass, email, role) VALUES (@username, @pass, @email, @Role)";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("username", username);
                        cmd.Parameters.AddWithValue("pass", passwordHash);  // You may want to hash the password before storing it.
                        cmd.Parameters.AddWithValue("email", email);
                        cmd.Parameters.AddWithValue("Role", role);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }
        public string Login(string username, string password)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT pass, role FROM pengguna WHERE username = @username";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("username", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPassword = reader.GetString(0);
                            string role = reader.GetString(1);

                            if ((username == "admin" && password == storedPassword) ||
                                BCrypt.Net.BCrypt.Verify(password, storedPassword))
                            {
                                User.CurrentUsername = username; // Set the current username
                                return role; // Return the user's role
                            }
                        }
                    }
                }
            }
            return null; // Return null if authentication fails
        }

        public List<Produk> LoadUserProducts()
        {
            List<Produk> products = new List<Produk>();
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
            SELECT 
                produkid, nama, deskripsi, harga, namatoko, status, stok, photourl
            FROM produk";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
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
                    Console.WriteLine("Error loading user products: " + ex.Message);
                    throw;
                }
            }
            return products;
        }

    }
}
