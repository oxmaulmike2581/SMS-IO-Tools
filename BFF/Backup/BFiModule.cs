// Decompiled with JetBrains decompiler
// Type: BFF.BFiModule
// Assembly: BFF, Version=1.0.22.114, Culture=neutral, PublicKeyToken=null
// MVID: 19420469-CC8C-4F6A-85E1-92AAE51BB3F4
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BFF.dll

using System.IO;
using System.Text;
using System.Xml;

namespace BFF
{
  public class BFiModule
  {
    private string _script;

    public string Module { get; set; }

    public string Author { get; set; }

    public int MajorVersion { get; set; }

    public string MinorVersion { get; set; }

    public string SupportUrl { get; set; }

    public string Description { get; set; }

    public string ScriptName => this._script;

    public BFiModule() => this.Description = string.Empty;

    public BFiModule(string bfiScript)
    {
      StreamReader streamReader = new StreamReader(bfiScript, Encoding.Default);
      if (streamReader.ReadLine() == "BFI")
      {
        string str1 = streamReader.ReadLine();
        if (str1 != null)
        {
          this._script = str1;
          string str2 = this._script.Substring(0, this._script.LastIndexOf(".") - 1);
          string str3 = str2.Substring(0, str2.LastIndexOf(".")) + ".xml";
          if (!File.Exists(str3))
            throw new FileNotFoundException("XML install file not found");
          XmlDocument xmlDocument = new XmlDocument();
          xmlDocument.Load(str3);
          XmlNode xmlNode1 = xmlDocument.SelectSingleNode("//Reflection/Mod");
          if (xmlNode1 != null)
          {
            XmlAttribute attribute1 = xmlNode1.Attributes["mod"];
            XmlAttribute attribute2 = xmlNode1.Attributes["author"];
            XmlAttribute attribute3 = xmlNode1.Attributes["major-version"];
            XmlAttribute attribute4 = xmlNode1.Attributes["minor-version"];
            XmlAttribute attribute5 = xmlNode1.Attributes["support-url"];
            if (xmlNode1 != null && attribute2 != null && (attribute3 != null && attribute4 != null) && attribute5 != null)
            {
              this.Module = attribute1.Value;
              this.Author = attribute2.Value;
              int result;
              int.TryParse(attribute3.Value, out result);
              this.MajorVersion = result;
              this.MinorVersion = attribute4.Value;
              this.SupportUrl = attribute5.Value;
              XmlNode xmlNode2 = xmlNode1.SelectSingleNode(nameof (Description));
              if (xmlNode2 != null)
                this.Description = xmlNode2.InnerText;
            }
          }
        }
      }
      streamReader.Close();
    }
  }
}
