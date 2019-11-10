using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using DataDecision.onnx;

namespace ConsoleApp1
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
            var labels=System.IO.File.ReadAllLines("imagenet_labels.txt");
            Console.WriteLine("請問您想測試的是:");
            Console.WriteLine("0.  測試imagenet");


            string taskOption = Console.ReadKey().KeyChar.ToString();

            if (taskOption == "0")
            {
                try
                {
                    DateTime d = DateTime.Now;
                    Console.WriteLine("載入模型");
                    InferHelper.SettingSession("Models/mobilenetv2-1.0.onnx");
                    Console.WriteLine("輸入圖片為dog.jpg");
                    var result = InferHelper.InferImagenet(new Bitmap("Images/dog.jpg")).ToList()[0];
                    var probs = result.AsEnumerable<float>().ToList();
                    probs = probs.Select(x => (float)Math.Exp(x)).ToList();
                    float sum_probs = probs.Sum();
                    probs = probs.Select(x => x / sum_probs).ToList();
                    var maxidx = probs.ArgMax(x => x);
                    Console.WriteLine(labels[maxidx]);
                    Console.WriteLine(string.Format("機率為:{0:p3}", probs[maxidx]));

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
         
            Console.ReadKey();
            Console.WriteLine(" ");
            ProcessTest();
        }
    }
}
