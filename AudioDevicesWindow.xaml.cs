using NAudio.CoreAudioApi;
using System.Windows;

namespace RoopBackBentou;

public partial class AudioDevicesWindow : Window
{
    private MainWindow _mainWindow;

    public AudioDevicesWindow(MainWindow mainWindow)
    {
        InitializeComponent();

        _mainWindow = mainWindow;

        RefreshDevices();
    }

    private void RefreshDevices()
    {
        PlaybackText.Text = "";
        RecordingText.Text = "";

        var enumerator =
        new MMDeviceEnumerator();

        int playIndex = 0;

        foreach (var device in
        enumerator.EnumerateAudioEndPoints(
        DataFlow.Render,
        DeviceState.Active))
        {
            PlaybackText.Text +=
            $"{playIndex++}: {device.FriendlyName}\r\n";
        }

        int recIndex = 0;

        foreach (var device in
        enumerator.EnumerateAudioEndPoints(
        DataFlow.Capture,
        DeviceState.Active))
        {
            RecordingText.Text +=
            $"{recIndex++}: {device.FriendlyName}\r\n";
        }
    }

    private void Refresh_Click(
    object sender,
    RoutedEventArgs e)
    {
        RefreshDevices();

        _mainWindow.RefreshAudioInfo();

        _mainWindow.RefreshAudioDevices();
    }
}