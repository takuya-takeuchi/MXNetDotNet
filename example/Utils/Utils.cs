using System;
using System.Collections.Generic;
using System.IO;
using MXNetDotNet;

namespace Examples
{

    public class Utils
    {

        #region Methods

        public static bool IsFileExists(string filename)
        {
            return File.Exists(filename);
        }

        public static bool CheckDataFiles(IList<string> dataFiles)
        {
            for (var index = 0; index < dataFiles.Count; index++)
            {
                if (IsFileExists(dataFiles[index]))
                    continue;

                Logging.LG($"Error: File does not exist: {dataFiles[index]}");
                return false;
            }

            return true;
        }

        public static bool SetDataIter(MXDataIter iter, string useType, IList<string> dataFiles, int batchSize)
        {
            if (!CheckDataFiles(dataFiles))
            {
                return false;
            }

            iter.SetParam("batch_size", batchSize);
            iter.SetParam("shuffle", 1);
            iter.SetParam("flat", 1);

            switch (useType)
            {
                case "Train":
                    iter.SetParam("image", dataFiles[0]);
                    iter.SetParam("label", dataFiles[1]);
                    break;
                case "Label":
                    iter.SetParam("image", dataFiles[2]);
                    iter.SetParam("label", dataFiles[3]);
                    break;
            }

            iter.CreateDataIter();

            return true;
        }

        #endregion

    }

}
