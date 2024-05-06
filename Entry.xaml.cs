using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using UserControl = System.Windows.Controls.UserControl;

namespace USTMaker
{
    /// <summary>
    /// Interaction logic for Entry.xaml
    /// </summary>
    public partial class Entry : UserControl
    {
        public string Key
        {
            get => key.Text;
            set => key.Text = value;
        }
        public string Value
        {
            get => value.Text;
            set => this.value.Text = value;
        }
        private int Index;
        private USTEntry Parent;
        public Entry(string key, string value, int index, USTEntry parent)
        {
            InitializeComponent();
            Key = key;
            Value = value;
            Index = index;
            Parent = parent;
        }
        public void RefreshIndex()
        {
            int index = Parent.listBox.Items.IndexOf(this);
            if(index != -1)
            {
                Index = index;
            }
        }

        private void select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Audio Files | *.mp3; *.wav; *.ogg";
            bool? result = dialog.ShowDialog();
            if(result != null && result == true)
            {
                value.Text = dialog.FileName;
            }
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            Parent.DeleteEntry(Index);
        }

        private void key_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void value_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
