// Decompiled with JetBrains decompiler
// Type: BFF.RedistFile
// Assembly: BFF, Version=1.0.22.114, Culture=neutral, PublicKeyToken=null
// MVID: 19420469-CC8C-4F6A-85E1-92AAE51BB3F4
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BFF.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BFF
{
  public class RedistFile
  {
    public Dictionary<string, List<string>> Files;
    public List<string> AllFiles;

    public RedistFile()
    {
      this.Files = new Dictionary<string, List<string>>();
      this.AllFiles = new List<string>();
    }

    private void Assert(int n, int expected)
    {
      if (n != expected)
        throw new Exception(string.Format("Can't read {0} bytes. Reader returned {1}", (object) expected, (object) n));
    }

    public void AddFile(string archive, string file)
    {
      if (this.Files.ContainsKey(archive))
      {
        List<string> file1 = this.Files[archive];
        if (!file1.Contains(file))
          file1.Add(file);
      }
      else
        this.Files.Add(archive, new List<string>() { file });
      if (this.AllFiles.Contains(file))
        return;
      this.AllFiles.Add(file);
    }

    public void InstallRedist(
      string gamepath,
      string source,
      RedistFile.UpdateDelegate upd,
      bool uninstall)
    {
      this.InstallRedist(gamepath, source, upd, uninstall, (List<string>) null);
    }

    public void InstallRedist(
      string gamepath,
      string source,
      RedistFile.UpdateDelegate upd,
      bool uninstall,
      List<string> ignoreList)
    {
      StreamReader streamReader = new StreamReader(source, Encoding.Default);
      string path1 = !(streamReader.ReadLine() != "BFI") ? streamReader.ReadLine() : throw new Exception("Not a valid install script");
      string str1 = string.Format("{0}\\PakFiles\\{1}", (object) gamepath, (object) path1);
      string str2 = string.Format("{0}\\PakFiles\\{1}_Uninstall.bfi", (object) gamepath, (object) path1.Substring(0, path1.LastIndexOf(".") - 4));
      if (File.Exists(str1) && File.Exists(str2))
      {
        if (!uninstall)
          throw new PackageAlreadyInstalledException();
        this.UnInstall(gamepath, str2, str1, upd);
      }
      else
      {
        if (upd != null)
          upd("Preparing install. Please wait...");
        else
          Console.WriteLine("Preparing install. Please wait...");
        FileStream fileStream1 = new FileStream(path1, FileMode.Open, FileAccess.Read);
        Dictionary<string, int> dictionary1 = new Dictionary<string, int>();
        Dictionary<string, RedistFileEntry> dictionary2 = new Dictionary<string, RedistFileEntry>();
        byte[] numArray1 = new byte[4];
        this.Assert(fileStream1.Read(numArray1, 0, 4), 4);
        if (Encoding.Default.GetString(numArray1) == "BFFZ")
        {
          fileStream1.Seek(8L, SeekOrigin.Begin);
          this.Assert(fileStream1.Read(numArray1, 0, 4), 4);
          int int32_1 = BitConverter.ToInt32(numArray1, 0);
          this.Assert(fileStream1.Read(numArray1, 0, 4), 4);
          int int32_2 = BitConverter.ToInt32(numArray1, 0);
          byte[] numArray2 = new byte[256];
          fileStream1.Seek(16L, SeekOrigin.Begin);
          for (int index = 0; index < int32_2; ++index)
          {
            this.Assert(fileStream1.Read(numArray2, 0, 256), 256);
            string str3 = Encoding.Default.GetString(numArray2);
            this.Assert(fileStream1.Read(numArray1, 0, 4), 4);
            int int32_3 = BitConverter.ToInt32(numArray1, 0);
            this.Assert(fileStream1.Read(numArray1, 0, 4), 4);
            int int32_4 = BitConverter.ToInt32(numArray1, 0);
            dictionary1.Add(str3.Replace(char.MinValue, '?').Replace("?", ""), int32_3);
            fileStream1.Seek((long) int32_4, SeekOrigin.Begin);
          }
          fileStream1.Seek((long) int32_1, SeekOrigin.Begin);
          this.Assert(fileStream1.Read(numArray1, 0, 4), 4);
          int result = BitConverter.ToInt32(numArray1, 0);
          List<string> stringList1 = new List<string>();
          for (int i = 0; i < result; ++i)
          {
            this.Assert(fileStream1.Read(numArray1, 0, 4), 4);
            int int32_3 = BitConverter.ToInt32(numArray1, 0);
            this.Assert(fileStream1.Read(numArray1, 0, 4), 4);
            int int32_4 = BitConverter.ToInt32(numArray1, 0);
            this.Assert(fileStream1.Read(numArray1, 0, 4), 4);
            int int32_5 = BitConverter.ToInt32(numArray1, 0);
            byte[] buffer = new byte[1];
            this.Assert(fileStream1.Read(buffer, 0, 1), 1);
            int length = (int) buffer[0];
            byte[] numArray3 = new byte[length];
            this.Assert(fileStream1.Read(numArray3, 0, length), length);
            int position = (int) fileStream1.Position;
            RedistFileEntry redistFileEntry = new RedistFileEntry(i, Encoding.Default.GetString(numArray3), int32_4, int32_5, position);
            dictionary2.Add(redistFileEntry.Name, redistFileEntry);
            stringList1.Add(redistFileEntry.Name);
            fileStream1.Seek((long) int32_3, SeekOrigin.Begin);
          }
          byte[] buffer1 = new byte[4];
          RedistHeader redistHeader = new RedistHeader(str1.Substring(0, str1.LastIndexOf("\\")));
          File.Copy(new FileInfo(source).FullName, str2);
          FileStream fileStream2 = new FileStream(str1, FileMode.Create, FileAccess.ReadWrite);
          fileStream2.Write(redistHeader.Magic, 0, 4);
          int position1 = (int) fileStream2.Position;
          fileStream2.Write(BitConverter.GetBytes(redistHeader.FileSize), 0, 4);
          int position2 = (int) fileStream2.Position;
          fileStream2.Write(BitConverter.GetBytes(0), 0, 4);
          fileStream2.Write(BitConverter.GetBytes(int32_2), 0, 4);
          fileStream2.Seek(16L, SeekOrigin.Begin);
          foreach (string key in dictionary1.Keys)
          {
            int num1 = dictionary1[key];
            for (int index = 0; index < numArray2.Length; ++index)
              numArray2[index] = (byte) 0;
            Encoding.Default.GetBytes(key).CopyTo((Array) numArray2, 0);
            fileStream2.Write(numArray2, 0, numArray2.Length);
            fileStream2.Write(BitConverter.GetBytes(num1), 0, 4);
            int position3 = (int) fileStream2.Position;
            fileStream2.Write(buffer1, 0, 4);
            foreach (string str3 in stringList1)
            {
              str3.Replace("\\-", "\\").Replace("\\+", "\\");
              fileStream2.Write(BitConverter.GetBytes(stringList1.IndexOf(str3)), 0, 4);
            }
            int position4 = (int) fileStream2.Position;
            int num2 = position4 + (16 - position4 % 16);
            fileStream2.Seek((long) position3, SeekOrigin.Begin);
            fileStream2.Write(BitConverter.GetBytes(num2), 0, 4);
            fileStream2.Seek((long) num2, SeekOrigin.Begin);
          }
          int position5 = (int) fileStream2.Position;
          int num3 = position5 + (16 - position5 % 16);
          fileStream2.Seek((long) position2, SeekOrigin.Begin);
          fileStream2.Write(BitConverter.GetBytes(num3), 0, 4);
          fileStream2.Seek((long) num3, SeekOrigin.Begin);
          int num4 = num3;
          fileStream2.Write(buffer1, 0, 4);
          Dictionary<string, RedistFileEntry> dictionary3 = new Dictionary<string, RedistFileEntry>();
          List<string> stringList2 = new List<string>();
          BFF bff = new BFF();
label_30:
          string str4 = streamReader.ReadLine();
          if (str4 != null)
          {
            string[] strArray = str4.Split('=');
            string str3 = strArray[0];
            if (!int.TryParse(strArray[1], out result))
              throw new Exception("BFI script is corrupt. Not valid number of files");
            string str5 = (gamepath + "\\" + str3).Replace("\\\\", "\\");
            if (!File.Exists(str5))
            {
              for (int index = 0; index < result; ++index)
                streamReader.ReadLine();
              goto label_30;
            }
            else
            {
              string str6 = str5 + ".bkp";
              if (!File.Exists(str6))
              {
                if (!stringList2.Contains(str6))
                  stringList2.Add(str6);
                File.Copy(str5, str6);
              }
              string msg = string.Format("Updating {0} file(s) in {1}...", (object) result, (object) str3);
              if (upd != null)
                upd(msg);
              else
                Console.WriteLine(msg);
              List<string> fileList = bff.GetFileList(str5);
              FileStream fs = new FileStream(str5, FileMode.Open);
              try
              {
                for (int index = 0; index < result; ++index)
                {
                  string str7 = streamReader.ReadLine();
                  string str8 = str7.Replace("\\-", "\\").Replace("\\+", "\\");
                  if (dictionary2.ContainsKey(str7))
                  {
                    RedistFileEntry redistFileEntry1 = dictionary2[str7];
                    if (!dictionary3.ContainsKey(str8))
                    {
                      int num1 = 304 + 42 * fileList.IndexOf(str8);
                      fs.Seek((long) (num1 + 8), SeekOrigin.Begin);
                      fs.Read(numArray1, 0, 4);
                      int int32_3 = BitConverter.ToInt32(numArray1, 0);
                      fs.Read(numArray1, 0, 4);
                      fs.Read(numArray1, 0, 4);
                      int int32_4 = BitConverter.ToInt32(numArray1, 0);
                      fs.Read(numArray1, 0, 4);
                      int int32_5 = BitConverter.ToInt32(numArray1, 0);
                      RedistFileEntry redistFileEntry2 = new RedistFileEntry(redistFileEntry1.Index, redistFileEntry1.Name, int32_4, int32_5, int32_3);
                      dictionary3.Add(str7, redistFileEntry2);
                      byte[] buffer2 = new byte[int32_4];
                      fs.Seek((long) int32_3, SeekOrigin.Begin);
                      fs.Read(buffer2, 0, int32_4);
                      int position3 = (int) fileStream2.Position;
                      fileStream2.Write(buffer1, 0, 4);
                      fileStream2.Write(BitConverter.GetBytes(int32_4), 0, 4);
                      fileStream2.Write(BitConverter.GetBytes(int32_5), 0, 4);
                      byte[] buffer3 = new byte[1]
                      {
                        (byte) str8.Length
                      };
                      fileStream2.Write(buffer3, 0, 1);
                      fileStream2.Write(Encoding.Default.GetBytes(str8), 0, str8.Length);
                      fileStream2.Write(buffer2, 0, int32_4);
                      int position4 = (int) fileStream2.Position;
                      int num2 = position4 + (16 - position4 % 16);
                      fileStream2.Seek((long) position3, SeekOrigin.Begin);
                      fileStream2.Write(BitConverter.GetBytes(num2), 0, 4);
                      fileStream2.Seek((long) num2, SeekOrigin.Begin);
                    }
                    if (ignoreList == null || ignoreList != null && !ignoreList.Contains(str8))
                    {
                      byte[] numArray3 = new byte[redistFileEntry1.Size];
                      fileStream1.Seek((long) redistFileEntry1.Offset, SeekOrigin.Begin);
                      this.Assert(fileStream1.Read(numArray3, 0, redistFileEntry1.Size), redistFileEntry1.Size);
                      if (!str3.StartsWith("PakFiles\\"))
                        str3 = "PakFiles\\" + str3;
                      bff.Inject(str5, str7, numArray3, redistFileEntry1.Size, redistFileEntry1.USize, fs, fileList, (RedistFile.UpdateDelegate) null);
                      if (upd != null && !bff.Result.StartsWith("OK"))
                        upd(bff.Result);
                    }
                    else if (upd != null)
                      upd(string.Format("Ignoring file {0}", (object) str8));
                  }
                }
                goto label_30;
              }
              finally
              {
                fs.Close();
              }
            }
          }
          else
          {
            int position3 = (int) fileStream2.Position;
            fileStream2.Seek((long) num4, SeekOrigin.Begin);
            fileStream2.Write(BitConverter.GetBytes(dictionary3.Count), 0, 4);
            fileStream2.Seek((long) position1, SeekOrigin.Begin);
            fileStream2.Write(BitConverter.GetBytes(position3), 0, 4);
            fileStream2.Close();
            foreach (string path2 in stringList2)
              File.Delete(path2);
          }
        }
        fileStream1.Close();
        streamReader.Close();
        if (upd != null)
          upd("Install finished");
        else
          Console.WriteLine("Install finished");
      }
    }

    public void UnInstall(
      string gamepath,
      string source,
      string unpath,
      RedistFile.UpdateDelegate upd)
    {
      StreamReader streamReader = new StreamReader(source, Encoding.Default);
      if (upd != null)
        upd("Starting MOD uninstall, please wait...");
      if (streamReader.ReadLine() != "BFI")
        throw new Exception("Not a valid install script");
      streamReader.ReadLine();
      FileStream fileStream = new FileStream(unpath, FileMode.Open, FileAccess.Read);
      Dictionary<string, int> dictionary1 = new Dictionary<string, int>();
      Dictionary<string, RedistFileEntry> dictionary2 = new Dictionary<string, RedistFileEntry>();
      byte[] numArray1 = new byte[4];
      this.Assert(fileStream.Read(numArray1, 0, 4), 4);
      if (Encoding.Default.GetString(numArray1) == "BFFZ")
      {
        fileStream.Seek(8L, SeekOrigin.Begin);
        this.Assert(fileStream.Read(numArray1, 0, 4), 4);
        int int32_1 = BitConverter.ToInt32(numArray1, 0);
        this.Assert(fileStream.Read(numArray1, 0, 4), 4);
        int int32_2 = BitConverter.ToInt32(numArray1, 0);
        fileStream.Seek(16L, SeekOrigin.Begin);
        for (int index = 0; index < int32_2; ++index)
        {
          byte[] numArray2 = new byte[256];
          this.Assert(fileStream.Read(numArray2, 0, 256), 256);
          string key = Encoding.Default.GetString(numArray2);
          this.Assert(fileStream.Read(numArray1, 0, 4), 4);
          int int32_3 = BitConverter.ToInt32(numArray1, 0);
          this.Assert(fileStream.Read(numArray1, 0, 4), 4);
          int int32_4 = BitConverter.ToInt32(numArray1, 0);
          dictionary1.Add(key, int32_3);
          fileStream.Seek((long) int32_4, SeekOrigin.Begin);
        }
        fileStream.Seek((long) int32_1, SeekOrigin.Begin);
        this.Assert(fileStream.Read(numArray1, 0, 4), 4);
        int result = BitConverter.ToInt32(numArray1, 0);
        for (int i = 0; i < result; ++i)
        {
          this.Assert(fileStream.Read(numArray1, 0, 4), 4);
          int int32_3 = BitConverter.ToInt32(numArray1, 0);
          this.Assert(fileStream.Read(numArray1, 0, 4), 4);
          int int32_4 = BitConverter.ToInt32(numArray1, 0);
          this.Assert(fileStream.Read(numArray1, 0, 4), 4);
          int int32_5 = BitConverter.ToInt32(numArray1, 0);
          byte[] buffer = new byte[1];
          this.Assert(fileStream.Read(buffer, 0, 1), 1);
          int length = (int) buffer[0];
          byte[] numArray2 = new byte[length];
          this.Assert(fileStream.Read(numArray2, 0, length), length);
          int position = (int) fileStream.Position;
          RedistFileEntry redistFileEntry = new RedistFileEntry(i, Encoding.Default.GetString(numArray2), int32_4, int32_5, position);
          dictionary2.Add(redistFileEntry.Name, redistFileEntry);
          fileStream.Seek((long) int32_3, SeekOrigin.Begin);
        }
        List<string> stringList = new List<string>();
        BFF bff = new BFF();
label_12:
        string str1 = streamReader.ReadLine();
        if (str1 != null)
        {
          string[] strArray = str1.Split('=');
          string str2 = strArray[0];
          if (!int.TryParse(strArray[1], out result))
            throw new Exception("BFI script is corrupt. Not valid number of files");
          string str3 = (gamepath + "\\" + str2).Replace("\\\\", "\\");
          if (!File.Exists(str3))
          {
            for (int index = 0; index < result; ++index)
              streamReader.ReadLine();
            goto label_12;
          }
          else
          {
            string path = str3 + ".bkp";
            if (!File.Exists(path))
            {
              if (!stringList.Contains(path))
                stringList.Add(path);
              File.Copy(str3, str3 + ".bkp");
            }
            string msg = string.Format("Restoring {0} file(s) in {1}...", (object) result, (object) str2);
            if (upd != null)
              upd(msg);
            else
              Console.WriteLine(msg);
            List<string> fileList = bff.GetFileList(str3);
            FileStream fs = new FileStream(str3, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            try
            {
              for (int index = 0; index < result; ++index)
              {
                string str4 = streamReader.ReadLine().Replace("\\-", "\\").Replace("\\+", "\\");
                if (dictionary2.ContainsKey(str4))
                {
                  RedistFileEntry redistFileEntry = dictionary2[str4];
                  byte[] numArray2 = new byte[redistFileEntry.Size];
                  fileStream.Seek((long) redistFileEntry.Offset, SeekOrigin.Begin);
                  this.Assert(fileStream.Read(numArray2, 0, redistFileEntry.Size), redistFileEntry.Size);
                  if (!str2.StartsWith("PakFiles\\"))
                    str2 = "PakFiles\\" + str2;
                  bff.Inject(str3, str4, numArray2, redistFileEntry.Size, redistFileEntry.USize, fs, fileList, (RedistFile.UpdateDelegate) null);
                  if (upd != null && !bff.Result.StartsWith("OK"))
                    upd(bff.Result);
                }
              }
              goto label_12;
            }
            catch (Exception ex)
            {
              if (upd != null)
                return;
              Console.WriteLine(ex.Message + "\r\nPress ENTER: ");
              Console.ReadLine();
              return;
            }
            finally
            {
              fs.Close();
            }
          }
        }
        else
        {
          foreach (string path in stringList)
            File.Delete(path);
        }
      }
      fileStream.Close();
      streamReader.Close();
      if (upd != null)
        upd("Deleting uninstall files");
      File.Delete(source);
      File.Delete(unpath);
      if (upd == null)
        return;
      upd("Uninstall finished");
    }

    public void CreateRedist(string sourceFolder, string output) => this.CreateRedist(sourceFolder, output, (RedistFile.UpdateDelegate) null);

    public void CreateRedist(string sourceFolder, string output, RedistFile.UpdateDelegate upd)
    {
      RedistHeader redistHeader = new RedistHeader(sourceFolder);
      if (upd != null)
        upd("Initializing redist file");
      FileStream fileStream1 = new FileStream(output, FileMode.Create);
      fileStream1.Write(redistHeader.Magic, 0, 4);
      int position1 = (int) fileStream1.Position;
      fileStream1.Write(BitConverter.GetBytes(redistHeader.FileSize), 0, 4);
      int position2 = (int) fileStream1.Position;
      fileStream1.Write(BitConverter.GetBytes(0), 0, 4);
      fileStream1.Write(BitConverter.GetBytes(this.Files.Keys.Count), 0, 4);
      fileStream1.Seek(16L, SeekOrigin.Begin);
      IntPtr pContext = IntPtr.Zero;
      try
      {
        if (Compress.My_XMemCreateCompressionContext(1, IntPtr.Zero, (ushort) 0, out pContext) != 0)
          throw new Exception("Cannot create compression context. Aborted");
      }
      catch (DllNotFoundException ex)
      {
        if (upd != null)
          upd(string.Format("DLL not found exception ({0})", (object) ex.Message));
        fileStream1.Close();
        return;
      }
      byte[] buffer1 = new byte[4];
      if (upd != null)
        upd("Creating install script");
      StreamWriter streamWriter = new StreamWriter(string.Format("{0}\\Install.bfi", (object) output.Substring(0, output.LastIndexOf("\\"))), false, Encoding.Default);
      streamWriter.WriteLine("BFI");
      streamWriter.WriteLine(output.Substring(output.LastIndexOf("\\") + 1));
      foreach (string key in this.Files.Keys)
      {
        List<string> file = this.Files[key];
        streamWriter.WriteLine(string.Format("{0}={1}", (object) key, (object) file.Count));
        RedistEntry redistEntry = new RedistEntry(key, file.Count);
        fileStream1.Write(redistEntry.ArchiveName, 0, redistEntry.ArchiveName.Length);
        fileStream1.Write(BitConverter.GetBytes(redistEntry.FilesInArchive), 0, 4);
        int position3 = (int) fileStream1.Position;
        fileStream1.Write(buffer1, 0, 4);
        foreach (string str1 in file)
        {
          string str2 = str1.Substring(sourceFolder.Length + 1);
          streamWriter.WriteLine(str2);
          fileStream1.Write(BitConverter.GetBytes(this.AllFiles.IndexOf(str1)), 0, 4);
        }
        int position4 = (int) fileStream1.Position;
        int num = position4 + (16 - position4 % 16);
        fileStream1.Seek((long) position3, SeekOrigin.Begin);
        fileStream1.Write(BitConverter.GetBytes(num), 0, 4);
        fileStream1.Seek((long) num, SeekOrigin.Begin);
      }
      streamWriter.Close();
      int position5 = (int) fileStream1.Position;
      int num1 = position5 + (16 - position5 % 16);
      fileStream1.Seek((long) position2, SeekOrigin.Begin);
      fileStream1.Write(BitConverter.GetBytes(num1), 0, 4);
      fileStream1.Seek((long) num1, SeekOrigin.Begin);
      fileStream1.Write(BitConverter.GetBytes(this.AllFiles.Count), 0, 4);
      foreach (string allFile in this.AllFiles)
      {
        string s = allFile.Substring(sourceFolder.Length + 1);
        int position3 = (int) fileStream1.Position;
        fileStream1.Write(buffer1, 0, 4);
        int position4 = (int) fileStream1.Position;
        fileStream1.Write(buffer1, 0, 4);
        int position6 = (int) fileStream1.Position;
        fileStream1.Write(buffer1, 0, 4);
        byte[] buffer2 = new byte[1]{ (byte) s.Length };
        fileStream1.Write(buffer2, 0, 1);
        fileStream1.Write(Encoding.Default.GetBytes(s), 0, s.Length);
        int length;
        int pDestSize;
        if (allFile.IndexOf("\\-") == -1)
        {
          FileStream fileStream2 = new FileStream(allFile, FileMode.Open);
          length = (int) fileStream2.Length;
          byte[] numArray1 = new byte[length];
          this.Assert(fileStream2.Read(numArray1, 0, length), length);
          fileStream2.Close();
          pDestSize = length * 3;
          byte[] numArray2 = new byte[pDestSize];
          int num2 = Compress.My_XMemCompress(pContext, numArray2, out pDestSize, numArray1, length);
          switch (num2)
          {
            case -2116149247:
              fileStream1.Write(numArray1, 0, length);
              pDestSize = length;
              break;
            case 0:
              if (pDestSize > length)
              {
                fileStream1.Write(numArray1, 0, length);
                pDestSize = length;
                break;
              }
              fileStream1.Write(numArray2, 0, pDestSize);
              break;
            default:
              throw new Exception(string.Format("Cannot compress file. Aborted\r\nHRESULT = {0}", (object) num2.ToString("X8")));
          }
        }
        else
        {
          length = 0;
          pDestSize = 0;
        }
        int position7 = (int) fileStream1.Position;
        int num3 = position7 + (16 - position7 % 16);
        fileStream1.Seek((long) position3, SeekOrigin.Begin);
        fileStream1.Write(BitConverter.GetBytes(num3), 0, 4);
        fileStream1.Seek((long) position4, SeekOrigin.Begin);
        fileStream1.Write(BitConverter.GetBytes(pDestSize), 0, 4);
        fileStream1.Seek((long) position6, SeekOrigin.Begin);
        fileStream1.Write(BitConverter.GetBytes(length), 0, 4);
        fileStream1.Seek((long) num3, SeekOrigin.Begin);
      }
      int position8 = (int) fileStream1.Position;
      fileStream1.Seek((long) position1, SeekOrigin.Begin);
      fileStream1.Write(BitConverter.GetBytes(position8), 0, 4);
      fileStream1.Close();
      if (upd == null)
        return;
      upd("Finished");
    }

    public void Inject(string gamepath, RedistFile.UpdateDelegate upd)
    {
      BFF bff = new BFF();
      foreach (string key in this.Files.Keys)
      {
        string str = (gamepath + "\\" + key).Replace("\\\\", "\\");
        List<string> fileList = bff.GetFileList(str);
        FileStream fs = new FileStream(str, FileMode.Open);
        try
        {
          foreach (string filename in this.Files[key])
          {
            if (filename.IndexOf("|") != -1)
              this._inject(str, fs, fileList, filename.Split('|'), upd);
            else
              this._inject(str, fs, fileList, filename, upd);
          }
        }
        catch (Exception ex)
        {
          throw ex;
        }
        finally
        {
          fs.Close();
        }
      }
    }

    private void _inject(
      string archivePath,
      FileStream fs,
      List<string> fs_list,
      string[] fileinfo,
      RedistFile.UpdateDelegate upd)
    {
      FileInfo fi = new FileInfo(fileinfo[0]);
      if (!fi.Exists)
        return;
      this._inject(archivePath, fs, fs_list, fi, fileinfo[1], upd);
    }

    private void _inject(
      string archivePath,
      FileStream fs_archive,
      List<string> fs_list,
      FileInfo fi,
      string filename,
      RedistFile.UpdateDelegate upd)
    {
      IntPtr pContext = IntPtr.Zero;
      if (Compress.My_XMemCreateCompressionContext(1, IntPtr.Zero, (ushort) 0, out pContext) != 0)
        throw new Exception("Cannot create compression context. Aborted");
      FileStream fileStream = fi.OpenRead();
      int length = (int) fileStream.Length;
      byte[] numArray1 = new byte[length];
      fileStream.Read(numArray1, 0, length);
      fileStream.Close();
      int pDestSize = length * 3;
      byte[] numArray2 = new byte[pDestSize];
      if (Compress.My_XMemCompress(pContext, numArray2, out pDestSize, numArray1, length) != 0)
        throw new Exception("Cannot compress data.");
      BFF bff = new BFF();
      if (pDestSize > length)
        bff.Inject(archivePath, filename, numArray1, length, length, fs_archive, fs_list, upd);
      else
        bff.Inject(archivePath, filename, numArray2, pDestSize, length, fs_archive, fs_list, upd);
    }

    private void _inject(
      string archivePath,
      FileStream fs,
      List<string> fs_list,
      string filename,
      RedistFile.UpdateDelegate upd)
    {
      FileInfo fi = new FileInfo(filename);
      if (!fi.Exists)
        return;
      this._inject(archivePath, fs, fs_list, fi, filename, upd);
    }

    public delegate void UpdateDelegate(string msg);
  }
}
