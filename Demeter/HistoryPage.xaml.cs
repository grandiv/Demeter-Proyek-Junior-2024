using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Collections.Specialized.BitVector32;
using System.Configuration;

namespace Demeter
{
    /// <summary>
    /// Interaction logic for HistoryPage.xaml
    /// </summary>
    public partial class HistoryPage : Page
    {
        public HistoryPage()
        {
            InitializeComponent();
            LoadHistory();
        }

        private void HomeLogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CustomerDashboardWindow dashboard = new CustomerDashboardWindow();
            dashboard.Show();
            Window.GetWindow(this).Close();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the CustomerProfile page
            NavigationService.Navigate(new CustomerProfile());
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the CustomerProfile page
            NavigationService.Navigate(new CartPage());
        }
        private void LoadHistory()
        {
            HistoryItemsPanel.Children.Clear();

            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();

                // Get all history entries for current customer
                string historyQuery = @"
                SELECT 
    h.historyid, 
    h.tanggalbelanja, 
    h.status, 
    h.totalharga,
    s.namatoko as sellername
FROM history h
INNER JOIN seller s ON h.sellerid = s.sellerid
INNER JOIN customer c ON h.custid = c.custid
INNER JOIN pengguna p ON c.userid = p.userid
WHERE p.username = @username
ORDER BY h.tanggalbelanja DESC";

                using (var cmd = new NpgsqlCommand(historyQuery, conn))
                {
                    cmd.Parameters.AddWithValue("username", User.CurrentUsername);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Create border for each history entry
                            var historyBorder = new Border
                            {
                                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ebebeb")),
                                CornerRadius = new CornerRadius(10),
                                Margin = new Thickness(0, 0, 0, 10),
                                Padding = new Thickness(15)
                            };

                            var mainPanel = new StackPanel();

                            // Header panel
                            var headerPanel = new Grid();
                            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                            // Seller name
                            var sellerName = new TextBlock
                            {
                                Text = reader["sellername"].ToString(),
                                FontWeight = FontWeights.Bold
                            };
                            Grid.SetColumn(sellerName, 0);

                            // Status
                            var status = new TextBlock
                            {
                                Text = reader["status"].ToString(),
                                HorizontalAlignment = HorizontalAlignment.Right
                            };
                            Grid.SetColumn(status, 2);

                            headerPanel.Children.Add(sellerName);
                            headerPanel.Children.Add(status);

                            mainPanel.Children.Add(headerPanel);

                            // Products panel
                            var productsPanel = new StackPanel { Margin = new Thickness(0, 10, 0, 10) };

                            // Get products for this history entry
                            using (var connProducts = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
                            {
                                connProducts.Open();
                                string productsQuery = @"
                                SELECT 
    p.nama as namaproduk, 
    p.harga as hargaproduk, 
    p.photourl as gambar,
    COUNT(hp.produkid) as quantity
FROM historyproduk hp
INNER JOIN produk p ON hp.produkid = p.produkid
WHERE hp.historyid = @historyid
GROUP BY p.produkid, p.nama, p.harga, p.photourl";

                                using (var cmdProducts = new NpgsqlCommand(productsQuery, connProducts))
                                {
                                    cmdProducts.Parameters.AddWithValue("historyid", reader["historyid"]);

                                    using (var productsReader = cmdProducts.ExecuteReader())
                                    {
                                        while (productsReader.Read())
                                        {
                                            var productPanel = new Grid { Margin = new Thickness(0, 5, 0, 5) };
                                            productPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
                                            productPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                                            productPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                                            // Product image
                                            var image = new Image
                                            {
                                                Source = new BitmapImage(new Uri(productsReader["gambar"].ToString())),
                                                Width = 40,
                                                Height = 40
                                            };
                                            Grid.SetColumn(image, 0);

                                            // Product name
                                            var productName = new TextBlock
                                            {
                                                Text = productsReader["namaproduk"].ToString(),
                                                VerticalAlignment = VerticalAlignment.Center
                                            };
                                            Grid.SetColumn(productName, 1);

                                            // Quantity and price
                                            var quantityPrice = new TextBlock
                                            {
                                                Text = $"{productsReader["quantity"]}x @ Rp{productsReader["hargaproduk"]:N0}",
                                                VerticalAlignment = VerticalAlignment.Center,
                                                HorizontalAlignment = HorizontalAlignment.Right
                                            };
                                            Grid.SetColumn(quantityPrice, 2);

                                            productPanel.Children.Add(image);
                                            productPanel.Children.Add(productName);
                                            productPanel.Children.Add(quantityPrice);

                                            productsPanel.Children.Add(productPanel);
                                        }
                                    }
                                }
                            }

                            mainPanel.Children.Add(productsPanel);

                            // Footer with date
                            var orderDate = new TextBlock
                            {
                                Text = ((DateTime)reader["tanggalbelanja"]).ToString("dd MMMM yyyy HH:mm"),
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Margin = new Thickness(0, 5, 0, 0)
                            };
                            mainPanel.Children.Add(orderDate);

                            historyBorder.Child = mainPanel;
                            HistoryItemsPanel.Children.Add(historyBorder);
                        }
                    }
                }

            }
        }
    }
}
