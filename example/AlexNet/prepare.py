import zipfile
import os

# extract zips
zip = zipfile.ZipFile('train.zip', 'r')
zip.extractall()
zip = zipfile.ZipFile('test.zip', 'r')
zip.extractall()

# move to files
source_dir = "./data"
train_dir = "./train"
valid_dir = "./test"

if not os.path.exists("./data"):
  os.makedirs("./data")
if not os.path.exists("./data/train"):
  os.makedirs("./data/train")
if not os.path.exists("./data/test"):
  os.makedirs("./data/test")
if not os.path.exists("./data/test/dogs"):
  os.makedirs("./data/test/dogs")
if not os.path.exists("./data/test/cats"):
  os.makedirs("./data/test/cats")
if not os.path.exists("./data/train/dogs"):
  os.makedirs("./data/train/dogs")
if not os.path.exists("./data/train/cats"):
  os.makedirs("./data/train/cats")

for i in range(10000):
    os.rename("%s/dog.%d.jpg" % ("./train", i),
              "%s/dogs/dog%05d.jpg" % ("./data/train", i))
    os.rename("%s/cat.%d.jpg" % ("./train", i),
              "%s/cats/cat%05d.jpg" % ("./data/train", i))

for i in range(2500):
    os.rename("%s/dog.%d.jpg" % ("./train", i + 10000),
              "%s/dogs/dog%05d.jpg" % ("./data/test", i+ 10000))
    os.rename("%s/cat.%d.jpg" % ("./train", i+ 10000),
              "%s/cats/cat%05d.jpg" % ("./data/test", i+ 10000))