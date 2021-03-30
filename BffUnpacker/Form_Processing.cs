// Decompiled with JetBrains decompiler
// Type: BffUnpacker.Form_Processing
// Assembly: BffUnpacker, Version=1.0.11.2051, Culture=neutral, PublicKeyToken=null
// MVID: 6905B5B6-9334-40C1-AEBB-A5EB06804AFD
// Assembly location: D:\hexing\[tools]\[nfs]\NFS Shift BFF Tools 1.45 by japamd\BffUnpacker.exe

using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace BffUnpacker
{
  public class Form_Processing : Form
  {
    private ManualResetEvent _stop;
    private IContainer components;
    private ProgressBar progressBar1;
    private Label label_file;
    private Button button_stop;

    public ManualResetEvent Stop => _stop;

    public Form_Processing()
    {
      InitializeComponent();
      _stop = new ManualResetEvent(false);
    }

    public void SetMax(int max)
    {
      progressBar1.Maximum = max;
      progressBar1.Value = 0;
      _stop.Reset();
    }

    public void UpdateProgress(string file, int inc)
    {
      if (InvokeRequired)
        Invoke((Delegate) new Form_Processing.uiUpdate(updateLabel), (object) file, (object) inc);
      else
        updateLabel(file, inc);
    }

    private void updateLabel(string file, int inc)
    {
      label_file.Text = string.Format("Extracting {0}", (object) file);
      progressBar1.Increment(inc);
    }

    private void button_stop_Click(object sender, EventArgs e)
    {
      _stop.Set();
      DialogResult = DialogResult.Cancel;
      button_stop.Enabled = false;
    }

    public void Finished()
    {
      if (InvokeRequired)
        Invoke((Delegate) new Form_Processing.closeDelegate(close));
      else
        close();
    }

    private void close() => DialogResult = DialogResult.OK;

    protected override void Dispose(bool disposing)
    {
      if (disposing && components != null)
        components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      progressBar1 = new ProgressBar();
      label_file = new Label();
      button_stop = new Button();
      SuspendLayout();
      progressBar1.Location = new Point(12, 39);
      progressBar1.Name = "progressBar1";
      progressBar1.Size = new Size(558, 23);
      progressBar1.TabIndex = 0;
      label_file.AutoSize = true;
      label_file.Location = new Point(13, 13);
      label_file.Name = "label_file";
      label_file.Size = new Size(63, 13);
      label_file.TabIndex = 1;
      label_file.Text = "Extracting...";
      button_stop.Location = new Point(253, 79);
      button_stop.Name = "button_stop";
      button_stop.Size = new Size(75, 23);
      button_stop.TabIndex = 2;
      button_stop.Text = "&STOP";
      button_stop.UseVisualStyleBackColor = true;
      button_stop.Click += new EventHandler(button_stop_Click);
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      CancelButton = (IButtonControl) button_stop;
      ClientSize = new Size(582, 109);
      Controls.Add((Control) button_stop);
      Controls.Add((Control) label_file);
      Controls.Add((Control) progressBar1);
      DoubleBuffered = true;
      FormBorderStyle = FormBorderStyle.FixedSingle;
      MaximizeBox = false;
      // Name = nameof (Form_Processing);
      SizeGripStyle = SizeGripStyle.Hide;
      Text = "Processing";
      ResumeLayout(false);
      PerformLayout();
    }

    private delegate void uiUpdate(string file, int inc);

    private delegate void closeDelegate();
  }
}
