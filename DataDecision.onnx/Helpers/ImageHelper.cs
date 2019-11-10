using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenCvSharp.Extensions;

namespace DataDecision.onnx
{
    public static class ImageHelper
    {
       private static Func<int, int, int, int> GetPixelMapper(PixelFormat pixelFormat, int heightStride)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return (h, w, c) => h * heightStride + w * 4 + c;  // bytes are B-G-R-A
                case PixelFormat.Format24bppRgb:
                default:
                    return (h, w, c) => h * heightStride + w * 3 + c;  // bytes are B-G-R

            }
        }

        public static byte[] Image2ByteArray(this Bitmap image)
        {
            // We use local variables to avoid contention on the image object through the multiple threads.
            int channelStride = image.Width * image.Height;
            int imageWidth = image.Width;
            int imageHeight = image.Height;
            var features = new byte[imageWidth * imageHeight * 3];
            var bitmapData = image.LockBits(new System.Drawing.Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.ReadOnly, image.PixelFormat);

            IntPtr ptr = bitmapData.Scan0;
            int bytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;
            byte[] rgbValues = new byte[bytes];
            int stride = bitmapData.Stride;
            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            return rgbValues;
        }

        public static Bitmap FloatListToBitmap(List<IList<float>> p, int width, int height, PixelNormalizationMode zeroCentral)
        {
            using (Bitmap bm = new Bitmap(width, height,PixelFormat.Format32bppArgb))
            {
                //long quality = 90; //
                //EncoderParameters parametre = new EncoderParameters(1);
                //parametre.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

                var features = p[0];
                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        int basenum = width * h * 3 + w * 3;
                        if (features[width * h + w] == 1)
                        {
                            bm.SetPixel(w, h, Color.FromArgb(255, 128, 0, 0));
                        }
                        else if (features[width * h + w] == 2)
                        {
                            bm.SetPixel(w, h, Color.FromArgb(255, 0, 128, 0));
                        }
                        else if (features[width * h + w] >= 3)
                        {
                            bm.SetPixel(w, h, Color.FromArgb(255, 128, 128, 0));
                        }
                    }
            }
                bm.Save("D:/Medusa.Server/Medusa.Test/bin/x64/Release/netcoreapp2.1/Images/Calibaratetest02.png", ImageFormat.Png);

                return bm;
            }
        }
        public static float StdDev(this IEnumerable<float> values)
        {
            float mean = 0.0f;
            float sum = 0.0f;
            float stdDev = 0.0f;
            int n = 0;
            foreach (float val in values)
            {
                n++;
                float delta = val - mean;
                mean += delta / n;
                sum += delta * (val - mean);
            }
            if (1 < n)
                stdDev = (float)Math.Sqrt(sum / (n - 1));

            return stdDev;

        }

       public static List<float> ParallelExtractCHW(this Bitmap image, PixelNormalizationMode mode = PixelNormalizationMode.ZeroCentral)
        {
            int channelStride = image.Width * image.Height;
            int imageWidth = image.Width;
            int imageHeight = image.Height;
            var features = new float[imageWidth * imageHeight * 3];

            var bitmapData = image.LockBits(new System.Drawing.Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.ReadOnly, image.PixelFormat);

            IntPtr ptr = bitmapData.Scan0;

            int bytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;

            byte[] rgbValues = new byte[bytes];
            int stride = bitmapData.Stride;
            // 將RGB值複製到array
            Marshal.Copy(ptr, rgbValues, 0, bytes);
            // 根據pixel format對應像素
            Func<int, int, int, int> mapPixel = GetPixelMapper(image.PixelFormat, stride);
                Parallel.For(0, imageHeight, (int h) =>
                {
                    Parallel.For(0, imageWidth, (int w) =>
                    {
                        Parallel.For(0, 3, (int c) =>
                        {

                            if (mode == PixelNormalizationMode.ZeroBased){
                                features[channelStride * c + imageWidth * h + w] = (float)(rgbValues[mapPixel(h, w, c)])/255f;
                            }
                            else if (mode == PixelNormalizationMode.ZeroCentral){
                                features[channelStride * c + imageWidth * h + w] = (float)(rgbValues[mapPixel(h, w, c)]-127.5f) / 127.5f;
                            }
                            else if (mode == PixelNormalizationMode.imagenet){
                                //[0.485, 0.456, 0.406]
                                //[0.229, 0.224, 0.225]
                                if (c == 0)
                                {
                                    features[channelStride * c + imageWidth * h + w] =((float)(rgbValues[mapPixel(h, w, c)] / 255f)- 0.485f)/ 0.229f;
                                }
                                else if (c == 1)
                                {
                                    features[channelStride * c + imageWidth * h + w] = ((float)(rgbValues[mapPixel(h, w, c)] / 255f) - 0.456f) / 0.224f;
                                }
                                else 
                                {
                                    features[channelStride * c + imageWidth * h + w] = ((float)(rgbValues[mapPixel(h, w, c)] / 255f) - 0.406f) / 0.225f;
                                }
                            }
                            else{
                                features[channelStride * c + imageWidth * h + w] = (float)(rgbValues[mapPixel(c, h, w)]) ;
                            }
                        });
                    });
                });

            image.UnlockBits(bitmapData);


         
           return features.ToList();
           

        }

        public static List<float> ParallelExtractCHW(this Bitmap image)
        {
            int channelStride = image.Width * image.Height;
            int imageWidth = image.Width;
            int imageHeight = image.Height;

            var features = new byte[imageWidth * imageHeight * 3];
            var bitmapData = image.LockBits(new System.Drawing.Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.ReadOnly, image.PixelFormat);
            IntPtr ptr = bitmapData.Scan0;
            int bytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;
            byte[] rgbValues = new byte[bytes];

            int stride = bitmapData.Stride;
            int heightstrides = image.Width * 3;
            // 將RGB值複製到array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // 根據pixel format對應像素
            Func<int, int, int,int> mapPixel = GetPixelMapper(image.PixelFormat, stride);


            Parallel.For(0, imageHeight, (int h) =>
            {
                Parallel.For(0, imageWidth, (int w) =>
                {
                    Parallel.For(0, 3, (int c) =>
                    {
                        features[channelStride * c + imageWidth * h + w] = rgbValues[mapPixel(h, w, c)];
                    });
                });
            });



            image.UnlockBits(bitmapData);
            //請注意這段有改寫，需要根據平均值標準差調整
            float mean = features.Select(x=>(float)x).Average();
            float std = features.Select(x => (float)x).StdDev();
            return features.Select(b => (float)((b - mean) / std)).ToList();
        }

        public static float[] ParallelExtractHWC(this Bitmap image, PixelNormalizationMode mode = PixelNormalizationMode.ZeroCentral)
        {
            int heightStride = image.Width * 3;
            int imageWidth = image.Width;
            int imageHeight = image.Height;
            var features = new float[imageWidth * imageHeight * 3];

            var bitmapData = image.LockBits(new System.Drawing.Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.ReadOnly, image.PixelFormat);
            IntPtr ptr = bitmapData.Scan0;

            int bytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;

            byte[] rgbValues = new byte[bytes];
            int stride = bitmapData.Stride;
            // 將RGB值複製到array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            // 根據pixel format對應像素
            Func<int, int, int, int> mapPixel = GetPixelMapper(image.PixelFormat, stride);
            Parallel.For(0, imageHeight, (int h) =>
            {
                Parallel.For(0, imageWidth, (int w) =>
                {
                    Parallel.For(0, 3, (int c) =>
                    {
                        if (mode == PixelNormalizationMode.ZeroBased)
                        {
                            features[heightStride * h + 3 * w + c] = (float)(rgbValues[mapPixel(h, w, c)]) / 255f;
                        }
                        else if (mode == PixelNormalizationMode.ZeroCentral)
                        {
                            features[heightStride * h + 3 * w + c] = (float)(rgbValues[mapPixel(h, w, c)] - 127.5f) / 127.5f;
                        }
                        else
                        {
                            features[heightStride * h + 3 * w + c] = (float)(rgbValues[mapPixel(h, w, c)]);
                        }
                    });
                });
            });

            image.UnlockBits(bitmapData);



            return features;


        }

        public static List<float> ParallelExtractHWC(this Bitmap image)
        {
            int heightStride = image.Width * 3;
            int imageWidth = image.Width;
            int imageHeight = image.Height;

            var features = new byte[imageWidth * imageHeight * 3];
            var bitmapData = image.LockBits(new System.Drawing.Rectangle(0, 0, imageWidth, imageHeight), ImageLockMode.ReadOnly, image.PixelFormat);
            IntPtr ptr = bitmapData.Scan0;
            int bytes = Math.Abs(bitmapData.Stride) * bitmapData.Height;
            byte[] rgbValues = new byte[bytes];

            int stride = bitmapData.Stride;
            int heightstrides = image.Width * 3;
            // 將RGB值複製到array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // 根據pixel format對應像素
            Func<int, int, int, int> mapPixel = GetPixelMapper(image.PixelFormat, stride);


            Parallel.For(0, imageHeight, (int h) =>
            {
                Parallel.For(0, imageWidth, (int w) =>
                {
                    Parallel.For(0, 3, (int c) =>
                    {
                        features[heightStride * h + 3 * w + c] = rgbValues[mapPixel(h, w, c)];
                    });
                });
            });



            image.UnlockBits(bitmapData);
            //請注意這段有改寫，需要根據平均值標準差調整
            float mean = features.Select(x => (float)x).Average();
            float std = features.Select(x => (float)x).StdDev();
            return features.Select(b => (float)((b - mean) / std)).ToList();
        }




        public static Bitmap PadToFit(this Bitmap image,int width,int height)
        {
            Bitmap desImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(desImage))
            {
                Rectangle desrect= new Rectangle(0, 0, desImage.Width, desImage.Height);
                g.FillRectangle(Brushes.White, desrect);
                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
            }
            return desImage;
        }


        /// 獲取等比例縮放圖片的方法
        /// </summary>
        /// <param name="imgPath">待縮放圖片路徑</param>
        /// <param name="scaling">要保持的寬度或高度</param>
        /// <param name="keepWidthOrHeight">如果爲true則保持寬度爲scaling，否則保持高度爲scaling</param>
        /// <returns></returns>

        public static Bitmap GetThumbnail(this Bitmap image, int scalingWith, int scalingHeigth)
        {
            try
            {
                
                    int width = 0;
                    int height = 0;
                    int tw = image.Width;//图像的实际宽度
                    int th = image.Height;//图像的实际高度
                    //if (tw > th)
                    //{
                    //    keepWidthOrHeight = true;
                    //}
                    //else
                    //{
                    //    keepWidthOrHeight = false;
                    //}
                    if (scalingWith >= scalingHeigth)//保持宽度
                    {
                        #region 自动保持宽度
                        if (scalingWith >= tw)
                        {
                            width = tw;
                            height = th;
                        }
                        else
                        {
                            double ti = Convert.ToDouble(tw) / Convert.ToDouble(scalingWith);
                            if (ti == 0d)
                            {
                                width = tw;
                                height = th;
                            }
                            else
                            {
                                width = scalingWith;
                                height = Convert.ToInt32(Convert.ToDouble(th) / ti);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region 自动保持高度
                        if (scalingWith >= th)
                        {
                            width = tw;
                            height = th;
                        }
                        else
                        {
                            double ti = Convert.ToDouble(th) / Convert.ToDouble(scalingWith);
                            if (ti == 0d)
                            {
                                width = tw;
                                height = th;
                            }
                            else
                            {
                                width = Convert.ToInt32(Convert.ToDouble(tw) / ti);
                                height = scalingWith;
                            }
                        }
                        #endregion
                    }
                    return new Bitmap(image.GetThumbnailImage(width, height, () => { return false; }, IntPtr.Zero));

                

            }
            catch
            {
                return null;
            }

        }

        /// <summary>

        /// Resizes an image
        /// </summary>
        /// <param name="image">The image to resize</param>
        /// <param name="width">New width in pixels</param>
        /// <param name="height">New height in pixesl</param>
        /// <param name="useHighQuality">Resize quality</param>
        /// <returns>The resized image</returns>
        public static Bitmap Resize(this Bitmap image, int width, int height, bool useHighQuality=true)
        {

            var newImg = new Bitmap(width, height);
            newImg.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (var g = Graphics.FromImage(newImg))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                if (useHighQuality)
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                }
                else
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;

                }
                var attributes = new ImageAttributes();
                attributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                g.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);

            }



            return newImg;

        }


        public static  Bitmap FloatListToBitmap(float[] imagearr,int width,int height, PixelNormalizationMode mode = PixelNormalizationMode.ZeroCentral)
        {
            int stride = width * height;
            List<float> floatlist = new List<float>();
            List<float> features = new List<float>();
            int  imageHeight= width;
            int imageWidth= height;
         

            //if (mode == ColorNormalizationMode.None)
            //{
            //    arr = imagearr.Select(x => (byte)x).ToArray();
            //}
            //else if (mode == ColorNormalizationMode.ZeroBased)
            //{
            //    arr = imagearr.Select(x => (byte)(int)(x * 255)).ToArray();
            //}
            //else if (mode == ColorNormalizationMode.ZeroCentral)
            //{
            imagearr = imagearr.Select(x =>(float)((x-127.5)/127.5)).ToArray();

            Func<int, int, int> mapPixel = (h, w) => h * imageWidth + w;// GetPixelMapper(image.PixelFormat, stride);
            //Func<int, int, int, int> mapPixel = (h, w, c) => c * channelStride + w * h + c;// GetPixelMapper(image.PixelFormat, stride);

            Parallel.For(0, imageHeight, (int h) =>
             {
                 Parallel.For(0, imageWidth, (int w) =>
                 {
                     features[imageWidth * h + w] = (float)imagearr[mapPixel(h, w)];
                 });
             });

            features.Select(b => (float)b).ToList();



            var dis = imagearr.Distinct().ToList();
            Console.WriteLine(dis);
            // Fill array with random values
            Bitmap bitmap = new Bitmap(width, height);
      
         
              
             for (int y = 0; y < height; ++y)
              {
                for (int x = 0; x < width; ++x)
                {
                    if (imagearr[y * imageWidth + x]==0)
                       {
                            bitmap.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0));
                        }
                    else if (imagearr[y * imageWidth + x] == 1)
                    {

                        bitmap.SetPixel(x, y, Color.FromArgb(255, 0, 128, 0));
                    }
                    else if (imagearr[y * imageWidth + x] == 2)
                        {
 
                            bitmap.SetPixel(x, y, Color.FromArgb(255, 128, 128,0));
                         }
                        else if (imagearr[y * imageWidth + x] == 3)
                        {

                            bitmap.SetPixel(x, y, Color.FromArgb(255, 0, 64, 0));
                        }
                        else if (imagearr[y * imageWidth + x] == 4)
                        {

                            bitmap.SetPixel(x, y, Color.FromArgb(255, 192, 0, 0));
                        }
                }
                }
                    
                
            

   
            return bitmap;

     

        }

        public static List<float> PrepareImageFloatArray(Bitmap img, int width, int height, PixelNormalizationMode normmode = PixelNormalizationMode.ZeroCentral)
        {

            return img.PadToFit(width, height).ParallelExtractCHW(normmode).Select(x => (float)x).ToList();
        }

        //public Mat Bitmap2Mat(Bitmap img)
        //{
        //    img.ToMat().
        //}
    }
}
