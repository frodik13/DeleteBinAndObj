using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace DeleteBinAndObj;

public partial class MainWindow
{
    private string _path = "";
    private readonly List<string> _binDirectories = new();
    private readonly List<string> _objDirectories = new();
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Browse_OnClick(object sender, RoutedEventArgs e)
    {
        var pathOpened = new OpenFolderDialog();
        if (pathOpened.ShowDialog() == false) return;

        var path = pathOpened.FolderName;

        PathTextBox.Text = path;
        _path = path;
    }

    private void Accept_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_path)) return;
        var directories = GetAllDirectories(_path);

        _binDirectories.Clear();
        _objDirectories.Clear();

        BinListBox.ItemsSource = null;
        ObjListBox.ItemsSource = null;

        _binDirectories.AddRange(directories.Where(x => x.EndsWith("bin")).ToList());
        _objDirectories.AddRange(directories.Where(x => x.EndsWith("obj")).ToList());

        BinListBox.ItemsSource = _binDirectories;
        ObjListBox.ItemsSource = _objDirectories;
    }

    private List<string> GetAllDirectories(string parentPath)
    {
        var result = new List<string>();
        try
        {
            var directories = Directory.GetDirectories(parentPath);
            foreach (var d in directories)
            {
                result.Add(d);
                var childrenDirectories = Directory.GetDirectories(d);
                if (childrenDirectories.Length <= 0) continue;
            
                foreach (var childrenDirectory in childrenDirectories)
                {
                    result.Add(childrenDirectory);
                    result.AddRange(GetAllDirectories(childrenDirectory));
                }
            }
        }
        catch{ }

        return result;
    }

    private void DeleteBin_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;

        if (_binDirectories.Count == 0) return;

        button.IsEnabled = false;

        foreach (var directory in _binDirectories)
        {
            DeleteDirectories(directory);
            Directory.Delete(directory);
        }

        _binDirectories.Clear();
        BinListBox.ItemsSource = null;

        button.IsEnabled = true;
    }

    private void DeleteObj_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;

        if (_objDirectories.Count == 0) return;

        button.IsEnabled = false;

        foreach (var directory in _objDirectories)
        {
            DeleteDirectories(directory);
            Directory.Delete(directory);
        }

        _objDirectories.Clear();
        ObjListBox.ItemsSource = null;

        button.IsEnabled = true;
    }

    private void DeleteDirectories(string parentPath)
    {
        //parentPath = parentPath.Trim();
        try
        {
            var files = Directory.GetFiles(parentPath);
            foreach (var file in files)
            {
                File.Delete(EnableLongPaths(file));
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        try
        {
            var childrenDirectories = Directory.GetDirectories(parentPath);
            foreach (var childrenDirectory in childrenDirectories)
            {
                DeleteDirectories(EnableLongPaths(childrenDirectory));
                Directory.Delete(EnableLongPaths(childrenDirectory));
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    private string EnableLongPaths(string path)
    {
        if (!path.StartsWith(@"\\?\"))
        {
            path = @"\\?\" + path;
        }
        return path;
    }
}