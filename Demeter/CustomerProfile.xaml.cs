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
    /// Interaction logic for CustomerProfile.xaml
    /// </summary>
    public partial class CustomerProfile : Page
    {
        public CustomerProfile()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            if (!string.IsNullOrEmpty(User.CurrentUsername))
            {
                User user = new User();
                var publicData = user.GetPublicData(User.CurrentUsername);

                // Assuming TextBlocks for Username and Email are named UsernameTextBlock and EmailTextBlock
                UsernameTextBlock.Text = publicData.ContainsKey("username") ? publicData["username"] : "Unknown";
                EmailTextBlock.Text = publicData.ContainsKey("email") ? publicData["email"] : "Unknown";
                // Password remains as "**************"
            }
        }
    }
}
