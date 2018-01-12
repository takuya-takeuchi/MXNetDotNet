# Test Score

This sample is what is ported from https://github.com/apache/incubator-mxnet/blob/master/cpp-package/example/test_score.cpp to C#.

## How to use?

## 1. Build project

````
cd <TestScore_dir>
dotnet build --configuration Release
````

At last, copy ***libmxnet.dll*** and dependencies to output directory; &lt;TestScore_dir&gt;\bin\Release\netcoreapp2.0.

## 2. Download test data

Download mnist data from [THE MNIST DATABASE of handwritten digits](http://yann.lecun.com/exdb/mnist/).</br>
The following command do downloading test data automatically.

````
cd <TestScore_dir>
mnist.bat
````

## 3. Run

````
cd <TestScore_dir>
dotnet run --configuration Release 0.9
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