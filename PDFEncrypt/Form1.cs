using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;


namespace PDFEncrypt
{
    public partial class Form1 : Form
    {
        private string password = "";
        private const int CONVERT_TIMEOUT = 15000;
        public bool singleFileMode = false;
        public string singleFileName = "";

        public Form1()
        {
            InitializeComponent();
        }

        private DialogResult InputBox(string Title, ref string Password, ref string Verify)
        {
            System.Drawing.Size size = new System.Drawing.Size(200, 100);

            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = Title;

            //inputBox.Parent = this;
            inputBox.StartPosition = FormStartPosition.CenterParent;
            inputBox.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            inputBox.ShowInTaskbar = false;
            inputBox.MinimizeBox = false;
            inputBox.MaximizeBox = false;

            
            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.UseSystemPasswordChar = true;
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = "";
            inputBox.Controls.Add(textBox);

            System.Windows.Forms.TextBox textBox2 = new TextBox();
            textBox2.UseSystemPasswordChar = true;
            textBox2.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox2.Location = new System.Drawing.Point(5, 30);
            textBox2.Text = "";
            inputBox.Controls.Add(textBox2);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 29);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 60);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 29);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 60);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton; 

            DialogResult result = inputBox.ShowDialog();
            Password = textBox.Text;
            Verify = textBox2.Text;
            return result;
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void openInputDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectInputFolder();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetPassword();
        }

        private DialogResult GetPassword()
        {
            string verify = "";
            while (this.InputBox("Enter PDF Password (twice)", ref password, ref verify) == DialogResult.OK)
            {
                if (password.CompareTo(verify) == 0)
                {
                    StatusBarLabel.Text = "Password Entered";
                    return DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Passwords don't match. Please re-enter.");
                    password = "";
                    verify = "";
                }
            }
            return DialogResult.Cancel;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private DialogResult SelectInputFolder()
        {
            this.folderBrowserDialog1.Description = "Select Input Folder containing source PDF files:";
            folderBrowserDialog1.ShowNewFolderButton = false;
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                inputFolder.Text = folderBrowserDialog1.SelectedPath;
                StatusBarLabel.Text = "Input Folder Set";
                return DialogResult.OK;
            }
            return DialogResult.Cancel;
        }

        private DialogResult SelectOutputFolder()
        {
            folderBrowserDialog2.Description = "Select Output Folder for encrypted PDF files:";
            folderBrowserDialog2.ShowNewFolderButton = true;
            if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
            {
                outputFolder.Text = folderBrowserDialog2.SelectedPath;
                StatusBarLabel.Text = "Output Folder Set";
                return DialogResult.OK;
            }
            return DialogResult.Cancel;
        }

        private void outputDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectOutputFolder();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectInputFolder();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SelectOutputFolder();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            EncryptFiles();
        }

        private void EncryptFiles()
        {
            while(password.Trim().Length == 0)
                if (GetPassword() != DialogResult.OK)
                    return;

            while (inputFolder.Text.Trim().Length == 0)
                if (SelectInputFolder() != DialogResult.OK)
                    return;

            while (outputFolder.Text.Trim().Length == 0)
                if (SelectOutputFolder() != DialogResult.OK)
                    return;

            string[] sfiles = Directory.GetFiles(inputFolder.Text, "*.pdf");
            StatusBarProgress.Visible = true;
            StatusBarProgress.ProgressBar.Minimum = 0;
            StatusBarProgress.Maximum = sfiles.Length;
            int count = 0;
            foreach(string sfile in sfiles)
            {
                StatusBarProgress.Value = count++;
                encryptSingleFile(inputFolder.Text, outputFolder.Text, sfile);
            }
            StatusBarLabel.Text = string.Format("Encryption complete.  {0} files converted.", count);
            StatusBarProgress.Visible = false;
        }

        private bool encryptSingleFile(string indir, string outdir, string sfile)
        {
            StatusBarLabel.Text = "Converting " + sfile;
            string filename = Path.GetFileName(sfile);
            string cmdline = string.Format("\"{0}\" \"{1}\" --encrypt {2} {2} 256 --",
                indir + "\\" + filename,
                outdir + "\\" + filename,
                password);

            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "qpdf.exe",
                    Arguments = cmdline,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            try
            {
               proc.Start();
            }

            catch (Exception ex)
            {
                MessageBox.Show("PDF Conversion Exception:" + "\r\n" + ex.Message.ToString(), "PDFEncrypt Error");
                return false;
            }

            if (!proc.WaitForExit(CONVERT_TIMEOUT))
            {
                MessageBox.Show("PDF Conversion Timeout:" + "\r\n" + proc.StandardError.ReadToEnd() + "\r\n" + proc.StandardOutput.ReadToEnd(), "PDFEncrypt Error");
                return false;
            }

            if (proc.ExitCode != 0)
            {
                MessageBox.Show("PDF Conversion Failure:" + "\r\n" + proc.StandardError.ReadToEnd() + "\r\n" + proc.StandardOutput.ReadToEnd(), "PDFEncrypt Error");
                return false;
            }

            return true;
        }

        private void encryptPDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EncryptFiles();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (singleFileMode)
            {
                if (!this.singleFileName.ToLower().EndsWith(".pdf"))
                {
                    MessageBox.Show(singleFileName + " is not a PDF file.", "PDF Encryption Failed");
                }
                else
                {
                    if (GetPassword() != DialogResult.OK)
                    {
                        MessageBox.Show("Password entry failure\r\n" + singleFileName + " is unchanged.", "PDF Encryption Failed");
                        singleFileMode = false;
                        Close();
                        return;
                    }
                    string temp_path = System.IO.Path.GetTempPath().ToString();
                    string filename = Path.GetFileName(singleFileName);
                    string temp_pathname = temp_path + filename;
                    string dest_path = Path.GetDirectoryName(singleFileName);
                    temp_path = temp_path.TrimEnd('\\'); 
                    File.Copy(singleFileName, temp_pathname, true);
                    if (encryptSingleFile(temp_path, dest_path, filename))
                    {
                        MessageBox.Show(singleFileName + " has been encrypted with your password.", "PDF Encrypt");
                    }
                    else
                    {
                        File.Copy(temp_pathname, singleFileName, true);
                        MessageBox.Show("An error prevented " + singleFileName + " from being encrypted.  The file is unchanged.", "PDF Encrypt", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    File.Delete(temp_pathname);
                }
                singleFileMode = false;
                Close();
            }
        }
    }
}
