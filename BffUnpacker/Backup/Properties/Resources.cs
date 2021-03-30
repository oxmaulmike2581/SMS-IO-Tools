// Decompiled with JetBrains decompiler
// Type: BffUnpacker.Properties.Resources
// Assembly: BffUnpacker, Version=1.0.11.2051, Culture=neutral, PublicKeyToken=null
// MVID: 6905B5B6-9334-40C1-AEBB-A5EB06804AFD
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BffUnpacker.exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace BffUnpacker.Properties
{
  [DebuggerNonUserCode]
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (BffUnpacker.Properties.Resources.resourceMan == null)
          BffUnpacker.Properties.Resources.resourceMan = new ResourceManager("BffUnpacker.Properties.Resources", typeof (BffUnpacker.Properties.Resources).Assembly);
        return BffUnpacker.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get => BffUnpacker.Properties.Resources.resourceCulture;
      set => BffUnpacker.Properties.Resources.resourceCulture = value;
    }
  }
}
