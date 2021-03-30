// Decompiled with JetBrains decompiler
// Type: BFF.InfoTable
// Assembly: BFF, Version=1.0.22.114, Culture=neutral, PublicKeyToken=null
// MVID: 19420469-CC8C-4F6A-85E1-92AAE51BB3F4
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BFF.dll

using System.Runtime.InteropServices;

namespace BFF
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct InfoTable
  {
    public uint dummy1;
    public uint dummy2;
    public int offset;
    public int everzero;
    public int zsize;
    public int size;
    public int everzero2;
    public int everzero3;
    public short type;
    public uint crc;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] extension;
  }
}
