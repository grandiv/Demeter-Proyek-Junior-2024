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
using System.Configuration;

namespace Demeter
{
    /// <summary>
    /// Interaction logic for OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Page
    {
        public OrderPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void HomeLogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SellerDashboardWindow dashboard = new SellerDashboardWindow();
            dashboard.Show();
            Window.GetWindow(this).Close();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the SellerProfile page
            NavigationService.Navigate(new SellerProfile());
        }

        private void LoadOrders()
        {
            OrdersPanel.Children.Clear();
            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                string orderQuery = @"
                    SELECT 
                        h.historyid,
                        h.tanggalbelanja,
                        h.status,
                        h.totalharga,
                        c.nama as customername,
                        c.alamatpengiriman
                    FROM history h
                    INNER JOIN customer c ON h.custid = c.custid
                    INNER JOIN pengguna p ON p.userid = (
                        SELECT userid FROM seller WHERE sellerid = h.sellerid
                    )
                    WHERE p.username = @username
                    ORDER BY h.tanggalbelanja DESC";

                using (var cmd = new NpgsqlCommand(orderQuery, conn))
                {
                    cmd.Parameters.AddWithValue("username", User.CurrentUsername);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Border orderBorder = new Border
                            {
                                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5")),
                                CornerRadius = new CornerRadius(8),
                                Margin = new Thickness(0, 0, 0, 20),
                                Padding = new Thickness(20)
                            };

                            Grid orderGrid = new Grid();
                            orderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            orderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                            // Left side - Customer info and price
                            StackPanel leftPanel = new StackPanel();

                            TextBlock customerName = new TextBlock
                            {
                                Text = reader["customername"].ToString(),
                                FontSize = 16,
                                FontWeight = FontWeights.Bold,
                                Margin = new Thickness(0, 0, 0, 5)
                            };

                            TextBlock address = new TextBlock
                            {
                                Text = reader["alamatpengiriman"].ToString(),
                                Margin = new Thickness(0, 0, 0, 5),
                                TextWrapping = TextWrapping.Wrap
                            };

                            TextBlock price = new TextBlock
                            {
                                Text = $"Rp{reader["totalharga"]:N0}",
                                FontWeight = FontWeights.Bold,
                                VerticalAlignment = VerticalAlignment.Center
                            };

                            leftPanel.Children.Add(customerName);
                            leftPanel.Children.Add(address);
                            leftPanel.Children.Add(price);

                            // Right side - Status and date
                            StackPanel rightPanel = new StackPanel
                            {
                                HorizontalAlignment = HorizontalAlignment.Right
                            };

                            TextBlock status = new TextBlock
                            {
                                Text = reader["status"]?.ToString() ?? "Menunggu Konfirmasi",
                                FontWeight = FontWeights.Bold,
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#004b0c")),
                                Margin = new Thickness(0, 0, 0, 5),
                                HorizontalAlignment = HorizontalAlignment.Right
                            };

                            TextBlock date = new TextBlock
                            {
                                Text = ((DateTime)reader["tanggalbelanja"]).ToString("dd MMMM yyyy"),
                                HorizontalAlignment = HorizontalAlignment.Right,
                                Margin = new Thickness(0, 5, 0, 0)
                            };

                            rightPanel.Children.Add(status);
                            rightPanel.Children.Add(date);

                            Grid.SetColumn(leftPanel, 0);
                            Grid.SetColumn(rightPanel, 1);

                            orderGrid.Children.Add(leftPanel);
                            orderGrid.Children.Add(rightPanel);
                            orderBorder.Child = orderGrid;
                            OrdersPanel.Children.Add(orderBorder);

                            orderBorder.MouseLeftButtonDown += ShowOrderDetails;
                            orderBorder.Tag = reader.GetInt32(0); // historyid
                            orderBorder.Cursor = Cursors.Hand;
                        }
                    }
                }
            }
        }

        private int currentOrderId;

        private void ShowOrderDetails(object sender, MouseButtonEventArgs e)
        {
            var orderBorder = (Border)sender;
            currentOrderId = (int)orderBorder.Tag;

            LoadOrderDetails();
            OrderDetailsModal.Visibility = Visibility.Visible;
        }

        private void CloseOrderDetailsModal(object sender, RoutedEventArgs e)
        {
            OrderDetailsModal.Visibility = Visibility.Collapsed;
        }

        private void LoadOrderDetails()
        {
            OrderItemsPanel.Children.Clear();

            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();

                // Get order details
                string orderQuery = @"
                    SELECT 
                        h.historyid,
                        h.tanggalbelanja,
                        h.status,
                        h.totalharga,
                        c.nama as customername,
                        c.alamatpengiriman
                    FROM history h
                    INNER JOIN customer c ON h.custid = c.custid
                    WHERE h.historyid = @historyid";

                using (var cmd = new NpgsqlCommand(orderQuery, conn))
                {
                    cmd.Parameters.AddWithValue("historyid", currentOrderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Set customer info
                            CustomerNameText.Text = reader["customername"].ToString();
                            AddressText.Text = reader["alamatpengiriman"].ToString();
                            PriceText.Text = $"Rp{reader.GetDouble(3):N0}";
                            OrderDateText.Text = ((DateTime)reader["tanggalbelanja"]).ToString("dd MMMM yyyy");

                            // Set current status
                            string currentStatus = reader["status"].ToString();
                            foreach (ComboBoxItem item in StatusComboBox.Items)
                            {
                                if (item.Content.ToString() == currentStatus)
                                {
                                    StatusComboBox.SelectedItem = item;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Get order items (rest of the code remains the same)
                string itemsQuery = @"
                    SELECT 
                        p.photourl,
                        p.nama,
                        p.harga,
                        COUNT(*) as quantity
                    FROM historyproduk hp
                    INNER JOIN produk p ON hp.produkid = p.produkid
                    WHERE hp.historyid = @historyid
                    GROUP BY p.produkid, p.photourl, p.nama, p.harga";

                using (var cmd = new NpgsqlCommand(itemsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("historyid", currentOrderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Grid itemGrid = new Grid();
                            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                            itemGrid.Margin = new Thickness(0, 0, 0, 10);

                            // Product image
                            Image productImage = new Image
                            {
                                Source = new BitmapImage(new Uri(reader["photourl"].ToString())),
                                Width = 50,
                                Height = 50
                            };
                            Grid.SetColumn(productImage, 0);

                            // Product info
                            StackPanel productInfo = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
                            productInfo.Children.Add(new TextBlock
                            {
                                Text = reader["nama"].ToString(),
                                FontWeight = FontWeights.Bold
                            });
                            productInfo.Children.Add(new TextBlock
                            {
                                Text = $"Rp{reader.GetDouble(2):N0}"
                            });
                            Grid.SetColumn(productInfo, 1);

                            // Quantity
                            TextBlock quantity = new TextBlock
                            {
                                Text = $"x{reader["quantity"]}",
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            Grid.SetColumn(quantity, 2);

                            itemGrid.Children.Add(productImage);
                            itemGrid.Children.Add(productInfo);
                            itemGrid.Children.Add(quantity);

                            OrderItemsPanel.Children.Add(itemGrid);
                        }
                    }
                }
            }
        }

        private void SaveOrderChanges(object sender, RoutedEventArgs e)
        {
            string newStatus = ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString();

            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["AppConnectionString"].ConnectionString))
            {
                conn.Open();
                string updateQuery = "UPDATE history SET status = @status WHERE historyid = @historyid";

                using (var cmd = new NpgsqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("status", newStatus);
                    cmd.Parameters.AddWithValue("historyid", currentOrderId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Order status updated successfully!");
            CloseOrderDetailsModal(null, null);
            LoadOrders(); // Refresh the orders list
        }
    }
}
