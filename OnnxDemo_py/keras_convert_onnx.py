
import keras
import numpy as np
from keras.preprocessing import image
from keras.applications.mobilenet_v2 import preprocess_input,MobileNetV2

# image preprocessing
img_path = 'dog.jpg'   # make sure the image is in img_path
img_size = 224
img = image.load_img(img_path, target_size=(img_size, img_size))
x = image.img_to_array(img)
x = np.expand_dims(x, axis=0)
x = preprocess_input(x)

model = MobileNetV2(alpha=1.0, include_top=True,weights='imagenet',pooling=None,classes=1000)

import keras2onnx
onnx_model = keras2onnx.convert_keras(model, model.name)
import onnx
onnx.save_model(onnx_model, 'mobilenet_v2.onnx')

import onnxruntime
content = onnx_model.SerializeToString()
sess = onnxruntime.InferenceSession(content)
x = x if isinstance(x, list) else [x]
feed = dict([(input.name, x[n]) for n, input in enumerate(sess.get_inputs())])
pred_onnx = sess.run(None, feed)

results=keras.applications.imagenet_utils.decode_predictions(pred_onnx, top = 5)