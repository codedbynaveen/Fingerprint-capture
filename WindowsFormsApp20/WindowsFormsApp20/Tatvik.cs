using System;
using System.Runtime.InteropServices;

// Token: 0x02000002 RID: 2
internal class TMF20FPWrapper
{
    // Token: 0x06000001 RID: 1
    [DllImport("TMF20Driver.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern int tmfIsDeviceConnected();

    // Token: 0x06000002 RID: 2
    [DllImport("TMF20Driver.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern int tmfCaptureFingerprint(byte[] imgData, int[] imgWidth, int[] imgHeight, int timeoutInMills, int flagLeave);

    // Token: 0x06000003 RID: 3
    [DllImport("TMF20Driver.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern int tmfGetDeviceUniqueCode(byte[] deviceUUID, byte[] serialNumber);

    // Token: 0x06000004 RID: 4
    [DllImport("TMF20Driver.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern int tmfGetTzFMR(byte[] inpImgData, int nWidth, int nHeight, byte[] tzBuf, int[] fmrRecordSize);

    // Token: 0x06000005 RID: 5
    [DllImport("TMF20Driver.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern int tmfFingerMatchFMR(byte[] fingerdata1, byte[] fingerdata2, int level);
}
