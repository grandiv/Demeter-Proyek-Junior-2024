﻿using System;
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
                    VerticalAlignment = VerticalAlignment.Center
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

                ProductDetailsModal.Visibility = Visibility.Visible;
            }
        }

        private void CloseProductDetailsModal(object sender, RoutedEventArgs e)
        {
            ProductDetailsModal.Visibility = Visibility.Collapsed;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            // Logic to add product to cart
            int quantity = int.TryParse(QuantityTextBox.Text, out int result) ? result : 1;

            // Add product and quantity to cart here (this might involve a Cart class or collection)
            MessageBox.Show($"Added {quantity} of {ProductNameTextBlock.Text} to cart.");
        }


        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide the ProductsGrid when navigating to the profile
            ProductsGrid.Visibility = Visibility.Collapsed;

            // Navigate to the CustomerProfile page in the CustomerFrame
            CustomerFrame.Navigate(new CustomerProfile());
        }
    }
}