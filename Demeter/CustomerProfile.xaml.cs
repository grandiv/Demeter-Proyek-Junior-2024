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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Demeter
{
    /// <summary>
    /// Interaction logic for CustomerProfile.xaml
    /// </summary>
    public partial class CustomerProfile : Page
    {
        private Customer currentCustomer;

        public CustomerProfile()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            if (!string.IsNullOrEmpty(User.CurrentUsername))
            {
                currentCustomer = new Customer();
                currentCustomer.LoadCustomerData(User.CurrentUsername);
                var publicData = currentCustomer.GetPublicData(User.CurrentUsername);

                UsernameTextBlock.Text = publicData.ContainsKey("username") ? publicData["username"] : "JohnDoe";
                EmailTextBlock.Text = publicData.ContainsKey("email") ? publicData["email"] : "johndoe@gmail.com";
                NamaTextBlock.Text = !string.IsNullOrEmpty(currentCustomer.nama) ? currentCustomer.nama : "Unknown";
                TeleponTextBlock.Text = currentCustomer.noTelp != 0 ? currentCustomer.noTelp.ToString() : "Unknown";
                AlamatTextBlock.Text = !string.IsNullOrEmpty(currentCustomer.alamatPengiriman) ? currentCustomer.alamatPengiriman : "Unknown";
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            NamaTextBlock.Visibility = Visibility.Collapsed;
            TeleponTextBlock.Visibility = Visibility.Collapsed;
            AlamatTextBlock.Visibility = Visibility.Collapsed;

            NamaTextBox.Text = NamaTextBlock.Text;
            TeleponTextBox.Text = TeleponTextBlock.Text;
            AlamatTextBox.Text = AlamatTextBlock.Text;

            NamaTextBox.Visibility = Visibility.Visible;
            TeleponTextBox.Visibility = Visibility.Visible;
            AlamatTextBox.Visibility = Visibility.Visible;

            EditButton.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int noTelp = int.Parse(TeleponTextBox.Text);
                currentCustomer.editProfile(
                    NamaTextBox.Text,
                    noTelp,
                    AlamatTextBox.Text
                );

                // Update UI
                NamaTextBlock.Text = NamaTextBox.Text;
                TeleponTextBlock.Text = TeleponTextBox.Text;
                AlamatTextBlock.Text = AlamatTextBox.Text;

                // Reset visibility
                NamaTextBlock.Visibility = Visibility.Visible;
                TeleponTextBlock.Visibility = Visibility.Visible;
                AlamatTextBlock.Visibility = Visibility.Visible;

                NamaTextBox.Visibility = Visibility.Collapsed;
                TeleponTextBox.Visibility = Visibility.Collapsed;
                AlamatTextBox.Visibility = Visibility.Collapsed;

                SaveButton.Visibility = Visibility.Collapsed;
                EditButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Collapsed;

                MessageBox.Show("Profile updated successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating profile: " + ex.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NamaTextBlock.Visibility = Visibility.Visible;
            TeleponTextBlock.Visibility = Visibility.Visible;
            AlamatTextBlock.Visibility = Visibility.Visible;

            NamaTextBox.Visibility = Visibility.Collapsed;
            TeleponTextBox.Visibility = Visibility.Collapsed;
            AlamatTextBox.Visibility = Visibility.Collapsed;

            SaveButton.Visibility = Visibility.Collapsed;
            EditButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Collapsed;

        }
    }
}
