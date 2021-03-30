// Decompiled with JetBrains decompiler
// Type: BFF.BFF
// Assembly: BFF, Version=1.0.22.114, Culture=neutral, PublicKeyToken=null
// MVID: 19420469-CC8C-4F6A-85E1-92AAE51BB3F4
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BFF.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using Oodle.NET;

namespace BFF
{
  public class BFF : IDisposable
  {
    private InputInfo _files;
    private uint[] _crctable;
    public ManualResetEvent StopProcessing;

    [DllImport("XComp.dll")]
    public static extern int My_XMemCreateCompressionContext(int codec_type, IntPtr codec_params, ushort flags, out IntPtr pContext);

    [DllImport("XComp.dll")]
    public static extern int My_XMemCompress(IntPtr pContext, [MarshalAs(UnmanagedType.LPArray)] byte[] pDestBuffer, out int pDestSize, [MarshalAs(UnmanagedType.LPArray)] byte[] source, int sourceSize);

    [DllImport("XComp.dll")]
    public static extern int My_XMemCreateDecompressionContext(int codec_type, IntPtr codec_params, ushort flags, out IntPtr pContext);

    [DllImport("XComp.dll")]
    public static extern int My_XMemDestroyDecompressionContext(IntPtr pContext);

    [DllImport("XComp.dll")]
    public static extern int My_XMemDecompress(IntPtr pContext, [MarshalAs(UnmanagedType.LPArray)] byte[] pDestBuffer, out int pDestSize, [MarshalAs(UnmanagedType.LPArray)] byte[] source, int sourceSize);

    public string Result { get; set; }

    public event EventHandler RebuildStarted;

    public event EventHandler FileProcessing;

    public event EventHandler RebuildEnded;

    public BFF()
    {
      this._buildCRCTable();
      this.StopProcessing = new ManualResetEvent(false);
    }

    public BFF(string f1, string f2, string o)
      : this()
      => this.SetParams(f1, f2, o);

    public BFF(string f1, string f2)
      : this()
      => this.SetParams(f1, f2);

    public void SetParams(string f1, string f2) => this.SetParams(f1, f2, "");

    public void SetParams(string f1, string f2, string o) => this._files = new BFF.InputInfo(f1, f2, o);

    private void Assert(int n, int e)
    {
      if (n != e)
        throw new Exception(string.Format("Cannot read from file. Expected {0} bytes, Reader returned {1}", (object) e, (object) n));
    }

    public void Inject(string archive, string file, byte[] data, int dataLen, int usize) => this.Inject(archive, file, data, dataLen, usize, (FileStream) null, (List<string>) null, (RedistFile.UpdateDelegate) null);

    public void Inject(string archive, string file, byte[] data, int dataLen, int usize, RedistFile.UpdateDelegate upd)
    {
      this.Inject(archive, file, data, dataLen, usize, (FileStream) null, (List<string>) null, upd);
    }

