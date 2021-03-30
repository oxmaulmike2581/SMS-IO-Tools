// Decompiled with JetBrains decompiler
// Type: BFF.RedistEntry
// Assembly: BFF, Version=1.0.22.114, Culture=neutral, PublicKeyToken=null
// MVID: 19420469-CC8C-4F6A-85E1-92AAE51BB3F4
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BFF.dll

using System;
using System.Text;

namespace BFF
{
  public class RedistEntry
  {
    public byte[] ArchiveName;
    public int FilesInArchive;
    public int NextArchiveOffset;

    public RedistEntry(string archive, int files)
    {
      this.ArchiveName = new byte[256];
      Encoding.Default.GetBytes(archive).CopyTo((Array) this.ArchiveName, 0);
      this.FilesInArchive = files;
      this.NextArchiveOffset = 0;
    }
  }
}
