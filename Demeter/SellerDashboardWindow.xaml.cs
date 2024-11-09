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
    /// <summary>
    /// Interaction logic for CustomerDashboardWindow.xaml
    /// </summary>
    public partial class SellerDashboardWindow : Window
    {
        private Seller currentSeller;
        public SellerDashboardWindow()
        {
            InitializeComponent();
            LoadSellerProducts();
        }

        private void ProfileButton_Click(object sender, MouseButtonEventArgs e)
        {
            // Navigate to the CustomerProfile page in the MainContentFrame
            SellerFrame.Navigate(new SellerProfile());
        }

        private void ShowAddProductModal(object sender, MouseButtonEventArgs e)
        {
            // Show the Add Product modal
            AddProductModal.Visibility = Visibility.Visible;
        }

        private void CloseAddProductModal(object sender, RoutedEventArgs e)
        {
            // Hide the Add Product modal
            AddProductModal.Visibility = Visibility.Collapsed;
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Produk newProduk = new Produk
                {
                    namaProduk = ProductNameTextBox.Text,
                    deskripsiProduk = ProductDescriptionTextBox.Text,
                    hargaProduk = double.Parse(ProductPriceTextBox.Text),
                    photoUrl = ImageLinkTextBox.Text,
                    stok = int.Parse(ProductStockTextBox.Text)
                };

                Seller currentSeller = new Seller();
                currentSeller.addProduk(newProduk);

                // Update UI including profile picture
                if (!string.IsNullOrEmpty(ImageLinkTextBox.Text))
                {
                    var imageBrush = new ImageBrush();
                    var bitmapImage = new BitmapImage(new Uri(ImageLinkTextBox.Text));
                    imageBrush.ImageSource = bitmapImage;
                    ProductPictureRectangle.Fill = imageBrush;
                }


                MessageBox.Show("Product added successfully!");

                // Clear the textboxes
                ProductNameTextBox.Clear();
                ProductDescriptionTextBox.Clear();
                ProductPriceTextBox.Clear();
                ImageLinkTextBox.Clear();
                ProductStockTextBox.Text = "1";

                // Set the default product image
                var defaultImageBrush = new ImageBrush();
                var defaultBitmapImage = new BitmapImage(new Uri("pack://application:,,,/Images/DefaultProduct.jpg", UriKind.Absolute));
                defaultImageBrush.ImageSource = defaultBitmapImage;
                ProductPictureRectangle.Fill = defaultImageBrush;

                // Close the modal
                AddProductModal.Visibility = Visibility.Collapsed;

                // Optionally, refresh the product list or clear the input fields
                LoadSellerProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add product: " + ex.Message);
            }
        }

        private void IncreaseStock_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ProductStockTextBox.Text, out int stock))
            {
                ProductStockTextBox.Text = (stock + 1).ToString();
            }
        }

        private void DecreaseStock_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ProductStockTextBox.Text, out int stock) && stock > 0)
            {
                ProductStockTextBox.Text = (stock - 1).ToString();
            }
        }

        private void LoadSellerProducts()
        {
            currentSeller = new Seller();
            List<Produk> products = currentSeller.LoadSellerProducts();

            ProductsGrid.Children.Clear();

            foreach (var product in products)
            {
                Border productBorder = new Border
                {
                    Width = 200,
                    Height = 250,
                    Background = new SolidColorBrush(Colors.LightGray),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(10)
                };

                StackPanel productPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Image productImage = new Image
                {
                    Source = new BitmapImage(new Uri(product.photoUrl)),
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

            // Add the default "Add Product" button
            Border addProductBorder = new Border
            {
                Width = 200,
                Height = 250,
                Background = new SolidColorBrush(Colors.LightGray),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(10),
                Cursor = Cursors.Hand
            };
            addProductBorder.MouseLeftButtonDown += ShowAddProductModal;

            StackPanel addProductPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock addProductText = new TextBlock
            {
                Text = "+",
                FontSize = 48,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            addProductPanel.Children.Add(addProductText);
            addProductBorder.Child = addProductPanel;

            ProductsGrid.Children.Add(addProductBorder);
        }
    }
}
