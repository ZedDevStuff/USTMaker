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

namespace USTMaker
{
    /// <summary>
    /// Interaction logic for NewUSTDialog.xaml
    /// </summary>
    public partial class NewUSTDialog : Window
    {
        public record USTDialogResult(string name, string author, string description, bool done);
        public USTDialogResult Result = new("", "", "", false);
        public NewUSTDialog()
        {
            InitializeComponent();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void done_Click(object sender, RoutedEventArgs e)
        {
            Result = new(name.Text, author.Text, description.Text, true);
            Close();
        }

    }
}
