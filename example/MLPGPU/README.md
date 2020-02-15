# MLP: Multi-Layer Perceptrons for MNIST by GPU

This sample is what is ported from https://github.com/apache/incubator-mxnet/blob/master/cpp-package/example/mlp_gpu.cpp to C#.

## How to use?

## 1. Build project

````
$ cd <MLPGPU_dir>
$ dotnet build --configuration Release
````

At last, copy ***libmxnet.dll*** and dependencies to output directory; &lt;MLPGPU_dir&gt;\bin\Release\netcoreapp2.0

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
$ cd <MLPGPU_dir>
$ dotnet run -c Release 0.9
[23:56:39] D:\Works\OpenSource\MXNetDotNet\src\incubator-mxnet\src\io\iter_mnist.cc:110: MNISTIter: load 60000 images, shuffle=1, shape=(100,784)
[23:56:39] D:\Works\OpenSource\MXNetDotNet\src\incubator-mxnet\src\io\iter_mnist.cc:110: MNISTIter: load 10000 images, shuffle=1, shape=(100,784)
Epoch[0] 130718.954248366 samples/sec Train-Accuracy=0.1116167
Epoch[0] Val-Accuracy=0.1135
Epoch[1] 131578.947368421 samples/sec Train-Accuracy=0.2681
Epoch[1] Val-Accuracy=0.6195
Epoch[2] 134831.460674157 samples/sec Train-Accuracy=0.71435
Epoch[2] Val-Accuracy=0.8314
Epoch[3] 135440.180586907 samples/sec Train-Accuracy=0.8646833
Epoch[3] Val-Accuracy=0.8889
Epoch[4] 135135.135135135 samples/sec Train-Accuracy=0.90135
Epoch[4] Val-Accuracy=0.9104
Epoch[5] 133928.571428571 samples/sec Train-Accuracy=0.9155167
Epoch[5] Val-Accuracy=0.9233
Epoch[6] 133630.289532294 samples/sec Train-Accuracy=0.9247833
Epoch[6] Val-Accuracy=0.9286
Epoch[7] 134529.147982063 samples/sec Train-Accuracy=0.9294834
Epoch[7] Val-Accuracy=0.933
Update[5001]: Change learning rate to 0.01
Epoch[8] 132158.59030837 samples/sec Train-Accuracy=0.9365333
Epoch[8] Val-Accuracy=0.941
Epoch[9] 131291.028446389 samples/sec Train-Accuracy=0.9396167
Epoch[9] Val-Accuracy=0.9406
````

## 4. Others

### Program does NOT exit!!

Please refer [C++ demo memory release problem](https://github.com/apache/incubator-mxnet/issues/7973)