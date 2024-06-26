﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using UserControl = System.Windows.Controls.UserControl;

namespace USTMaker
{
    /// <summary>
    /// Interaction logic for USTEntry.xaml
    /// </summary>
    public partial class USTEntry : UserControl
    {
        public KeyValuePair<string, Dictionary<string, string>> Entry;
        public USTEntry()
        {
            Entry = new("new", new());
            InitializeComponent();
            name.Text = Entry.Key;
            int index = 0;
            foreach (var e in Entry.Value)
            {
                AddEntry(new(e.Key, e.Value, index, this));
                index++;
            }
        }
        public USTEntry(KeyValuePair<string, Dictionary<string, string>> entry)
        {
            Entry = entry;
            InitializeComponent();
            name.Text = entry.Key;
            int index = 0;
            foreach(var e in entry.Value)
            {
                AddEntry(new(e.Key, e.Value, index, this));
                index++;
            }
        }
        private void NewEntry(object sender, RoutedEventArgs e)
        {
            int target = listBox.Items.Count - 1;
            if (target <= 0) target = 0;
            Entry entry = new("", "", target, this);
            AddEntry(entry);
        }
        private void AddEntry(Entry entry)
        {
            int target = listBox.Items.Count - 1;
            if (target <= 0) target = 0;
            listBox.Items.Insert(target, entry);
        }
        public void DeleteEntry(int index)
        {
            listBox.Items.RemoveAt(index);
            foreach (object? item in listBox.Items)
            {
                if(item.GetType() == typeof(Entry))
                {
                    ((Entry)item).RefreshIndex();
                }
            }
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.DeleteLevel(this);
        }
    }
}
