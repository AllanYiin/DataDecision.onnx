using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDecision.onnx
{
    public static class InferHelper
    {

        public static SessionOptions options = new SessionOptions();
        public static InferenceSession session ;
        public static string inputName = "";

    

        public static void SettingSession(string model_path)
        {

            options.GraphOptimizationLevel= GraphOptimizationLevel.ORT_ENABLE_BASIC;

            try
            {
                session = new InferenceSession(model_path, SessionOptions.MakeSessionOptionWithCudaProvider(0));
            }
            catch (Exception e)
            {
                session = new InferenceSession(model_path, options);
            }
            inputName = session.InputMetadata.Keys.ToList()[0];


        }

        public static IDisposableReadOnlyCollection<DisposableNamedOnnxValue> InferImagenet(Bitmap img)
        {
            var container = new List<NamedOnnxValue>();
            container.Add(TensorHelpers.BitmapToTensor(img));
            var results = session.Run(container);
            return results;
       
        }
        //public static IDisposableReadOnlyCollection<DisposableNamedOnnxValue> InferYolo(List<Bitmap> imgs)
        //{
        //    var container = new List<NamedOnnxValue>();
        //    foreach (var img in imgs)
        //    {
        //        container.Add(TensorHelpers.BitmapToTensor(img));
        //    }
        //    var results = session.Run(container);
        //    return results;
        //}

    }
}
