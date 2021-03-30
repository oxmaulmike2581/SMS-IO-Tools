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
    private List<FileItem> _files;
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
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private CheckBox checkBox5;
        private CheckBox checkBox4;
        private CheckBox checkBox3;
        private CheckBox checkBox2;
        private CheckBox checkBox1;
        private CheckBox check_uncompressed_only;

    public string CurrentArchive
    {
      get => _currentArchive;
      set => _currentArchive = value;
    }

    public Form1()
    {
      InitializeComponent();
    }

    private void Button_browse_Click(object sender, EventArgs e)
    {
      if (processing)
      {
        frmProcessing.Stop.Set();
      }
      else
      {
        if (DialogResult.OK != openFileDialog1.ShowDialog())
          return;
        text_file.Text = openFileDialog1.FileName;
        unpack(text_file.Text);
        showSelectedItems();
      }
    }

    private void unpack(string file)
    {
      _currentArchive = file;
      BFF.BFF bff = new BFF.BFF();
      _files = new List<FileItem>();
      files = bff.GetFileList(file);
      FileStream fileStream = new FileStream(text_file.Text, FileMode.Open);
      int num1 = 0;
      byte[] buffer1 = new byte[4];
      fileStream.Seek(280L, SeekOrigin.Begin);
      fileStream.Read(buffer1, 0, 4);
      int int32 = BitConverter.ToInt32(buffer1, 0);
      byte[] buffer2 = new byte[int32];
      fileStream.Seek(304L, SeekOrigin.Begin);
      fileStream.Read(buffer2, 0, int32);
      fileStream.Close();
      foreach (string file1 in files)
      {
        int num2 = 42 * num1 + 8;
        int startIndex1 = num2;
        int startIndex2 = num2 + 8;
        int startIndex3 = num2 + 12;
        int startIndex4 = startIndex2 + 18;
        int startIndex5 = startIndex3 + 12;
        _files.Add(new FileItem()
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
      showFiles("");
    }

    private void showFiles(string folder)
    {
      string[] items = new string[6];
      listView1.SuspendLayout();
      listView1.Items.Clear();
      foreach (string file in files)
      {
        int index = files.IndexOf(file);
        items[0] = index.ToString();
        items[1] = file;
        items[2] = _files[index].Size.ToString();
        items[3] = _files[index].USize.ToString();
        items[4] = _files[index].FileOffset.ToString("X4").PadLeft(4, '0');
        items[5] = _files[index].CRC.ToString("X8").PadLeft(8, '0');
        ListViewItem listViewItem = new ListViewItem(items);
        if (!_files[index].Compressed)
          listViewItem.ForeColor = Color.Blue;
        listView1.Items.Add(listViewItem);
      }
      listView1.ResumeLayout();
    }

    private void toolDelete_Click(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count <= 0 || DialogResult.Yes != MessageBox.Show((IWin32Window) this, listView1.SelectedItems.Count != 1 ? string.Format("This will affect {0} files and can't be undone. Are you sure?\r\n\r\nAlways make a backup of original archives!", (object) listView1.SelectedItems.Count) : "Delete file? This can't be undone\r\n\r\nAlways make a backup of original archives!", "Warning", MessageBoxButtons.YesNo))
        return;
      int num = 304;
      byte[] buffer = new byte[4];
      listView1.SuspendLayout();
      FileStream fileStream = new FileStream(_currentArchive, FileMode.Open);
      foreach (ListViewItem selectedItem in listView1.SelectedItems)
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
        _files[result].Compressed = false;
      }
      fileStream.Close();
      listView1.ResumeLayout();
    }

    private void button_close_Click(object sender, EventArgs e) => Close();

    private void button_filter_Click(object sender, EventArgs e)
    {
      string newValue = "[0-9a-z_!@$%,;\\-\\.\\\\s]+.";
      Regex regex = new Regex(new Regex("\\[|\\]\\{|\\}|&").Replace(text_filter.Text, "").Replace(".", "\\.").Replace("*", newValue) + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
      foreach (ListViewItem listViewItem in listView1.Items)
        listViewItem.Selected = regex.IsMatch(listViewItem.SubItems[1].Text);
      listView1.Focus();
      if (listView1.SelectedItems.Count > 0)
        listView1.SelectedItems[0].EnsureVisible();
      showSelectedItems();
    }

    private void showSelectedItems() => label_files.Text = string.Format("{0} file(s) selected of {1}", (object) listView1.SelectedItems.Count, (object) listView1.Items.Count);

    private void extractToolStripMenuItem_Click(object sender, EventArgs e) => extractFile(false);

    private void extractFile(bool all)
    {
      if (DialogResult.OK != folderBrowserDialog1.ShowDialog())
        return;
      string selectedPath = folderBrowserDialog1.SelectedPath;
      if (all || listView1.SelectedItems.Count > 1)
      {
        List<FileItem> fileItemList = new List<FileItem>();
        List<ListViewItem> listViewItemList = new List<ListViewItem>();
        if (all)
        {
          foreach (ListViewItem listViewItem in listView1.Items)
            listViewItemList.Add(listViewItem);
        }
        else
        {
          foreach (ListViewItem selectedItem in listView1.SelectedItems)
            listViewItemList.Add(selectedItem);
        }
        foreach (ListViewItem listViewItem in listViewItemList)
        {
          int result;
          int.TryParse(listViewItem.SubItems[0].Text, out result);
          fileItemList.Add(_files[result]);
        }
        processed.Reset();
        processing = true;
        frmProcessing = new Form_Processing();
        frmProcessing.SetMax(listViewItemList.Count);
        ThreadPool.QueueUserWorkItem(new WaitCallback(startProcessing), (object) new Form1.ThreadData()
        {
          List = fileItemList,
          OutputFolder = selectedPath
        });
        DialogResult dialogResult = frmProcessing.ShowDialog();
        processed.WaitOne();
        if (dialogResult == DialogResult.Cancel)
        {
          frmProcessing.Close();
          int num = (int) MessageBox.Show("Stopped by user", "Warning");
        }
        else
        {
          frmProcessing.Close();
          if (DialogResult.Yes == MessageBox.Show(string.Format("{0} file(s) extracted to {1}\r\n\r\nView extracted files now?", (object) listViewItemList.Count, (object) selectedPath), "Success", MessageBoxButtons.YesNo))
            Process.Start("explorer.exe", selectedPath);
        }
        frmProcessing.Dispose();
      }
      else
      {
        int result;
        int.TryParse(listView1.SelectedItems[0].SubItems[0].Text, out result);
        extractFile(result, selectedPath);
      }
      processing = false;
    }

    private void startProcessing(object state)
    {
      Form1.ThreadData threadData = state as Form1.ThreadData;
      string outputFolder = threadData.OutputFolder;
      foreach (Form1.FileItem fileItem in threadData.List)
      {
        if (frmProcessing.Stop.WaitOne(1, true))
        {
          processed.Set();
          return;
        }
        extractFile(fileItem.Index, outputFolder);
        if (frmProcessing.Stop.WaitOne(1, true))
        {
          processed.Set();
          return;
        }
        frmProcessing.UpdateProgress(fileItem.Filename, 1);
      }
      frmProcessing.Finished();
      processed.Set();
    }

    private void extractallToolStripMenuItem_Click(object sender, EventArgs e) => extractFile(true);

    private void extractFile(int index, string outputFolder)
    {
      string file = files[index];
      string path1 = outputFolder + "\\" + file;
      int fileOffset = _files[index].FileOffset;
      int size = _files[index].Size;
      int usize = _files[index].USize;
      string[] strArray = file.Split('\\');
      string path2 = outputFolder;
      for (int index1 = 0; index1 < strArray.Length - 1; ++index1)
      {
        path2 = path2 + "\\" + strArray[index1];
        if (!Directory.Exists(path2))
          Directory.CreateDirectory(path2);
      }
      if (_files[index].Compressed)
      {
        byte[] numArray = new byte[size];
        byte[] buffer;
        FileStream fileStream1 = new FileStream(_currentArchive, FileMode.Open);
        fileStream1.Seek((long) fileOffset, SeekOrigin.Begin);
        fileStream1.Read(numArray, 0, size);
        fileStream1.Close();

        // Changed because PS4 BFF's uses Oodle compression instead of XMemDecompress used in PC and X360 BFF's.
        if (radioButton1.Checked)
        {
            // decompressing using XMemCompress
            buffer = new BFF.BFF().XDecompress(numArray, size, usize);
            FileStream fileStream2 = new FileStream(path1, FileMode.Create);
            fileStream2.Write(buffer, 0, usize);
            fileStream2.Close();
        }
        else if (radioButton2.Checked)
        {
            // decompressing using Oodle
            buffer = new BFF.BFF().XDecompress(numArray, size, usize);
            FileStream fileStream2 = new FileStream(path1, FileMode.Create);
            fileStream2.Write(buffer, 0, usize);
            fileStream2.Close();
        }
      }
      else
      {
        byte[] buffer = new byte[usize];
        FileStream fileStream1 = new FileStream(_currentArchive, FileMode.Open);
        fileStream1.Seek((long) fileOffset, SeekOrigin.Begin);
        fileStream1.Read(buffer, 0, usize);
        fileStream1.Close();
        FileStream fileStream2 = new FileStream(path1, FileMode.Create);
        fileStream2.Write(buffer, 0, usize);
        fileStream2.Close();
      }
    }

    private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
    {
      showSelectedItems();
    }

    private void check_uncompressed_only_CheckedChanged(object sender, EventArgs e)
    {
      if (check_uncompressed_only.Checked)
      {
        List<ListViewItem> listViewItemList = new List<ListViewItem>();
        for (int index = 0; index < listView1.Items.Count; ++index)
        {
          if (!_files[index].Compressed)
            listViewItemList.Add(listView1.Items[index]);
        }
        listView1.SuspendLayout();
        listView1.Items.Clear();
        foreach (ListViewItem listViewItem in listViewItemList)
          listView1.Items.Add(listViewItem);
        listView1.ResumeLayout();
        showSelectedItems();
      }
      else
      {
        unpack(_currentArchive);
        showSelectedItems();
      }
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.AppendChild((XmlNode) xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"));
      XmlNode xmlNode = xmlDocument.AppendChild((XmlNode) xmlDocument.CreateElement("config"));
      XmlNode element1 = (XmlNode) xmlDocument.CreateElement("sourceFolder");
      XmlAttribute attribute1 = xmlDocument.CreateAttribute("value");
      attribute1.Value = text_file.Text;
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
      attribute3.Value = folderBrowserDialog1.SelectedPath;
      element3.Attributes.Append(attribute3);
      xmlNode.AppendChild(element3);
      xmlDocument.Save(unpackerConfigPath);
    }

    private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
    {
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      if (_currentArchive == null || !(_currentArchive != string.Empty) || !File.Exists(_currentArchive))
        return;
      text_file.Text = _currentArchive;
      unpack(text_file.Text);
      showSelectedItems();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && components != null)
        components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.text_file = new System.Windows.Forms.TextBox();
            this.button_browse = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.c1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.c7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.text_filter = new System.Windows.Forms.TextBox();
            this.button_filter = new System.Windows.Forms.Button();
            this.button_close = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label_files = new System.Windows.Forms.Label();
            this.check_uncompressed_only = new System.Windows.Forms.CheckBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Current File";
            // 
            // text_file
            // 
            this.text_file.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.text_file.Location = new System.Drawing.Point(79, 10);
            this.text_file.Name = "text_file";
            this.text_file.Size = new System.Drawing.Size(749, 20);
            this.text_file.TabIndex = 1;
            // 
            // button_browse
            // 
            this.button_browse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_browse.Location = new System.Drawing.Point(845, 8);
            this.button_browse.Name = "button_browse";
            this.button_browse.Size = new System.Drawing.Size(72, 23);
            this.button_browse.TabIndex = 2;
            this.button_browse.Text = "Browse...";
            this.button_browse.UseVisualStyleBackColor = true;
            this.button_browse.Click += new System.EventHandler(this.Button_browse_Click);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.c1,
            this.c2,
            this.c4,
            this.c5,
            this.c6,
            this.c7});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(16, 37);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(901, 421);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listView1_ItemSelectionChanged);
            // 
            // c1
            // 
            this.c1.Text = "#";
            this.c1.Width = 42;
            // 
            // c2
            // 
            this.c2.Text = "Filename";
            this.c2.Width = 387;
            // 
            // c4
            // 
            this.c4.Text = "Compressed size";
            this.c4.Width = 117;
            // 
            // c5
            // 
            this.c5.Text = "Uncompressed size";
            this.c5.Width = 115;
            // 
            // c6
            // 
            this.c6.Text = "File offset";
            this.c6.Width = 100;
            // 
            // c7
            // 
            this.c7.Text = "CRC";
            this.c7.Width = 100;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractToolStripMenuItem,
            this.extractallToolStripMenuItem,
            this.toolStripMenuItem1,
            this.toolDelete});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(198, 76);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // extractToolStripMenuItem
            // 
            this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
            this.extractToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.extractToolStripMenuItem.Text = "&Extract selected file(s)...";
            this.extractToolStripMenuItem.Click += new System.EventHandler(this.extractToolStripMenuItem_Click);
            // 
            // extractallToolStripMenuItem
            // 
            this.extractallToolStripMenuItem.Name = "extractallToolStripMenuItem";
            this.extractallToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.extractallToolStripMenuItem.Text = "Extract &all...";
            this.extractallToolStripMenuItem.Click += new System.EventHandler(this.extractallToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(194, 6);
            // 
            // toolDelete
            // 
            this.toolDelete.Name = "toolDelete";
            this.toolDelete.Size = new System.Drawing.Size(197, 22);
            this.toolDelete.Text = "&Delete";
            this.toolDelete.Click += new System.EventHandler(this.toolDelete_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "bff";
            this.openFileDialog1.FileName = "*.bff";
            this.openFileDialog1.Filter = "BFF Archives (*.bff)|*.bff";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 513);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Filter";
            // 
            // text_filter
            // 
            this.text_filter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.text_filter.Location = new System.Drawing.Point(67, 510);
            this.text_filter.Name = "text_filter";
            this.text_filter.Size = new System.Drawing.Size(222, 20);
            this.text_filter.TabIndex = 5;
            this.text_filter.Text = "*.*";
            // 
            // button_filter
            // 
            this.button_filter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_filter.Location = new System.Drawing.Point(305, 508);
            this.button_filter.Name = "button_filter";
            this.button_filter.Size = new System.Drawing.Size(75, 23);
            this.button_filter.TabIndex = 6;
            this.button_filter.Text = "&Filter";
            this.button_filter.UseVisualStyleBackColor = true;
            this.button_filter.Click += new System.EventHandler(this.button_filter_Click);
            // 
            // button_close
            // 
            this.button_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_close.Location = new System.Drawing.Point(842, 508);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(75, 23);
            this.button_close.TabIndex = 7;
            this.button_close.Text = "Close";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // label_files
            // 
            this.label_files.AutoSize = true;
            this.label_files.Location = new System.Drawing.Point(406, 513);
            this.label_files.Name = "label_files";
            this.label_files.Size = new System.Drawing.Size(83, 13);
            this.label_files.TabIndex = 8;
            this.label_files.Text = "0 file(s) selected";
            // 
            // check_uncompressed_only
            // 
            this.check_uncompressed_only.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.check_uncompressed_only.AutoSize = true;
            this.check_uncompressed_only.Location = new System.Drawing.Point(686, 512);
            this.check_uncompressed_only.Name = "check_uncompressed_only";
            this.check_uncompressed_only.Size = new System.Drawing.Size(147, 17);
            this.check_uncompressed_only.TabIndex = 9;
            this.check_uncompressed_only.Text = "Show uncompressed only";
            this.check_uncompressed_only.UseVisualStyleBackColor = true;
            this.check_uncompressed_only.CheckedChanged += new System.EventHandler(this.check_uncompressed_only_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(6, 16);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(39, 17);
            this.radioButton1.TabIndex = 10;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "PC";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(51, 16);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(45, 17);
            this.radioButton2.TabIndex = 11;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "PS4";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Location = new System.Drawing.Point(13, 465);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(97, 39);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Game platform";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBox5);
            this.groupBox2.Controls.Add(this.checkBox4);
            this.groupBox2.Controls.Add(this.checkBox3);
            this.groupBox2.Controls.Add(this.checkBox2);
            this.groupBox2.Controls.Add(this.checkBox1);
            this.groupBox2.Location = new System.Drawing.Point(364, 465);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(553, 39);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(6, 16);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(101, 17);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "Fix MEB header";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(114, 16);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(99, 17);
            this.checkBox2.TabIndex = 1;
            this.checkBox2.Text = "Fix TEX header";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(220, 16);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(127, 17);
            this.checkBox3.TabIndex = 2;
            this.checkBox3.Text = "Convert DDS to PNG";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(353, 16);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(102, 17);
            this.checkBox4.TabIndex = 3;
            this.checkBox4.Text = "Skip name table";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(462, 16);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(87, 17);
            this.checkBox5.TabIndex = 4;
            this.checkBox5.Text = "Debug mode";
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(929, 538);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.check_uncompressed_only);
            this.Controls.Add(this.label_files);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_filter);
            this.Controls.Add(this.button_close);
            this.Controls.Add(this.text_filter);
            this.Controls.Add(this.button_browse);
            this.Controls.Add(this.text_file);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(650, 300);
            this.Name = "Form1";
            this.Text = "BFF Unpacker - TEST";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
