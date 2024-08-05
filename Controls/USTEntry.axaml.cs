using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;

namespace USTMaker;

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
        foreach (var e in entry.Value)
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
            if (item.GetType() == typeof(Entry))
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