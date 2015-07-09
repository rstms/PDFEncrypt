using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace PDFEncrypt
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool flag = false;
            string filename = "";

            if(args.Length > 0)
            {
                if (args[0].ToLower() == "-installmenu")
                {
                    string exename = System.Reflection.Assembly.GetEntryAssembly().Location;
                    Registry.SetValue("HKEY_CLASSES_ROOT\\*\\shell\\Encrypt PDF with Password\\command", null, "\"" + exename + "\" -encrypt \"%1\"");
                    return;
                }
                else if (args[0].ToLower() == "-removemenu")
                {
                    string exename = System.Reflection.Assembly.GetEntryAssembly().Location;
                    string keyname = @"*\\shell";

                    using(RegistryKey key = Registry.ClassesRoot.OpenSubKey(keyname, true))
                    {
                        if(key != null)
                            key.DeleteSubKeyTree(@"Encrypt PDF with Password");
                    }
                    return;
                }
                else if (args[0].ToLower() == "-encrypt")
                {
                    flag = true;
                    filename = args[1];
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 appform = new Form1();
            if (flag)
            {
                appform.singleFileMode = true;
                appform.singleFileName = filename;
            }
            Application.Run(appform);
        }
    }
}
