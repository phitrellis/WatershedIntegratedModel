using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WatershedIntegratedModel
{
    public class Ecohydro_socioeco_Integrated_model
    {
        private string mCurrpath;
        private int mParaNum;
        private GlobalFunctions mGFunc;
        private IndicatorsModels mIndiModels;
        private EcohydrologyModel mUpEcoHydro;
        private double[,] mIndiWS;          // water stress (0: county;1:year)
        private double[,] mIndiWP;          // water productivity
        private double[,] mIndiAWP;         // agricultural water productivity
        private double[,] mIndiAWUE;        // 农业水利用效率
        private double[,] mIndiGDPpc;       // GDP per capita
        private double[,] mIndiGDPpe;       // GDP per employee
        private double[,] mIndiFCR;         // Forest cover rate
        private double[,] mIndiGCI;         // Green cover index
        private double[,] mIndiGrassCR;     // 草地覆盖率
        private double[,] mIndiGradArea;    // 退化土地面积
        private double[,] mIndiUR;          // Urbanization rate
        private double[,] mIndiFLA;         // Farmland area
        private double[,] mIndiGW;          // Groundwater withdrawals
        private double[,] mIndiDrinkWater;  // 安全饮用水人口比例

        private double[] mYingluoxia;       // Yingluoxia flow
        private double[] mOtherRivers;      // Ohter rivers flow
        private int mCountyNum;
        private int mSimlong;
        private int mWatershedArea;         // 流域面积，ha
        private string[,] mWaterCoefficient;            // 水系数，经济用水系数，农业用水系数
        private string[,] mIntegModelOut;       // 集成模型输出结果：项目名称，年份，县区，莺落峡径流量（亿立方米），其他河流径流量（亿立方米），
        // 地表引水量（亿立方米），地下引水量（亿立方），耕地面积ET（亿立方米），农业产值变化率（%），
        // 工业产值变化率（%），服务业产值变化率（%），农业用地面积变化率（%），工业用地面积变化率（%），
        // 服务业用地面积变化率（%），水价变化率（%)，就业率变化率（%），地表水需水量变化率（%），
        // 地下水需水量变化率（%），非农业需水变化率（%），农业产值（亿元），服务业产值（亿元），
        // 农业用地面积（公顷），工业用地面积(公顷），服务业用地面积(公顷），水价（元/立方米），
        // 就业率（小数），地表需水量（亿立方米），非农业需水量（亿立方），人口总数（万人），
        // 城镇人口数量（万人），城市化率（%），温度（），降水（cm），林地面积（ha），草地面积（ha)，
        // 湿地面积（ha),退化土地面积（ha）
        // [模拟时段长度*县区个数,输出变量个数]=[mSiglong*11,37]
        private double[,,] mEcoModelChgRate;       // 经济模型输出的变化率数据（0：yr，1：county；2：变化率）
        private double[,] mMidRealTemp;             // 中游实际温度
        private double[,] mMidRealpric;             // 中游实际的降水
        private string[,] mSDGsOutput;            // 可持续发展目标输出

        public Ecohydro_socioeco_Integrated_model()
        {
            InitGlobalVars();
            InitBaseValue();
            InitWateruseCoeff();
        }


        /// <summary>
        /// 初始化simulate中的全局变量
        /// </summary>
        private void InitGlobalVars()
        {
            mCurrpath = System.AppDomain.CurrentDomain.BaseDirectory;
            mIndiModels = new IndicatorsModels();
            mGFunc = new GlobalFunctions();
            mUpEcoHydro = new EcohydrologyModel();

            mCountyNum = GlobalVars.CountyName.Length - 1;
            mSimlong = GlobalVars.SimEndYear - GlobalVars.SimStartYear + 1;

            mYingluoxia = new double[mSimlong];
            mOtherRivers = new double[mSimlong];

            mIntegModelOut = new string[mSimlong * GlobalVars.CountyName.Length + 1, 38];
            mEcoModelChgRate = new double[mSimlong, GlobalVars.CountyName.Length, 11];
            mMidRealpric = new double[mSimlong, GlobalVars.CountyName.Length];
            mMidRealTemp = new double[mSimlong, GlobalVars.CountyName.Length];
            mSDGsOutput = new string[mSimlong * GlobalVars.CountyName.Length + 1, 16];
        }

        /// <summary>
        /// 初始化上游水文模型基准值列表
        /// </summary>
        private void InitBaseValue()
        {
            try
            {
                // Base value of midreach ecohydrological model
                mParaNum = 24;
                string _filename = "", _sheet = "";
                string[,] _data = new string[GlobalVars.CountyName.Length, mParaNum];
                _filename = mCurrpath + "//Configuration files//" + "Basevalue.xls";
                _sheet = "Basevalue";
                object[,] _basevaluedata;
                _basevaluedata = mGFunc.ReadExcelData(_filename, _sheet, mParaNum);
                for (int i = 0; i < GlobalVars.CountyName.Length; i++)
                {
                    for (int j = 0; j < mParaNum; j++)
                    {
                        _data[i, j] = _basevaluedata[i, j].ToString().Trim();
                    }
                }
                GlobalVars.ModelBasicValue = _data;

                // Basevalue of upstream ecohydrological model
                int _mUpparanum = 9;
                _sheet = "Upstream";
                object[,] _upbasev;
                _upbasev = mGFunc.ReadExcelData(_filename, _sheet, _mUpparanum);
                GlobalVars.ThresholdUpPreci = new double[2];
                GlobalVars.ThresholdUpTemp = new double[2];
                GlobalVars.TempBasicValue = new double[2];
                GlobalVars.PreciBasicValue = new double[2];

                GlobalVars.ThresholdUpTemp[0] = double.Parse(_upbasev[1, 1].ToString().Trim());      // 269.0;
                GlobalVars.ThresholdUpTemp[1] = double.Parse(_upbasev[1, 2].ToString().Trim());      // 275;
                GlobalVars.ThresholdUpPreci[0] = double.Parse(_upbasev[1, 3].ToString().Trim());         // 407;
                GlobalVars.ThresholdUpPreci[1] = double.Parse(_upbasev[1, 4].ToString().Trim());     // 1000;
                GlobalVars.TempBasicValue[0] = double.Parse(_upbasev[1, 5].ToString().Trim());       //270.24;     // 干流区
                GlobalVars.TempBasicValue[1] = double.Parse(_upbasev[1, 6].ToString().Trim());       //271.60;
                GlobalVars.PreciBasicValue[0] = double.Parse(_upbasev[1, 7].ToString().Trim());      //669.15;     // 干流区
                GlobalVars.PreciBasicValue[1] = double.Parse(_upbasev[1, 8].ToString().Trim());      //814.35;
            }
            catch (Exception err)
            {
            }
        }

        /// <summary>
        /// 初始化用水比例，包括经济和农业
        /// </summary>
        private void InitWateruseCoeff()
        {
            try
            {
                // economic water use coefficient and agricultural water use coeffiicent
                mParaNum = 5;
                string _filename = "", _sheet = "";
                string[,] _data = new string[GlobalVars.CountyName.Length, mParaNum];
                mWaterCoefficient = new string[GlobalVars.CountyName.Length, mParaNum - 1];
                _filename = mCurrpath + "//Configuration files//" + "WateruseCoeff";
                _sheet = "WateruseCoeff";
                object[,] _basevaluedata;
                _basevaluedata = mGFunc.ReadExcelData(_filename, _sheet, mParaNum);
                for (int i = 0; i < GlobalVars.CountyName.Length; i++)
                {
                    for (int j = 0; j < mParaNum; j++)
                    {
                        _data[i, j] = _basevaluedata[i, j].ToString().Trim();
                        if (j > 0)
                        {
                            mWaterCoefficient[i, j - 1] = _basevaluedata[i, j].ToString().Trim();
                        }
                    }
                }

            }
            catch (Exception err)
            {
            }
        }

        /// <summary>
        /// 读取经济模型输出文件result.xls
        /// </summary>
        /// <param name="fullpath"></param>
        /// <returns>将result文件中的数据输出到三维数组中：0：数据类别（产值，土地，价格等）；1：产业（48个产业）；2：县区</returns>
        private double[,,] ReadResultEcoModel(string fullpath)
        {
            try
            {
                double[,,] _res = new double[6, 48, 10];
                if (System.IO.File.Exists(fullpath))
                {
                    object[,] _temp;
                    // 产值数据
                    _temp = mGFunc.ReadExcelData(fullpath, "XTOT", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[0, i, j] = double.Parse(_temp[8 + i, 2 + j].ToString().Trim());
                            }
                        }
                    }
                    _temp = null;
                    // 土地数据
                    _temp = mGFunc.ReadExcelData(fullpath, "XLND", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[1, i, j] = double.Parse(_temp[8 + i, 2 + j].ToString().Trim());
                            }
                        }
                    }
                    _temp = null;
                    // 水价数据
                    _temp = mGFunc.ReadExcelData(fullpath, "PRWT", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[2, i, j] = double.Parse(_temp[8 + i, 2 + j].ToString().Trim());
                            }
                        }
                    }
                    _temp = null;
                    // 就业率数据
                    _temp = mGFunc.ReadExcelData(fullpath, "XLAB", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[3, i, j] = double.Parse(_temp[8 + i, 2 + j].ToString().Trim());
                            }
                        }
                    }
                    _temp = null;
                    // 地表水数据
                    _temp = mGFunc.ReadExcelData(fullpath, "XSWT", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[4, i, j] = double.Parse(_temp[8 + i, 2 + j].ToString().Trim());
                            }
                        }
                    }
                    _temp = null;
                    // 地下水数据
                    _temp = mGFunc.ReadExcelData(fullpath, "XUWT", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[5, i, j] = double.Parse(_temp[8 + i, 2 + j].ToString().Trim());
                            }
                        }
                    }
                }
                else
                {
                }
                return _res;
            }
            catch (Exception err)
            {
                return null;
            }
        }

        /// <summary>
        /// 读取经济模型输出文件percent.xls
        /// </summary>
        /// <param name="fullpath"></param>
        /// <returns>将percent文件中的数据输出到三维数组中：0：数据类别（产值，土地，价格等）；1：产业（48个产业）；2：县区</returns>
        private double[,,] ReadPercentEcoModel(string fullpath)
        {
            try
            {
                double[,,] _res = new double[6, 48, 10];
                if (System.IO.File.Exists(fullpath))
                {
                    object[,] _temp;
                    // 产值数据
                    _temp = mGFunc.ReadExcelData(fullpath, "产值", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[0, i, j] = double.Parse(_temp[1 + i, 1 + j].ToString().Trim());
                            }
                        }
                    }
                    _temp = null;
                    // 土地数据
                    _temp = mGFunc.ReadExcelData(fullpath, "土地租赁", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[1, i, j] = double.Parse(_temp[1 + i, 1 + j].ToString().Trim());
                            }
                        }
                    }
                    _temp = null;
                    // 水价数据
                    _temp = mGFunc.ReadExcelData(fullpath, "水价", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[2, i, j] = double.Parse(_temp[1 + i, 1 + j].ToString().Trim());
                            }
                        }
                    }
                    _temp = null;
                    // 就业率数据
                    _temp = mGFunc.ReadExcelData(fullpath, "就业", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[3, i, j] = double.Parse(_temp[1 + i, 1 + j].ToString().Trim());
                            }
                        }
                    }
                    _temp = null;
                    // 地表水数据
                    _temp = mGFunc.ReadExcelData(fullpath, "地表水", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[4, i, j] = double.Parse(_temp[1 + i, 1 + j].ToString().Trim());
                            }
                        }
                    }
                    _temp = null;
                    // 地下水数据
                    _temp = mGFunc.ReadExcelData(fullpath, "地下水", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[5, i, j] = double.Parse(_temp[1 + i, 1 + j].ToString().Trim());
                            }
                        }
                    }
                }
                else
                {
                }
                return _res;
            }
            catch (Exception err)
            {
                return null;
            }
        }

        /// <summary>
        /// 生态水文-社会经济集成模型
        /// </summary>
        /// <param name="climateScena">气候情景</param>
        /// <param name="landScena">土地利用情景</param>
        /// <param name="socioScena">社会情景</param>
        /// <param name="governScena">政府管理情景</param>
        public void Ecohrdo_Socioeco_IntegratedModel(string[,] climateScena, string[,] landScena, string[,] socioScena, string[,] governScena)
        {
        }


    }
}
