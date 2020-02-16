#ifndef _CPP_AUXILIARY_H_
#define _CPP_AUXILIARY_H_

#include "export.h"
#include "shared.h"

DLLEXPORT bool is_support_vulkan()
{
#if NCNN_VULKAN
    return true;
#else
    return false;
#endif
}

#endif
