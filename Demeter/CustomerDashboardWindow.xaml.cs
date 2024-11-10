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
using System.Windows.Shapes;

namespace Demeter
{
    public partial class CustomerDashboardWindow : Window
    {
        private User currentUser;
        private Produk selectedProduct;

        public CustomerDashboardWindow()
        {
            InitializeComponent();
            LoadUserProducts();
        }

        private void LoadUserProducts()
        {
            currentUser = new User();
            List<Produk> products = currentUser.LoadUserProducts();
            ProductsGrid.Children.Clear();

            foreach (var product in products)
            {
                Border productBorder = new Border
                {
                    Width = 200,
                    Height = 250,
                    Background = new SolidColorBrush(Colors.LightGray),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(10),
                    Tag = product
                };

                productBorder.MouseLeftButtonDown += ShowProductDetails;

                StackPanel productPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Cursor = Cursors.Hand
                };

                Image productImage = new Image
                {
                    Source = string.IsNullOrEmpty(product.photoUrl)
                        ? new BitmapImage(new Uri("/Images/DefaultProduct.jpg", UriKind.Relative))
                        : new BitmapImage(new Uri(product.photoUrl)),
                    Width = 150,
                    Height = 150,
                    Margin = new Thickness(0, 10, 0, 10)
                };

                TextBlock productName = new TextBlock
                {
                    Text = product.namaProduk,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                TextBlock productPrice = new TextBlock
                {
                    Text = $"Rp{product.hargaProduk:N0}",
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                productPanel.Children.Add(productImage);
                productPanel.Children.Add(productName);
                productPanel.Children.Add(productPrice);

                productBorder.Child = productPanel;
                ProductsGrid.Children.Add(productBorder);
            }
        }

        private void ShowProductDetails(object sender, MouseButtonEventArgs e)
        {
            Border selectedProductBorder = sender as Border;
            Produk selectedProduct = selectedProductBorder.Tag as Produk;

            if (selectedProduct != null)
            {
                ProductNameTextBlock.Text = selectedProduct.namaProduk;
                ProductDescriptionTextBlock.Text = selectedProduct.deskripsiProduk;
                ProductPriceTextBlock.Text = $"Rp{selectedProduct.hargaProduk:N0}";
                ProductStockTextBlock.Text = $"Stok: {selectedProduct.stok}";
                ProductSellerTextBlock.Text = selectedProduct.namaToko;

                if (!string.IsNullOrEmpty(selectedProduct.photoUrl))
                {
                    ProductImage.Source = new BitmapImage(new Uri(selectedProduct.photoUrl));
                }
                else
                {
                    ProductImage.Source = new BitmapImage(new Uri("/Images/DefaultProduct.jpg", UriKind.Relative));
                }

                // Store the selected product in a field for later use
                this.selectedProduct = selectedProduct;

                ProductDetailsModal.Visibility = Visibility.Visible;
            }
        }

        private void CloseProductDetailsModal(object sender, RoutedEventArgs e)
        {
            ProductDetailsModal.Visibility = Visibility.Collapsed;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityTextBox.Text, out int quantity) && quantity > 0)
    {
                // Use the stored selected product
                Produk selectedProduct = this.selectedProduct;

                if (selectedProduct != null)
                {
                    Customer currentCustomer = new Customer();
                    currentCustomer.addToCart(selectedProduct, quantity);
                    MessageBox.Show($"Added {quantity} of {ProductNameTextBlock.Text} to cart.");
                }
                else
                {
                    MessageBox.Show("No product selected.");
                }
            }
            else
            {
                MessageBox.Show("Invalid quantity.");
            }
        }


        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide the ProductsGrid when navigating to the profile
            ProductsGrid.Visibility = Visibility.Collapsed;

            // Navigate to the CustomerProfile page in the CustomerFrame
            CustomerFrame.Navigate(new CustomerProfile());
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide the ProductsGrid when navigating to the profile
            ProductsGrid.Visibility = Visibility.Collapsed;

            // Navigate to the CustomerProfile page in the CustomerFrame
            CustomerFrame.Navigate(new CartPage());
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityTextBox.Text, out int stock))
            {
                QuantityTextBox.Text = (stock + 1).ToString();
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityTextBox.Text, out int stock) && stock > 0)
            {
                QuantityTextBox.Text = (stock - 1).ToString();
            }
        }
    }
}