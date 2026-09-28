# RoopBackBentou v0.1.0
 Japanese Version => https://github.com/SwishmarShell/RoopBackBentou-v0.1.0/blob/master/JP_REDME.md
## Changelog
### v0.1.1
Fixed:
- Fixed an issue where the application process could remain in the background after closing.
- Improved audio meter UI update handling.

### Download:[Latest Release](https://github.com/SwishmarShell/RoopBackBentou-v0.1.0/releases)
--- 
### Monitoring Windows audio and loopback output.

RoopBackBentou is a real-time audio monitoring tool for comparing Windows Loopback output and recording input levels. It displays Peak, RMS, and level differences to help identify unexpected audio attenuation in the signal path.<br><br>
※ If you're only monitoring Windows Loopback, it can also be displayed with the HD Audio Driver for Display Audio.<br><br>
※ In v0.2.0, the plan is to also allow recording devices with Realtec(R) Audio Stereo Mix.
## Features
> - Real-time Peak and RMS monitoring<br>
> - Windows Loopback output monitoring<br>
> - Recording input monitoring (Zen Go Recording 3/4)<br>
> - Input vs Output RMS difference display<br>
> - Input vs Output Peak difference display<br>
> - Reference level capture<br>
> - Peak Hold reset<br>
> - Freeze / Resume display<br>
> - Automatic warning when level difference exceeds ±3 dB
 
## Screenshot
![](https://github.com/SwishmarShell/RoopBackBentou-v0.1.0/blob/master/LoopBackBentou.png "Check　Monitor")<br>

## Requirements
> Windows 10 / 11 (x64)<br>
> Using Audio Driver<br>
> ※ [Recording] Antelope Audio ZenGo USB  Only<br>
> .NET 8 (Using Version)<br>

## Other
````
 //// Fast Lunch UP ////

A.)  Mic Using > Permission
B.) Later than>
	Win Key > Surch "microphon" > App-specific microphone usage security \ Lunch Folder (RoopBackBentou.exe) > ON.

!! Excuse me, v0.1.0 does not close perfectly. >Task Manager > Details Checked !!
   If you don't mind, please use "v0.1.1."
````
> - Developed to investigate a fixed audio level attenuation observed after a Windows 11 Insider Preview update. The tool compares Windows Loopback output with the actual recording input signal to detect and visualize level differences in real time.<br>

> - It's handy when there's no Peak Hold display on the volume level meter panel for outputs like an audio interface or EQ.

> [ Specific objective ]<br>
> - Windows 11 Insider Experimental Preview<br>
> Build 26340.9502　(2026/9/18)<br>
> <br>
It was immediately after this update.
> > ・With the USB audio (ASIO) 3/4 output and pass-through connected,<br>
> >while checking on the DAW (SONER) in recording standby monitor,<br>
> >it is confirmed that -4.0 dB is the maximum input (actual audio data).<br>

> >・At the same time, the volume on the USB audio interface's EQ, etc., was above Peak (Red) on the level meter.<br>

> >・The volume of approximately -3.9 dB to 4.0 dB is consistently and fixedly attenuated due to some cause.

> - This phenomenon was monitored by measuring the internal audio stream along the path, and it was compared and fed back.<br>

WindowsAudioMIXER==**"loopBack"**==>> USB Audio==！**"Recording 3/4"**==>> DAW(SONER)<br>
! In the actual Windows sound settings screen, selecting the output side enables monitoring.


## Recording 3/4 Monitering (MainWindow.xaml.cs)
````
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
  ~~
}
````
Using foreach(), we are specifically targeting the output of Zen Go.

## Difference +3 dB < Color Red (MainWindow.xaml.cs )
````
private void Capture_DataAvailable(
object? sender,
WaveInEventArgs e)
{
  ~~
  Dispatcher.Invoke(() =>
{
  ~~
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
````
When it exceeds +3 dB, the display turns red.
## License
> - MIT License
