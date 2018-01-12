# LeNet with MSDataIter

This sample is what is ported from https://github.com/apache/incubator-mxnet/blob/master/cpp-package/example/lenet_with_mxdataiter.cpp to C#.

## How to use?

## 1. Build project

````
cd <LeNetWithMXDataIter_dir>
dotnet build --configuration Release
````

At last, copy ***libmxnet.dll*** and dependencies to output directory; &lt;LeNetWithMXDataIter_dir&gt;\bin\Release\netcoreapp2.0.

## 2. Download test data

Download mnist data from [THE MNIST DATABASE of handwritten digits](http://yann.lecun.com/exdb/mnist/).</br>
The following command do downloading test data automatically.

````
cd <LeNetWithMXDataIter_dir>
mnist.bat
````

## 3. Run

````
cd <LeNetWithMXDataIter_dir>
dotnet run --configuration Release
````

## 4. Others

### Use CPU

If you want to use CPU, change the following code, build and run!!

````
var contest = Context.Gpu();
````

### Program does NOT exit!!

Please refer [C++ demo memory release problem](https://github.com/apache/incubator-mxnet/issues/7973)