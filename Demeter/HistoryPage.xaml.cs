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
    /// Interaction logic for HistoryPage.xaml
    /// </summary>
    public partial class HistoryPage : Page
    {
        public HistoryPage()
        {
            InitializeComponent();
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
    }
}
