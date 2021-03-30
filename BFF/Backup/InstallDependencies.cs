// Decompiled with JetBrains decompiler
// Type: BFF.InstallDependencies
// Assembly: BFF, Version=1.0.22.114, Culture=neutral, PublicKeyToken=null
// MVID: 19420469-CC8C-4F6A-85E1-92AAE51BB3F4
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BFF.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BFF
{
  public class InstallDependencies
  {
    public Dictionary<string, List<string>> Files;
    public List<string> Archives;

    public InstallDependencies(string _path, string item)
    {
      this.Files = new Dictionary<string, List<string>>();
      this.Archives = new List<string>();
      string path;
      if (item.IndexOf("\\") == -1)
      {
        path = string.Format("{0}\\PakFiles\\{1}_Uninstall.bfi", (object) _path, (object) item);
      }
      else
      {
        path = string.Format("{0}\\Install.bfi", (object) item.Substring(0, item.LastIndexOf("\\")));
        item = item.Substring(item.LastIndexOf("\\") + 1);
        item = item.Substring(0, item.LastIndexOf("."));
      }
      StreamReader streamReader = File.Exists(path) ? new StreamReader(path, Encoding.Default) : throw new Exception(string.Format("Script not found ({0}.bfi)", (object) item));
      try
      {
        if (streamReader.ReadLine() != "BFI")
          throw new Exception("File invalid!");
        if (streamReader.ReadLine() != string.Format("{0}.zip.bfi", (object) item))
          throw new Exception("Script corrupted");
label_10:
        string str = streamReader.ReadLine();
        if (str == null)
          return;
        string[] strArray = str.Split('=');
        int result;
        if (!int.TryParse(strArray[1], out result))
          throw new Exception("Script corrupted. Not valid number of files");
        if (strArray[0].StartsWith("PakFiles\\"))
          strArray[0] = strArray[0].Substring(9);
        this.Archives.Add(strArray[0]);
        List<string> stringList = new List<string>();
        this.Files.Add(strArray[0], stringList);
        for (int index = 0; index < result; ++index)
          stringList.Add(streamReader.ReadLine() ?? throw new Exception("Script corrupted. Unexpected end of file"));
        goto label_10;
      }
      catch (Exception ex)
      {
        throw;
      }
      finally
      {
        streamReader.Close();
      }
    }
  }
}
