// Decompiled with JetBrains decompiler
// Type: BFF.NameTable
// Assembly: BFF, Version=1.0.22.114, Culture=neutral, PublicKeyToken=null
// MVID: 19420469-CC8C-4F6A-85E1-92AAE51BB3F4
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BFF.dll

using System.Runtime.InteropServices;

namespace BFF
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct NameTable
  {
    public int filename_offset;
    public int zero;
    public int zero1;
    public int zero2;
  }
}
