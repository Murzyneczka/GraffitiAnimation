using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using ImageMagick;

namespace GraffitiAnimation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        private void SaveAsGif_Click(object sender, RoutedEventArgs e)
        {
            Canvas drawingCanvas = this.FindName("DrawingCanvas") as Canvas;
            if (drawingCanvas == null) return;

            int fps = 30; // Liczba klatek na sekundę
            double totalDuration = 3.0; // Całkowity czas animacji w sekundach (z XAML: 2.5 + 0.5)
            int totalFrames = (int)(totalDuration * fps);

            // Renderowanie każdej klatki
            for (int frame = 0; frame < totalFrames; frame++)
            {
                double time = frame / (double)fps;
                RenderFrame(drawingCanvas, time, frame);
            }

            // Połączenie w GIF za pomocą ImageMagick
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "convert",
                Arguments = $"-delay 10 -loop 0 frame_*.png output.gif",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process process = Process.Start(psi);
            process.WaitForExit();

            // Usunięcie tymczasowych plików
            for (int frame = 0; frame < totalFrames; frame++)
            {
                File.Delete($"frame_{frame:03d}.png");
            }

            MessageBox.Show("Plik output.gif został wygenerowany!");
        }

        private void RenderFrame(Canvas canvas, double time, int frameIndex)
        {
            // Tworzenie kopii wizualnej sceny
            canvas.UpdateLayout();
            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)canvas.ActualWidth, (int)canvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(canvas);

            // Zapis do pliku
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using (var stream = File.Create($"frame_{frameIndex:03d}.png"))
            {
                encoder.Save(stream);
            }
        }
    }
}
