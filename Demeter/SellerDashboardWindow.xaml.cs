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
        public SellerDashboardWindow()
        {
            InitializeComponent();
        }

        private void ProfileButton_Click(object sender, MouseButtonEventArgs e)
        {
            // Navigate to the CustomerProfile page in the MainContentFrame
            SellerFrame.Navigate(new SellerProfile());
        }

    }
}
