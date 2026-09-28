# RoopBackBentou v0.1.0
<br>

## RoopBackBentou v0.1.1 Bug Fix Release
### 修正：
> アプリを閉じた後も処理がバックグラウンドに残る場合があった問題を修正。<br>
> オーディオメーターのUI更新処理を改善。<br>
### Download:[Latest Release](https://github.com/SwishmarShell/RoopBackBentou-v0.1.0/releases)
--- 
<br>

### Windows の 音声、ループバック出力を、モニタリング。

ソースコードは、Antelope Audio Zen Go USB オーディオデバイスの、3/4 （３と４のパスで、ステレオ音声）の出力用です。<br><br>
※ Windows Loopback のみ、モニタリングする場合、HD Audio Driver for Display Audio でも表示可能。<br><br>
※ v0.2.0 では、録音デバイスが、Realtec(R) Audio Stereo Mix でも可能にする予定。
## Features
>各出力：Peak、RMS 値のリアルタイム表示<br>
>　∟ Windowのオーディオ・ミキサーの、Loopback<br>
>　∟ Recording 3/4 の、Peak Max<br>
>
> Output から、Input RMS Diff / Peak Diff 各差分の、リアルタイム表示<br>
>
> Controls: (３ボタン＆音声デバイス表示)<br>
> - "Set Reference" (Reference固定表示)<br>
> - "Reset Peak Max"（Recording 3/4 の固定表示のリセット）
> - "Freeze Display"（押したタイミングの、全数値の固定表示)<br>
or "Resume Display"(固定表示の解除）<br>
>
 
## Screenshot
![](https://github.com/SwishmarShell/RoopBackBentou-v0.1.0/blob/master/LoopBackBentou.png "モニタリング画面")<br>

## Requirements
> Windows 10 / 11 (x64)<br>
> 使用している、オーディオ ドライバー<br>
> ※ ただし、Recording側のソースコード上は、Antelope Audio ZenGo USB オーディオインターフェース用の仕様<br>
> .NET 8 (もしくは、お使いのバージョン)<br>

## Other
````
A.）初回起動に、マイクの使用許可が求められる。＞許可する。
B.）後で、許可する場合。
    ＞Win キー を押し＞検索に ”マイク” と入力
    ＞マイクのセキュリティ を開き、起動したフォルダの（RoopBackBentou.exe)＞ON.にする。
　すると、次回から、起動します。

!! v0.1.0 は、タスクマネージャー＞詳細 に「RoopBackBentou」残り、完全に終了しません。!!
　 v0.1.1 をお使いください。
````
> ・オーディオインターフェース EQ等の出力で、音量レベルメーターのパネルに、Peak Hold 表示が無い場合に、便利。

> [ 開発の目的 ]<br>
> - Windows 11 Insider Experimental Preview<br>
> ビルド 26340.9502　(2026/9/18)<br>
> この更新の直後でした。<br>

> > ・USB オーディオ（ASIO）3/4 出力とパスで、繋げていた、<br>
> >DAW（SONER）録音待機モニターで確認中、<br>
> >-4.0 dB が最大入力（実際の音声データ）であること。<br>

> >・同時に、USBオーディオインタフェース の EQ等 で音量を、レベルメーターで、Peak（Red）以上であったこと。<br>

> >・約-3.9 dB ～ 4.0 dB の音量が、何かの原因で、確実に、固定的に減衰している。

> - この現象を、内部の音声ストリームから、経路の途中で測定し、比較するためのモニタリングして、フィードバックした。<br>

Windowsオーディオ・ミキサー==**"loopBack"**==>> USBオーディオ==！**"Recording 3/4"**==>> DAW(SONER)<br>
! 実際の、Windowsサウンド設定画面では、出力側を、選択すると、モニタリングが有効。


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
foreach () で、限定的に、Zen Go の出力をターゲットしています。

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
````
+3 dB を越えると、表示が赤色になる。
## License
> - MIT License
