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
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void RegisterTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Create a new instance of LoginWindow
            RegisterWindow registerWindow = new RegisterWindow();

            // Show the LoginWindow
            registerWindow.Show();

            // Close the current RegisterWindow
            this.Close();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            User user = new User();
            string role = user.Login(username, password);

            if (role != null)
            {
                MessageBox.Show("Login successful!");

                if (role == "Customer")
                {
                    // Redirect to Customer Dashboard
                    CustomerDashboardWindow dashboard = new CustomerDashboardWindow();
                    dashboard.Show();
                }
                else if (role == "Seller")
                {
                    SellerDashboardWindow dashboard = new SellerDashboardWindow();
                    dashboard.Show();
                }
                else
                {
                    MessageBox.Show("Access denied. You do not have permission to access the customer or seller dashboard.");
                }

                // Close the Login window
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }
    }
}
