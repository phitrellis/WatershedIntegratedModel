using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using MathNet.Numerics;

namespace WatershedIntegratedModel
{
    public class GlobalFunctions
    {
        public GlobalFunctions()
        {
        }

        /// <summary>
        /// Read data from excel file
        /// </summary>
        /// <param name="filepath">excel file path</param>
        /// <param name="sheetname">sheet name of excel file</param>
        /// <param name="col">column number</param>
        /// <returns></returns>
        public System.Object[,] ReadExcelData(string filepath, string sheetname, int col)
        {
            try
            {
                System.Data.DataTable _dt = new System.Data.DataTable();
                System.Object[,] _vars = null;
                int _cols = 0;
                if (filepath != "")
                {
                    // 打开excel文件读取数据
                    _dt = ExcelToDataTable(filepath, sheetname);
                    _vars = new System.Object[_dt.Rows.Count, col];
                    if (_dt.Rows.Count > 0)
                    {
                        _vars = new System.Object[_dt.Rows.Count, col];
                        _cols = _dt.Columns.Count;
                        if (_dt.Rows[1].ItemArray[1].ToString() != "" || _dt.Rows[1].ItemArray[1].ToString() != null)
                        {
                            for (int j = 0; j < _dt.Rows.Count; j++)
                            {
                                for (int i = 0; i < col; i++)
                                {
                                    _vars[j, i] = _dt.Rows[j].ItemArray[i].ToString();
                                }
                            }
                        }
                    }
                }
                return _vars;
            }
            catch (Exception err)
            {
                return null;
            }
        }

        /// <summary>
        /// 从excel中读取数据到一个DataTable中
        /// </summary>
        /// <param name="strExcelFileName"></param>
        /// <param name="strSheetName"></param>
        /// <returns></returns>
        public static System.Data.DataTable ExcelToDataTable(string strExcelFileName, string strSheetName)
        {
            //源的定义
            string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + strExcelFileName + ";" + "Extended Properties='Excel 8.0;HDR=NO;IMEX=1';";
            //string strConn = @"Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + strExcelFileName + ";" + "Extended Properties='Excel 8.0;HDR=NO;IMEX=1';";
            //string strConn = @"Provider=Microsoft.ACE.OLEDB.16.0;" + "Data Source=" + strExcelFileName + ";" + "Extended Properties='Excel 8.0;HDR=YES';";

            //Sql语句
            //string strExcel = string.Format("select * from [{0}$]", strSheetName); 这是一种方法
            string strExcel = "select * from [" + strSheetName + "$]";

            //定义存放的数据表z
            DataSet ds = new DataSet();

            //连接数据源
            OleDbConnection conn = new OleDbConnection(strConn);

            conn.Open();


            //适配到数据源
            OleDbDataAdapter adapter = new OleDbDataAdapter(strExcel, strConn);
            adapter.Fill(ds, strSheetName);


            conn.Close();
            adapter.Dispose();
            conn.Dispose();

            return ds.Tables[strSheetName];
        }

