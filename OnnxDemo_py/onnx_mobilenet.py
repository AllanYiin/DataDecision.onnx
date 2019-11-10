import numpy as np
import onnxruntime
import cv2

#labels=open('imagenet_labels.txt',encoding='utf-8-sig').readlines()

with open('imagenet_labels.txt', 'r') as f:
    labels = [l.rstrip() for l in f]

img=cv2.imread('dog.jpg') #opencv讀入形狀為 HWC  (224,224,3) 像素排列為BGR
img=img.transpose([2,0,1])/255. #把形狀調整成CHW

mean = np.expand_dims(np.expand_dims(np.array([0.485, 0.456, 0.406]),-1),-1)
std =  np.expand_dims(np.expand_dims(np.array([0.229, 0.224, 0.225]),-1),-1)
img=(img-mean)/std #依照平均值與標準差正規化

imgf=np.reshape(img,-1)

img=np.expand_dims(img,0).astype(np.float32)  #把形狀變成(1,3,224,224)



sess = onnxruntime.InferenceSession('../Models/mobilenetv2-1.0.onnx')

input_shape=sess.get_inputs()[0].shape
input_name = sess.get_inputs()[0].name
output_name = sess.get_outputs()[0].name
pred_onnx =sess.run ([output_name], {input_name:img})[0]

pred_onnx = np.squeeze(pred_onnx)
pred_onnx=np.exp(pred_onnx)/np.sum(np.exp(pred_onnx))
prob = list(np.argsort(pred_onnx)[::-1][:5])

print('\n'.join(['{0}  {1:.3%}'.format(labels[item],pred_onnx[item]) for item in prob]))

#results=keras.applications.imagenet_utils.decode_predictions(pred_onnx, top = 5)