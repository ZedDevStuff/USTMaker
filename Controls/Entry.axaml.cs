using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

namespace USTMaker;

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
        if (index != -1)
        {
            Index = index;
        }
    }
    public static FilePickerOpenOptions AudioFilterOptions { get; set; } = new FilePickerOpenOptions()
    { 
        AllowMultiple = false,
        FileTypeFilter = [new FilePickerFileType("Audio Files") { Patterns = ["*.mp3", "*.wav", "*.ogg"] }]
    };
    private async void select_Click(object sender, RoutedEventArgs e)
    {
        if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var result = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(AudioFilterOptions);
            if (result.Count == 1)
            {
                value.Text = result[0].Path.LocalPath;
            }
        }
    }

    private void delete_Click(object sender, RoutedEventArgs e)
    {
        Parent.DeleteEntry(Index);
    }
}