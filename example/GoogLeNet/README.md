# GoogLeNet

This sample is what is ported from https://github.com/apache/incubator-mxnet/blob/master/cpp-package/example/googlenet.cpp to C#.

## How to use?

## 1. Build project

````
cd <GoogLeNet_dir>
dotnet build --configuration Release
````

At last, copy ***libmxnet.dll*** and dependencies to output directory; &lt;GoogLeNet_dir&gt;\bin\Release\netcoreapp2.0.

## 2. Download test data

### Prepare

If you did not install OpenCV for Python, execute the following command.

````
python -m pip install opencv-python
````

### Download

Download **Caltech 101** data from [here](http://www.vision.caltech.edu/Image_Datasets/Caltech101/Caltech101.html).</br>

The following command do downloading test data automatically.</br>
Download mnist data from [here](http://data.mxnet.io/mxnet/data/cifar10.zip).</br>
The following command do downloading test data automatically.

````
cd <GoogLeNet_dir>
python -m wget http://www.vision.caltech.edu/Image_Datasets/Caltech101/101_ObjectCategories.tar.gz
python
>> import tarfile
>> tar = tarfile.open('101_ObjectCategories.tar.gz', mode='r:gz')
>> tar.extractall()
>> exit()
````

### Create train and test data

At first, you MUST build and copy dependency libraries to **<GoogLeNet_dir>\bin\Release\netcoreapp2.0**

````
cd <GoogLeNet_dir>
python caltech101.py

cd <GoogLeNet_dir>\bin\Release\netcoreapp2.0
python ..\..\..\..\..\incubator-mxnet\tools\im2rec.py --list True --recursive True ..\..\..\train ..\..\..\caltech_101_train/
python ..\..\..\..\..\incubator-mxnet\tools\im2rec.py --list True --recursive True ..\..\..\val ..\..\..\101_ObjectCategories/
python ..\..\..\..\..\incubator-mxnet\tools\im2rec.py --resize 256 --quality 90 --num-thread 16 ..\..\..\val ..\..\..\101_ObjectCategories/
python ..\..\..\..\..\incubator-mxnet\tools\im2rec.py --resize 256 --quality 90 --num-thread 16 ..\..\..\train ..\..\..\caltech_101_train/
````

## 3. Run

````
cd <GoogLeNet_dir>
dotnet run --configuration Release
````

## 4. Others

### Use CPU

If you want to use CPU, change the following code, build and run!!

````
// change device type if you want to use GPU
var context = Context.Cpu();
````

### Program does NOT exit!!

Please refer [C++ demo memory release problem](https://github.com/apache/incubator-mxnet/issues/7973)