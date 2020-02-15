# MLP: Multi-Layer Perceptrons for MNIST by CPU

This sample is what is ported from https://github.com/apache/incubator-mxnet/blob/master/cpp-package/example/mlp_cpu.cpp to C#.

## How to use?

## 1. Build project

````
$ cd <MLPCPU_dir>
$ dotnet build --configuration Release
````

At last, copy ***libmxnet.dll*** and dependencies to output directory; &lt;MLPCPU_dir&gt;\bin\Release\netcoreapp2.0

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
$ cd <MLPCPU_dir>
$ dotnet run -c Release 0.9
[00:04:18] D:\Works\OpenSource\MXNetDotNet\src\incubator-mxnet\src\io\iter_mnist.cc:110: MNISTIter: load 60000 images, shuffle=1, shape=(100,784)
[00:04:18] D:\Works\OpenSource\MXNetDotNet\src\incubator-mxnet\src\io\iter_mnist.cc:110: MNISTIter: load 10000 images, shuffle=1, shape=(100,784)
Epoch: 0 82304.5267489712 samples/sec Accuracy: 0.1135
Epoch: 1 85714.2857142857 samples/sec Accuracy: 0.7224
Epoch: 2 87209.3023255814 samples/sec Accuracy: 0.8591
Epoch: 3 86830.6801736614 samples/sec Accuracy: 0.8949
Epoch: 4 84507.0422535211 samples/sec Accuracy: 0.9127
Epoch: 5 85714.2857142857 samples/sec Accuracy: 0.9234
Epoch: 6 85714.2857142857 samples/sec Accuracy: 0.9306
Epoch: 7 86206.8965517241 samples/sec Accuracy: 0.9346
Epoch: 8 86330.9352517986 samples/sec Accuracy: 0.9377
Epoch: 9 85714.2857142857 samples/sec Accuracy: 0.9398
````

## 4. Others

### Program does NOT exit!!

Please refer [C++ demo memory release problem](https://github.com/apache/incubator-mxnet/issues/7973)