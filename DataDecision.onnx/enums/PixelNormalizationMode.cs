using System;
using System.Collections.Generic;
using System.Text;

namespace DataDecision.onnx
{
    public enum PixelNormalizationMode
    {
        ZeroCentral = 0,
        ZeroBased=1,
        imagenet=2,
       None=9
    }
}
