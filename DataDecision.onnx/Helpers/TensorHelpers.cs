﻿using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using System.Text;

namespace DataDecision.onnx
{
    public static class TensorHelpers
    {


        public static NamedOnnxValue BitmapToTensor(Bitmap img)
        {
            float[]data=img.ParallelExtractCHW(PixelNormalizationMode.imagenet).ToArray();
            var tensor = new Microsoft.ML.OnnxRuntime.Tensors.DenseTensor<float>(data, InferHelper.session.InputMetadata[InferHelper.inputName].Dimensions);
            return NamedOnnxValue.CreateFromTensor<float>(InferHelper.inputName, tensor);
        }
        


        // var tensor = Onnx.TensorProto.Parser.ParseFrom(file);
    }
}
