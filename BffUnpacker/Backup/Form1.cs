// Decompiled with JetBrains decompiler
// Type: BffUnpacker.Form1
// Assembly: BffUnpacker, Version=1.0.11.2051, Culture=neutral, PublicKeyToken=null
// MVID: 6905B5B6-9334-40C1-AEBB-A5EB06804AFD
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BffUnpacker.exe

using BFF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace BffUnpacker
{
  public class Form1 : Form
  {
    private Prefs prefs;
    private List<string> files;
    private List<Form1.FileItem> _files;
    private string _currentArchive;
    private bool processing;
    private Form_Processing frmProcessing;
    private string unpackerConfigPath;
    private ManualResetEvent processed;
    private IContainer components;
    private Label label1;
    private TextBox text_file;
    private Button button_browse;
    private ListView listView1;
    private ColumnHeader c1;
    private ColumnHeader c2;
    private ColumnHeader c4;
    private ColumnHeader c5;
    private ColumnHeader c6;
    private OpenFileDialog openFileDialog1;
    private ContextMenuStrip contextMenuStrip1;
    private ToolStripMenuItem toolDelete;
    private Label label2;
    private TextBox text_filter;
    private Button button_filter;
    private Button button_close;
    private ColumnHeader c7;
    private ToolStripMenuItem extractToolStripMenuItem;
    private FolderBrowserDialog folderBrowserDialog1;
    private Label label_files;
    private ToolStripMenuItem extractallToolStripMenuItem;
    private ToolStripSeparator toolStripMenuItem1;
    private CheckBox check_uncompressed_only;

    public string CurrentArchive
    {
      get => this._currentArchive;
      set => this._currentArchive = value;
    }

    public Form1()
    {
      this.InitializeComponent();
      Version version = Assembly.GetExecutingAssembly().GetName().Version;
      this.Text = string.Format(this.Text, (object) version.Major, (object) version.Minor, (object) version.Revision);
      this.prefs = new Prefs();
      this.unpackerConfigPath = string.Format("{0}\\BFF_Unpacker.config", (object) Environment.GetFolderPath(Environment.SpecialFolder.Personal));
      this.loadPrefs();
      this.processed = new ManualResetEvent(false);
    }

    private void loadPrefs()
    {
      try
      {
        this.prefs.LoadPrefs(this.unpackerConfigPath);
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show((IWin32Window) this, ex.Message, "Error", MessageBoxButtons.OK);
      }
      if (this.prefs.SourceFolder != "")
        this.openFileDialog1.InitialDirectory = this.prefs.SourceFolder;
      if (!(this.prefs.DestinationFile != ""))
        return;
      this.folderBrowserDialog1.SelectedPath = this.prefs.DestinationFile;
    }

    private void button_browse_Click(object sender, EventArgs e)
    {
      if (this.processing)
      {
        this.frmProcessing.Stop.Set();
      }
      else
      {
        if (DialogResult.OK != this.openFileDialog1.ShowDialog((IWin32Window) this))
          return;
        this.text_file.Text = this.openFileDialog1.FileName;
        this.unpack(this.text_file.Text);
        this.showSelectedItems();
      }
    }

    private void unpack(string file)
    {
      this._currentArchive = file;
      BFF.BFF bff = new BFF.BFF();
      this._files = new List<Form1.FileItem>();
      this.files = bff.GetFileList(file);
      FileStream fileStream = new FileStream(this.text_file.Text, FileMode.Open);
      int num1 = 0;
      byte[] buffer1 = new byte[4];
      fileStream.Seek(280L, SeekOrigin.Begin);
      fileStream.Read(buffer1, 0, 4);
      int int32 = BitConverter.ToInt32(buffer1, 0);
      byte[] buffer2 = new byte[int32];
      fileStream.Seek(304L, SeekOrigin.Begin);
      fileStream.Read(buffer2, 0, int32);
      fileStream.Close();
      foreach (string file1 in this.files)
      {
        int num2 = 42 * num1 + 8;
        int startIndex1 = num2;
        int startIndex2 = num2 + 8;
        int startIndex3 = num2 + 12;
        int startIndex4 = startIndex2 + 18;
        int startIndex5 = startIndex3 + 12;
        this._files.Add(new Form1.FileItem()
        {
          Index = num1,
          Filename = file1,
          FileOffset = BitConverter.ToInt32(buffer2, startIndex1),
          FileOffset_Offset = startIndex1,
          CRC_Offset = startIndex4,
          CRC = BitConverter.ToInt32(buffer2, startIndex4),
          Size_Offset = startIndex2,
          Size = BitConverter.ToInt32(buffer2, startIndex2),
          USize_Offset = startIndex3,
          USize = BitConverter.ToInt32(buffer2, startIndex3),
          Compressed = BitConverter.ToChar(buffer2, startIndex5) == '\x0002'
        });
        ++num1;
      }
      this.showFiles("");
    }

    private void showFiles(string folder)
    {
      string[] items = new string[6];
      this.listView1.SuspendLayout();
      this.listView1.Items.Clear();
      foreach (string file in this.files)
      {
        int index = this.files.IndexOf(file);
        items[0] = index.ToString();
        items[1] = file;
        items[2] = this._files[index].Size.ToString();
        items[3] = this._files[index].USize.ToString();
        items[4] = this._files[index].FileOffset.ToString("X4").PadLeft(4, '0');
        items[5] = this._files[index].CRC.ToString("X8").PadLeft(8, '0');
        ListViewItem listViewItem = new ListViewItem(items);
        if (!this._files[index].Compressed)
          listViewItem.ForeColor = Color.Blue;
        this.listView1.Items.Add(listViewItem);
      }
      this.listView1.ResumeLayout();
    }

    private void toolDelete_Click(object sender, EventArgs e)
    {
      if (this.listView1.SelectedItems.Count <= 0 || DialogResult.Yes != MessageBox.Show((IWin32Window) this, this.listView1.SelectedItems.Count != 1 ? string.Format("This will affect {0} files and can't be undone. Are you sure?\r\n\r\nAlways make a backup of original archives!", (object) this.listView1.SelectedItems.Count) : "Delete file? This can't be undone\r\n\r\nAlways make a backup of original archives!", "Warning", MessageBoxButtons.YesNo))
        return;
      int num = 304;
      byte[] buffer = new byte[4];
      this.listView1.SuspendLayout();
      FileStream fileStream = new FileStream(this._currentArchive, FileMode.Open);
      foreach (ListViewItem selectedItem in this.listView1.SelectedItems)
      {
        int result;
        int.TryParse(selectedItem.SubItems[0].Text, out result);
        fileStream.Seek((long) (num + 42 * result + 16), SeekOrigin.Begin);
        fileStream.Read(buffer, 0, 4);
        fileStream.Seek(-4L, SeekOrigin.Current);
        fileStream.Write(BitConverter.GetBytes(0), 0, 4);
        fileStream.Write(BitConverter.GetBytes(0), 0, 4);
        fileStream.Seek(8L, SeekOrigin.Current);
        fileStream.Write(new byte[1], 0, 1);
        fileStream.Seek(1L, SeekOrigin.Current);
        fileStream.Write(new byte[4]
        {
          byte.MaxValue,
          byte.MaxValue,
          byte.MaxValue,
          byte.MaxValue
        }, 0, 4);
        selectedItem.SubItems[2].Text = "0";
        selectedItem.SubItems[3].Text = "0";
        selectedItem.SubItems[5].Text = "FFFFFFFF";
        selectedItem.ForeColor = Color.Blue;
        this._files[result].Compressed = false;
      }
      fileStream.Close();
      this.listView1.ResumeLayout();
    }

    private void button_close_Click(object sender, EventArgs e) => this.Close();

    private void button_filter_Click(object sender, EventArgs e)
    {
      string newValue = "[0-9a-z_!@$%,;\\-\\.\\\\s]+.";
      Regex regex = new Regex(new Regex("\\[|\\]\\{|\\}|&").Replace(this.text_filter.Text, "").Replace(".", "\\.").Replace("*", newValue) + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
      foreach (ListViewItem listViewItem in this.listView1.Items)
        listViewItem.Selected = regex.IsMatch(listViewItem.SubItems[1].Text);
      this.listView1.Focus();
      if (this.listView1.SelectedItems.Count > 0)
        this.listView1.SelectedItems[0].EnsureVisible();
      this.showSelectedItems();
    }

    private void showSelectedItems() => this.label_files.Text = string.Format("{0} file(s) selected of {1}", (object) this.listView1.SelectedItems.Count, (object) this.listView1.Items.Count);

    private void extractToolStripMenuItem_Click(object sender, EventArgs e) => this.extractFile(false);

    private void extractFile(bool all)
    {
      if (DialogResult.OK != this.folderBrowserDialog1.ShowDialog((IWin32Window) this))
        return;
      string selectedPath = this.folderBrowserDialog1.SelectedPath;
      if (all || this.listView1.SelectedItems.Count > 1)
      {
        List<Form1.FileItem> fileItemList = new List<Form1.FileItem>();
        List<ListViewItem> listViewItemList = new List<ListViewItem>();
        if (all)
        {
          foreach (ListViewItem listViewItem in this.listView1.Items)
            listViewItemList.Add(listViewItem);
        }
        else
        {
          foreach (ListViewItem selectedItem in this.listView1.SelectedItems)
            listViewItemList.Add(selectedItem);
        }
        foreach (ListViewItem listViewItem in listViewItemList)
        {
          int result;
          int.TryParse(listViewItem.SubItems[0].Text, out result);
          fileItemList.Add(this._files[result]);
        }
        this.processed.Reset();
        this.processing = true;
        this.frmProcessing = new Form_Processing();
        this.frmProcessing.SetMax(listViewItemList.Count);
        ThreadPool.QueueUserWorkItem(new WaitCallback(this.startProcessing), (object) new Form1.ThreadData()
        {
          List = fileItemList,
          OutputFolder = selectedPath
        });
        DialogResult dialogResult = this.frmProcessing.ShowDialog((IWin32Window) this);
        this.processed.WaitOne();
        if (dialogResult == DialogResult.Cancel)
        {
          this.frmProcessing.Close();
          int num = (int) MessageBox.Show("Stopped by user", "Warning");
        }
        else
        {
          this.frmProcessing.Close();
          if (DialogResult.Yes == MessageBox.Show(string.Format("{0} file(s) extracted to {1}\r\n\r\nView extracted files now?", (object) listViewItemList.Count, (object) selectedPath), "Success", MessageBoxButtons.YesNo))
            Process.Start("explorer.exe", selectedPath);
        }
        this.frmProcessing.Dispose();
      }
      else
      {
        int result;
        int.TryParse(this.listView1.SelectedItems[0].SubItems[0].Text, out result);
        this.extractFile(result, selectedPath);
      }
      this.processing = false;
    }

    private void startProcessing(object state)
    {
      Form1.ThreadData threadData = state as Form1.ThreadData;
      string outputFolder = threadData.OutputFolder;
      foreach (Form1.FileItem fileItem in threadData.List)
      {
        if (this.frmProcessing.Stop.WaitOne(1, true))
        {
          this.processed.Set();
          return;
        }
        this.extractFile(fileItem.Index, outputFolder);
        if (this.frmProcessing.Stop.WaitOne(1, true))
        {
          this.processed.Set();
          return;
        }
        this.frmProcessing.UpdateProgress(fileItem.Filename, 1);
      }
      this.frmProcessing.Finished();
      this.processed.Set();
    }

    private void extractallToolStripMenuItem_Click(object sender, EventArgs e) => this.extractFile(true);

    private void extractFile(int index, string outputFolder)
    {
      string file = this.files[index];
      string path1 = outputFolder + "\\" + file;
      int fileOffset = this._files[index].FileOffset;
      int size = this._files[index].Size;
      int usize = this._files[index].USize;
      string[] strArray = file.Split('\\');
      string path2 = outputFolder;
      for (int index1 = 0; index1 < strArray.Length - 1; ++index1)
      {
        path2 = path2 + "\\" + strArray[index1];
        if (!Directory.Exists(path2))
          Directory.CreateDirectory(path2);
      }
      if (this._files[index].Compressed)
      {
        byte[] numArray = new byte[size];
        FileStream fileStream1 = new FileStream(this._currentArchive, FileMode.Open);
        fileStream1.Seek((long) fileOffset, SeekOrigin.Begin);
        fileStream1.Read(numArray, 0, size);
        fileStream1.Close();
        byte[] buffer = new BFF.BFF().XDecompress(numArray, size, usize);
        FileStream fileStream2 = new FileStream(path1, FileMode.Create);
        fileStream2.Write(buffer, 0, usize);
        fileStream2.Close();
      }
      else
      {
        byte[] buffer = new byte[usize];
        FileStream fileStream1 = new FileStream(this._currentArchive, FileMode.Open);
        fileStream1.Seek((long) fileOffset, SeekOrigin.Begin);
        fileStream1.Read(buffer, 0, usize);
        fileStream1.Close();
        FileStream fileStream2 = new FileStream(path1, FileMode.Create);
        fileStream2.Write(buffer, 0, usize);
        fileStream2.Close();
      }
    }

    private void listView1_ItemSelectionChanged(
      object sender,
      ListViewItemSelectionChangedEventArgs e)
    {
      this.showSelectedItems();
    }

    private void check_uncompressed_only_CheckedChanged(object sender, EventArgs e)
    {
      if (this.check_uncompressed_only.Checked)
      {
        List<ListViewItem> listViewItemList = new List<ListViewItem>();
        for (int index = 0; index < this.listView1.Items.Count; ++index)
        {
          if (!this._files[index].Compressed)
            listViewItemList.Add(this.listView1.Items[index]);
        }
        this.listView1.SuspendLayout();
        this.listView1.Items.Clear();
        foreach (ListViewItem listViewItem in listViewItemList)
          this.listView1.Items.Add(listViewItem);
        this.listView1.ResumeLayout();
        this.showSelectedItems();
      }
      else
      {
        this.unpack(this._currentArchive);
        this.showSelectedItems();
      }
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.AppendChild((XmlNode) xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"));
      XmlNode xmlNode = xmlDocument.AppendChild((XmlNode) xmlDocument.CreateElement("config"));
      XmlNode element1 = (XmlNode) xmlDocument.CreateElement("sourceFolder");
      XmlAttribute attribute1 = xmlDocument.CreateAttribute("value");
      attribute1.Value = this.text_file.Text;
      if (attribute1.Value.IndexOf("\\") != -1)
        attribute1.Value = attribute1.Value.Substring(0, attribute1.Value.LastIndexOf("\\"));
      element1.Attributes.Append(attribute1);
      xmlNode.AppendChild(element1);
      XmlNode element2 = (XmlNode) xmlDocument.CreateElement("originalFile");
      XmlAttribute attribute2 = xmlDocument.CreateAttribute("value");
      attribute2.Value = "";
      element2.Attributes.Append(attribute2);
      xmlNode.AppendChild(element2);
      XmlNode element3 = (XmlNode) xmlDocument.CreateElement("destinationFile");
      XmlAttribute attribute3 = xmlDocument.CreateAttribute("value");
      attribute3.Value = this.folderBrowserDialog1.SelectedPath;
      element3.Attributes.Append(attribute3);
      xmlNode.AppendChild(element3);
      xmlDocument.Save(this.unpackerConfigPath);
    }

    private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
    {
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      if (this._currentArchive == null || !(this._currentArchive != string.Empty) || !File.Exists(this._currentArchive))
        return;
      this.text_file.Text = this._currentArchive;
      this.unpack(this.text_file.Text);
      this.showSelectedItems();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new Container();
      this.label1 = new Label();
      this.text_file = new TextBox();
      this.button_browse = new Button();
      this.listView1 = new ListView();
      this.c1 = new ColumnHeader();
      this.c2 = new ColumnHeader();
      this.c4 = new ColumnHeader();
      this.c5 = new ColumnHeader();
      this.c6 = new ColumnHeader();
      this.c7 = new ColumnHeader();
      this.contextMenuStrip1 = new ContextMenuStrip(this.components);
      this.extractToolStripMenuItem = new ToolStripMenuItem();
      this.extractallToolStripMenuItem = new ToolStripMenuItem();
      this.toolStripMenuItem1 = new ToolStripSeparator();
      this.toolDelete = new ToolStripMenuItem();
      this.openFileDialog1 = new OpenFileDialog();
      this.label2 = new Label();
      this.text_filter = new TextBox();
      this.button_filter = new Button();
      this.button_close = new Button();
      this.folderBrowserDialog1 = new FolderBrowserDialog();
      this.label_files = new Label();
      this.check_uncompressed_only = new CheckBox();
      this.contextMenuStrip1.SuspendLayout();
      this.SuspendLayout();
      this.label1.AutoSize = true;
      this.label1.Location = new Point(13, 13);
      this.label1.Name = "label1";
      this.label1.Size = new Size(60, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Current File";
      this.text_file.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.text_file.Location = new Point(79, 10);
      this.text_file.Name = "text_file";
      this.text_file.Size = new Size(749, 20);
      this.text_file.TabIndex = 1;
      this.button_browse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.button_browse.Location = new Point(845, 8);
      this.button_browse.Name = "button_browse";
      this.button_browse.Size = new Size(72, 23);
      this.button_browse.TabIndex = 2;
      this.button_browse.Text = "Browse...";
      this.button_browse.UseVisualStyleBackColor = true;
      this.button_browse.Click += new EventHandler(this.button_browse_Click);
      this.listView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.listView1.Columns.AddRange(new ColumnHeader[6]
      {
        this.c1,
        this.c2,
        this.c4,
        this.c5,
        this.c6,
        this.c7
      });
      this.listView1.ContextMenuStrip = this.contextMenuStrip1;
      this.listView1.FullRowSelect = true;
      this.listView1.Location = new Point(16, 37);
      this.listView1.Name = "listView1";
      this.listView1.Size = new Size(901, 383);
      this.listView1.TabIndex = 3;
      this.listView1.UseCompatibleStateImageBehavior = false;
      this.listView1.View = View.Details;
      this.listView1.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(this.listView1_ItemSelectionChanged);
      this.c1.Text = "#";
      this.c1.Width = 42;
      this.c2.Text = "Filename";
      this.c2.Width = 387;
      this.c4.Text = "Compressed size";
      this.c4.Width = 117;
      this.c5.Text = "Uncompressed size";
      this.c5.Width = 115;
      this.c6.Text = "File offset";
      this.c6.Width = 100;
      this.c7.Text = "CRC";
      this.c7.Width = 100;
      this.contextMenuStrip1.Items.AddRange(new ToolStripItem[4]
      {
        (ToolStripItem) this.extractToolStripMenuItem,
        (ToolStripItem) this.extractallToolStripMenuItem,
        (ToolStripItem) this.toolStripMenuItem1,
        (ToolStripItem) this.toolDelete
      });
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new Size(197, 76);
      this.contextMenuStrip1.Opening += new CancelEventHandler(this.contextMenuStrip1_Opening);
      this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
      this.extractToolStripMenuItem.Size = new Size(196, 22);
      this.extractToolStripMenuItem.Text = "&Extract selected file(s)...";
      this.extractToolStripMenuItem.Click += new EventHandler(this.extractToolStripMenuItem_Click);
      this.extractallToolStripMenuItem.Name = "extractallToolStripMenuItem";
      this.extractallToolStripMenuItem.Size = new Size(196, 22);
      this.extractallToolStripMenuItem.Text = "Extract &all...";
      this.extractallToolStripMenuItem.Click += new EventHandler(this.extractallToolStripMenuItem_Click);
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new Size(193, 6);
      this.toolDelete.Name = "toolDelete";
      this.toolDelete.Size = new Size(196, 22);
      this.toolDelete.Text = "&Delete";
      this.toolDelete.Click += new EventHandler(this.toolDelete_Click);
      this.openFileDialog1.DefaultExt = "bff";
      this.openFileDialog1.FileName = "*.bff";
      this.openFileDialog1.Filter = "BFF Archives (*.bff)|*.bff";
      this.label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      this.label2.AutoSize = true;
      this.label2.Location = new Point(16, 442);
      this.label2.Name = "label2";
      this.label2.Size = new Size(29, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "Filter";
      this.text_filter.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      this.text_filter.Location = new Point(67, 439);
      this.text_filter.Name = "text_filter";
      this.text_filter.Size = new Size(222, 20);
      this.text_filter.TabIndex = 5;
      this.text_filter.Text = "*.*";
      this.button_filter.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      this.button_filter.Location = new Point(305, 437);
      this.button_filter.Name = "button_filter";
      this.button_filter.Size = new Size(75, 23);
      this.button_filter.TabIndex = 6;
      this.button_filter.Text = "&Filter";
      this.button_filter.UseVisualStyleBackColor = true;
      this.button_filter.Click += new EventHandler(this.button_filter_Click);
      this.button_close.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.button_close.Location = new Point(842, 437);
      this.button_close.Name = "button_close";
      this.button_close.Size = new Size(75, 23);
      this.button_close.TabIndex = 7;
      this.button_close.Text = "Close";
      this.button_close.UseVisualStyleBackColor = true;
      this.button_close.Click += new EventHandler(this.button_close_Click);
      this.label_files.AutoSize = true;
      this.label_files.Location = new Point(401, 442);
      this.label_files.Name = "label_files";
      this.label_files.Size = new Size(83, 13);
      this.label_files.TabIndex = 8;
      this.label_files.Text = "0 file(s) selected";
      this.check_uncompressed_only.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.check_uncompressed_only.AutoSize = true;
      this.check_uncompressed_only.Location = new Point(686, 441);
      this.check_uncompressed_only.Name = "check_uncompressed_only";
      this.check_uncompressed_only.Size = new Size(147, 17);
      this.check_uncompressed_only.TabIndex = 9;
      this.check_uncompressed_only.Text = "Show uncompressed only";
      this.check_uncompressed_only.UseVisualStyleBackColor = true;
      this.check_uncompressed_only.CheckedChanged += new EventHandler(this.check_uncompressed_only_CheckedChanged);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(929, 467);
      this.Controls.Add((Control) this.check_uncompressed_only);
      this.Controls.Add((Control) this.label_files);
      this.Controls.Add((Control) this.label2);
      this.Controls.Add((Control) this.button_filter);
      this.Controls.Add((Control) this.button_close);
      this.Controls.Add((Control) this.text_filter);
      this.Controls.Add((Control) this.button_browse);
      this.Controls.Add((Control) this.text_file);
      this.Controls.Add((Control) this.listView1);
      this.Controls.Add((Control) this.label1);
      this.MinimumSize = new Size(650, 300);
      this.Name = nameof (Form1);
      this.Text = "BFF Unpacker {0}.{1}{2}";
      this.Load += new EventHandler(this.Form1_Load);
      this.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);
      this.contextMenuStrip1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    internal class FileItem
    {
      public int Index;
      public int FileOffset;
      public int FileOffset_Offset;
      public int CRC;
      public int CRC_Offset;
      public int Size;
      public int Size_Offset;
      public int USize;
      public int USize_Offset;
      public bool Compressed;
      public string Filename;
    }

    internal class ThreadData
    {
      public List<Form1.FileItem> List { get; set; }

      public string OutputFolder { get; set; }
    }
  }
}
