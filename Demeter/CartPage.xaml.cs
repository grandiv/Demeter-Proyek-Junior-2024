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

namespace Demeter
{
    /// <summary>
    /// Interaction logic for CartPage.xaml
    /// </summary>
    public partial class CartPage : Page
    {
        private Cart currentCart;

        public CartPage()
        {
            InitializeComponent();
            LoadCart();
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

        private void LoadCart()
        {
            currentCart = new Cart();
            currentCart.LoadCartData();
            CartItemsPanel.Children.Clear();

            foreach (var cartItem in currentCart.DaftarBelanja)
            {
                Border productBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ebebeb")),
                    CornerRadius = new CornerRadius(10),
                    Margin = new Thickness(0, 0, 0, 10),
                    Padding = new Thickness(15)
                };

                Grid productGrid = new Grid();
                productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
                productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Image
                productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // Name
                productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Price
                productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // Quantity

                // Delete button
                Button deleteBtn = new Button
                {
                    Content = "🗑️",
                    Width = 30,
                    Height = 30,
                    Tag = cartItem,
                    Background = new SolidColorBrush(Colors.Transparent),
                    BorderThickness = new Thickness(0),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff3636")),
                    Cursor = Cursors.Hand
                };
                deleteBtn.Click += DeleteCartItem_Click;
                Grid.SetColumn(deleteBtn, 4);

                // Product Image
                Image productImage = new Image
                {
                    Source = new BitmapImage(new Uri(cartItem.Produk.photoUrl)),
                    Width = 80,
                    Height = 80,
                    Stretch = Stretch.Uniform
                };
                Grid.SetColumn(productImage, 0);

                // Product Name
                TextBlock nameBlock = new TextBlock
                {
                    Text = cartItem.Produk.namaProduk,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0)
                };
                Grid.SetColumn(nameBlock, 1);

                // Product Price
                TextBlock priceBlock = new TextBlock
                {
                    Text = $"Rp{cartItem.Produk.hargaProduk * cartItem.Quantity:N0}",
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 20, 0)
                };
                Grid.SetColumn(priceBlock, 2);

                // Quantity Controls
                StackPanel quantityPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Button decreaseBtn = new Button
                {
                    Content = "-",
                    Width = 30,
                    Height = 30,
                    Tag = cartItem
                };
                decreaseBtn.Click += DecreaseQuantity_Click;

                TextBlock quantityBlock = new TextBlock
                {
                    Text = cartItem.Quantity.ToString(),
                    Margin = new Thickness(10.0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                Button increaseBtn = new Button
                {
                    Content = "+",
                    Width = 30,
                    Height = 30,
                    Tag = cartItem
                };
                increaseBtn.Click += IncreaseQuantity_Click;

                quantityPanel.Children.Add(decreaseBtn);
                quantityPanel.Children.Add(quantityBlock);
                quantityPanel.Children.Add(increaseBtn);
                Grid.SetColumn(quantityPanel, 3);

                productGrid.Children.Add(deleteBtn);
                productGrid.Children.Add(productImage);
                productGrid.Children.Add(nameBlock);
                productGrid.Children.Add(priceBlock);
                productGrid.Children.Add(quantityPanel);

                productBorder.Child = productGrid;
                CartItemsPanel.Children.Add(productBorder);
            }

            TotalPriceText.Text = $"Total: Rp{currentCart.TotalHarga:N0}";
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var cartItem = (Cart.CartItem)button.Tag;
            try
            {
                currentCart.IncreaseQuantity(cartItem);
                LoadCart(); // Refresh the cart display
            }
            catch (Exception ex) when (ex.Message == "Stock limit reached")
            {
                MessageBox.Show($"Cannot add more items. Stock limit reached.",
                    "Stock Limit", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var cartItem = (Cart.CartItem)button.Tag;
            if (cartItem.Quantity > 1)
            {
                currentCart.DecreaseQuantity(cartItem);
                LoadCart(); // Refresh the cart display
            }
        }

        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement purchase functionality here
            MessageBox.Show("Purchase functionality will be implemented here.");
        }

        private void DeleteCartItem_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var cartItem = (Cart.CartItem)button.Tag;

            var result = MessageBox.Show(
                $"Are you sure you want to remove {cartItem.Produk.namaProduk} from your cart?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                currentCart.DeleteFromCart(cartItem);
                LoadCart(); // Refresh the cart display
            }
        }
    }
}
