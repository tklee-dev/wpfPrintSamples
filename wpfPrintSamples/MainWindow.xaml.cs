using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Path = System.IO.Path;

namespace wpfPrintSamples
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        string imgPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "sample_city.jpg");
        string testSavePath1 = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "1140537.jpg");
        string testSavePath2 = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "sample_city_test2.jpg");
        string printerDescription = "samplePic";

        //테스트용
        public static void SaveBitmapSource(string filePath, BitmapSource bs)
        {
            var image = bs;
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
        }


        private BitmapSource ImageLoad()
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;

            bi.UriSource = new Uri(testSavePath1);
            bi.EndInit();

            return bi;
        }

        private void Fit_Auto_FE(Visual v)
        {
            //https://stackoverflow.com/questions/7931961/wpf-printing-to-fit-page , seanzi

            System.Windows.FrameworkElement fe = v as System.Windows.FrameworkElement;
            if (fe == null)
                return;

            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                //store original scale
                Transform originalScale = fe.LayoutTransform;
                //get selected printer capabilities
                System.Printing.PrintCapabilities capabilities = pd.PrintQueue.GetPrintCapabilities(pd.PrintTicket);

                //get scale of the print wrt to screen of WPF visual
                double scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / fe.ActualWidth, capabilities.PageImageableArea.ExtentHeight /fe.ActualHeight);

                //Transform the Visual to scale
                fe.LayoutTransform = new ScaleTransform(scale, scale);

                //get the size of the printer page (일반적으로 UI가 더작기때문에 MIN임)
                System.Windows.Size sz = new System.Windows.Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);

                //update the layout of the visual to the printer page size.
                fe.Measure(sz);
                fe.Arrange(new System.Windows.Rect(new System.Windows.Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));

                //now print the visual to printer to fit on the one page.
                pd.PrintVisual(v, "SamplePic");

                //apply the original transform. FrameworkElement(UI)를 바로 사용하는 경우도 있기때문에.
                fe.LayoutTransform = originalScale;
            }
        }

        private BitmapSource Fit_Auto(BitmapSource bs)
        {
            //x SaveBitmapSource(testSavePath1, bs);
            PrintDialog pd = new PrintDialog();

            //get default printer capabilities
            System.Printing.PrintCapabilities capabilities = pd.PrintQueue.GetPrintCapabilities(pd.PrintQueue.DefaultPrintTicket);

            //get scale of the print wrt to screen of WPF visual 일반적으로 res사진이 더 크기때문에 MAX임
            double scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / bs.Width, capabilities.PageImageableArea.ExtentHeight / bs.Height);

            BitmapSource bi_t = new TransformedBitmap(bs, new ScaleTransform(scale, scale));
            //x SaveBitmapSource(testSavePath2, bi_t);            

            return bi_t;
        }

        private BitmapSource Rotate_Auto(BitmapSource bi)
        {
            if (bi.Width > bi.Height)
            {
                TransformedBitmap tb = new TransformedBitmap();
                tb.BeginInit();
                tb.Source = bi;

                RotateTransform rt = new RotateTransform(90);
                tb.Transform = rt;
                tb.EndInit();
                return tb;

            }
            else
            {
                return bi;
            }
        }

        private Visual BitmapSource2Visual(BitmapSource bi)
        {
            var vi = new DrawingVisual();
            var dc = vi.RenderOpen();
            dc.DrawImage(bi, new Rect { Width = bi.Width, Height = bi.Height });
            dc.Close();

            return vi;
        }

        private void Print_Dialog(Visual vi)
        {
            var pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                pd.PrintVisual(vi, printerDescription);
            }
        }

        private void Print_NonDialog(BitmapSource bs)
        {
            PrintDialog pd = new PrintDialog();
            System.Printing.PrintCapabilities capabilities = pd.PrintQueue.GetPrintCapabilities(pd.PrintQueue.DefaultPrintTicket);
            Visual vi  = BitmapSource2Visual(bs);
            pd.PrintVisual(vi, printerDescription);
        }

        //이미지로드
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            image.Source = ImageLoad();
        }

        //출력
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource bs = ImageLoad();
            Visual vi = BitmapSource2Visual(bs);
            Print_Dialog(vi);
        }

        //Autofit 출력
        private void button3_Click(object sender, RoutedEventArgs ea)
        {
            Fit_Auto_FE(image);
        }

        //AutoRotate
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource bi = ImageLoad();
            bi = Rotate_Auto(bi);
            bi = Fit_Auto(bi);
            Print_NonDialog(bi);
        }
    }
}
