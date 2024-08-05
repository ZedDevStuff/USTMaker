using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Windowing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using USTManager.Data;
using Avalonia.Platform.Storage;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System.Security.Cryptography;
using Microsoft.VisualBasic;

namespace USTMaker
{
    public partial class MainWindow : Window
    {
        private static MainWindow win;
        public MainWindow()
        {
            win = this;
            InitializeComponent();
            saveUst.IsEnabled = CurrentUST != null;
            exportUst.IsEnabled = CurrentUST != null;
            exportAsZip.IsEnabled = CurrentUST != null;
            addLevel.IsEnabled = CurrentUST != null;
        }
        private async void newUst_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new NewUSTDialog();
            await dialog.ShowDialog(this);
            var result = dialog.Result;
            if (result.done)
            {
                CurrentUST = new(result.name, result.author, result.description);
                CurrentUST.Levels = new(CustomUST.GetTemplate().Levels);
                Title = $"{CurrentUST.Name} by {CurrentUST.Author} - USTMaker";
                DrawUST();
            }
            saveUst.IsEnabled = CurrentUST != null;
            exportUst.IsEnabled = CurrentUST != null;
            exportAsZip.IsEnabled = CurrentUST != null;
            addLevel.IsEnabled = CurrentUST != null;
        }
        public static CustomUST? CurrentUST;

