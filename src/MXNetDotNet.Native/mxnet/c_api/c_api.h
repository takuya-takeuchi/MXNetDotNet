#ifndef _CPP_C_API_C_API_H_
#define _CPP_C_API_C_API_H_

#include "mxnet/c_api.h"
#include "../export.h"
#include "../shared.h"

DLLEXPORT int c_api_MXGetVersion(int* out)
{
    return MXGetVersion(out);
}

#endif
