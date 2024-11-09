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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void LoginTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Create a new instance of LoginWindow
            LoginWindow loginWindow = new LoginWindow();

            // Show the LoginWindow
            loginWindow.Show();

            // Close the current RegisterWindow
            this.Close();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            // Assuming TextBoxes for username, email, password, and ComboBox for role.
            string username = UsernameTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;
            string role = ((ComboBoxItem)RoleComboBox.SelectedItem).Content.ToString();

            User user = new User(username, password, email, role);
            user.Register();

            MessageBox.Show("User registered successfully!");

            // Create a new instance of LoginWindow
            LoginWindow loginWindow = new LoginWindow();

            // Show the LoginWindow
            loginWindow.Show();

            // Close the current RegisterWindow
            this.Close();
        }
    }
}