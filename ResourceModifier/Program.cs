using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;

namespace ResourceModifier
{
    public static class Console
    {
        public static string prelog = "";
        public static RichTextBox Target = null;
        public static void Write(object input)
        {
            Write(input, Color.Black);
        }
        public static void Write(object input, Color color)
        {
            System.Console.WriteLine(input);
            if (Target is null) prelog += input.ToString() + "\n";
            else
            {
                Target.SelectionColor = color;
                Target.AppendText(input.ToString() + "\n");
                Target.ScrollToCaret();
            }
        }
    }
    static class Program
    {
        public static List<Type> ExternalTypes;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ExternalTypes = new List<Type>();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        static public void LoadExternalAssemblies()
        {
            foreach (string fp in Directory.GetFiles(Path.GetDirectoryName(Application.ExecutablePath)))
            {
                if (Path.GetExtension(fp).ToUpper() != ".DLL") continue;
                Assembly DLL = Assembly.LoadFile(fp);
                foreach (Type type in DLL.GetExportedTypes())
                {
                    if(!type.IsSubclassOf(typeof(File)))continue;
                    var methodIsImportable = type.GetMethod("IsImportable");
                    if (methodIsImportable == null)
                    {
                        Console.Write(string.Format("Failed to load {0}: Missing \"IsImportable()\" function.", type.ToString()), Color.DarkRed);
                        continue;
                    }
                    object iI = methodIsImportable.Invoke(null, null);
                    if (!(iI is bool))
                    {
                        Console.Write(string.Format("Failed to load {0}: Function \"IsImportable()\" does not return a boolean.", type.ToString()), Color.DarkRed);
                        continue;
                    }
                    if (!((bool)iI)) continue;
                    var methodGetTypeName = type.GetMethod("GetTypeName");
                    if (methodGetTypeName == null)
                    {
                        Console.Write(string.Format("Failed to load {0}: Missing \"GetTypeName()\" function.", type.ToString()), Color.DarkRed);
                        continue;
                    }
                    object TN = methodGetTypeName.Invoke(null, null);
                    if (!(TN is string))
                    {
                        Console.Write(string.Format("Failed to load {0}: Function \"GetTypeName()\" does not return a string.", type.ToString()), Color.DarkRed);
                        continue;
                    }
                    var methodGetExtension = type.GetMethod("GetExtension");
                    if (methodGetExtension == null)
                    {
                        Console.Write(string.Format("Failed to load {0}: Missing \"GetExtension()\" function.", type.ToString()), Color.DarkRed);
                        continue;
                    }
                    object Ext = methodGetExtension.Invoke(null, null);
                    if (!(Ext is string))
                    {
                        Console.Write(string.Format("Failed to load {0}: Function \"GetExtension()\" does not return a string.", type.ToString()), Color.DarkRed);
                        continue;
                    }
                    Console.Write(string.Format("File Format \"{0}\" ({1}) successfully loaded.", TN, Ext), Color.Green);
                    ExternalTypes.Add(type);
                }
            }
        }
    }
}
