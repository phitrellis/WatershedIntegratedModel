using System;
using System.IO;

namespace WatershedIntegratedModel
{
    class Program
    {
        static GlobalFunctions mGFunc = new GlobalFunctions();
        private static void InitGlobalVar()
        {
            // 上游生态水文模型所需参数
            GlobalVars.ThresholdUpTemp[0] = 269.0;
            GlobalVars.ThresholdUpTemp[1] = 275;
            GlobalVars.ThresholdUpPreci[0] = 407;
            GlobalVars.ThresholdUpPreci[1] = 1000;
            GlobalVars.TempBasicValue[0] = 270.24;     // 干流区
            GlobalVars.TempBasicValue[1] = 271.60;
            GlobalVars.PreciBasicValue[0] = 669.15;     // 干流区
            GlobalVars.PreciBasicValue[1] = 814.35;

        }

        private static void readProjectInfo(string _subfolder)
        {
            try
            {
                // 获取被打开项目所在的目录
                string _path = System.AppDomain.CurrentDomain.BaseDirectory;
                _path = System.IO.Path.Combine(_path, _subfolder);
                // 存储当前项目目录到全局变量ProgramtDirectory
                GlobalVars.ProgramDirectory = _path;
                System.Collections.ArrayList _projectinfo = new System.Collections.ArrayList();
                // 打开项目信息文件，并写入相应位置
                string _filename = _subfolder + ".CSV";
                string _fullfilebasicinfo = "";
                _fullfilebasicinfo = System.IO.Path.Combine(_path, _filename);
                _projectinfo = GlobalFunctions.ReadDataCSV(_fullfilebasicinfo, "没有找到该项目的信息文件！", "读取项目信息错误");

                if (_projectinfo != null)
                {
                    // 写入项目信息到页面
                    var userName = _projectinfo[0].ToString().Substring(_projectinfo[0].ToString().IndexOf(",") + 1);
                    var ProgramName = _projectinfo[1].ToString().Substring(_projectinfo[1].ToString().IndexOf(",") + 1);
                    var ProjectDes = _projectinfo[2].ToString().Substring(_projectinfo[2].ToString().IndexOf(",") + 1);
                    var dateProject = _projectinfo[5].ToString().Substring(_projectinfo[5].ToString().IndexOf(",") + 1);

                    GlobalVars.SimStartYear = System.Int16.Parse(_projectinfo[3].ToString().Substring(_projectinfo[3].ToString().IndexOf(",") + 1).Trim());
                    GlobalVars.SimEndYear = System.Int16.Parse(_projectinfo[4].ToString().Substring(_projectinfo[4].ToString().IndexOf(",") + 1).Trim());

                    // 打开SDGs指标文件，并写入全局变量SDGs中
                    string _fullfileSDGs = "";
                    _filename = "SDGs.CSV";
                    _fullfileSDGs = System.IO.Path.Combine(_path, _filename);
                    _projectinfo = GlobalFunctions.ReadDataCSV(_fullfileSDGs, "没有找到SDGs文件！", "读取SDGs文件错误");
                    string[,] _sdgsinfo = new string[_projectinfo.Count, 5];              // 0:目标等级；1：目标描述；2：check类型；3：weight
                    System.Collections.ArrayList _goalweight = new System.Collections.ArrayList(); ;
                    for (int i = 0; i < _projectinfo.Count; i++)
                    {
                        string[] _stemp = GlobalFunctions.ReadListofSynergy(_projectinfo[i].ToString(), 5, "#");
                        for (int j = 0; j < _stemp.Length; j++)
                        {
                            _sdgsinfo[i, j] = _stemp[j];
                        }
                        if (_sdgsinfo[i, 0] == "1")
                        {
                            _goalweight.Add(_sdgsinfo[i, 3]);
                        }
                    }
                    GlobalVars.SDGs = _sdgsinfo;
                    if (_sdgsinfo[_sdgsinfo.GetLength(0) - 1, 0] == "主观赋权")
                    {
                        GlobalVars.WeightType = 0;
                    }
                    else if (_sdgsinfo[_sdgsinfo.GetLength(0) - 1, 0] == "客观赋权")
                    {
                        GlobalVars.WeightType = 1;
                    }
                    GlobalVars.GoalWeight = new double[_goalweight.Count];
                    for (int i = 0; i < _goalweight.Count; i++)
                    {
                        GlobalVars.GoalWeight[i] = System.Double.Parse(_goalweight[i].ToString().Trim());
                    }
                    //-------------------- 打开情景设置文件，并将情景参数写入情景全局变量Climate,Land,Socioeconomic,Government
                    string _scenarioLocat = "";
                    System.Object[,] _scenarioInfo;
                    _filename = "scenariopara.xls";
                    //_path = System.IO.Directory.GetCurrentDirectory() + "\\Configuration files\\";
                    _scenarioLocat = System.IO.Path.Combine(_path, _filename);
                    // cliamte scenario
                    _scenarioInfo = mGFunc.ReadExcelData(_scenarioLocat, "climate", 2);       // "没有找到情景配置文件！", "读取情景参数配置文件错误");
                    string[,] _climatedata = new string[GlobalVars.CountyName.Length, _scenarioInfo.GetLength(1) + 1];
                    for (int i = 0; i < GlobalVars.CountyName.Length; i++)
                    {
                        _climatedata[i, 0] = GlobalVars.CountyName[i];
                        for (int j = 0; j < _scenarioInfo.GetLength(1); j++)
                        {
                            _climatedata[i, j + 1] = _scenarioInfo[i, j].ToString().Trim();
                        }
                    }
                    GlobalVars.ClimateScenario = _climatedata;
                    // Land scenario
                    _scenarioInfo = mGFunc.ReadExcelData(_scenarioLocat, "Land", 4);       // "没有找到情景配置文件！", "读取情景参数配置文件错误");
                    _climatedata = new string[GlobalVars.CountyName.Length, _scenarioInfo.GetLength(1) + 1];
                    for (int i = 0; i < GlobalVars.CountyName.Length; i++)
                    {
                        _climatedata[i, 0] = GlobalVars.CountyName[i];
                        for (int j = 0; j < _scenarioInfo.GetLength(1); j++)
                        {
                            _climatedata[i, j + 1] = _scenarioInfo[i, j].ToString().Trim();
                        }
                    }
                    GlobalVars.LandScenario = _climatedata;
                    // Economic scenario
                    _scenarioInfo = mGFunc.ReadExcelData(_scenarioLocat, "Economic", 7);       // "没有找到情景配置文件！", "读取情景参数配置文件错误");
                    _climatedata = new string[GlobalVars.CountyName.Length, _scenarioInfo.GetLength(1) + 1];
                    for (int i = 0; i < GlobalVars.CountyName.Length; i++)
                    {
                        _climatedata[i, 0] = GlobalVars.CountyName[i];
                        for (int j = 0; j < _scenarioInfo.GetLength(1); j++)
                        {
                            _climatedata[i, j + 1] = _scenarioInfo[i, j].ToString().Trim();
                        }
                    }
                    GlobalVars.EconomicScenario = _climatedata;
                    // Government
                    _scenarioInfo = mGFunc.ReadExcelData(_scenarioLocat, "Government", 3);       // "没有找到情景配置文件！", "读取情景参数配置文件错误");
                    _climatedata = new string[GlobalVars.CountyName.Length, _scenarioInfo.GetLength(1) + 1];
                    for (int i = 0; i < GlobalVars.CountyName.Length; i++)
                    {
                        _climatedata[i, 0] = GlobalVars.CountyName[i];
                        for (int j = 0; j < _scenarioInfo.GetLength(1); j++)
                        {
                            _climatedata[i, j + 1] = _scenarioInfo[i, j].ToString().Trim();
                        }
                    }
                    GlobalVars.GovernScenario = _climatedata;


                }
            }
            catch (Exception err)
            {
                
            }
        }

        static void Main(string[] args)
        {
            //InitGlobalVar();
            if (args.Length > 0)
            {
                var project = args[0];
                Console.WriteLine(project);
                readProjectInfo(project);  // "SSP1 - Max"
                var sim = new Simulation();
                sim.Simulate();
                var statusPath = ".status";
                if (File.Exists(statusPath))
                    File.Delete(statusPath);
            }
        }
    }
}
