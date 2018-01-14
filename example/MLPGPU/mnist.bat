python -m wget http://yann.lecun.com/exdb/mnist/train-images-idx3-ubyte.gz
python -m wget http://yann.lecun.com/exdb/mnist/train-labels-idx1-ubyte.gz
python -m wget http://yann.lecun.com/exdb/mnist/t10k-images-idx3-ubyte.gz
python -m wget http://yann.lecun.com/exdb/mnist/t10k-labels-idx1-ubyte.gz
python mnist.py
del t10k-images-idx3-ubyte.gz
del t10k-labels-idx1-ubyte.gz
del train-images-idx3-ubyte.gz
del train-labels-idx1-ubyte.gz