// Decompiled with JetBrains decompiler
// Type: BFF.RedistHeader
// Assembly: BFF, Version=1.0.22.114, Culture=neutral, PublicKeyToken=null
// MVID: 19420469-CC8C-4F6A-85E1-92AAE51BB3F4
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BFF.dll

using System.Text;

namespace BFF
{
  public class RedistHeader
  {
    public string SourceFolder;
    public byte[] Magic;
    public int FileSize;

    public RedistHeader(string sourceFolder)
    {
      this.SourceFolder = sourceFolder;
      this.Magic = Encoding.Default.GetBytes("BFFZ");
      this.FileSize = 0;
    }
  }
}
