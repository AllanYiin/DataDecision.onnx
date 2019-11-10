這個專案是2019 .net Conf的課程展示
介紹如何使用onnx runtime的python版以及.net版來進行圖像的推論

**ImageHelper**:提供圖像處理的常用方法，包括:
    
    ParallelExtractCHW : 依照CHW/ BGR順序讀取像素
    
    ParallelExtractHWC : 依照HWC/ RGB序讀取像素
    
    PadToFit : 將圖片填滿至指定大小
    
    GetThumbnail :  將圖片等比例縮放至指定大小
    
    Resize :  圖片縮放
    
    FloatListToBitmap : 將List<float>轉圖片 


**TensorHelper**:提供將float[]打包成tensor的方法

    BitmapCHWToTensor : 將圖片依照CHW/ BGR以及指定正歸化方法轉成tensor


**InferHelper** : 提供推論階段所需之方法，包括

    SettingSession: 設定onnxruntime Inference Session的方法
    
    InferImagenet: 進行imagenet推論