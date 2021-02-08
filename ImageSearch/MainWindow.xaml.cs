using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
using Microsoft.Win32;
using OpenCvSharp;

namespace ImageSearch
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow
    {
        Bitmap bmpOrigin = null;
        Bitmap bmpFind = null;

        const double HIT_VAL = 0.9;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOriginImgLoad_Click(object sender, RoutedEventArgs e)
        {
            tbxOriginPath.Text = openFileImage(imgOrigin, ref bmpOrigin);
        }

        private void btnFindImgLoad_Click(object sender, RoutedEventArgs e)
        {
            tbxFindPath.Text = openFileImage(imgFind, ref bmpFind);
        }

        private string openFileImage(System.Windows.Controls.Image img, ref Bitmap bmp)
        {
            string result = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.PNG;*.BMP;*.JPG;*.GIF)|*.PNG;*.BMP;*.JPG;*.GIF|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = "C:\\Users";
            if (openFileDialog.ShowDialog() == true)
            {
                if (File.Exists(openFileDialog.FileName) == true)
                {
                    bmp = new Bitmap(openFileDialog.FileName);
                    img.Source = bmp2BitmapImage(bmp);
                    result = openFileDialog.FileName;
                }
                else result = "파일 찾지 못함";
            }
            else result = "파일 불러오기 취소";

            return result;
        }

        private BitmapImage bmp2BitmapImage(Bitmap bmp)
        {
            MemoryStream memory = new MemoryStream();
            bmp.Save(memory, ImageFormat.Bmp);
            memory.Position = 0; BitmapImage bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();
            return bitmapimage;
        }

        private string searchImg(Bitmap orgImg, Bitmap findImg)
        {
            /* TemplateMatchModes.SqDiff = 0
             * TemplateMatchModes.SqDiffNormed = 1
             * TemplateMatchModes.CCorr = 2
             * TemplateMatchModes.CCorrNormed = 3
             * TemplateMatchModes.CCoeff = 4
             * TemplateMatchModes.CCoeffNormed = 5
             */

            string result = "";

            if (bmpOrigin != null && bmpFind != null)
            {
                using (Mat originMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(orgImg))
                using (Mat findMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(findImg))
                using (Mat res = new Mat())
                {
                    if(cbxGray.IsChecked == true)
                    {
                        Mat grayorg = new Mat();
                        Mat grayfind = new Mat();
                        Cv2.CvtColor(originMat, grayorg, ColorConversionCodes.BGR2GRAY);
                        Cv2.CvtColor(findMat, grayfind, ColorConversionCodes.BGR2GRAY);
                        Cv2.MatchTemplate(grayorg, grayfind, res, TemplateMatchModes.CCoeffNormed);
                    }
                    else
                    {
                        Cv2.MatchTemplate(originMat, findMat, res, TemplateMatchModes.CCoeffNormed);
                    }

                    double minval, maxval = 0;
                    OpenCvSharp.Point minloc, maxloc;

                    Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);

                    if (maxval > numHit.Value * 0.01)
                    {
                        result += "minval : " + minval + "\n";
                        result += "maxval : " + maxval + "\n";
                        result += "minloc.X : " + minloc.X + "\n";
                        result += "minloc.Y : " + minloc.Y + "\n";
                        result += "maxloc.X : " + maxloc.X + "\n";
                        result += "maxloc.Y : " + maxloc.Y + "\n";
                        result += "res.Rows : " + res.Rows + "\n";
                        result += "res.Cols : " + res.Cols + "\n";
                        result += "originMat.Rows : " + originMat.Rows + "\n";
                        result += "originMat.Cols : " + originMat.Cols + "\n";
                        result += "findMat.Rows : " + findMat.Rows + "\n";
                        result += "findMat.Cols : " + findMat.Cols + "\n\n";
                        Cv2.FloodFill(res, maxloc, new Scalar());
                        imgOrigin.Source = bmp2BitmapRectImage(bmpOrigin, maxloc.X, maxloc.Y, bmpFind.Width, bmpFind.Height);
                    }
                    else
                    {
                        result += "NOT FOUND";
                        imgOrigin.Source = bmp2BitmapImage(bmpOrigin);
                    }

                    return result;
                }
            }
            else if (bmpOrigin == null && bmpFind == null)
            {
                result = "원본/찾는 이미지 상태 : NULL";
            }
            else if(bmpOrigin == null)
            {
                result = "원본 이미지 상태 : NULL";
            }
            else if(bmpFind == null)
            {
                result = "찾는 이미지 상태 : NULL";
            }

            return result;
        }

        private BitmapImage bmp2BitmapRectImage(Bitmap bmp, int x, int y, int w, int h)
        {
            Bitmap bmpgr = new Bitmap(bmp);
            Graphics bitmapGraphics = Graphics.FromImage(bmpgr);
            System.Drawing.Pen pn = new System.Drawing.Pen(System.Drawing.Color.Red, 10);
            bitmapGraphics.DrawRectangle(pn, x, y, w, h);
            return bmp2BitmapImage(bmpgr);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            tbxOutput.Text = searchImg(bmpOrigin, bmpFind);

            sw.Stop();

            tbxSpeed.Text = sw.ElapsedMilliseconds.ToString() + " ms";
        }

        private void tbxOutput_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }
    }
}
