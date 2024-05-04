using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using USTManager.Data;

namespace USTMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

        private void newUst_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new NewUSTDialog();
            dialog.ShowDialog();
            var result = dialog.Result; 
            if(result.done)
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

        private void openUst_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new();
            ofd.Title = "Select the .ust file to edit";
            ofd.Filter = "UST File | *.ust; *.ust.json";
            bool? result = ofd.ShowDialog();
            if(result != null && result == true)
            {
                DirectoryInfo? ustLocation = new FileInfo(ofd.FileName).Directory;
                CustomUST? ust = JsonConvert.DeserializeObject<CustomUST>(File.ReadAllText(ofd.FileName));
                if(ust != null && ustLocation != null)
                {
                    Dictionary<string, Dictionary<string, string>> levels = new();
                    foreach(var level in ust.Levels)
                    {
                        levels.Add(level.Key, new());
                        foreach(var entry in level.Value)
                        {
                            string target = entry.Key, path = entry.Value;
                            if(!File.Exists(path))
                            {
                                FileInfo? audio = ustLocation.GetFiles(path.Split(['\\', '/']).Last(), new EnumerationOptions() { RecurseSubdirectories = true}).FirstOrDefault();
                                if(audio != null)
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
        public void DrawUST()
        {
            object addButton = listBox.Items[listBox.Items.Count - 1];
            listBox.Items.Clear();
            foreach(var level in CurrentUST.Levels)
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
            foreach(var level in listBox.Items)
            {
                if(level.GetType() == typeof(USTEntry))
                {
                    string levelName = ((USTEntry)level).name.Text;
                    levels.Add(levelName, new());
                    foreach(var entry in ((USTEntry)level).listBox.Items)
                    {
                        if(entry.GetType() == typeof(Entry))
                        {
                            levels[levelName].Add(((Entry)entry).Key, ((Entry)entry).Value);
                        }
                    }
                }
            }
            CurrentUST.Levels = levels;
        }

        private void saveUst_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges();
            SaveFileDialog sfd = new();
            sfd.Title = "Select where to save the .ust file";
            sfd.Filter = "UST file | *.ust; *.ust.json";
            sfd.CreateTestFile = false;
            sfd.AddExtension = true;
            string sanitizedName = string.Join("", CurrentUST.Name.Split(Path.GetInvalidFileNameChars()));
            sfd.FileName = sanitizedName;
            bool? result = sfd.ShowDialog();
            if(result != null && result == true)
            {
                File.WriteAllText(sfd.FileName, JsonConvert.SerializeObject(CurrentUST, Formatting.Indented));
                MessageBox.Show("UST Saved");
            }
        }

        private void exportUst_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges();
            OpenFolderDialog ofd = new();
            ofd.Title = "Select where to create the UST folder";
            bool? result = ofd.ShowDialog();
            if(result != null && result == true)
            {
                string sanitizedName = string.Join("", CurrentUST.Name.Split(Path.GetInvalidPathChars()));
                string basePath = Path.Combine(ofd.FolderName, sanitizedName);
                string audioPath = Path.Combine(basePath, "audio");
                Directory.CreateDirectory(audioPath);
                List<string> alreadyAddedAudio = new();
                foreach(var level in CurrentUST.Levels)
                {
                    foreach(var entry in level.Value)
                    {
                        if(!alreadyAddedAudio.Contains(entry.Value))
                        {
                            FileInfo file = new(entry.Value);
                            if(file.Exists) File.Copy(entry.Value, Path.Combine(audioPath, file.Name));
                            alreadyAddedAudio.Add(entry.Value);
                        }
                    }
                }
                Dictionary<string, Dictionary<string, string>> finalLevels = new();
                foreach(var level in CurrentUST.Levels)
                {
                    finalLevels.Add(level.Key, new());
                    foreach(var entry in level.Value)
                    {
                        string relativePath = Path.Combine("audio", new FileInfo(entry.Value).Name);
                        finalLevels[level.Key].Add(entry.Key, relativePath);
                    }
                }
                CustomUST finalUST = new(CurrentUST.Name, CurrentUST.Author, CurrentUST.Description);
                finalUST.Levels = finalLevels;
                File.WriteAllText(Path.Combine(basePath, sanitizedName + ".ust"), JsonConvert.SerializeObject(finalUST, Formatting.Indented));

                MessageBox.Show("UST Exported");
            }
        }
        private void exportAsZip_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges();
            SaveFileDialog sfd = new();
            sfd.Title = "Select where to create the UST zip";
            sfd.Filter = "ZIP Archive | *.zip";
            sfd.AddExtension = true;
            string sanitizedName = string.Join("", CurrentUST.Name.Split(Path.GetInvalidFileNameChars()));
            sfd.FileName = sanitizedName + ".zip";
            bool? result = sfd.ShowDialog();
            if(result != null && result == true)
            {
                using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                using (ZipArchive archive = new(stream, ZipArchiveMode.Create))
                {
                    List<string> alreadyAddedAudio = new();
                    foreach(var level in CurrentUST.Levels)
                    {
                        foreach(var entry in level.Value)
                        {
                            if(!alreadyAddedAudio.Contains(entry.Value))
                            {
                                FileInfo file = new(entry.Value);
                                Debug.Assert(file.Exists);
                                if(file.Exists)
                                {
                                    archive.CreateEntryFromFile(file.FullName, Path.Combine("audio", file.Name));
                                }
                                alreadyAddedAudio.Add(entry.Value);
                            }
                        }
                    }
                    Dictionary<string, Dictionary<string, string>> finalLevels = new();
                    foreach(var level in CurrentUST.Levels)
                    {
                        finalLevels.Add(level.Key, new());
                        foreach(var entry in level.Value)
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
                    MessageBox.Show("UST Exported");
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