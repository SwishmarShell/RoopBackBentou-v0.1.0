using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.ComponentModel;
using System.IO;
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
    
    // Gate state variable
    private GateState _gateState = GateState.Closed;
    private double _openThreshold = 20.0;
    private double _closeThreshold = 12.0;
    // Timing variables for gate state transitions
    private DateTime _openStartTime;
    private DateTime _closeStartTime;
    private int _openDelayMs = 50;
    private int _closeDelayMs = 500;
    
    public MainWindow()
    {
        InitializeComponent();

        LoadIni();

        UpdateCurrentDevices();

        OpenThresholdBox.Text =
            _openThreshold.ToString();

        CloseThresholdBox.Text =
            _closeThreshold.ToString();
        
        OpenDelayBox.Text =
            _openDelayMs.ToString();
        
        CloseDelayBox.Text =
            _closeDelayMs.ToString();

        StartCapture();

        StartInputCapture();
    }
    // Gate state enumeration
    public enum GateState
    {
        Closed,
        Open
    }
    // Current device (Output & Input)
    private void UpdateCurrentDevices()
    {
        var enumerator =
        new MMDeviceEnumerator();

        var output =
        enumerator.GetDefaultAudioEndpoint(
        DataFlow.Render,
        Role.Multimedia);

        CurrentOutputText.Text =
        output.FriendlyName;

        var input =
        enumerator.GetDefaultAudioEndpoint(
        DataFlow.Capture,
        Role.Multimedia);

        CurrentInputText.Text =
        input.FriendlyName;
    }

    private void AudioDevices_Click(
        object sender,
        RoutedEventArgs e)
    {
        var window =
        new AudioDevicesWindow(this);

        window.Show();
    }

    // Method to update the gate state based on the peak difference
    private void UpdateGateState(double peakDiff)
    {
        if (_gateState == GateState.Closed)
        {
            if (peakDiff >= _openThreshold)
            {
                if (_openStartTime == DateTime.MinValue)
                {
                    _openStartTime = DateTime.Now;
                }

                if ((DateTime.Now - _openStartTime).TotalMilliseconds
                >= _openDelayMs)
                {
                    _gateState = GateState.Open;
                    _openStartTime = DateTime.MinValue;
                }
            }
            else
            {
                _openStartTime = DateTime.MinValue;
            }
        }
        if (_gateState == GateState.Open)
        {
            if (peakDiff <= _closeThreshold)
            {
                if (_closeStartTime == DateTime.MinValue)
                {
                    _closeStartTime = DateTime.Now;
                }

                if ((DateTime.Now - _closeStartTime).TotalMilliseconds
                >= _closeDelayMs)
                {
                    _gateState = GateState.Closed;
                    _closeStartTime = DateTime.MinValue;
                }
            }
            else
            {
                _closeStartTime = DateTime.MinValue;
            }
        }
    }

    private void StartInputCapture()
    {
        var enumerator =
        new MMDeviceEnumerator();

        var target =
        enumerator.GetDefaultAudioEndpoint(
        DataFlow.Capture,
        Role.Multimedia);

        inputCapture =
        new WasapiCapture(target);

        inputCapture.DataAvailable +=
        InputCapture_DataAvailable;

        inputCapture.StartRecording();
    }

    private void StartCapture()
    {
        capture =
        new WasapiLoopbackCapture();

        capture.DataAvailable +=
        Capture_DataAvailable;

        capture.StartRecording();
    }

    // AudioDvicesWindow Refresh Botton Click Event
    public void RefreshAudioInfo()
    {
        UpdateCurrentDevices();
    }

    public void RefreshAudioDevices()
    {
        // Test
        //MessageBox.Show("Start Refresh");

        if (inputCapture != null)
        {
            inputCapture.DataAvailable -=
            InputCapture_DataAvailable;

            inputCapture.StopRecording();
            capture.StopRecording();
        }

        if (capture != null)
        {
            capture.DataAvailable -=
            Capture_DataAvailable;

            inputCapture.Dispose();
            capture.Dispose();
        }


        inputCapture =
            new WasapiCapture(
            new MMDeviceEnumerator()
            .GetDefaultAudioEndpoint(
            DataFlow.Capture,
            Role.Multimedia));
        inputCapture.DataAvailable +=
        InputCapture_DataAvailable;


        capture =
            new WasapiLoopbackCapture();
        capture.DataAvailable +=
        Capture_DataAvailable;

        inputCapture.StartRecording();
        capture.StartRecording();

        // Test
        //MessageBox.Show("RefreshAudioDevices");
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

        Dispatcher.BeginInvoke(() =>
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
            // Update the gate state based on the peak difference
            UpdateGateState(peakDiff);

            GateStateText.Text =
                    _gateState.ToString();
            if (_gateState == GateState.Open)
            {
                GateStateText.Text = "OPEN";
                GateStateText.Foreground = Brushes.LimeGreen;
            }
            else
            {
                GateStateText.Text = "CLOSED";
                GateStateText.Foreground = Brushes.Red;
            }


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
    // Event handler for the Apply button click event
    private void ApplyButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (double.TryParse(OpenThresholdBox.Text, out double open))
            _openThreshold = open;

        if (double.TryParse(CloseThresholdBox.Text, out double close))
            _closeThreshold = close;

        if (int.TryParse(OpenDelayBox.Text, out int openDelay))
            _openDelayMs = openDelay;

        if (int.TryParse(CloseDelayBox.Text, out int closeDelay))
            _closeDelayMs = closeDelay;

        SaveIni();


        _openThreshold =
        double.Parse(OpenThresholdBox.Text);

        _closeThreshold =
        double.Parse(CloseThresholdBox.Text);

        _openDelayMs =
        int.Parse(OpenDelayBox.Text);

        _closeDelayMs =
        int.Parse(CloseDelayBox.Text);
    }

    private readonly string IniPath =
        Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "VoiceGate.ini");

    private void SaveIni()
    {
        var lines = new[]
        {
            "[VoiceGate]",
            $"OpenThreshold={_openThreshold}",
            $"CloseThreshold={_closeThreshold}",
            $"OpenDelay={_openDelayMs}",
            $"CloseDelay={_closeDelayMs}"
        };

        File.WriteAllLines(IniPath, lines);
    }

    private void LoadIni()
    {
        if (!File.Exists(IniPath))
            return;

        foreach (var line in File.ReadAllLines(IniPath))
        {
            if (line.StartsWith("OpenThreshold="))
                _openThreshold =
                    double.Parse(line.Split('=')[1]);

            if (line.StartsWith("CloseThreshold="))
                _closeThreshold =
                    double.Parse(line.Split('=')[1]);

            if(line.StartsWith("OpenDelay="))
                _openDelayMs =
                    int.Parse(line.Split('=')[1]);

            if(line.StartsWith("CloseDelay="))
                _closeDelayMs =
                    int.Parse(line.Split('=')[1]);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        SaveIni();

        MessageBox.Show("Closed [ RoopBackBentou v0.1.1 ]");
        if (capture != null)
        {
            capture.DataAvailable -= Capture_DataAvailable;
            capture.StopRecording();
            capture.Dispose();
        }

        if (inputCapture != null)
        {
            inputCapture.DataAvailable -= InputCapture_DataAvailable;
            inputCapture.StopRecording();
            inputCapture.Dispose();
        }

        base.OnClosed(e);
    }
}