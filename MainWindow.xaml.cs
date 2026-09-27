using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Windows;
using System.Windows.Media;

namespace RoopBackBentou;

public partial class MainWindow : Window
{
    private WasapiLoopbackCapture capture;

    private WasapiCapture inputCapture;

    private double currentPeakDb;
    private double currentRmsDb;

    private double inputPeakDb;
    private double inputRmsDb;

    private double inputPeakMaxDb = -100;

    private double referencePeakDb = double.NaN;
    private double referenceRmsDb = double.NaN;

    private bool freezeDisplay = false;

    public MainWindow()
    {
        InitializeComponent();

        ShowInputDevices();

        StartCapture();

        StartInputCapture();
    }

    private void ShowInputDevices()
    {
        DeviceListText.Text = "";

        var enumerator =
        new MMDeviceEnumerator();

        int index = 0;

        foreach (var device in
        enumerator.EnumerateAudioEndPoints(
        DataFlow.Capture,
        DeviceState.Active))
        {
            DeviceListText.Text +=
            $"{index++}: {device.FriendlyName}\r\n";
        }
    }

    private void StartCapture()
    {
        capture =
        new WasapiLoopbackCapture();

        capture.DataAvailable +=
        Capture_DataAvailable;

        capture.StartRecording();
    }

    private void StartInputCapture()
    {
        var enumerator =
        new MMDeviceEnumerator();

        MMDevice? target = null;

        foreach (var device in
        enumerator.EnumerateAudioEndPoints(
        DataFlow.Capture,
        DeviceState.Active))
        {
            if (device.FriendlyName.Contains("Recording 3/4"))
            {
                target = device;
                break;
            }
        }

        if (target == null)
        {
            MessageBox.Show(
            "Recording 3/4 が見つかりません");

            return;
        }

        inputCapture =
        new WasapiCapture(target);

        inputCapture.DataAvailable +=
        InputCapture_DataAvailable;

        inputCapture.StartRecording();
    }

    private void InputCapture_DataAvailable(
    object? sender,
    WaveInEventArgs e)
    {
        int samples =
        e.BytesRecorded / 4;

        float peak = 0;

        double sum = 0;

        for (int i = 0; i < samples; i++)
        {
            float sample =
            BitConverter.ToSingle(
            e.Buffer,
            i * 4);

            float abs =
            Math.Abs(sample);

            if (abs > peak)
                peak = abs;

            sum += sample * sample;
        }

        double rms =
        Math.Sqrt(sum / samples);

        inputPeakDb =
        peak > 0
        ? 20 * Math.Log10(peak)
        : -100;

        inputRmsDb =
        rms > 0
        ? 20 * Math.Log10(rms)
        : -100;

        if (inputPeakDb > inputPeakMaxDb)
        {
            inputPeakMaxDb =
            inputPeakDb;
        }
    }

    private void Capture_DataAvailable(
    object? sender,
    WaveInEventArgs e)
    {
        if (freezeDisplay)
            return;

        int samples =
        e.BytesRecorded / 4;

        float peak = 0;

        double sum = 0;

        for (int i = 0; i < samples; i++)
        {
            float sample =
            BitConverter.ToSingle(
            e.Buffer,
            i * 4);

            float abs =
            Math.Abs(sample);

            if (abs > peak)
                peak = abs;

            sum += sample * sample;
        }

        double rms =
        Math.Sqrt(sum / samples);

        currentPeakDb =
        peak > 0
        ? 20 * Math.Log10(peak)
        : -100;

        currentRmsDb =
        rms > 0
        ? 20 * Math.Log10(rms)
        : -100;

        Dispatcher.Invoke(() =>
        {
            PeakText.Text =
            $"{currentPeakDb:F1} dB";

            RmsText.Text =
            $"{currentRmsDb:F1} dB";

            InputPeakText.Text =
            $"{inputPeakDb:F1} dB";

            InputRmsText.Text =
            $"{inputRmsDb:F1} dB";

            InputPeakMaxText.Text =
            $"{inputPeakMaxDb:F1} dB";

            double rmsDiff =
            inputRmsDb - currentRmsDb;

            double peakDiff =
            inputPeakDb - currentPeakDb;

            IoDifferenceText.Text =
            $"{rmsDiff:+0.0;-0.0;0.0} dB";

            PeakDifferenceText.Text =
            $"{peakDiff:+0.0;-0.0;0.0} dB";

            IoDifferenceText.Foreground =
            Math.Abs(rmsDiff) > 3
            ? Brushes.Red
            : Brushes.Black;

            PeakDifferenceText.Foreground =
            Math.Abs(peakDiff) > 3
            ? Brushes.Red
            : Brushes.Black;
        });
    }

    private void ReferenceButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        referencePeakDb =
        currentPeakDb;

        referenceRmsDb =
        currentRmsDb;

        ReferencePeakText.Text =
        $"{referencePeakDb:F1} dB";

        ReferenceRmsText.Text =
        $"{referenceRmsDb:F1} dB";
    }

    private void ResetPeakMax_Click(
    object sender,
    RoutedEventArgs e)
    {
        inputPeakMaxDb = -100;
    }

    private void FreezeButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        freezeDisplay = !freezeDisplay;

        FreezeButton.Content =
        freezeDisplay
        ? "Resume Display"
        : "Freeze Display";
    }

    protected override void OnClosed(
    EventArgs e)
    {
        capture?.StopRecording();
        capture?.Dispose();

        inputCapture?.StopRecording();
        inputCapture?.Dispose();

        base.OnClosed(e);
    }
}