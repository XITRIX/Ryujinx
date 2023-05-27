using Microsoft.Extensions.Configuration;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.GraphicsDriver;
using Ryujinx.Common.Logging;
using Ryujinx.SDL2.Common;
using Ryujinx.Ui.Common;
using Ryujinx.Ui.Common.Configuration;
using Ryujinx.Ui.Common.Helper;

namespace Ryujunx.MAUI;

public partial class MainPage : ContentPage
{
    int count = 0;
    public static string ConfigurationPath { get; private set; }

    public MainPage()
    {
        InitializeComponent();
        Initialize();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);

    }

    //protected override async void OnAppearing()
    //{
    //    base.OnAppearing();

    //    await DisplayAlert("No prod keys", $"Put prod.keys file into path: {Path.Combine(AppDataManager.KeysDirPath, "prod.keys")}", "OK");
    //}

    private async void Initialize()
    {
        // Hook unhandled exception and process exit events.
        //AppDomain.CurrentDomain.UnhandledException += (sender, e) => ProcessUnhandledException(e.ExceptionObject as Exception, e.IsTerminating);
        //AppDomain.CurrentDomain.ProcessExit += (sender, e) => Exit();

        // Setup base data directory.
        AppDataManager.Initialize(FileSystem.Current.AppDataDirectory);

        // Initialize the configuration.
        ConfigurationState.Initialize();

        // Initialize the logger system.
        LoggerModule.Initialize();

        // Initialize SDL2 driver
        SDL2Driver.MainThreadDispatcher = action => MainThread.InvokeOnMainThreadAsync(action);

        ReloadConfig();

        // Enable OGL multithreading on the driver, when available.
        DriverUtilities.ToggleOGLThreading(ConfigurationState.Instance.Graphics.BackendThreading == BackendThreading.Off);



        // Check if keys exists.
        Appearing += async delegate {
            var path = Path.Combine(AppDataManager.KeysDirPath, "prod.keys");
            if (!File.Exists(path))
            {
                await Task.Delay(1000);

                await DisplayAlert("No prod keys", $"Put prod.keys file into path: {Path.Combine(AppDataManager.KeysDirPath, "prod.keys")}", "OK");
                FetchKeys();
            }
        };
    }

    public static void ReloadConfig()
    {
        string localConfigurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json");
        string appDataConfigurationPath = Path.Combine(AppDataManager.BaseDirPath, "Config.json");

        // Now load the configuration as the other subsystems are now registered
        if (File.Exists(localConfigurationPath))
        {
            ConfigurationPath = localConfigurationPath;
        }
        else if (File.Exists(appDataConfigurationPath))
        {
            ConfigurationPath = appDataConfigurationPath;
        }

        if (ConfigurationPath == null)
        {
            // No configuration, we load the default values and save it to disk
            ConfigurationPath = appDataConfigurationPath;

            ConfigurationState.Instance.LoadDefault();
            ConfigurationState.Instance.ToFileFormat().SaveConfig(ConfigurationPath);
        }
        else
        {
            if (ConfigurationFileFormat.TryLoad(ConfigurationPath, out ConfigurationFileFormat configurationFileFormat))
            {
                ConfigurationState.Instance.Load(configurationFileFormat, ConfigurationPath);
            }
            else
            {
                ConfigurationState.Instance.LoadDefault();

                Logger.Warning?.PrintMsg(LogClass.Application, $"Failed to load config! Loading the default config instead.\nFailed config location {ConfigurationPath}");
            }
        }

        // Check if graphics backend was overridden
        if (CommandLineState.OverrideGraphicsBackend != null)
        {
            if (CommandLineState.OverrideGraphicsBackend.ToLower() == "opengl")
            {
                ConfigurationState.Instance.Graphics.GraphicsBackend.Value = GraphicsBackend.OpenGl;
            }
            else if (CommandLineState.OverrideGraphicsBackend.ToLower() == "vulkan")
            {
                ConfigurationState.Instance.Graphics.GraphicsBackend.Value = GraphicsBackend.Vulkan;
            }
        }

        // Check if docked mode was overriden.
        if (CommandLineState.OverrideDockedMode.HasValue)
        {
            ConfigurationState.Instance.System.EnableDockedMode.Value = CommandLineState.OverrideDockedMode.Value;
        }

        // Check if HideCursor was overridden.
        if (CommandLineState.OverrideHideCursor is not null)
        {
            ConfigurationState.Instance.HideCursor.Value = CommandLineState.OverrideHideCursor!.ToLower() switch
            {
                "never" => HideCursorMode.Never,
                "onidle" => HideCursorMode.OnIdle,
                "always" => HideCursorMode.Always,
                _ => ConfigurationState.Instance.HideCursor.Value
            };
        }
    }

    private static void ProcessUnhandledException(Exception ex, bool isTerminating)
    {
        string message = $"Unhandled exception caught: {ex}";

        Logger.Error?.PrintMsg(LogClass.Application, message);

        if (Logger.Error == null)
        {
            Logger.Notice.PrintMsg(LogClass.Application, message);
        }

        if (isTerminating)
        {
            Exit();
        }
    }

    public static void Exit()
    {
        DiscordIntegrationModule.Exit();

        Logger.Shutdown();
    }

    async void FetchKeys() {
        var path = Path.Combine(AppDataManager.KeysDirPath, "prod.keys");

        var result = await FilePicker.Default.PickAsync(PickOptions.Default);
        if (result != null)
        {
            if (result.FileName.EndsWith("keys", StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(result.FullPath, path);
            }
            else
            {
                await DisplayAlert("Error", "Incorrect file format, it has to be prod.keys", "OK");
                FetchKeys();
            }
        }
    }
}


