// Decompiled with JetBrains decompiler
// Type: BFF.Prefs
// Assembly: BFF, Version=1.0.22.114, Culture=neutral, PublicKeyToken=null
// MVID: 19420469-CC8C-4F6A-85E1-92AAE51BB3F4
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BFF.dll

using System;
using System.IO;
using System.Xml;

namespace BFF
{
  public class Prefs
  {
    public string SourceFolder { get; set; }

    public string OriginalFile { get; set; }

    public string DestinationFile { get; set; }

    public Prefs()
    {
      this.SourceFolder = "";
      this.OriginalFile = "";
      this.DestinationFile = "";
    }

    public void LoadPrefs(string configPath)
    {
      try
      {
        if (!File.Exists(configPath))
          return;
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(configPath);
        this.SourceFolder = xmlDocument.SelectSingleNode("//config/sourceFolder").Attributes["value"].Value;
        this.OriginalFile = xmlDocument.SelectSingleNode("//config/originalFile").Attributes["value"].Value;
        this.DestinationFile = xmlDocument.SelectSingleNode("//config/destinationFile").Attributes["value"].Value;
      }
      catch (Exception ex)
      {
        throw new Exception("Can't load preferences. Configuration file may be corrupted and will be recreated when you exit the application");
      }
    }
  }
}
