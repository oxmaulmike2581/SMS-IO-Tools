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

    public ManualResetEvent Stop => this._stop;

    public Form_Processing()
    {
      this.InitializeComponent();
      this._stop = new ManualResetEvent(false);
    }

    public void SetMax(int max)
    {
      this.progressBar1.Maximum = max;
      this.progressBar1.Value = 0;
      this._stop.Reset();
    }

    public void UpdateProgress(string file, int inc)
    {
      if (this.InvokeRequired)
        this.Invoke((Delegate) new Form_Processing.uiUpdate(this.updateLabel), (object) file, (object) inc);
      else
        this.updateLabel(file, inc);
    }

    private void updateLabel(string file, int inc)
    {
      this.label_file.Text = string.Format("Extracting {0}", (object) file);
      this.progressBar1.Increment(inc);
    }

    private void button_stop_Click(object sender, EventArgs e)
    {
      this._stop.Set();
      this.DialogResult = DialogResult.Cancel;
      this.button_stop.Enabled = false;
    }

    public void Finished()
    {
      if (this.InvokeRequired)
        this.Invoke((Delegate) new Form_Processing.closeDelegate(this.close));
      else
        this.close();
    }

    private void close() => this.DialogResult = DialogResult.OK;

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.progressBar1 = new ProgressBar();
      this.label_file = new Label();
      this.button_stop = new Button();
      this.SuspendLayout();
      this.progressBar1.Location = new Point(12, 39);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new Size(558, 23);
      this.progressBar1.TabIndex = 0;
      this.label_file.AutoSize = true;
      this.label_file.Location = new Point(13, 13);
      this.label_file.Name = "label_file";
      this.label_file.Size = new Size(63, 13);
      this.label_file.TabIndex = 1;
      this.label_file.Text = "Extracting...";
      this.button_stop.Location = new Point(253, 79);
      this.button_stop.Name = "button_stop";
      this.button_stop.Size = new Size(75, 23);
      this.button_stop.TabIndex = 2;
      this.button_stop.Text = "&STOP";
      this.button_stop.UseVisualStyleBackColor = true;
      this.button_stop.Click += new EventHandler(this.button_stop_Click);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = (IButtonControl) this.button_stop;
      this.ClientSize = new Size(582, 109);
      this.Controls.Add((Control) this.button_stop);
      this.Controls.Add((Control) this.label_file);
      this.Controls.Add((Control) this.progressBar1);
      this.DoubleBuffered = true;
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.Name = nameof (Form_Processing);
      this.SizeGripStyle = SizeGripStyle.Hide;
      this.Text = "Processing";
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    private delegate void uiUpdate(string file, int inc);

    private delegate void closeDelegate();
  }
}
