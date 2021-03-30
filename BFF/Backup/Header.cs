// Decompiled with JetBrains decompiler
// Type: BFF.Header
// Assembly: BFF, Version=1.0.22.114, Culture=neutral, PublicKeyToken=null
// MVID: 19420469-CC8C-4F6A-85E1-92AAE51BB3F4
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BFF.dll

using System;
using System.Runtime.InteropServices;

namespace BFF
{
  [Serializable]
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct Header
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] sign;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] sign2;
    public int files;
    public int align;
    public int zeros1;
    public int offset;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public byte[] filename;
    public int x118_sizeof_infotable;
    public int zeros2;
    public int x120_sizeof_nametable_plus_308h;
    public int field_offset;
    public int field_size;
    public byte zero3;
    public byte x12d;
    public byte zero4;
    public byte zero5;
    [MarshalAs(UnmanagedType.SafeArray)]
    public byte[] infotable;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] sign3;
    public int zero6;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public byte[] gecko;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public byte[] filepath;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public byte[] pc;
    [MarshalAs(UnmanagedType.SafeArray)]
    public byte[] nametable;
  }
}