        public static FilePickerOpenOptions UstOptions = new()
        {
            Title = "Select a UST file",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("UST Files") { Patterns = ["*.ust", "*.ust.json"] }]
        };
        private async void openUst_Click(object sender, RoutedEventArgs e)
        {
            if(Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var result = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(UstOptions);
                if (result != null && result.Count == 1)
                {
                    DirectoryInfo? ustLocation = new FileInfo(result[0].Path.LocalPath).Directory;
                    CustomUST? ust = JsonConvert.DeserializeObject<CustomUST>(File.ReadAllText(result[0].Path.LocalPath));
                    if (ust != null && ustLocation != null)
                    {
                        Dictionary<string, Dictionary<string, string>> levels = new();
                        foreach (var level in ust.Levels)
                        {
                            levels.Add(level.Key, new());
                            foreach (var entry in level.Value)
                            {
                                string target = entry.Key, path = entry.Value;
                                if (!File.Exists(path))
                                {
                                    FileInfo? audio = ustLocation.GetFiles(path.Split(['\\', '/']).Last(), new EnumerationOptions() { RecurseSubdirectories = true }).FirstOrDefault();
                                    if (audio != null)
                                    {
                                        path = audio.FullName;
                                    }
                                }
                                levels[level.Key].Add(target, path);
                            }
                        }
                        ust.Levels = levels;
                        CurrentUST = ust;
                        Title = $"{CurrentUST.Name} by {CurrentUST.Author} - USTMaker";
                        DrawUST();
                    }
                }
                saveUst.IsEnabled = CurrentUST != null;
                exportUst.IsEnabled = CurrentUST != null;
                exportAsZip.IsEnabled = CurrentUST != null;
                addLevel.IsEnabled = CurrentUST != null;
            }
        }
        public void DrawUST()
        {
            object addButton = listBox.Items[listBox.Items.Count - 1];
            listBox.Items.Clear();
            foreach (var level in CurrentUST.Levels)
            {
                USTEntry entry = new(level);
                int target = listBox.Items.Count - 1;
                if (target < 0) target = 0;
                listBox.Items.Add(entry);
            }
            listBox.Items.Add(addButton);
        }
        public void ApplyChanges()
        {
            Dictionary<string, Dictionary<string, string>> levels = new();
            foreach (var level in listBox.Items)
            {
                if (level.GetType() == typeof(USTEntry))
                {
                    string levelName = ((USTEntry)level).name.Text;
                    levels.Add(levelName, new());
                    foreach (var entry in ((USTEntry)level).listBox.Items)
                    {
                        if (entry.GetType() == typeof(Entry))
                        {
                            levels[levelName].Add(((Entry)entry).Key, ((Entry)entry).Value);
                        }
                    }
                }
            }
            CurrentUST.Levels = levels;
        }
        public static FilePickerSaveOptions SaveUstOptions()
        {
            string sanitizedName = string.Join("", CurrentUST.Name.Split(Path.GetInvalidFileNameChars()));
            return new()
            {
                Title = "Select where to save the UST file",
                DefaultExtension = ".ust",
                SuggestedFileName = sanitizedName,
                FileTypeChoices = [new FilePickerFileType("UST Files") { Patterns = ["*.ust", "*.ust.json"] }]
            };
        }
        private async void saveUst_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges();
            if(Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var result = await desktop.MainWindow.StorageProvider.SaveFilePickerAsync(SaveUstOptions());
                if (result != null)
                {
                    await File.WriteAllTextAsync(result.Path.LocalPath, JsonConvert.SerializeObject(CurrentUST, Formatting.Indented));
                    //MessageBox.Show("UST Saved");
                }
            }
        }
        public static FilePickerSaveOptions ExportAsZipOptions()
        {
            string sanitizedName = string.Join("", CurrentUST.Name.Split(Path.GetInvalidFileNameChars()));
            return new()
            {
                Title = "Select where to save the UST zip",
                DefaultExtension = ".zip",
                SuggestedFileName = sanitizedName,
                FileTypeChoices = [new FilePickerFileType("ZIP Archive") { Patterns = ["*.zip"] }]
            };
        }
        public static FolderPickerOpenOptions ExportUstOptions = new()
        {
            Title = "Select where to create the UST folder",
            AllowMultiple = false
        };
        private async void exportUst_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges();
            if(Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var result = await desktop.MainWindow.StorageProvider.OpenFolderPickerAsync(ExportUstOptions);
                if (result != null && result.Count == 1)
                {
                    string sanitizedName = string.Join("", CurrentUST.Name.Split(Path.GetInvalidFileNameChars()));
                    string basePath = Path.Combine(result[0].Path.LocalPath, sanitizedName);
                    string audioPath = Path.Combine(basePath, "audio");
                    Directory.CreateDirectory(audioPath);
                    List<string> alreadyAddedAudio = new();
                    foreach (var level in CurrentUST.Levels)
                    {
                        foreach (var entry in level.Value)
                        {
                            if (!alreadyAddedAudio.Contains(entry.Value))
                            {
                                FileInfo file = new(entry.Value);
                                if (file.Exists) File.Copy(entry.Value, Path.Combine(audioPath, file.Name));
                                alreadyAddedAudio.Add(entry.Value);
                            }
                        }
                    }
                    Dictionary<string, Dictionary<string, string>> finalLevels = new();
                    foreach (var level in CurrentUST.Levels)
                    {
                        finalLevels.Add(level.Key, new());
                        foreach (var entry in level.Value)
                        {
                            string relativePath = Path.Combine("audio", new FileInfo(entry.Value).Name);
                            finalLevels[level.Key].Add(entry.Key, relativePath);
                        }
                    }
                    CustomUST finalUST = new(CurrentUST.Name, CurrentUST.Author, CurrentUST.Description);
                    finalUST.Levels = finalLevels;
                    await File.WriteAllTextAsync(Path.Combine(basePath, sanitizedName + ".ust"), JsonConvert.SerializeObject(finalUST, Formatting.Indented));
                    //MessageBox.Show("UST Exported");
                }
            }
        }
        private async void exportAsZip_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges();
            string sanitizedName = string.Join("", CurrentUST.Name.Split(Path.GetInvalidFileNameChars()));
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var result = await desktop.MainWindow.StorageProvider.SaveFilePickerAsync(ExportAsZipOptions());
                if (result != null)
                {
                    using (FileStream stream = new(result.Path.LocalPath, FileMode.Create))
                    using (ZipArchive archive = new(stream, ZipArchiveMode.Create))
                    {
                        List<string> alreadyAddedAudio = new();
                        foreach (var level in CurrentUST.Levels)
                        {
                            foreach (var entry in level.Value)
                            {
                                if (!alreadyAddedAudio.Contains(entry.Value))
                                {
                                    FileInfo file = new(entry.Value);
                                    if (file.Exists)
                                    {
                                        archive.CreateEntryFromFile(file.FullName, Path.Combine("audio", file.Name));
                                    }
                                    alreadyAddedAudio.Add(entry.Value);
                                }
                            }
                        }
                        Dictionary<string, Dictionary<string, string>> finalLevels = new();
                        foreach (var level in CurrentUST.Levels)
                        {
                            finalLevels.Add(level.Key, new());
                            foreach (var entry in level.Value)
                            {
                                string relativePath = Path.Combine("audio", new FileInfo(entry.Value).Name);
                                finalLevels[level.Key].Add(entry.Key, relativePath);
                            }
                        }
                        CustomUST finalUST = new(CurrentUST.Name, CurrentUST.Author, CurrentUST.Description);
                        finalUST.Levels = finalLevels;
                        var ustEntry = archive.CreateEntry(sanitizedName + ".ust");
                        byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(finalUST, Formatting.Indented));
                        ustEntry.Open().Write(buffer);
                        //new StreamWriter(ustEntry.Open()).Write(JsonConvert.SerializeObject(finalUST, Formatting.Indented));
                        //MessageBox.Show("UST Exported");
                    }
                }
            }
        }
        public static void DeleteLevel(USTEntry entry)
        {
            win.listBox.Items.Remove(entry);
        }
        private void addLevel_Click(object sender, RoutedEventArgs e)
        {
            listBox.Items.Insert(listBox.Items.Count - 1, new USTEntry());
        }
    }
}