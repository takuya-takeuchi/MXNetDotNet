# LeNet with MSDataIter

This sample is what is ported from https://github.com/apache/incubator-mxnet/blob/master/cpp-package/example/lenet_with_mxdataiter.cpp to C#.

## How to use?

## 1. Build project

````
$ cd <LeNetWithMXDataIter_dir>
$ dotnet build --configuration Release
````

At last, copy ***libmxnet.dll*** and dependencies to output directory; &lt;LeNetWithMXDataIter_dir&gt;\bin\Release\netcoreapp2.0.

### Windows

Depencencies are 

* libgcc_s_seh-1.dll
* libgfortran-3.dll
* libopenblas.dll
* libquadmath-0.dll
* opencv_world349.dll

## 2. Download test data

Download mnist data from [THE MNIST DATABASE of handwritten digits](http://yann.lecun.com/exdb/mnist/).</br>
The following command do downloading test data automatically.

````
$ pwsh prepare.ps1
````

## 3. Run

````
$ cd <LeNetWithMXDataIter_dir>
$ dotnet run --configuration Release
dotnet run -c Release
[01:05:02] D:\Works\OpenSource\MXNetDotNet\src\incubator-mxnet\src\io\iter_mnist.cc:110: MNISTIter: load 60000 images, shuffle=1, shape=(128,784)
[01:05:02] D:\Works\OpenSource\MXNetDotNet\src\incubator-mxnet\src\io\iter_mnist.cc:110: MNISTIter: load 10000 images, shuffle=1, shape=(128,784)
[01:05:02] d:\works\opensource\mxnetdotnet\src\incubator-mxnet\src\operator\nn\cudnn\./cudnn_algoreg-inl.h:97: Running performance tests to find the best convolution algorithm, this can take a while... (set the environment variable MXNET_CUDNN_AUTOTUNE_DEFAULT to 0 to disable)
Epoch[0] 48742.0667209113 samples/sec Train-Accuracy=0.6432459
Epoch[0] Val-Accuracy=0.7708333
Epoch[1] 51641.3793103448 samples/sec Train-Accuracy=0.7891626
Epoch[1] Val-Accuracy=0.8296274
Epoch[2] 51156.2766865927 samples/sec Train-Accuracy=0.8228666
Epoch[2] Val-Accuracy=0.8483574
..
..
Epoch[98] 51864.9350649351 samples/sec Train-Accuracy=0.9965445
Epoch[98] Val-Accuracy=0.9653445
Epoch[99] 52226.6782911944 samples/sec Train-Accuracy=0.9967782
Epoch[99] Val-Accuracy=0.9653445
````

## 4. Others

### Use CPU

If you want to use CPU, change the following code, build and run!!

````
var contest = Context.Gpu();
````

### Program does NOT exit!!

Please refer [C++ demo memory release problem](https://github.com/apache/incubator-mxnet/issues/7973)