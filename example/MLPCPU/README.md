# MLP: Multi-Layer Perceptrons for MNIST by CPU

This sample is what is ported from https://github.com/apache/incubator-mxnet/blob/master/cpp-package/example/mlp_cpu.cpp to C#.

## How to use?

## 1. Build project

````
cd <MLPCPU_dir>
dotnet build --configuration Release
````

At last, copy ***libmxnet.dll*** and dependencies to output directory; &lt;MLPCPU_dir&gt;\bin\Release\netcoreapp2.0

## 2. Download test data

Download mnist data from [THE MNIST DATABASE of handwritten digits](http://yann.lecun.com/exdb/mnist/).</br>
The following command do downloading test data automatically.

````
cd <MLPCPU_dir>
mnist.bat
````

## 3. Run

````
cd <MLPCPU_dir>
dotnet run --configuration Release
````

## 4. Others

### Program does NOT exit!!

Please refer [C++ demo memory release problem](https://github.com/apache/incubator-mxnet/issues/7973)