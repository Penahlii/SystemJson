#nullable disable

using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using System.Xml.Linq;
using SystemJsonHW.Commands;
using SystemJsonHW.Models;
using SystemJsonHW.Services;

namespace SystemJsonHW.ViewModels.WindowViewModels;

public class MainWindowViewModel : NotificationService
{
    // private fields

    private object lockObject = new();
    private bool startBtnEnabled;
    private bool refreshBtnEnabled;
    private string timetxt;
    private bool isSingleChecked;
    private ObservableCollection<Car> cars;
    private ObservableCollection<Car> WillBeAddedCars;
    private DispatcherTimer timer;
    private TimeSpan elapsedTime;

    // public fields

    public string TimeTXT { get => this.timetxt; set { this.timetxt = value; OnPropertyChanged(); } }
    public bool IsSingleChecked { get => this.isSingleChecked; set { this.isSingleChecked = value; OnPropertyChanged(); } }
    public ObservableCollection<Car> Cars { get => this.cars; set { this.cars = value; OnPropertyChanged(); } }
    public bool StartBtnEnabled { get => this.startBtnEnabled; set { this.startBtnEnabled = value; OnPropertyChanged(); } }
    public bool RefreshBtnEnabled { get => this.refreshBtnEnabled; set { this.refreshBtnEnabled = value; OnPropertyChanged(); } }

    // Commands

    public ICommand? StartCommand { get; set; }
    public ICommand? RefreshCOmmand { get; set; }

    // Constructor

    public MainWindowViewModel()
    {
        this.Cars = new();

        this.IsSingleChecked = true;
        this.StartBtnEnabled = true;
        this.RefreshBtnEnabled = false;

        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;

        StartCommand = new SystemJsonHW.Commands.RelayCommand(start);
        RefreshCOmmand = new RelayCommand(refresh);
    }

    // Methods

    public void start(object? parameter)
    {
        if (IsSingleChecked)
        {

            timer.Start();
            StartBtnEnabled = false;
            elapsedTime = TimeSpan.Zero;
            TimeTXT = "00:00:00";

            Thread thread = new(() =>
            {
                ProcessJsonFilesSingleMode();
            });
            thread.Start();
            timer.Stop();
            this.RefreshBtnEnabled = true;
        }
        else
        {
            timer.Start();
            StartBtnEnabled = false;
            elapsedTime = TimeSpan.Zero;
            TimeTXT = "00:00:00";
            ProcessJsonFileMultiMode();
            timer.Stop();
            this.RefreshBtnEnabled = true;
        }
    }

    public void refresh(object? parameter)
    {
        this.Cars = new();
        TimeTXT = "00:00:00";
        this.StartBtnEnabled = true;
        this.RefreshBtnEnabled = false;
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));
        TimeTXT = elapsedTime.ToString(@"hh\:mm\:ss");
    }

    private void ProcessJsonFilesSingleMode()
    {
        Thread.Sleep(6000);
        string folderName = "CarsJsonFies"; 
        string parentDirectory = Directory.GetParent(Environment.CurrentDirectory).FullName;
        string grandParentDirectory = Directory.GetParent(parentDirectory).FullName;
        string greatGrandParentDirectory = Directory.GetParent(grandParentDirectory).FullName;


        string folderPath = Path.Combine(greatGrandParentDirectory, folderName);

        string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

        foreach (string jsonFile in jsonFiles)
        {
            var jsonContent = File.ReadAllText(jsonFile);
            this.WillBeAddedCars = JsonSerializer.Deserialize<ObservableCollection<Car>>(jsonContent);
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var car in WillBeAddedCars)
                    this.Cars.Add(car);
            });
        }
    }

    private void ProcessJsonFileMultiMode()
    {
        string folderName = "CarsJsonFies";
        string parentDirectory = Directory.GetParent(Environment.CurrentDirectory).FullName;
        string grandParentDirectory = Directory.GetParent(parentDirectory).FullName;
        string greatGrandParentDirectory = Directory.GetParent(grandParentDirectory).FullName;


        string folderPath = Path.Combine(greatGrandParentDirectory, folderName);

        string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

        foreach (string jsonFile in jsonFiles)
        {
            Thread thread = new(() =>
            {
                var jsonContent = File.ReadAllText(jsonFile);
                this.WillBeAddedCars = JsonSerializer.Deserialize<ObservableCollection<Car>>(jsonContent);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var car in WillBeAddedCars)
                    {
                        lock (lockObject)
                        {
                            this.Cars.Add(car);
                        }
                    }
                });
            });
        }
    }
}