    public void Inject(string archive, string file, byte[] data, int dataLen, int usize, FileStream fs, List<string> fs_list, RedistFile.UpdateDelegate upd)
    {
      if (!File.Exists(archive))
      {
        this.Result = string.Format("File {0} not found, ignored.", (object) archive);
        if (upd == null)
          return;
        upd(this.Result);
      }
      else
      {
        if (upd != null)
          upd(string.Format("Injecting {0} into {1}", (object) file, (object) archive));
        List<string> stringList;
        if (fs == null)
        {
          stringList = this.getFileList(archive);
          File.Delete(archive + ".tmp");
          File.Copy(archive, archive + ".tmp", true);
          fs = new FileStream(archive + ".tmp", FileMode.Open, FileAccess.ReadWrite);
        }
        else
          stringList = fs_list;
        int num1 = 288;
        int num2 = 304;
        byte[] buffer1 = new byte[4];
        fs.Seek(280L, SeekOrigin.Begin);
        this.Assert(fs.Read(buffer1, 0, 4), 4);
        int int32_1 = BitConverter.ToInt32(buffer1, 0);
        int num3 = num1 + (int32_1 + 776 + 16);
        fs.Seek(288L, SeekOrigin.Begin);
        this.Assert(fs.Read(buffer1, 0, 4), 4);
        BitConverter.ToInt32(buffer1, 0);
        int num4 = 0;
        byte[] buffer2 = new byte[1];
        try
        {
          string str1 = file;
          bool flag = false;
          string str2 = str1.Replace("\\-", "\\").Replace("\\+", "\\");
          int num5 = stringList.IndexOf(str2);
          if (num5 == -1)
            return;
          int num6 = num2 + 42 * num5;
          fs.Seek((long) (num6 + 8), SeekOrigin.Begin);
          this.Assert(fs.Read(buffer1, 0, 4), 4);
          int int32_2 = BitConverter.ToInt32(buffer1, 0);
          this.Assert(fs.Read(buffer1, 0, 4), 4);
          int position = (int) fs.Position;
          this.Assert(fs.Read(buffer1, 0, 4), 4);
          int num7 = BitConverter.ToInt32(buffer1, 0);
          if (num7 == 0)
          {
            num7 = dataLen;
            flag = true;
          }
          this.Assert(fs.Read(buffer1, 0, 4), 4);
          BitConverter.ToInt32(buffer1, 0);
          int num8 = position + 18;
          fs.Seek((long) num8, SeekOrigin.Begin);
          this.Assert(fs.Read(buffer1, 0, 4), 4);
          BitConverter.ToInt32(buffer1, 0);
          fs.Seek((long) position, SeekOrigin.Begin);
          fs.Write(BitConverter.GetBytes(dataLen), 0, 4);
          fs.Write(BitConverter.GetBytes(usize), 0, 4);
          fs.Seek((long) num8, SeekOrigin.Begin);
          uint crc = this._getCRC(data, dataLen);
          fs.Write(BitConverter.GetBytes(crc), 0, 4);
          int num9 = num2 + 42 * num5 + 38 - 6;
          fs.Seek((long) num9, SeekOrigin.Begin);
          this.Assert(fs.Read(buffer2, 0, 1), 1);
          fs.Seek(-1L, SeekOrigin.Current);

          // new byte[1][0] = buffer2[0];
          
          buffer2[0] = dataLen == usize ? (byte) 0 : (byte) 2;
          fs.Write(buffer2, 0, 1);
          if (flag)
          {
            int num10 = num3 + 16 * num5;
            fs.Seek((long) num10, SeekOrigin.Begin);
            this.Assert(fs.Read(buffer1, 0, 4), 4);
            int int32_3 = BitConverter.ToInt32(buffer1, 0);
            byte[] buffer3 = new byte[1];
            fs.Seek((long) int32_3, SeekOrigin.Begin);
            this.Assert(fs.Read(buffer3, 0, 1), 1);
            int length = (int) buffer3[0];
            byte[] buffer4 = new byte[length];
            this.Assert(fs.Read(buffer4, 0, length), length);
            int index = length - 1;
            byte[] numArray = new byte[5];
            while (buffer4[index] != (byte) 46 && index > 0)
              --index;
            if (buffer4[index] == (byte) 46)
            {
              int num11 = index + 1;
              int num12 = 0;
              while (num11 < length)
                numArray[num12++] = buffer4[num11++];
            }
            int num13 = num2 + 42 * num5 + 38;
            fs.Seek((long) num13, SeekOrigin.Begin);
            byte[] buffer5 = new byte[3];
            this.Assert(fs.Read(buffer5, 0, 3), 3);
            if ((int) buffer5[0] == (int) numArray[2] && (int) buffer5[1] == (int) numArray[1] && (int) buffer5[2] == (int) numArray[0])
            {
              byte num11 = buffer5[0];
              buffer5[0] = buffer5[2];
              buffer5[2] = num11;
              fs.Seek((long) num13, SeekOrigin.Begin);
              fs.Write(buffer5, 0, 3);
            }
          }
          if (dataLen - num7 >= num7 % 2048 && num5 < stringList.Count - 1)
          {
            fs.Seek((long) (num2 + 42 * (num5 + 1) + 8), SeekOrigin.Begin);
            this.Assert(fs.Read(buffer1, 0, 4), 4);
            if (BitConverter.ToInt32(buffer1, 0) - int32_2 < dataLen)
            {
              FileStream fileStream = new FileStream(archive, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
              int num10 = dataLen - num7;
              for (int index = stringList.Count - 1; index > num5; --index)
              {
                fs.Seek((long) (num2 + 42 * index + 8), SeekOrigin.Begin);
                this.Assert(fs.Read(buffer1, 0, 4), 4);
                int int32_3 = BitConverter.ToInt32(buffer1, 0);
                fs.Seek(4L, SeekOrigin.Current);
                this.Assert(fs.Read(buffer1, 0, 4), 4);
                int int32_4 = BitConverter.ToInt32(buffer1, 0);
                int num11 = int32_3 + num10 + 2048 - (int32_3 + num10) % 2048;
                fs.Seek((long) (num2 + 42 * index + 8), SeekOrigin.Begin);
                fs.Write(BitConverter.GetBytes(num11), 0, 4);
                fileStream.Seek((long) int32_3, SeekOrigin.Begin);
                byte[] buffer3 = new byte[int32_4];
                fileStream.Read(buffer3, 0, int32_4);
                fs.Seek((long) num11, SeekOrigin.Begin);
                fs.Write(buffer3, 0, int32_4);
              }
              fileStream.Close();
            }
          }
          fs.Seek((long) int32_2, SeekOrigin.Begin);
          fs.Write(data, 0, dataLen);
          int num14 = num4 + 1;
        }
        finally
        {
          this.Result = string.Format("OK. {0} injected into {1}", (object) file, (object) archive);
          if (upd != null)
            upd(this.Result);
          if (fs_list == null)
          {
            fs.Close();
            try
            {
              if (!File.Exists(archive + ".bkp"))
                File.Move(archive, archive + ".bkp");
              else
                File.Delete(archive);
              int millisecondsTimeout = 1000;
              for (int index = 0; index < 5; ++index)
              {
                try
                {
                  File.Move(archive + ".tmp", archive);
                  break;
                }
                catch (Exception ex)
                {
                  Thread.Sleep(millisecondsTimeout);
                }
              }
            }
            catch (Exception ex)
            {
              this.Result = "Can't rename working archive.";
              if (upd != null)
                upd(this.Result);
            }
          }
        }
      }
    }

    public void Inject(bool alwaysUpdate)
    {
      List<FileInfo> files = new List<FileInfo>();
      DirectoryInfo di = new DirectoryInfo(this._files.SourceFolder);
      this.getSubFolderFiles(files, di);
      this.getFiles(files, di);
      if (files.Count == 0)
        this.Result = "No files found";
      else
        this.doInject(files, alwaysUpdate);
    }

    private void doInject(List<FileInfo> files, bool alwaysUpdate)
    {
      DateTime lastWriteTime1 = File.GetLastWriteTime(this._files.OriginalArchive);
      EventData eventData = new EventData();
      List<string> fileList = this.getFileList();
      int startIndex = this._files.SourceFolder.Length + 1;
      foreach (FileSystemInfo file in files)
      {
        string str = file.FullName.Substring(startIndex);
        if (fileList.Contains(str))
          ++eventData.TotalFiles;
      }
      if (this.RebuildStarted != null)
        this.RebuildStarted((object) eventData, EventArgs.Empty);
      File.Copy(this._files.OriginalArchive, this._files.OriginalArchive + ".tmp", true);
      IntPtr pContext = IntPtr.Zero;
      BFF.My_XMemCreateCompressionContext(1, IntPtr.Zero, (ushort) 0, out pContext);
      int num1 = 304;
      byte[] buffer1 = new byte[4];
      FileStream fileStream1 = new FileStream(this._files.OriginalArchive + ".tmp", FileMode.Open);
      fileStream1.Seek(280L, SeekOrigin.Begin);
      this.Assert(fileStream1.Read(buffer1, 0, 4), 4);
      BitConverter.ToInt32(buffer1, 0);
      int num2 = 0;
      try
      {
        int index1 = 0;
        foreach (FileInfo file in files)
        {
          if (!this.StopProcessing.WaitOne(1))
          {
            string str = file.FullName.Substring(startIndex);
            DateTime lastWriteTime2 = file.LastWriteTime;
            if (alwaysUpdate || lastWriteTime2.CompareTo(lastWriteTime1) >= 0)
            {
              int num3 = fileList.IndexOf(str);
              if (num3 != -1)
              {
                int num4 = num1 + 42 * num3;
                fileStream1.Seek((long) (num4 + 8), SeekOrigin.Begin);
                this.Assert(fileStream1.Read(buffer1, 0, 4), 4);
                int int32_1 = BitConverter.ToInt32(buffer1, 0);
                this.Assert(fileStream1.Read(buffer1, 0, 4), 4);
                int position = (int) fileStream1.Position;
                this.Assert(fileStream1.Read(buffer1, 0, 4), 4);
                int int32_2 = BitConverter.ToInt32(buffer1, 0);
                this.Assert(fileStream1.Read(buffer1, 0, 4), 4);
                BitConverter.ToInt32(buffer1, 0);
                int num5 = position + 18;
                fileStream1.Seek((long) num5, SeekOrigin.Begin);
                this.Assert(fileStream1.Read(buffer1, 0, 4), 4);
                BitConverter.ToInt32(buffer1, 0);
                FileStream fileStream2 = new FileStream(file.FullName, FileMode.Open);
                int length = (int) fileStream2.Length;
                byte[] numArray1 = new byte[length];
                fileStream2.Read(numArray1, 0, length);
                fileStream2.Close();
                int pDestSize = length * 3;
                byte[] numArray2 = new byte[pDestSize];
                switch (BFF.My_XMemCompress(pContext, numArray2, out pDestSize, numArray1, length))
                {
                  case -2116149247:
                  case 0:
                    if (pDestSize > length)
                      pDestSize = length;
                    if (pDestSize - int32_2 >= int32_2 % 2048 && num3 < fileList.Count - 1)
                    {
                      fileStream1.Seek((long) (num1 + 42 * (num3 + 1) + 8), SeekOrigin.Begin);
                      this.Assert(fileStream1.Read(buffer1, 0, 4), 4);
                      if (BitConverter.ToInt32(buffer1, 0) - num4 < pDestSize)
                      {
                        FileStream fileStream3 = new FileStream(this._files.OriginalArchive, FileMode.Open);
                        int num6 = pDestSize - int32_2;
                        for (int index2 = fileList.Count - 1; index2 > num3; ++index2)
                        {
                          fileStream1.Seek((long) (num1 + 42 * index2 + 8), SeekOrigin.Begin);
                          this.Assert(fileStream1.Read(buffer1, 0, 4), 4);
                          int int32_3 = BitConverter.ToInt32(buffer1, 0);
                          fileStream1.Seek(4L, SeekOrigin.Current);
                          this.Assert(fileStream1.Read(buffer1, 0, 4), 4);
                          int int32_4 = BitConverter.ToInt32(buffer1, 0);
                          int num7 = int32_3 + num6 + 2048 - (int32_3 + num6) % 2048;
                          fileStream1.Seek((long) (num1 + 42 * index2 + 8), SeekOrigin.Begin);
                          fileStream1.Write(BitConverter.GetBytes(num7), 0, 4);
                          fileStream3.Seek((long) int32_3, SeekOrigin.Begin);
                          byte[] buffer2 = new byte[int32_4];
                          fileStream3.Read(buffer2, 0, int32_4);
                          fileStream1.Seek((long) num7, SeekOrigin.Begin);
                          fileStream1.Write(buffer2, 0, int32_4);
                        }
                        fileStream3.Close();
                      }
                    }
                    fileStream1.Seek((long) int32_1, SeekOrigin.Begin);
                    if (pDestSize == length)
                      fileStream1.Write(numArray1, 0, length);
                    else
                      fileStream1.Write(numArray2, 0, pDestSize);
                    fileStream1.Seek((long) position, SeekOrigin.Begin);
                    fileStream1.Write(BitConverter.GetBytes(pDestSize), 0, 4);
                    fileStream1.Write(BitConverter.GetBytes(length), 0, 4);
                    if (pDestSize == length)
                    {
                      fileStream1.Seek(8L, SeekOrigin.Current);
                      byte[] buffer2 = new byte[1];
                      this.Assert(fileStream1.Read(buffer2, 0, 1), 1);
                      fileStream1.Seek(-1L, SeekOrigin.Current);
                      buffer2[0] = (byte) 0;
                      fileStream1.Write(buffer2, 0, 1);
                    }
                    fileStream1.Seek((long) num5, SeekOrigin.Begin);
                    fileStream1.Write(BitConverter.GetBytes(this._getCRC(numArray2, pDestSize)), 0, 4);
                    ++num2;
                    if (this.FileProcessing != null)
                    {
                      eventData.CurrentFile = index1;
                      eventData.Filename = files[index1].FullName.Substring(startIndex);
                      eventData.Message = string.Format("(O:{0} KB - C:{1} KB)", (object) Decimal.Round((Decimal) length / 1024M, 2).ToString("f2"), (object) Decimal.Round((Decimal) pDestSize / 1024M, 2).ToString("f0"));
                      this.FileProcessing((object) eventData, EventArgs.Empty);
                      break;
                    }
                    break;
                  default:
                    throw new Exception("Can't compress. Aborting processing.");
                }
              }
              else
              {
                eventData.CurrentFile = -1;
                eventData.Filename = str;
                eventData.Message = string.Format("File {0} not found in original archive, ignored.", (object) str);
                this.FileProcessing((object) eventData, EventArgs.Empty);
              }
            }
            else
            {
              eventData.CurrentFile = -1;
              eventData.Filename = str;
              eventData.Message = string.Format("File {0} is older than archive, ignored.", (object) str);
              this.FileProcessing((object) eventData, EventArgs.Empty);
            }
            ++index1;
          }
          else
            break;
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
      finally
      {
        fileStream1.Close();
        try
        {
          File.Delete(this._files.OriginalArchive);
          File.Move(this._files.OriginalArchive + ".tmp", this._files.OriginalArchive);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Can't rename working archive.");
        }
      }
      this.Result = string.Format("{0} file(s) injected", (object) num2);
      if (this.RebuildEnded == null)
        return;
      this.RebuildEnded((object) null, EventArgs.Empty);
    }

    private void getSubFolderFiles(List<FileInfo> files, DirectoryInfo di)
    {
      foreach (DirectoryInfo directory in di.GetDirectories())
      {
        this.getSubFolderFiles(files, directory);
        this.getFiles(files, directory);
      }
    }

    private void getFiles(List<FileInfo> files, DirectoryInfo di)
    {
      foreach (FileInfo file in di.GetFiles())
        files.Add(file);
    }

    public void Build()
    {
      this.StopProcessing.Reset();
      EventData eventData = new EventData();
      List<string> fileList = this.getFileList();
      if (this.RebuildStarted != null)
      {
        eventData.TotalFiles = fileList.Count;
        this.RebuildStarted((object) eventData, EventArgs.Empty);
      }
      Header header = new Header();
      header.sign = new byte[4]
      {
        (byte) 32,
        (byte) 75,
        (byte) 65,
        (byte) 80
      };
      header.sign2 = new byte[4]
      {
        (byte) 3,
        (byte) 0,
        (byte) 64,
        (byte) 16
      };
      header.sign3 = new byte[5]
      {
        (byte) 32,
        (byte) 84,
        (byte) 88,
        (byte) 69,
        (byte) 84
      };
      header.filename = new byte[256];
      header.gecko = new byte[256];
      header.filepath = new byte[256];
      header.pc = new byte[256];
      int[] fromOriginalArchive = this.getStaticDataFromOriginalArchive();
      header.align = fromOriginalArchive[0];
      header.field_offset = fromOriginalArchive[1];
      header.field_size = fromOriginalArchive[2];
      header.offset = header.align;
      List<BFF.StaticData> staticData1 = this.getStaticData(fileList);
      header.files = fileList.Count;
      string name = new FileInfo(this._files.OutputArchive).Name;
      Encoding.Default.GetBytes(name.Substring(0, name.Length - 4)).CopyTo((Array) header.filename, 0);
      Encoding.Default.GetBytes("Gecko.xml").CopyTo((Array) header.gecko, 0);
      Encoding.Default.GetBytes(this._files.SourceFolder).CopyTo((Array) header.filepath, 0);
      Encoding.Default.GetBytes("PC").CopyTo((Array) header.pc, 0);
      InfoTable[] infoTableArray = new InfoTable[fileList.Count];
      for (int index = 0; index < fileList.Count; ++index)
      {
        BFF.StaticData staticData2 = staticData1[index];
        infoTableArray[index].dummy1 = staticData2.dummy1;
        infoTableArray[index].dummy2 = staticData2.dummy2;
        infoTableArray[index].offset = header.align;
        infoTableArray[index].type = (short) 2;
        infoTableArray[index].everzero = staticData2.everzero;
        infoTableArray[index].everzero2 = staticData2.everzero2;
        infoTableArray[index].everzero3 = staticData2.everzero3;
        infoTableArray[index].size = staticData2.fileSize;
        infoTableArray[index].zsize = staticData2.fileSize;
      }
      header.infotable = new byte[42 * fileList.Count];
      header.x118_sizeof_infotable = 42 * fileList.Count;
      FileStream fileStream1 = new FileStream(this._files.OriginalArchive, FileMode.Open);
      fileStream1.Read(header.infotable, 0, header.x118_sizeof_infotable);
      byte[] buffer1 = new byte[header.field_size];
      fileStream1.Seek((long) header.field_offset, SeekOrigin.Begin);
      fileStream1.Read(buffer1, 0, header.field_size);
      fileStream1.Close();
      for (int index = 0; index < fileList.Count; ++index)
      {
        string str = string.Format("{0}\\{1}", (object) this._files.SourceFolder, (object) fileList[index]);
        string s = new FileInfo(str).Extension.Substring(1);
        if (s.Length > 3)
          s = s.Substring(0, 3);
        byte[] bytes = Encoding.Default.GetBytes(s);
        infoTableArray[index].extension = new byte[4];
        bytes.CopyTo((Array) infoTableArray[index].extension, 0);
        FileStream fileStream2 = new FileStream(str, FileMode.Open);
        byte[] buffer2 = new byte[(int) fileStream2.Length];
        fileStream2.Read(buffer2, 0, (int) fileStream2.Length);
        fileStream2.Close();
        infoTableArray[index].size = buffer2.Length;
        infoTableArray[index].zsize = buffer2.Length;
      }
      IntPtr num1 = Marshal.AllocHGlobal(header.x118_sizeof_infotable);
      for (int index = 0; index < fileList.Count; ++index)
        Marshal.StructureToPtr((object) infoTableArray[index], new IntPtr(num1.ToInt64() + (long) (42 * index)), false);
      Marshal.Copy(num1, header.infotable, 0, header.infotable.Length);
      Marshal.FreeHGlobal(num1);
      header.x120_sizeof_nametable_plus_308h = 16 * fileList.Count;
      for (int index = 0; index < fileList.Count; ++index)
        header.x120_sizeof_nametable_plus_308h += fileList[index].Length + 1;
      byte[] buffer3 = new byte[4];
      FileStream fileStream3 = new FileStream(this._files.OutputArchive, FileMode.Create);
      fileStream3.Seek(0L, SeekOrigin.Begin);
      fileStream3.Write(header.sign, 0, 4);
      fileStream3.Write(header.sign2, 0, 4);
      fileStream3.Write(BitConverter.GetBytes(header.files), 0, 4);
      fileStream3.Write(BitConverter.GetBytes(header.align), 0, 4);
      fileStream3.Seek(4L, SeekOrigin.Current);
      fileStream3.Write(BitConverter.GetBytes(2048), 0, 4);
      fileStream3.Write(header.filename, 0, header.filename.Length);
      fileStream3.Write(BitConverter.GetBytes(header.x118_sizeof_infotable), 0, 4);
      fileStream3.Seek(4L, SeekOrigin.Current);
      fileStream3.Write(BitConverter.GetBytes(header.x120_sizeof_nametable_plus_308h + 776), 0, 4);
      fileStream3.Write(BitConverter.GetBytes(header.field_offset), 0, 4);
      fileStream3.Write(BitConverter.GetBytes(header.field_size), 0, 4);
      fileStream3.Write(buffer3, 0, 1);
      fileStream3.Write(buffer3, 0, 1);
      fileStream3.Write(buffer3, 0, 1);
      fileStream3.Write(buffer3, 0, 1);
      int position1 = (int) fileStream3.Position;
      fileStream3.Write(header.infotable, 0, header.infotable.Length);
      fileStream3.Write(header.sign3, 0, 5);
      fileStream3.Write(buffer3, 0, 3);
      fileStream3.Write(header.gecko, 0, header.gecko.Length);
      fileStream3.Write(header.filepath, 0, header.filepath.Length);
      fileStream3.Write(header.pc, 0, header.pc.Length);
      long position2 = fileStream3.Position;
      int count = fileList.Count;
      int nametablePlus308h = header.x120_sizeof_nametable_plus_308h;
      byte[] buffer4 = new byte[nametablePlus308h];
      FileStream fileStream4 = new FileStream(this._files.OriginalArchive, FileMode.Open);
      fileStream4.Seek(fileStream3.Position, SeekOrigin.Begin);
      fileStream4.Read(buffer4, 0, nametablePlus308h);
      fileStream4.Close();
      fileStream3.Write(buffer4, 0, nametablePlus308h);
      fileStream3.Write(buffer1, 0, buffer1.Length);
      fileStream3.Seek((long) header.offset, SeekOrigin.Begin);
      IntPtr pContext = IntPtr.Zero;
      BFF.My_XMemCreateCompressionContext(1, IntPtr.Zero, (ushort) 0, out pContext);
      for (int index = 0; index < fileList.Count && !this.StopProcessing.WaitOne(1); ++index)
      {
        if (this.FileProcessing != null)
        {
          eventData.CurrentFile = index;
          eventData.Filename = fileList[index];
          eventData.FileSize = infoTableArray[index].size;
          this.FileProcessing((object) eventData, EventArgs.Empty);
        }
        FileStream fileStream2 = new FileStream(string.Format("{0}\\{1}", (object) this._files.SourceFolder, (object) fileList[index]), FileMode.Open);
        int length = (int) fileStream2.Length;
        byte[] numArray1 = new byte[length];
        fileStream2.Read(numArray1, 0, length);
        fileStream2.Close();
        int position3 = (int) fileStream3.Position;
        int pDestSize = length;
        byte[] numArray2 = new byte[pDestSize];
        bool flag = false;
        byte[] buffer2 = new byte[1];
        switch (BFF.My_XMemCompress(pContext, numArray2, out pDestSize, numArray1, length))
        {
          case -2116149247:
            if (!flag)
            {
              fileStream3.Write(numArray1, 0, length);
              pDestSize = length;
              buffer2[0] = (byte) 0;
            }
            else
            {
              buffer2[0] = (byte) 2;
              fileStream3.Write(numArray2, 0, pDestSize);
            }
            int position4 = (int) fileStream3.Position;
            fileStream3.Seek((long) (position1 + 16 + 42 * index), SeekOrigin.Begin);
            fileStream3.Write(BitConverter.GetBytes(pDestSize), 0, 4);
            uint num2 = !flag ? this._getCRC(numArray1, length) : this._getCRC(numArray2, pDestSize);
            fileStream3.Seek(14L, SeekOrigin.Current);
            fileStream3.Write(BitConverter.GetBytes(num2), 0, 4);
            fileStream3.Seek(-6L, SeekOrigin.Current);
            byte[] buffer5 = new byte[1];
            this.Assert(fileStream3.Read(buffer5, 0, 1), 1);
            fileStream3.Seek(-1L, SeekOrigin.Current);
            fileStream3.Write(buffer2, 0, 1);
            if (index >= 0)
            {
              fileStream3.Seek((long) (position1 + 8 + 42 * index), SeekOrigin.Begin);
              fileStream3.Write(BitConverter.GetBytes(position3), 0, 4);
              fileStream3.Seek((long) position4, SeekOrigin.Begin);
            }
            int position5 = (int) fileStream3.Position;
            int num3 = 2048 - position5 % 2048;
            if (num3 == 0)
              num3 = 2048;
            int num4 = position5 + num3;
            fileStream3.Seek((long) num4, SeekOrigin.Begin);
            continue;
          case 0:
            flag = true;
            goto case -2116149247;
          default:
            throw new Exception("Cannot compress file. Aborting");
        }
      }
      fileStream3.Close();
      this.Result = !this.StopProcessing.WaitOne(1) ? string.Format("{0} file(s) added to {1}", (object) fileList.Count, (object) this._files.OutputArchive) : "Aborted by user";
      if (this.RebuildEnded == null)
        return;
      this.RebuildEnded((object) null, EventArgs.Empty);
    }

    private List<BFF.StaticData> getStaticData(List<string> files)
    {
      byte[] buffer = new byte[4];
      FileStream fileStream = new FileStream(this._files.OriginalArchive, FileMode.Open);
      List<BFF.StaticData> staticDataList = new List<BFF.StaticData>();
      for (int index = 0; index < files.Count; ++index)
      {
        FileInfo fileInfo = new FileInfo(string.Format("{0}\\{1}", (object) this._files.SourceFolder, (object) files[index]));
        BFF.StaticData staticData = new BFF.StaticData();
        staticData.fileSize = (int) fileInfo.Length;
        fileStream.Seek(304L + (long) (index * 42), SeekOrigin.Begin);
        fileStream.Read(buffer, 0, 4);
        staticData.dummy1 = BitConverter.ToUInt32(buffer, 0);
        fileStream.Read(buffer, 0, 4);
        staticData.dummy2 = BitConverter.ToUInt32(buffer, 0);
        fileStream.Read(buffer, 0, 4);
        fileStream.Read(buffer, 0, 4);
        staticData.everzero = BitConverter.ToInt32(buffer, 0);
        fileStream.Read(buffer, 0, 4);
        staticData.compressedSize = BitConverter.ToInt32(buffer, 0);
        fileStream.Read(buffer, 0, 4);
        staticData.fileSize = BitConverter.ToInt32(buffer, 0);
        fileStream.Read(buffer, 0, 4);
        staticData.everzero2 = BitConverter.ToInt32(buffer, 0);
        fileStream.Read(buffer, 0, 4);
        staticData.everzero3 = BitConverter.ToInt32(buffer, 0);
        staticDataList.Add(staticData);
      }
      fileStream.Close();
      return staticDataList;
    }

    public List<string> GetFileList(string archive) => this.getFileList(archive);

    private List<string> getFileList() => this.getFileList(this._files.OriginalArchive);

    private List<string> getFileList(string archive)
    {
      List<string> stringList = new List<string>();
      int num1 = 1080;
      byte[] buffer1 = new byte[4];
      FileStream fileStream = new FileStream(archive, FileMode.Open, FileAccess.Read);
      fileStream.Seek(280L, SeekOrigin.Begin);
      fileStream.Read(buffer1, 0, 4);
      int int32_1 = BitConverter.ToInt32(buffer1, 0);
      int num2 = num1 + int32_1;
      byte[] buffer2 = new byte[1];
      for (int index = 0; index < int32_1 / 42; ++index)
      {
        fileStream.Seek((long) (num2 + index * 16), SeekOrigin.Begin);
        fileStream.Read(buffer1, 0, 4);
        int int32_2 = BitConverter.ToInt32(buffer1, 0);
        fileStream.Seek((long) int32_2, SeekOrigin.Begin);
        fileStream.Read(buffer2, 0, 1);
        int count = (int) buffer2[0];
        byte[] numArray = new byte[count];
        fileStream.Read(numArray, 0, count);
        stringList.Add(Encoding.Default.GetString(numArray, 0, count));
      }
      fileStream.Close();
      return stringList;
    }

    private int[] getStaticDataFromOriginalArchive()
    {
      int[] numArray = new int[3];
      FileStream fileStream = new FileStream(this._files.OriginalArchive, FileMode.Open);
      byte[] buffer = new byte[4];
      fileStream.Seek(12L, SeekOrigin.Begin);
      this.Assert(fileStream.Read(buffer, 0, 4), 4);
      numArray[0] = BitConverter.ToInt32(buffer, 0);
      fileStream.Seek(292L, SeekOrigin.Begin);
      this.Assert(fileStream.Read(buffer, 0, 4), 4);
      numArray[1] = BitConverter.ToInt32(buffer, 0);
      this.Assert(fileStream.Read(buffer, 0, 4), 4);
      numArray[2] = BitConverter.ToInt32(buffer, 0);
      fileStream.Close();
      return numArray;
    }

    private uint _getCRC(byte[] b, int size)
    {
      uint num = uint.MaxValue;
      for (int index = 0; index < size; ++index)
        num = _crctable[(uint) (((int) b[index] ^ (int) num) & (int) byte.MaxValue)] ^ num >> 8;
      return num;
    }

    private void _buildCRCTable()
    {
      this._crctable = new uint[256];
      for (int index1 = 0; index1 < 256; ++index1)
      {
        uint num = (uint) index1;
        for (int index2 = 8; index2 > 0; --index2)
        {
          if (((int) num & 1) != 0)
            num = num >> 1 ^ 3988292384U;
          else
            num >>= 1;
        }
        this._crctable[index1] = num;
      }
    }

    public byte[] XCompress(byte[] src, int srcLen, int usize, out int size)
    {
      IntPtr pContext = IntPtr.Zero;
      BFF.My_XMemCreateCompressionContext(1, IntPtr.Zero, (ushort) 0, out pContext);
      int pDestSize = usize * 2;
      byte[] pDestBuffer = new byte[pDestSize];
      if (BFF.My_XMemCompress(pContext, pDestBuffer, out pDestSize, src, srcLen) != 0)
        throw new Exception("Can't decompress. Aborting processing.");
      size = pDestSize;
      return pDestBuffer;
    }

    public byte[] XDecompress(byte[] src, int srcLen, int usize)
    {
      IntPtr pContext = IntPtr.Zero;
      BFF.My_XMemCreateDecompressionContext(1, IntPtr.Zero, (ushort) 0, out pContext);
      int pDestSize = usize;
      byte[] pDestBuffer = new byte[pDestSize];
      if (BFF.My_XMemDecompress(pContext, pDestBuffer, out pDestSize, src, srcLen) != 0)
        throw new Exception("Can't decompress. Aborting processing.");
      BFF.My_XMemDestroyDecompressionContext(pContext);
      return pDestBuffer;
    }

    public void Dispose()
    {
    }

    internal class InputInfo
    {
      public string SourceFolder { get; set; }

      public string OriginalArchive { get; set; }

      public string OutputArchive { get; set; }

      public InputInfo(string sourceFolder, string originalArchive, string outputArchive)
      {
        this.SourceFolder = sourceFolder;
        this.OriginalArchive = originalArchive;
        this.OutputArchive = outputArchive;
      }
    }

    internal class StaticData
    {
      public uint dummy1;
      public uint dummy2;
      public int everzero;
      public int everzero2;
      public int everzero3;
      public int compressedSize;
      public int fileSize;
    }
  }
}
