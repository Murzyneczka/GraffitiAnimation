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
        }

        private void SaveAsGif_Click(object sender, RoutedEventArgs e)
        {
            CaptureAnimationToGif(DrawingCanvas, "output.gif", 100, 3);
        }

        private void CaptureAnimationToGif(Canvas canvas, string outputPath, int frameDelayMs, double animationDurationSeconds)
        {
            int width = (int)canvas.Width;
            int height = (int)canvas.Height;

            // Oblicz liczbę klatek
            int totalFrames = (int)(animationDurationSeconds * 1000 / frameDelayMs);

            using (var collection = new MagickImageCollection())
            {
                // Przechwytywanie klatek
                for (int i = 0; i < totalFrames; i++)
                {
                    // Renderuj klatkę
                    var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                    renderBitmap.Render(canvas);

                    // Zapisz klatkę do strumienia
                    using (var stream = new System.IO.MemoryStream())
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                        encoder.Save(stream);
                        stream.Position = 0;

                        // Dodaj klatkę do GIF-a
                        var magickImage = new MagickImage(stream);
                        magickImage.AnimationDelay = frameDelayMs / 10; // Magick.NET używa 1/100 sekundy
                        collection.Add(magickImage);
                    }

                    // Odczekaj przed kolejną klatką
                    System.Threading.Thread.Sleep(frameDelayMs);
                }

                // Ustaw zapętlanie GIF-a (0 = nieskończone)
                collection[0].AnimationIterations = 0;

                // Optymalizuj GIF-a
                collection.Optimize();

                // Zapisz GIF-a
                collection.Write(outputPath);
                MessageBox.Show($"GIF zapisany jako {outputPath}");
            }
        }
    }
}