        /// <summary>
        /// 从CVS文件中读取数据
        /// </summary>
        /// <param name="fullpath"></param>
        /// <param name="info">当文件不存在时，抛出的错误信息</param>
        /// <param name="errinfo">错误信息标题</param>
        /// <returns></returns>
        public static System.Collections.ArrayList ReadDataCSV(string fullpath,string info,string errinfo)
        {
            try
            {
                System.Collections.ArrayList _result = new System.Collections.ArrayList();
                if (File.Exists(fullpath))
                {                   
                    StreamReader _sr = new StreamReader(fullpath,System.Text.Encoding.Default);
                    string _line = "";

                    while((_line = _sr.ReadLine())!= null)
                    {
                        _result.Add(_line);
                    }
                    _sr.Close();
                    return _result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception err)
            {
                return null;
            }
        }

        /// <summary>
        /// 写数据到CSV文件 - UTF-8
        /// </summary>
        /// <param name="foldername">目录名称</param>
        /// <param name="subfolder">子目录名称</param>
        /// <param name="filename">文件名</param>
        /// <param name="info">数据体</param>
        public void WriteGlobleCSVFileUTF(string foldername, string filename, string[,] info, string sign)
        {
            try
            {
                string _str = "";
                System.IO.Directory.CreateDirectory(foldername);

                filename = filename + ".CSV";

                foldername = System.IO.Path.Combine(foldername, filename);

                if (File.Exists(foldername))
                {
                    File.Delete(foldername);
                }

                System.IO.StreamWriter _sw = new System.IO.StreamWriter(foldername, true, System.Text.Encoding.GetEncoding("UTF-8"));

                for (int i = 0; i < info.GetLength(0); i++)
                {
                    _str = info[i, 0];
                    for (int j = 1; j < info.GetLength(1); j++)
                    {
                        _str = _str + sign + info[i, j];       // +"," + info[i, 2] + "," + info[i, 3] + "\n";
                    }
                    _sw.WriteLine(_str);
                }


                _sw.Close();
            }
            catch (Exception err)
            {
            }
        }


        /// <summary>
        /// 写数据到CSV文件 - gb2312
        /// </summary>
        /// <param name="foldername">目录名称</param>
        /// <param name="subfolder">子目录名称</param>
        /// <param name="filename">文件名</param>
        /// <param name="info">数据体</param>
        public void WriteGlobleCSVFile(string foldername, string subfolder, string filename, string[,] info)
        {
            try
            {
                string _str = "";
                string _pathstring = System.IO.Path.Combine(foldername, subfolder);
                System.IO.Directory.CreateDirectory(_pathstring);

                filename = filename.Trim() + ".CSV";

                _pathstring = System.IO.Path.Combine(_pathstring, filename);

                if (File.Exists(_pathstring))
                {
                    File.Delete(_pathstring);
                }

                System.IO.StreamWriter _sw = new System.IO.StreamWriter(_pathstring, true, System.Text.Encoding.GetEncoding("gb2312"));

                for (int i = 0; i < info.GetLength(0); i++)
                {
                    _str = info[i, 0];
                    for (int j = 1; j < info.GetLength(1); j++)
                    {
                        _str = _str + "," + info[i, j];       // +"," + info[i, 2] + "," + info[i, 3] + "\n";
                    }
                    _sw.WriteLine(_str);
                }


                _sw.Close();
            }
            catch (Exception err)
            {
            }
        }

        /// <summary>
        /// 写数据到CSV文件
        /// </summary>
        /// <param name="foldername">目录名称</param>
        /// <param name="subfolder">子目录名称</param>
        /// <param name="filename">文件名</param>
        /// <param name="info">数据体</param>
        public void WriteGlobleCSVFile2(string foldername, string filename, string[,] info,string sign)
        {
            try
            {
                string _str = "";
                System.IO.Directory.CreateDirectory(foldername);

                filename = filename + ".CSV";

                foldername = System.IO.Path.Combine(foldername, filename);

                if (File.Exists(foldername))
                {
                    File.Delete(foldername);
                }

                System.IO.StreamWriter _sw = new System.IO.StreamWriter(foldername, true, System.Text.Encoding.GetEncoding("UTF-8"));

                for (int i = 0; i < info.GetLength(0); i++)
                {
                    _str = info[i, 0];
                    for (int j = 1; j < info.GetLength(1); j++)
                    {
                        _str = _str + sign + info[i, j];       // +"," + info[i, 2] + "," + info[i, 3] + "\n";
                    }
                    _sw.WriteLine(_str);
                }


                _sw.Close();
            }
            catch (Exception err)
            {
            }
        }


        /// <summary>
        /// 写数据到文本文件-覆盖方法
        /// </summary>
        /// <param name="fullpath">文件全路径</param>
        /// <param name="data">数据体</param>
        public static void WriteTextFile(string fullpath, string[] data)
        {
            try
            {
                //StreamWriter _sw;
                //Stream _streamfile = sfd.OpenFile();

                //_sw = new StreamWriter(_streamfile, System.Text.Encoding.GetEncoding("gb2312"));
                StreamWriter _sw = new System.IO.StreamWriter(fullpath, false, System.Text.Encoding.GetEncoding("gb2312"));

                for (int i = 0; i < data.GetLength(0); i++)
                {
                    _sw.WriteLine(data[i]);
                }
                _sw.Close();
                //_streamfile.Close();
            }
            catch (Exception err)
            {
            }
        }

        /// <summary>
        /// 读取文本文件
        /// </summary>
        /// <param name="fullpath">文件全路径</param>
        /// 
        public static System.Collections.ArrayList ReadTextFile(string fullpath)
        {
            try
            {
                System.Collections.ArrayList _result = new System.Collections.ArrayList();

                //_sw = new StreamWriter(_streamfile, System.Text.Encoding.GetEncoding("gb2312"));
                StreamReader _sw = new System.IO.StreamReader(fullpath, System.Text.Encoding.GetEncoding("gb2312"));

                string _nextline = "";

                while ((_nextline = _sw.ReadLine()) != null)
                {
                    _result.Add(_nextline);
                }
                _sw.Close();
                return _result;
            }
            catch (Exception err)
            {
                return null;
            }
        }



        /// <summary>
        /// 将经济模型参数重写到经济模型的输入文件term.cmf中
        /// </summary>
        /// <param name="fullpath">文件所在全路径</param>
        /// <param name="info">经济模型参数数据，10各县5个参数</param>
        public static void ReWriteEcoTerm(string fullpath, string[] info)
        {
            try
            {
                if (File.Exists(fullpath))
                {
                    System.Collections.ArrayList _tempVar = new System.Collections.ArrayList();
                    // read data 
                    StreamReader _sr = new StreamReader(fullpath,System.Text.Encoding.Default);
                    string _line = "";
                    while ((_line = _sr.ReadLine()) != null)
                    {
                        if (_line.Contains("shock") && _line.Contains("uniform"))
                        {
                            break;
                        }
                        else
                        {
                            _tempVar.Add(_line);
                        }
                    }
          
                    _sr.Close();
                    for (int count = 0; count < info.Length; count++)
                    {
                        _tempVar.Add(info[count]);
                    }
                    //FileStream _fs = File.Open(fullpath, FileMode.Create);
                    System.IO.StreamWriter _sw = new System.IO.StreamWriter(fullpath, false, System.Text.Encoding.GetEncoding("gb2312"));
                    
                    for (int count = 0; count < _tempVar.Count; count++)
                    {
                        _sw.WriteLine(_tempVar[count].ToString());
                    }
                    _sw.Close();
                }
                else
                {
                }
            }
            catch (Exception err)
            {
            }
        }

        /// <summary>
        /// 写数据到excel文件中 （有问题！）
        /// </summary>
        /// <param name="path">文件目录</param>
        /// <param name="filename">文件名</param>
        /// <param name="sheet">活页表名称</param>
        /// <param name="info">数据体</param>
        /// <param name="sheetnumber">sheet sort number</param>
        public void WriteDatatoExcel(string path, string filename, string sheet, string[,] info)
        {
            try
            {
                // if the file exist, then delete it
                path = path + "\\" + filename;
                if(File.Exists(path))
                {
                    File.Delete(path);
                }
                //System.Object _nothing = System.Reflection.Missing.Value;
                //var app = new Excel.Application();
                //app.Visible = false;
                Excel.Application app = new Excel.Application();
                Excel.Workbook _workbook = app.Workbooks.Add(true);
                Excel.Worksheet _worksheet = _workbook.ActiveSheet as Excel.Worksheet;      // (Excel.Worksheet)_workbook.Sheets[sheetnumber];
                _worksheet = (Excel.Worksheet)_workbook.Worksheets.get_Item(0);
                _worksheet.Name = sheet;

                for (int row = 0; row < info.GetLength(0); row++)
                {
                    for (int col = 0; col < info.GetLength(1); col++)
                    {
                        _worksheet.Cells[row, col] = info[row, col];
                    }
                }

                app.Quit();
                _worksheet = null;
                _workbook = null;
                app = null;

            }
            catch (Exception err)
            {
            }
        }

        /// <summary>
        /// Separete the string accoring to the sign, such as ","," ",";"
        /// </summary>
        /// <param name="str">Separeted string</param>
        /// <param name="count">number of separated data</param>
        /// <param name="sign">separated sign</param>
        /// <returns></returns>
        public static string[] ReadListofSynergy(string str, int count, string sign)
        {
            try
            {
                string[] _result = new string[count];
                bool _adjust = false;
                int _start = 0, _end = 0, _count = 0;
                string _temp = "";

                do
                {
                    if (str.Contains(sign))
                    {
                        _adjust = true;
                        _end = str.IndexOf(sign);
                        if (_end > 0)
                        {
                            _temp = str.Substring(_start, _end - _start);

                            str = str.Substring(_end + 1, str.Length - _temp.Length - 1);

                            _result[_count] = _temp;

                            _count++;
                        }
                        else
                        {
                            _temp = str.Substring(_start, _end - _start);

                            str = str.Substring(_end + 1, str.Length - _temp.Length - 1);
                        }
                    }
                    else
                    {
                        _adjust = false;
                        _result[_count] = str;
                    }

                } while (_adjust == true);

                return _result;
            }
            catch (Exception err)
            {
                return null;
            }
        }

        /// <summary>
        /// 将可持续发展评价模型输出结果转换为系统输出结果
        /// </summary>
        /// <param name="str">指标字符串</param>
        /// <param name="splsign">分隔符</param>
        /// <param name="count">模拟年份数</param>
        /// <returns></returns>
        public static string[] ReadSDGIndex(string str,char splsign,int count)
        {
            string[] _result = new string[count];
            string[] _temp = str.Split(splsign);
            int _count = 0;

            for (int i = 0; i < _temp.Length; i++)
            {
                if (_temp[i] != "")
                {
                    _result[_count] = _temp[i];
                    _count++;
                }
            }
            return _result;
        }

        /// <summary>     
        /// 执行DOS命令，返回DOS命令的输出     
        /// </summary>     
        /// <param name="dosCommand">dos命令</param>     
        /// <param name="milliseconds">等待命令执行的时间（单位：毫秒），     
        /// 如果设定为0，则无限等待</param>     
        /// <returns>返回DOS命令的输出</returns>     
        public static string Execute(string command, string doscmdpath, int seconds)
        {
            string output = ""; //输出字符串     
            if (command != null && !command.Equals(""))
            {
                Process _process = new Process();//创建进程对象     
                //ProcessStartInfo startInfo = new ProcessStartInfo();
                //startInfo.FileName = doscmdpath;//设定需要执行的命令     
                //startInfo.Arguments = command;//“/C”表示执行完命令后马上退出     
                //startInfo.UseShellExecute = false;//不使用系统外壳程序启动     
                //startInfo.RedirectStandardInput = true;//不重定向输入     
                //startInfo.RedirectStandardOutput = true; //重定向输出     
                //startInfo.CreateNoWindow = true;//不创建窗口     
                //_process.StartInfo = startInfo;
                _process.StartInfo.FileName = doscmdpath;//设定需要执行的命令     
                _process.StartInfo.Arguments = command;//“/C”表示执行完命令后马上退出     
                _process.StartInfo.UseShellExecute = false;//不使用系统外壳程序启动     
                _process.StartInfo.RedirectStandardInput = true;//不重定向输入     
                _process.StartInfo.RedirectStandardOutput = true; //重定向输出     
                _process.StartInfo.RedirectStandardError = true;
                _process.StartInfo.CreateNoWindow = true;//不创建窗口     
                //_process.StartInfo = startInfo;

                try
                {
                    if (_process.Start())//开始进程     
                    {
                        if (seconds == 0)
                        {
                            _process.WaitForExit();//这里无限等待进程结束     
                        }
                        else
                        {
                            _process.WaitForExit(seconds); //等待进程结束，等待时间为指定的毫秒     
                        }
                        //output = _process.StandardOutput.ReadToEnd();//读取进程的输出     
                        output = _process.ExitTime.ToString();
                    }
                }
                catch(Exception err)
                {
                }
                finally
                {
                    if (_process != null)
                        _process.Close();
                }
            }
            return output;
        }


        /// <summary>     
        /// 执行DOS命令，返回DOS命令的输出     
        /// </summary>     
        /// <param name="dosCommand">dos命令</param>     
        /// <param name="milliseconds">等待命令执行的时间（单位：毫秒），     
        /// 如果设定为0，则无限等待</param>     
        /// <returns>返回DOS命令的输出</returns>     
        public static string ExecuteLi(string command, string doscmdpath, int seconds)
        {
            
            string output = ""; //输出字符串     
            if (command != null && !command.Equals(""))
            {
                Process _process = new Process();//创建进程对象     
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = doscmdpath;//设定需要执行的命令   // 初始化可执行文件名"可执行文件名称.后缀";   
 
                // 当我们需要给可执行文件传入参数时候可以设置这个参数
                // "para1 para2 para3" 参数为字符串形式，每一个参数用空格隔开
                startInfo.Arguments = command;//“/C”表示执行完命令后马上退出     
                startInfo.UseShellExecute = false;//不使用系统外壳程序启动     
                startInfo.RedirectStandardInput = false;//不重定向输入     
                startInfo.RedirectStandardOutput = true; //重定向输出     
                startInfo.CreateNoWindow = true;//不创建窗口     
                _process.StartInfo = startInfo;
                //_process.StartInfo = startInfo;

                try
                {
                    if (_process.Start())//开始进程     
                    {
                        if (seconds == 0)
                        {
                            _process.WaitForExit();//这里无限等待进程结束     
                        }
                        else
                        {
                            _process.WaitForExit(seconds); //等待进程结束，等待时间为指定的毫秒     
                        }
                        output = _process.StandardOutput.ReadToEnd();//读取进程的输出     
                    }
                }
                catch (Exception err)
                {
                }
                finally
                {
                    if (_process != null)
                        _process.Close();
                }
            }
            return output;
        }

        public static string ExcuteExeFile(string command, string doscmdpath, int seconds)
        {
            string output = ""; //输出字符串     
            if (command != null && !command.Equals(""))
            {
                Process _process = new Process();//创建进程对象     
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = doscmdpath;//设定需要执行的命令   // 初始化可执行文件名"可执行文件名称.后缀";   

                // 当我们需要给可执行文件传入参数时候可以设置这个参数
                // "para1 para2 para3" 参数为字符串形式，每一个参数用空格隔开
                startInfo.Arguments = command;//“/C”表示执行完命令后马上退出     
                startInfo.UseShellExecute = false;//不使用系统外壳程序启动     
                startInfo.RedirectStandardInput = false;//不重定向输入     
                startInfo.RedirectStandardOutput = true; //重定向输出     
                startInfo.CreateNoWindow = true;//不创建窗口     
                _process.StartInfo = startInfo;
                //_process.StartInfo = startInfo;

                try
                {
                    if (_process.Start())//开始进程     
                    {
                        if (seconds == 0)
                        {
                            _process.WaitForExit();//这里无限等待进程结束     
                        }
                        else
                        {
                            _process.WaitForExit(seconds); //等待进程结束，等待时间为指定的毫秒     
                        }
                        output = _process.StandardOutput.ReadToEnd();//读取进程的输出     
                        _process.Close();
                    }
                }
                catch (Exception err)
                {
                }
                finally
                {
                    if (_process != null)
                        _process.Close();
                }

            }
            return output;
            ////process用于调用外部程序
            //System.Diagnostics.Process p = new System.Diagnostics.Process();
            //p.StartInfo.WorkingDirectory = workingpath;
            //////调用cmd.exe
            //p.StartInfo.FileName = exefile;
            ////是否指定操作系统外壳进程启动程序
            //p.StartInfo.UseShellExecute = false;
            ////可能接受来自调用程序的输入信息
            ////重定向标准输入
            //p.StartInfo.RedirectStandardInput = true;
            ////重定向标准输出
            //p.StartInfo.RedirectStandardOutput = true;
            ////重定向错误输出
            //p.StartInfo.RedirectStandardError = true;
            ////不显示程序窗口
            ////p.StartInfo.CreateNoWindow = true;
            ////启动程序
            //p.Start();
            ////睡眠1s。
            //System.Threading.Thread.Sleep(1000);
            ////输入命令
            ////p.StandardInput.WriteLine(str);
            ////p.StandardInput.WriteLine(str);
            //p.StandardInput.AutoFlush = true;
            ////一定要关闭。
            //p.StandardInput.WriteLine("exit");
            //StreamReader reader = p.StandardOutput;//截取输出流

            //string output = reader.ReadLine();//每次读取一行

            //while (!reader.EndOfStream)
            //{
            //    output = reader.ReadLine();
            //}

            //p.WaitForExit();
        }

        /// <summary>
        /// 调用命令行执行函数
        /// </summary>
        /// <param name="str">执行命令字符串</param>
        private void create(string str)
        {
            //process用于调用外部程序
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            //调用cmd.exe
            p.StartInfo.FileName = "cmd.exe";
            //是否指定操作系统外壳进程启动程序
            p.StartInfo.UseShellExecute = false;
            //可能接受来自调用程序的输入信息
            //重定向标准输入
            p.StartInfo.RedirectStandardInput = true;
            //重定向标准输出
            p.StartInfo.RedirectStandardOutput = true;
            //重定向错误输出
            p.StartInfo.RedirectStandardError = true;
            //不显示程序窗口
            //p.StartInfo.CreateNoWindow = true;
            //启动程序
            p.Start();
            //睡眠1s。
            System.Threading.Thread.Sleep(1000);
            //输入命令
            p.StandardInput.WriteLine(str);
            p.StandardInput.WriteLine(str);
            p.StandardInput.AutoFlush = true;
            //一定要关闭。
            p.StandardInput.WriteLine("exit");
            StreamReader reader = p.StandardOutput;//截取输出流

            string output = reader.ReadLine();//每次读取一行

            while (!reader.EndOfStream)
            {
                output = reader.ReadLine();
            }

            p.WaitForExit();
        }

        /// <summary>
        /// 从可持续评价输出文件中获得到一个可持续发展指数
        /// </summary>
        /// <param name="str">可持续指数发展字符串</param>
        /// <returns></returns>
        public static string[] GetSDGIndex(string str, int simyear, char Sepsign)
        {
            string[] _result = new string[simyear];
            // 提取出数据段
            int _spoi = str.IndexOf('[');
            int _epoi = str.IndexOf(']');
            string _data = str.Substring(_spoi + 1, _epoi - _spoi - 1);
            string[] _temp = _data.Split(Sepsign);
            int _count = 0;

            for (int i = 0; i < _temp.Length; i++)
            {
                if (_temp[i] != "")
                {
                    _result[_count] = _temp[i];
                    _count++;
                }
            }
            return _result;
        }

        /// <summary>
        /// 利用Okun定律计算就业率的变化率，%
        /// </summary>
        /// <param name="GDPrate">GDP变化</param>
        /// <param name="threshold">阈值参数(小数），美国3%，中国1%</param>
        /// <returns></returns>
        public static double OkunLawEmp(double GDPrate, double threshold)
        {
            return (-0.5) * (GDPrate - threshold) * 100.0 * -1;
        }

        /// <summary>
        /// 多年增长率转换为年平均增长率
        /// </summary>
        /// <param name="x">多年增长率，如10年增长率10%,(%)</param>
        /// <param name="ynum">10年</param>
        /// <returns></returns>
        public static double MulyinctoAaverageInc(double x, double ynum)
        {
            return Math.Pow(1 + x / 100.0, 1 / ynum) - 1;
        }

        /// <summary>
        /// 亩转换为公顷
        /// </summary>
        /// <param name="mu"></param>
        /// <returns></returns>
        public static double MutoHa(double mu)
        {
            return mu / 15;
        }
        /// <summary>
        /// 公顷转换为亩
        /// </summary>
        /// <param name="ha"></param>
        /// <returns></returns>
        public static double HatoMu(double ha)
        {
            return ha * 15;
        }

        /// <summary>
        /// 公顷转换为平方米
        /// </summary>
        /// <param name="ha"></param>
        /// <returns></returns>
        public static double HatoSqMeter(double ha)
        {
            return ha * 10000.0;
        }

        /// <summary>
        /// 平方米转换为公顷
        /// </summary>
        /// <param name="sqmeter"></param>
        /// <returns></returns>
        public static double SqMetertoHa(double sqmeter)
        {
            return sqmeter / 10000.0;
        }
        /// <summary>
        /// 开氏度转换为摄氏度
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public static double KtoC(double k)
        {
            return k - 273.15;
        }
        /// <summary>
        /// 摄氏度转换为开氏度
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double CtoK(double c)
        {
            return c + 273.15;
        }
    }

}
