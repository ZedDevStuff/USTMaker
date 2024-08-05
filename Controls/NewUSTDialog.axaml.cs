using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Xml.Linq;

namespace USTMaker;

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