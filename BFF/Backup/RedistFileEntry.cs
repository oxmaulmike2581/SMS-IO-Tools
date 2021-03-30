// Decompiled with JetBrains decompiler
// Type: BFF.RedistFileEntry
// Assembly: BFF, Version=1.0.22.114, Culture=neutral, PublicKeyToken=null
// MVID: 19420469-CC8C-4F6A-85E1-92AAE51BB3F4
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BFF.dll

namespace BFF
{
  public class RedistFileEntry
  {
    public int Index { get; set; }

    public string Name { get; set; }

    public int Size { get; set; }

    public int USize { get; set; }

    public int Offset { get; set; }

    public RedistFileEntry(int i, string n, int s, int u, int o)
    {
      this.Index = i;
      this.Name = n;
      this.Size = s;
      this.USize = u;
      this.Offset = o;
    }
  }
}
