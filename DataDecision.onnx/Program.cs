using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace DataDecision.onnx
{
    class Program
    {
        static void Main(string[] args)
        {
           
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            ProcessTest();
        }

        public static void ProcessTest()
        {
            Console.WriteLine("請問您想測試的是:");
            Console.WriteLine("0.  測試imagenet");
            //Console.WriteLine("1.  名片傳輸測試");
            //Console.WriteLine("2.  庫存歸零測試");
            //Console.WriteLine("3.  商品infer測試");

            string taskOption = Console.ReadKey().KeyChar.ToString();

            if (taskOption == "0")
            {
                try
                {

                    Console.WriteLine("載入模型");
                    InferHelper.SettingSession("Models/mobilenetv2-1.0.onnx");
                    Console.WriteLine("輸入圖片為dog.jpg");
                    var result=InferHelper.InferImagenet(new Bitmap("Images/dog.jpg"));

                    Console.WriteLine(result.ToString());

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            //else if (taskOption == "1")
            //{
            //    string teststring = Properties.Resources.namecardtest.Replace("\\r\\n", "\r\n");
            //    RedisHelper.StringSet("bc_result_RTC1010AL_1558686000", Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(teststring)), 10);
            //    RedisHelper.StringSet("bc_orgimg_RTC1010AL_1558686000",RedisHelper.StringGet("bc_orgimg_RTC1010AL_1558687595"), 10);

            //    RedisHelper.Publish("namecard", "RTC1010AL_1558686000");
            //}
            //else if (taskOption == "2")
            //{
            //    RedisHelper.Publish("reset_inventory", "reset_inventory");
            //}
            //else if (taskOption == "3")
            //{

            //    string[] files = Directory.GetFiles(@"C:\Users\Allan\Pictures\", "*.bmp", SearchOption.AllDirectories).Where(x=>x.Contains("freezer")).ToArray();
            //    decimal totaltime = 0;
            //    InferHelper.SettingSession();
            //    Random rnd = new Random();

  
            //    for (int i = 0; i < files.Length; i++)
            //    {
            //        var watch = System.Diagnostics.Stopwatch.StartNew();
            //        //List<Bitmap> list = new List<Bitmap>();
            //        //list.Add(new Bitmap(files[i]));
            //        //list.Add(new Bitmap(files[i + 50]));
            //        //var result = InferHelper.InferYolo(list);
            
            //        using (Bitmap img = new Bitmap(files[i ]))
            //        {
            //            Bitmap img2 = (Bitmap)img.Clone();
            //            if (img.Width == 320)
            //            {
            //                var result = InferHelper.InferYolo(img);
            //                var finallist = InferHelper.YoloPostprocess(result.ToArray()[0]).ToList();

            //                Console.WriteLine(string.Format("獲得bbox  {0}", finallist.Count()));
            //                if (finallist.Count() > 1)
            //                {
            //                    Console.WriteLine(string.Format("獲得bbox  {0}", finallist.Count()));

            //                }
            //                using Graphics g = Graphics.FromImage(img2);
            //                foreach (var item in finallist)
            //                {
            //                    Pen pen = new Pen(Color.Yellow, 3);
            //                    g.DrawRectangle(pen, new Rectangle((int)(item.CenterX - 0.5 * item.Width), (int)(item.CenterY - 0.5 * item.Height), (int)item.Width, (int)item.Height));
            //                    g.Save();
            //                }
                            
            //                img2.Save(Path.Combine("yolo_bbox_infer/", Path.GetFileName(files[i])));
            //            }

            //        }
            //        watch.Stop();
            //        var elapsedMs = watch.ElapsedMilliseconds;
            //        totaltime += elapsedMs;
            //        Console.WriteLine(elapsedMs/1000.0);

            //    }
            //    Console.WriteLine(string.Format("總計耗用時間{0} 平均單次{1}", totaltime/(decimal)1000.0, totaltime / (decimal)100000.0));

            //}
            Console.ReadKey();
            ProcessTest();
        }


    }
}
