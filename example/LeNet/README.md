# LeNet

This sample is what is ported from https://github.com/apache/incubator-mxnet/blob/master/cpp-package/example/lenet.cpp to C#.

## How to use?

## 1. Build project

````
cd <LeNet_dir>
dotnet build --configuration Release
````

At last, copy ***libmxnet.dll*** and dependencies to output directory; &lt;LeNet_dir&gt;\bin\Release\netcoreapp2.0

## 2. Download test data

Download data from [Digit Recognizer](https://www.kaggle.com/c/digit-recognizer/data) on **Kaggle**.  
To download test data, you must create account of Kaggle.  
After login, go to avobe page and download it.

![KAggle](images/kaggle.png "Kaggle")

After download, downaloded file move to **<LeNet_dir>**.

## 3. Run

````
cd <LeNet_dir>
dotnet run --configuration Release 0.9
````

## 4. Others

### Use CPU

If you want to use CPU, change the following code, build and run!!

````
this._CtxDev = new Context(DeviceType.GPU, 0);
````

### Program does NOT exit!!

Please refer [C++ demo memory release problem](https://github.com/apache/incubator-mxnet/issues/7973)