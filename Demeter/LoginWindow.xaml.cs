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
            // Capture username and password from UI fields
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            // Create a new User instance and call the Login method
            User user = new User();
            bool isAuthenticated = user.Login(username, password);

            if (isAuthenticated)
            {
                MessageBox.Show("Login successful!");
                // Navigate to the main dashboard or another window
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }
    }
}
