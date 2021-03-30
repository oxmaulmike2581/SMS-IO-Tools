// Decompiled with JetBrains decompiler
// Type: BFF.EventData
// Assembly: BFF, Version=1.0.22.114, Culture=neutral, PublicKeyToken=null
// MVID: 19420469-CC8C-4F6A-85E1-92AAE51BB3F4
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BFF.dll

namespace BFF
{
  public class EventData
  {
    public int TotalFiles { get; set; }

    public int CurrentFile { get; set; }

    public string Filename { get; set; }

    public int FileSize { get; set; }

    public string Message { get; set; }
  }
}
