// Decompiled with JetBrains decompiler
// Type: BffUnpacker.Program
// Assembly: BffUnpacker, Version=1.0.11.2051, Culture=neutral, PublicKeyToken=null
// MVID: 6905B5B6-9334-40C1-AEBB-A5EB06804AFD
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BffUnpacker.exe

using System;
using System.IO;
using System.Windows.Forms;

namespace BffUnpacker
{
  internal static class Program
  {
    [STAThread]
    private static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Form1 form1 = new Form1();
      string[] commandLineArgs = Environment.GetCommandLineArgs();
      if (commandLineArgs.Length == 2 && File.Exists(commandLineArgs[1]))
        form1.CurrentArchive = commandLineArgs[1];
      Application.Run((Form) form1);
    }
  }
}
