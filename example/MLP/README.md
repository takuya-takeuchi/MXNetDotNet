# MLP: Multi-Layer Perceptrons

This sample is what is ported from https://github.com/apache/incubator-mxnet/blob/master/cpp-package/example/mlp.cpp to C#.

## How to use?

## 1. Build project

````
cd <MLP_dir>
dotnet build --configuration Release
````

At last, copy ***libmxnet.dll*** and dependencies to output directory; &lt;MLP_dir&gt;\bin\Release\netcoreapp2.0

## 2. Run

````
cd <MLP_dir>
dotnet run --configuration Release 0.9
````

## 3. Others

### Use GPU

If you want to use GPU, change the following code, build and run!!

````
var ctx_dev = new Context(DeviceType.CPU, 0);
````

### Program does NOT exit!!

Please refer [C++ demo memory release problem](https://github.com/apache/incubator-mxnet/issues/7973)