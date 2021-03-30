// Decompiled with JetBrains decompiler
// Type: BFF.Compress
// Assembly: BFF, Version=1.0.22.114, Culture=neutral, PublicKeyToken=null
// MVID: 19420469-CC8C-4F6A-85E1-92AAE51BB3F4
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BFF.dll

using System;
using System.Runtime.InteropServices;

namespace BFF
{
  public class Compress
  {
    [DllImport("XComp.dll", CharSet = CharSet.Unicode)]
    public static extern int My_XMemCreateCompressionContext(
      int codec_type,
      IntPtr codec_params,
      ushort flags,
      out IntPtr pContext);

    [DllImport("XComp.dll", CharSet = CharSet.Unicode)]
    public static extern int My_XMemCompress(
      IntPtr pContext,
      [MarshalAs(UnmanagedType.LPArray)] byte[] pDestBuffer,
      out int pDestSize,
      [MarshalAs(UnmanagedType.LPArray)] byte[] source,
      int sourceSize);
  }
}
