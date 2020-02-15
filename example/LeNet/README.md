# LeNet

This sample is what is ported from https://github.com/apache/incubator-mxnet/blob/master/cpp-package/example/lenet.cpp to C#.

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

Download data from [MNIST in CSV](https://pjreddie.com/projects/mnist-in-csv/).  
The following command do downloading test data automatically.

````
$ pwsh prepare.ps1
````

## 3. Run

````
$ cd <LeNet_dir>
$ dotnet run --configuration Release 0.9
data
conv1_w
conv1_b
conv2_w
conv2_b
conv3_w
conv3_b
fc1_w
fc1_b
fc2_w
fc2_b
data_label
here read fin
here slice fin
[21:10:41] d:\works\opensource\mxnetdotnet\src\incubator-mxnet\src\operator\nn\cudnn\./cudnn_algoreg-inl.h:97: Running performance tests to find the best convolution algorithm, this can take a while... (set the environment variable MXNET_CUDNN_AUTOTUNE_DEFAULT to 0 to disable)
Iter 0, accuracy: 0.2597619
Iter 1, accuracy: 0.26
Iter 2, accuracy: 0.2585714
Iter 3, accuracy: 0.2578571
````

## 4. Others

### Use CPU

If you want to use CPU, change the following code, build and run!!

````
this._CtxDev = new Context(DeviceType.GPU, 0);
````

### Program does NOT exit!!

Please refer [C++ demo memory release problem](https://github.com/apache/incubator-mxnet/issues/7973)