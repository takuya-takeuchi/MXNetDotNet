# Test Score

This sample is what is ported from https://github.com/apache/incubator-mxnet/blob/master/cpp-package/example/test_score.cpp to C#.

## How to use?

## 1. Build project

````
$ cd <TestScore_dir>
$ dotnet build --configuration Release
````

At last, copy ***libmxnet.dll*** and dependencies to output directory; &lt;TestScore_dir&gt;\bin\Release\netcoreapp2.0.

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
$ cd <TestScore_dir>
$ dotnet run -c Release 0.9
[23:21:44] D:\Works\OpenSource\MXNetDotNet\src\incubator-mxnet\src\io\iter_mnist.cc:110: MNISTIter: load 60000 images, shuffle=1, shape=(100,784)
[23:21:44] D:\Works\OpenSource\MXNetDotNet\src\incubator-mxnet\src\io\iter_mnist.cc:110: MNISTIter: load 10000 images, shuffle=1, shape=(100,784)
Epoch: 0 74534.1614906832 samples/sec Accuracy: 0.1135
Epoch: 1 80106.8090787717 samples/sec Accuracy: 0.5604
Epoch: 2 80000 samples/sec Accuracy: 0.8463
Epoch: 3 79893.4753661784 samples/sec Accuracy: 0.89
Epoch: 4 80428.9544235925 samples/sec Accuracy: 0.9101
Epoch: 5 79893.4753661784 samples/sec Accuracy: 0.9235
Epoch: 6 80106.8090787717 samples/sec Accuracy: 0.9324
Epoch: 7 80971.6599190283 samples/sec Accuracy: 0.9372
Update[5001]: Change learning rate to 0.01
Epoch: 8 79893.4753661784 samples/sec Accuracy: 0.9383
Epoch: 9 80428.9544235925 samples/sec Accuracy: 0.9388
````

The last argument is threshold. If calcurated score is greater than threshold, console prints '0', otherwise '1'.

## 4. Others

### Use CPU

If you want to use CPU, change the following code, build and run!!

````
var ctx = Context.Gpu();  // Use GPU for training
````

### Program does NOT exit!!

Please refer [C++ demo memory release problem](https://github.com/apache/incubator-mxnet/issues/7973)