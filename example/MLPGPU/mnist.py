import os
import gzip

if not os.path.exists('mnist'):
  os.mkdir('mnist_data')
files = ["train-labels-idx1-ubyte.gz", "t10k-images-idx3-ubyte.gz", "t10k-labels-idx1-ubyte.gz", "train-images-idx3-ubyte.gz"]
for ff in files:
  f = open('mnist_data/' + ff[:-3], 'wb')
  f.write(gzip.open(ff, mode='rb').read())
  f.close()