# Image Classification

This sample is what is ported from https://github.com/apache/incubator-mxnet/tree/master/example/image-classification/predict-cpp to C#.

## How to use?

## 1. Build project

````
cd <ImageClassification_dir>
dotnet build --configuration Release
````

At last, copy ***libmxnet.dll*** and dependencies to output directory; &lt;ImageClassification_dir&gt;\bin\Release\netcoreapp2.0.

## 2. Download model file

Download pre-trained file form [here](http://data.mxnet.io/mxnet/data/Inception.zip).

````
cd <ImageClassification_dir>
python -m pip install wget
python -m wget http://data.mxnet.io/mxnet/models/imagenet/inception-bn.tar.gz
python
>> import tarfile
>> tar = tarfile.open('inception-bn.tar.gz', mode='r:gz')
>> tar.extractall('model/Inception')
````

But the above file does ***NOT*** contain mean image file. It works well even if missing mean image. However, accuracy is a bit lower.

If you want to get mean image, please do the following command.

````
python -m wget http://data.mxnet.io/mxnet/data/Inception.zip
python
>> import zipfile
>> zip = zipfile.ZipFile('Inception.zip', 'r')
>> zip.extractall('model')
````


## 3. Run

````
cd <ImageClassification_dir>
python -m wget https://upload.wikimedia.org/wikipedia/commons/thumb/8/81/Florida_navel_orange_2.jpg/320px-Florida_navel_orange_2.jpg
dotnet run --configuration Release 320px-Florida_navel_orange_2.jpg

````

## 4. Others

### Use GPU

If you want to use GPU, change the following code, build and run!!

````
// Parameters
var devType = 1;          // 1: cpu, 2: gpu
const int devId = 0;      // arbitrary.
var numInputNodes = 1u;   // 1 for feedforward

````

### Program does NOT exit!!

Please refer [C++ demo memory release problem](https://github.com/apache/incubator-mxnet/issues/7973)