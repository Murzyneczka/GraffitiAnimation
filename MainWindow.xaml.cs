using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await System.Threading.Tasks.Task.Delay(3500); // Czekaj na zakończenie animacji
            CaptureAnimationToGif(DrawingCanvas, "output.gif", 100, 3);
            Close(); // Zamknij aplikację po wygenerowaniu GIF-a
        }

        private void SaveAsGif_Click(object sender, RoutedEventArgs e)
        {
            CaptureAnimationToGif(DrawingCanvas, "output.gif", 100, 3);
        }

        private void CaptureAnimationToGif(Canvas canvas, string outputPath, int frameDelayMs, double animationDurationSeconds)
        {
            int width = (int)canvas.Width;
            int height = (int)canvas.Height;
            int totalFrames = (int)(animationDurationSeconds * 1000 / frameDelayMs);

            using (var collection = new MagickImageCollection())
            {
                for (int i = 0; i < totalFrames; i++)
                {
                    var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                    renderBitmap.Render(canvas);
                    using (var stream = new System.IO.MemoryStream())
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                        encoder.Save(stream);
                        stream.Position = 0;
                        var magickImage = new MagickImage(stream);
                        magickImage.AnimationDelay = frameDelayMs / 10;
                        collection.Add(magickImage);
                    }
                    System.Threading.Thread.Sleep(frameDelayMs);
                }
                collection[0].AnimationIterations = 0;
                collection.Optimize();
                collection.Write(outputPath);
            }
        }
    }
}