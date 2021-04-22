using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.IO;

namespace WatershedIntegratedModel
{
    public class SDGsAssessModel
    {
        public SDGsAssessModel()
        {
        }

        /// <summary>
        /// Run python program
        /// </summary>
        /// <param name="pythonpath">Python file's path, such as @"D:\fast\fast_test_subjective_classification.py"</param>
        /// <param name="pythonname">Python excutive file's path, such as @"C:\Users\Gyc\PycharmProjects\untitled\venv\Scripts\python.exe"</param>
        public static string RunPythonScript(string pythonpath,string pythonname,int seconds)
        {
            string _output = "";
            Process p = new Process();
            string path = pythonpath;// 获得python文件的绝对路径
            p.StartInfo.FileName = pythonname;      // @"C:\Users\Gyc\PycharmProjects\untitled\venv\Scripts\python.exe";//没有配环境变量的话，可以像我这样写python.exe的绝对路径。如果配了，直接写"python.exe"即可
            string sArguments = path;

            p.StartInfo.Arguments = sArguments;

            p.StartInfo.UseShellExecute = false;

            p.StartInfo.RedirectStandardOutput = true;

            p.StartInfo.RedirectStandardInput = true;

            p.StartInfo.RedirectStandardError = true;

            p.StartInfo.CreateNoWindow = true;

            //p.Start();
            //p.WaitForExit();
            //_output = p.StandardOutput.ReadToEnd();
            //DisplayResult();
            try
            {
                if (p.Start())//开始进程     
                {
                    if (seconds == 0)
                    {
                        p.WaitForExit();//这里无限等待进程结束     
                    }
                    else
                    {
                        p.WaitForExit(seconds); //等待进程结束，等待时间为指定的毫秒     
                    }
                    _output = p.StandardOutput.ReadToEnd();//读取进程的输出     
                }
            }
            catch (Exception err)
            {
                return _output;
            }
            finally
            {
                if (p != null)
                    p.Close();
            }
            return _output;
        }

    }
}
