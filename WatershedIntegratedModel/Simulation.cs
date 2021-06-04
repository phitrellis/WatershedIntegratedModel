using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace WatershedIntegratedModel
{
    public partial class Simulation
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
        private double mWatershedArea;         // 流域面积，ha
        private string[,] mWaterCoefficient;            // 水系数，经济用水系数，农业用水系数
        private string[,] mIntegModelOut;       // 集成模型输出结果：项目名称，年份，县区，莺落峡径流量（亿立方米），其他河流径流量（亿立方米），
                                                // 地表引水量（亿立方米），地下引水量（亿立方），耕地面积ET（亿立方米），农业产值变化率（%），
                                                // 工业产值变化率（%），服务业产值变化率（%），农业用地面积变化率（%），工业用地面积变化率（%），
                                                // 服务业用地面积变化率（%），水价变化率（%)，就业率变化率（%），地表水需水量变化率（%），
                                                // 地下水需水量变化率（%），非农业需水变化率（%），农业产值（亿元），服务业产值（亿元），
                                                // 农业用地面积（公顷），工业用地面积(公顷），服务业用地面积(公顷），水价（元/立方米），
                                                // 就业率（小数），地表需水量（亿立方米），非农业需水量（亿立方），人口总数（万人），
                                                // 城镇人口数量（万人），城市化率（%），温度（），降水（cm），林地面积（ha），草地面积（ha)，
                                                // 湿地面积（ha),退化土地面积（ha）,GDP
                                                // [模拟时段长度*县区个数,输出变量个数]=[mSiglong*11,37]
        private double[, ,] mEcoModelChgRate;       // 经济模型输出的变化率数据（0：yr，1：county；2：变化率）
        private double[,] mMidRealTemp;             // 中游实际温度
        private double[,] mMidRealpric;             // 中游实际的降水
        private string[,] mSDGsOutput;            // 可持续发展目标输出
        private int mModelOuputVarsNum;         // 模型输出变量的个数

        public Simulation()
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
            mModelOuputVarsNum = 39;
            mCurrpath = System.AppDomain.CurrentDomain.BaseDirectory;
            mIndiModels = new IndicatorsModels();
            mGFunc = new GlobalFunctions();
            mUpEcoHydro = new EcohydrologyModel();

            mCountyNum = GlobalVars.CountyName.Length - 1;
            mSimlong = GlobalVars.SimEndYear - GlobalVars.SimStartYear + 1;

            mYingluoxia = new double[mSimlong];
            mOtherRivers = new double[mSimlong];

            mIntegModelOut = new string[mSimlong * GlobalVars.CountyName.Length + 1, mModelOuputVarsNum];
            mEcoModelChgRate = new double[mSimlong, GlobalVars.CountyName.Length, 11];
            mMidRealpric = new double[mSimlong, GlobalVars.CountyName.Length];
            mMidRealTemp = new double[mSimlong, GlobalVars.CountyName.Length];
            mSDGsOutput = new string[mSimlong * GlobalVars.CountyName.Length + 1, 16];
        }

        /// <summary>
        /// 初始化基准值列表
        /// </summary>
        private void InitBaseValue()
        {
            try
            {

                // Base value of midreach ecohydrological model
                mParaNum = 24;
                string _filename = "", _sheet = "";
                string[,] _data = new string[GlobalVars.CountyName.Length, mParaNum];
                _filename = mCurrpath + "\\Configuration files\\" + "Basevalue.xls";
                _sheet = "Basevalue";
                System.Object[,] _basevaluedata;
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
                System.Object[,] _upbasev;
                _upbasev = mGFunc.ReadExcelData(_filename, _sheet, _mUpparanum);
                string _show = _upbasev[0, 0].ToString() + "\t" + _upbasev[0, 1].ToString() + "\t" +
                                _upbasev[0, 2].ToString() + "\t" + _upbasev[0, 3].ToString() + "\t" +
                                _upbasev[0, 4].ToString() + "\t" + _upbasev[0, 5].ToString() + "\t" +
                                _upbasev[0, 6].ToString() + "\t" + _upbasev[0, 7].ToString() + "\r\n" +
                                _upbasev[1, 0].ToString() + "\t" + _upbasev[1, 1].ToString() + "\t\t" +
                                _upbasev[1, 2].ToString() + "\t\t" + _upbasev[1, 3].ToString() + "\t\t" +
                                _upbasev[1, 4].ToString() + "\t\t" + _upbasev[1, 5].ToString() + "\t\t\t" +
                                _upbasev[1, 6].ToString() + "\t\t\t" + _upbasev[1, 7].ToString();
                GlobalVars.ThresholdUpPreci = new double[2];
                GlobalVars.ThresholdUpTemp = new double[2];
                GlobalVars.TempBasicValue = new double[2];
                GlobalVars.PreciBasicValue = new double[2];

                GlobalVars.ThresholdUpTemp[0] = System.Double.Parse(_upbasev[1, 1].ToString().Trim());      // 269.0;
                GlobalVars.ThresholdUpTemp[1] = System.Double.Parse(_upbasev[1, 2].ToString().Trim());      // 275;
                GlobalVars.ThresholdUpPreci[0] = System.Double.Parse(_upbasev[1, 3].ToString().Trim());         // 407;
                GlobalVars.ThresholdUpPreci[1] = System.Double.Parse(_upbasev[1, 4].ToString().Trim());     // 1000;
                GlobalVars.TempBasicValue[0] = System.Double.Parse(_upbasev[1, 5].ToString().Trim());       //270.24;     // 干流区
                GlobalVars.TempBasicValue[1] = System.Double.Parse(_upbasev[1, 6].ToString().Trim());       //271.60;
                GlobalVars.PreciBasicValue[0] = System.Double.Parse(_upbasev[1, 7].ToString().Trim());      //669.15;     // 干流区
                GlobalVars.PreciBasicValue[1] = System.Double.Parse(_upbasev[1, 8].ToString().Trim());      //814.35;



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
                System.Object[,] _basevaluedata;
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
                Console.WriteLine(err.Message, "Initialize water use coefficient");
            }
        }


        /// <summary>
        /// 运行集成模型,需要初始化的参数包括：
        /// 经济模型路径:_economicPath
        /// 模拟时长：   mSimlong
        /// 
        /// </summary>
        public void Simulate()
        {
            try
            {
                string _economicPath = System.AppDomain.CurrentDomain.BaseDirectory + "\\Economic model\\";
                string _ecomodelfilename = "term.cmf";
                string _ecomodelfile = "term";
                string _ecomodelCMD = "";
                string _ecomodelResult = "";

                mIndiAWP = new double[GlobalVars.CountyName.Length, mSimlong];
                mIndiFCR = new double[GlobalVars.CountyName.Length, mSimlong];
                mIndiFLA = new double[GlobalVars.CountyName.Length, mSimlong];
                mIndiGCI = new double[GlobalVars.CountyName.Length, mSimlong];
                mIndiGDPpc = new double[GlobalVars.CountyName.Length, mSimlong];
                mIndiGDPpe = new double[GlobalVars.CountyName.Length, mSimlong];
                mIndiGW = new double[GlobalVars.CountyName.Length, mSimlong];
                mIndiUR = new double[GlobalVars.CountyName.Length, mSimlong];
                mIndiWP = new double[GlobalVars.CountyName.Length, mSimlong];
                mIndiWS = new double[GlobalVars.CountyName.Length, mSimlong];
                mIndiDrinkWater = new double[GlobalVars.CountyName.Length, mSimlong];
                mIndiGradArea = new double[GlobalVars.CountyName.Length, mSimlong];
                mIndiGrassCR = new double[GlobalVars.CountyName.Length, mSimlong];
                mIndiAWUE = new double[GlobalVars.CountyName.Length, mSimlong];

                                // -----  定义用于计算流域可持续发展指标的变量，累计各县区值  --------------
                double[] _rbGDP = new double[mSimlong];                              // 流域GDP
                double[] _rbSurWS = new double[mSimlong];                            // 流域地表水经济供水
                double[] _rbGrdWS = new double[mSimlong];                            // 流域地下水经济供水
                double[] _rbSurWD = new double[mSimlong];                            // 流域地表水经济需水
                double[] _rbGrdWD = new double[mSimlong];                            // 流域地下水经济需水
                double[] _rbArgSurWS = new double[mSimlong];                         // 流域农业地表水经济供水
                double[] _rbArgGrdWS = new double[mSimlong];                         // 流域农业地下水经济供水
                double[] _rbArgSurWD = new double[mSimlong];                         // 流域农业地表水经济需水
                double[] _rbArgGrdWD = new double[mSimlong];                         // 流域农业地下水经济需水
                double[] _rbSafeWPop = new double[mSimlong];                         // 流域饮用安全水人口数量
                double[] _rbTotArgET = new double[mSimlong];                            // 流域ET量
                double[] _rbFstArea = new double[mSimlong];                          // 流域森林面积
                double[] _rbGrsArea = new double[mSimlong];                          // 流域草地面积
                double[] _rbFLA = new double[mSimlong];                              // 流域耕地面积
                double[] _rbPop = new double[mSimlong];                              // 流域人口数量
                double[] _rbEmpPop = new double[mSimlong];                           // 流域就业人口数量
                double[] _rbUrbPop = new double[mSimlong];                           // 流域城镇人口数量
                double[] _rbArgValue = new double[mSimlong];                         // 流域农业产值
                double[] _rbArgET = new double[mSimlong];                            // 流域农业总ET量
                double _rbTotalArea = 0;                                            // 流域总面积
                double[] _rbIndValue = new double[mSimlong];                         // 流域工业产值
                double[] _rbSerValue = new double[mSimlong];                         // 流域服务业产值
                double[] _rbDegradedArea = new double[mSimlong];                     // 流域退化土地面积


                double _demandwater = 0, _supplywater = 0;
                double _GDP = 0, _surwater = 0, _groundwater = 0;
                // 模型计算开始时间
                string _wasttime = "Please waiting....\r\nStart time:" + System.DateTime.Now.ToString();
                Console.WriteLine(_wasttime);

                //----------- calculate parameters used in eco-hydrological model of upstream  ------------
                double[,] _upTemp = new double[mSimlong, 2];            // temperature change rate of upstream, 0:干流区；1：非干流区
                double[,] _upPreci = new double[mSimlong, 2];           // precipitation change rate of upstream
                // 计算出干流区和非干流区温度的年平均增长率，转换为摄氏度计算
                //double _upAveTemp = System.Double.Parse(GlobalVars.ClimateScenario[GlobalVars.CountyName.Length - 1, 1].Trim()) / (double)mSimlong;
                double _upAveTempYLX, _upAveTempOther;
                double _orgTempYLX = 0;
                double _startTempYLX = GlobalFunctions.KtoC(GlobalVars.TempBasicValue[0]);
                double _endTempYLX = GlobalFunctions.KtoC(GlobalVars.TempBasicValue[0]) + 
                    System.Double.Parse(GlobalVars.ClimateScenario[GlobalVars.CountyName.Length - 1, 1].Trim());
                if (_startTempYLX < 0.0)
                {
                    _orgTempYLX = _startTempYLX;
                    _startTempYLX = 1.0;
                    _endTempYLX = _startTempYLX + _orgTempYLX * -1 + _endTempYLX;
                }
                _upAveTempYLX = Math.Pow(_endTempYLX / _startTempYLX, 1.0 / (double)mSimlong) - 1.0;

                double _orgTempOther = 0;
                double _startTempOther = GlobalFunctions.KtoC(GlobalVars.TempBasicValue[1]);
                double _endTempOther = GlobalFunctions.KtoC(GlobalVars.TempBasicValue[1]) + 
                    System.Double.Parse(GlobalVars.ClimateScenario[GlobalVars.CountyName.Length - 1, 1].Trim());
                if (_startTempOther < 0.0)
                {
                    _orgTempOther = _startTempOther;
                    _startTempOther = 1.0;
                    _endTempOther = _startTempOther + _orgTempOther * -1 + _endTempOther;
                }
                _upAveTempOther = Math.Pow(_endTempOther / _startTempOther, 1.0 / mSimlong) - 1.0;

                // 计算出干流区和非干流区降水的年平均增长率
                //double _upAvePrici = GlobalFunctions.MulyinctoAaverageInc(System.Double.Parse(GlobalVars.ClimateScenario[GlobalVars.CountyName.Length - 1, 2].Trim()), mSimlong);
                double _upAvePreciYLX, _upAvePreciOther;
                double _starPreciYLX = GlobalVars.PreciBasicValue[0];
                double _endPreciYLX = GlobalVars.PreciBasicValue[0] * 
                    (1 + System.Double.Parse(GlobalVars.ClimateScenario[GlobalVars.CountyName.Length - 1, 2].Trim()) / 100.0);          // 将%转换为小数
                _upAvePreciYLX = Math.Pow(_endPreciYLX / _starPreciYLX, 1.0 / mSimlong) - 1.0;

                double _starPreciOther = GlobalVars.PreciBasicValue[1];
                double _endPreciOther = GlobalVars.PreciBasicValue[1] *
                    (1 + System.Double.Parse(GlobalVars.ClimateScenario[GlobalVars.CountyName.Length - 1, 2].Trim()) / 100.0);          // 将%转换为小数
                _upAvePreciOther = Math.Pow(_endPreciOther / _starPreciOther, 1.0 / mSimlong) - 1.0;
                //System.Random _random = new Random();
                for (int yr = 0; yr < mSimlong; yr++)
                {
                    // 计算干流区温度年变化率,并转变为温度,
                    _upTemp[yr, 0] = _orgTempYLX * Math.Pow((1 + _upAveTempYLX), yr);     // * (_random.NextDouble() * 0.5));
                    _upTemp[yr, 0] = GlobalFunctions.CtoK(_upTemp[yr, 0]);
                    // temperature threshold limitation
                    if (_upTemp[yr, 0] < GlobalVars.ThresholdUpTemp[0])
                    {
                        _upTemp[yr, 0] = GlobalVars.ThresholdUpTemp[0];
                    }
                    if (_upTemp[yr, 0] > GlobalVars.ThresholdUpTemp[1])
                    {
                        _upTemp[yr, 0] = GlobalVars.ThresholdUpTemp[1];
                    }
                    // 计算非干流区温度
                    _upTemp[yr, 1] = _orgTempOther * Math.Pow((1 + _upAveTempOther),yr);          // * (_random.NextDouble() * 0.5));
                    _upTemp[yr, 1] = GlobalFunctions.CtoK(_upTemp[yr, 1]);
                    // temperature threshold limitation
                    if (_upTemp[yr, 1] < GlobalVars.ThresholdUpTemp[0])
                    {
                        _upTemp[yr, 1] = GlobalVars.ThresholdUpTemp[0];
                    }
                    if (_upTemp[yr, 1] > GlobalVars.ThresholdUpTemp[1])
                    {
                        _upTemp[yr, 1] = GlobalVars.ThresholdUpTemp[1];
                    }

                    // 计算干流区降水量
                    //if (yr == 0)
                    //{
                    _upPreci[yr, 0] = GlobalVars.PreciBasicValue[0] * Math.Pow((1 + _upAvePreciYLX), yr);       // (_random.NextDouble() * 0.5));
                    //}
                    //else
                    //{
                    //    _upPreci[yr, 0] = _upPreci[yr - 1, 0] * (1 + _upAvePrici * (_random.NextDouble() * 0.5));
                    //}
                    if (_upPreci[yr, 0] < GlobalVars.ThresholdUpPreci[0])
                    {
                        _upPreci[yr, 0] = GlobalVars.ThresholdUpPreci[0];
                    }
                    if (_upPreci[yr, 0] > GlobalVars.ThresholdUpPreci[1])
                    {
                        _upPreci[yr, 0] = GlobalVars.ThresholdUpPreci[1];
                    }
                    // 计算非干流区降水量
                    _upPreci[yr, 1] = GlobalVars.PreciBasicValue[1] * Math.Pow((1 + _upAvePreciOther),yr);      // * (_random.NextDouble() * 0.5));
                    if (_upPreci[yr, 1] < GlobalVars.ThresholdUpPreci[0])
                    {
                        _upPreci[yr, 1] = GlobalVars.ThresholdUpPreci[0];
                    }
                    if (_upPreci[yr, 1] > GlobalVars.ThresholdUpPreci[1])
                    {
                        _upPreci[yr, 1] = GlobalVars.ThresholdUpPreci[1];
                    }

                }
                _wasttime = "上游生态水文模型参数初始化成功！";
                Console.WriteLine(_wasttime);

                //----------- calculate change rate used in economic model  -------------------
                // surface water, gourndwater, agricultural tech, industrial tech, service tech
                double[] _surWaterChgRate = new double[mCountyNum + 1];
                double[] _groWaterChgRate = new double[mCountyNum + 1];
                double[] _argTechChgRate = new double[mCountyNum + 1];
                double[] _indTechChgRate = new double[mCountyNum + 1];
                double[] _sevTechChgRate = new double[mCountyNum + 1];
                string[] _economicInputPara = new string[(mCountyNum + 1) * 5];         // 输入到文件中，修改经济模型输入文件

                int[] _index = new int[11] { 0, 8, 1, 2, 3, 4, 7, 6, 5, 9, 8 };             // 将系统默认的县区顺序调整为经济模型所需县区顺序
                int[] _revindex = new int[11] { 0, 2, 3, 4, 5, 8, 7, 6, 1, 9, 2 };          // 将经济模型中选取顺反转成系统默认顺序           
                string[] _couName = new string[11] { "GanZhou", "SuNan", "MinLe", "LinZe", "GaoTai", "ShanDan", "SuZhou", "JinTai", "JiaYuGuan", "EJNAQ", "QiLian" };
                string _cmdPath = @"C:\windows\system32\cmd.exe";
                string _inputPath = mCurrpath + @"\Economic model\term.cmf";
                string _runEconomicModel = "/C cd " + mCurrpath + @"\Economic model\" + " && term -cmf term.cmf";      // @"cd Economic model && C:\windows\system32\cmd.exe /c term -cmf term.cmf";
                string _runTransHAR = "/C cd " + mCurrpath + @"\Economic model\" + "&& sltoht -map=header.map term.sl4 results.sol";      // @"cd Economic model && c:\windows\system32\cmd.exe /c sltoht -map=header.map term.sl4 results.sol";
                string _runHARtoCSV = "/C cd " + mCurrpath + @"\Economic model\ && Har2xls results.sol results.xls";

                double[, ,] _realEcoModelOutput = new double[mSimlong, _couName.Length, 11];                    // 经济模型输出
                double[, ,] _MidEcoHydroModelOutput = new double[mSimlong, _couName.Length, 3];                 // 中游生态水文模型输出,地表供水（亿立方米），地下供水（亿立方），农业ET（亿立方）
                double[,] _populationData = new double[mSimlong, GlobalVars.CountyName.Length];                // 人口数据
                double[,] _urbanRateData = new double[mSimlong, GlobalVars.CountyName.Length];                 // 城市化率数组
                double[,] _forestAreaData = new double[mSimlong, GlobalVars.CountyName.Length];                 // 森林面积
                double[,] _grassAreaData = new double[mSimlong, GlobalVars.CountyName.Length];                  // 草地面积
                double[,] _gradedAreaData = new double[mSimlong, GlobalVars.CountyName.Length];                 // 退化土地面积

                //----------中游生态水文模式，气候参数初始化，温度和降水变化率---------------------------
                double[,] _midTemp = new double[_couName.Length, mSimlong];            // 每个县区的39年温度, 
                double[,] _midPreci = new double[_couName.Length, mSimlong];           // 每个县区的39年降水
                // 读取气候基准值文件
                System.Object[,] _climateBaseValue;             // 0:年份，1：县区，2：温度，3：降水（cm）
                _inputPath = mCurrpath + @"\Configuration files\BaseValue.xls";
                _climateBaseValue = mGFunc.ReadExcelData(_inputPath,"Climate",4);
                GlobalVars.ClimateBasicValue = new double[_climateBaseValue.GetLength(0) - 1,2];
                for(int cc = 1;cc < _climateBaseValue.GetLength(0);cc++)
                {
                    GlobalVars.ClimateBasicValue[cc - 1,0] = System.Double.Parse(_climateBaseValue[cc,2].ToString().Trim());
                    GlobalVars.ClimateBasicValue[cc - 1,1] = System.Double.Parse(_climateBaseValue[cc,3].ToString().Trim());
                }
                // 温度时间序列计算
                for (int county = 0; county < _couName.Length - 1; county++)
                {
                    double _StartTemp = GlobalVars.ClimateBasicValue[county, 0];              // 基准年温度,摄氏度
                    double _EndTemp = GlobalVars.ClimateBasicValue[county, 0] +
                        System.Double.Parse(GlobalVars.ClimateScenario[county, 1].Trim());                         // 目标年温度，摄氏度
                    double _annuAveIncR = Math.Pow(_EndTemp / _StartTemp, 1.0 / (double)mSimlong) - 1.0;            // 计算温度的年平均增长率
                    // 计算每年的温度
                    for (int i = 0; i < mSimlong; i++)
                    {
                        _midTemp[county, i] = _annuAveIncR;                                                 // 存储的年平均增长率
                        mMidRealTemp[i, county] = _StartTemp * Math.Pow(1 + _annuAveIncR, i);                   // 根据基准值转变为实际温度
                    }
                }
                // 降水时间序列计算
                System.Random _random = new Random();
                for (int county = 0; county < _couName.Length - 1; county++)
                {
                    double _Start = GlobalVars.ClimateBasicValue[county, 1];              // 基准年降水
                    double _End = _Start * (1+System.Double.Parse(GlobalVars.ClimateScenario[county, 2].Trim()) / 100.0);                         // 目标年降水,将降雨量变化率的%转换为小数
                    double _annuAveIncR = Math.Pow(_End / _Start, 1.0 / (double)mSimlong) - 1.0;            // 计算降水的年平均增长率
                    // 计算每年的降雨
                    for (int i = 0; i < mSimlong; i++)
                    {
                        _midPreci[county, i] = (_annuAveIncR + _random.NextDouble() / 100.0 - 0.005) * 100.0;        // 存储的降水年平均增长率
                        mMidRealpric[i, county] = _Start * Math.Pow(1 + _annuAveIncR, i);                  // 存储的是年实际降水量
                    }
                }

                _wasttime = "中游生态水文模型参数初始化成功！" + System.DateTime.Now.ToString();
                Console.WriteLine(_wasttime);

                //-------------  获取社会模型参数初值和基准值-------------------------------------
                _inputPath = mCurrpath + @"\Configuration files\BaseValue.xls";
                System.Object[,] _tempSocio;            // popxm,popx0,popr,urbanLamda,urbanNegaK,SafeDrinkWaterPop
                _tempSocio = mGFunc.ReadExcelData(_inputPath, "Society", 10);
                double[,] _SocioBasicValue = new double[_couName.Length - 1, 8];
                for (int county = 0; county < _couName.Length - 1; county++)
                {
                    for (int index = 0; index < 8; index++)
                    {
                        _SocioBasicValue[county, index] = System.Double.Parse(_tempSocio[county + 1, 2 + index].ToString().Trim());
                    }
                }
                _wasttime = "获取社会模型参数初值和基准值成功！" + System.DateTime.Now.ToString();
                Console.WriteLine(_wasttime);

                //-------------  获取土地利用模型参数和基准值 ------------------------------------
                for (int county = 0; county < GlobalVars.CountyName.Length - 1; county++)
                {
                    _rbTotalArea = _rbTotalArea + Double.Parse(GlobalVars.ModelBasicValue[county + 1, 4]);                              // 流域总面积

                }
                mWatershedArea = _rbTotalArea;
                ///  -----------------------------------------------------------------------------------------
                ///  集成模型主体
                ///  -----------------------------------------------------------------------------------------

         

                double _progressB = 94.0 / (double)mSimlong;
                int _progressV = 0;

                _wasttime = "集成模型运算开始：" + System.DateTime.Now.ToString();
                Console.WriteLine(_wasttime);

                for (int yr = 0; yr < mSimlong; yr++)
                {
                    //break;                      // 测试评价模型用
                    //-----------------------------------------------
                    // eco-hydrological model of upstream
                    //-----------------------------------------------
                    mYingluoxia[yr] = mUpEcoHydro.UpEcohydroModelYLX(_upTemp[yr, 0], _upPreci[yr, 0]) / 100000000.0;          // 莺落峡流量，立方米，转为亿立方米
                    mOtherRivers[yr] = mUpEcoHydro.UpEcohydroModelOther(_upTemp[yr, 1], _upPreci[yr, 1]) / 100000000.0;       // 其他河流流量，立方米，转为亿立方米

                    _wasttime = (2012 + yr).ToString() + "年：上游生态水文模型计算成功！" + System.DateTime.Now.ToString();
                    Console.WriteLine(_wasttime);

                    //-------------------------------------------------------------------------------
                    // economic model run,经济模型一次冲击一个流域11各县的数据，作为土地利用模型使用
                    //-------------------------------------------------------------------------------
                    if (yr == 0)
                    {
                        // 第一年使用基准值，变化率都为0
                        for (int county = 0; county < mCountyNum + 1; county++)
                        {
                            // surface water change rate
                            _surWaterChgRate[county] = 0.0; 
                            // groundwater chagne rate
                            _groWaterChgRate[county] = 0.0; 
                            // agricultural tech progress rate
                            _argTechChgRate[county] = 0.0;  
                            // industrial tech progress rate
                            _indTechChgRate[county] = 0.0; 
                            // service tech progress rate 
                            _sevTechChgRate[county] = 0.0;  

                        }
                    }
                    else if (yr == 1)
                    {
                        // 第二年使用情景参数
                        for (int county = 0; county < mCountyNum + 1; county++)
                        {
                            // surface water change rate
                            _surWaterChgRate[county] = System.Double.Parse(GlobalVars.EconomicScenario[_index[county], 4].Trim());  // System.Double.Parse(GlobalVars.EconomicScenario[county, 4].Trim());
                            // groundwater chagne rate
                            _groWaterChgRate[county] = System.Double.Parse(GlobalVars.EconomicScenario[_index[county], 5].Trim());          //System.Double.Parse(GlobalVars.EconomicScenario[county, 5].Trim());
                            // agricultural tech progress rate
                            _argTechChgRate[county] = System.Double.Parse(GlobalVars.EconomicScenario[_index[county], 1].Trim()) * (-1);    //System.Double.Parse(GlobalVars.EconomicScenario[county, 1].Trim()) * (-1);
                            // industrial tech progress rate
                            _indTechChgRate[county] = System.Double.Parse(GlobalVars.EconomicScenario[_index[county], 2].Trim()) * (-1);    //System.Double.Parse(GlobalVars.EconomicScenario[county, 2].Trim()) * (-1);
                            // service tech progress rate 
                            _sevTechChgRate[county] = System.Double.Parse(GlobalVars.EconomicScenario[_index[county], 3].Trim()) * (-1);    //System.Double.Parse(GlobalVars.EconomicScenario[county, 3].Trim()) * (-1);

                        }
                    }
                    else
                    {
                        // 第三年开始，使用中游生态水文模型输出的上一年的变化率
                        for (int county = 0; county < mCountyNum + 1; county++)
                        {
                            //----------   年变化率 -----------------
                            // surface water change rate
                            if (_MidEcoHydroModelOutput[yr - 1, county, 0] == 0)
                            {
                                _surWaterChgRate[county] = 0.0;
                            }
                            else
                            {
                                _surWaterChgRate[county] = (_MidEcoHydroModelOutput[yr - 1, _index[county], 0] - _MidEcoHydroModelOutput[yr - 2, _index[county], 0]) * 100.0 /
                                    _MidEcoHydroModelOutput[yr - 2, _index[county], 0];                     // 输入为%
                            }
                            // groundwater chagne rate
                            if (_MidEcoHydroModelOutput[yr - 1, county, 1] == 0)
                            {
                                _groWaterChgRate[county] = 0.0;
                            }
                            else
                            {
                                _groWaterChgRate[county] = (_MidEcoHydroModelOutput[yr - 1, _index[county], 1] - _MidEcoHydroModelOutput[yr - 2, _index[county], 1]) * 100.0 /
                                    _MidEcoHydroModelOutput[yr - 2, county, 1];                     // 输入为%
                            }
                            //------------------------ 与基准年的变化率  ---------------
                            //_surWaterChgRate[county] = (_MidEcoHydroModelOutput[yr - 1, county, 0] - _MidEcoHydroModelOutput[yr - 2, county, 0]) /
                            //    _MidEcoHydroModelOutput[yr - 1, county, 0];
                            //_groWaterChgRate[county] = (_MidEcoHydroModelOutput[yr - 1, county, 1] - _MidEcoHydroModelOutput[yr - 2, county, 1]) /
                            //    _MidEcoHydroModelOutput[yr - 1, county, 1];

                            // agricultural tech progress rate
                            _argTechChgRate[county] = System.Double.Parse(GlobalVars.EconomicScenario[_index[county], 1].Trim());
                            // industrial tech progress rate
                            _indTechChgRate[county] = System.Double.Parse(GlobalVars.EconomicScenario[_index[county], 2].Trim());
                            // service tech progress rate 
                            _sevTechChgRate[county] = System.Double.Parse(GlobalVars.EconomicScenario[_index[county], 3].Trim());

                        }
                    }
                    // 经济模型输入约束
                    for (int county = 0; county < mCountyNum + 1; county++)
                    {
                        // surface water change rate
                        _surWaterChgRate[county] = EconomicModel.WaterThresholdCon(_surWaterChgRate[county]);
                        // groundwater chagne rate
                        _groWaterChgRate[county] = EconomicModel.WaterThresholdCon(_groWaterChgRate[county]);
                        // agricultural tech progress rate
                        _argTechChgRate[county] = EconomicModel.TechAdvThsCon(_argTechChgRate[county]) * (-1);
                        // industrial tech progress rate
                        _indTechChgRate[county] = EconomicModel.TechAdvThsCon(_indTechChgRate[county]) * (-1);
                        // service tech progress rate 
                        _sevTechChgRate[county] = EconomicModel.TechAdvThsCon(_sevTechChgRate[county]) * (-1);

                    }

                    // 修改经济模型输入文件term.cmf
                    // 县区顺序：Ganzhou-0,Sunan-8, Minle-1,Linze-2, Gaotai-3,Shandan-4, Suzhou-7, Jintai-6,Jiayuguan-5, Ejinaq-9,Qilian-8
                    for (int county = 0; county < mCountyNum + 1; county++)
                    {
                        // 冲击地表水
                        _economicInputPara[county] = "shock xswt(IND,\"" + _couName[county] + "\") = uniform " + _surWaterChgRate[county].ToString() + ";";
                        // 冲击地下水
                        _economicInputPara[mCountyNum + 1 + county] = "shock xuwt(IND,\"" + _couName[county] + "\") = uniform " + _groWaterChgRate[county].ToString() + ";";
                        // 冲击农业技术进步率，负为正，正为负
                        _economicInputPara[2 * (mCountyNum + 1) + county] = "shock aprim(AGR,\"" + _couName[county] + "\") = uniform " + _argTechChgRate[county].ToString() + ";";
                        // 冲击工业技术进步率，负为正，正为负
                        _economicInputPara[3 * (mCountyNum + 1) + county] = "shock aprim(INDTR,\"" + _couName[county] + "\") = uniform " + _indTechChgRate[county].ToString() + ";";
                        // 冲击服务业技术进步率，负为正，正为负
                        _economicInputPara[4 * (mCountyNum + 1) + county] = "shock aprim(SER,\"" + _couName[county] + "\") = uniform " + _sevTechChgRate[county].ToString() + ";";
                    }

                    _wasttime = "中游经济模型参数初始化成功！";
                    Console.WriteLine(_wasttime);
                    // 修改经济模型输入文件
                    _inputPath = mCurrpath + @"\Economic model\term.cmf"; 
                    GlobalFunctions.ReWriteEcoTerm(_inputPath, _economicInputPara);

                    _wasttime = (2012 + yr).ToString() + "年：修改中游经济模型输入文件成功！" + System.DateTime.Now.ToString();
                    Console.WriteLine(_wasttime);

                    // execute economic model
                    // 运行经济模型
                    string _ecomodel = GlobalFunctions.ExecuteLi(_runEconomicModel, _cmdPath, 1);

                    _wasttime = (2012 + yr).ToString() + "年：经济模型运行成功！返回结果标识：" + System.DateTime.Now.ToString();
                    Console.WriteLine(_wasttime);

                    // 将经济模型运行结果转换为HAR文件，
                    string _har = GlobalFunctions.ExecuteLi(_runTransHAR,_cmdPath,1);

                    _wasttime = (2012 + yr).ToString() + "年：经济模型运行结果转换为HAR文件成功！" + System.DateTime.Now.ToString();
                    Console.WriteLine(_wasttime);

                    // 将HAR文件转换为CSV文件，存储解决模型输出，
                    string _csv = GlobalFunctions.ExecuteLi(_runHARtoCSV, _cmdPath, 1);

                    _wasttime = (2012 + yr).ToString() + "年：将HAR文件转换为CSV文件成功！" + System.DateTime.Now.ToString();
                    Console.WriteLine(_wasttime);

                    // 读取Result.xls文件中的数据，利用percent.xls文件中的加权系统，对result.xls文件中的数据进行加权处理，得到每个县区的结果
                    double[, ,] _resData;       // 经济模型输出文件result.xls
                    double[, ,] _perData;       // 经济模型输出文件percent.xls
                    string _fullfile = mCurrpath + @"\Economic model\results.xls";
                    _resData = ReadResultEcoModel(_fullfile);
                    _fullfile = mCurrpath + @"\Economic model\percent.xls";
                    _perData = ReadPercentEcoModel(_fullfile);
                    double[,] _tempEcoModelResultChgRate = null;    // 0：县区；1：输出类别（工业产值，农业产值，服务业产值，农业土地，
                                                                                // 工业土地，服务业土地，水价，就业，地表水，地下水
                    string[] _countryname = new string[10];
                    for (int i = 0; i < _couName.Length - 1; i++)
                    {
                        _countryname[i] = _couName[i];
                    }
                    _tempEcoModelResultChgRate = EconomicModel.ModelOutput(_resData, _perData, _countryname);
                    double[,] _EcoModelResultChgRate = new double[_tempEcoModelResultChgRate.GetLength(0),_tempEcoModelResultChgRate.GetLength(1)];        // 0：县区；1：输出类别（工业产值，农业产值，服务业产值，农业土地，
                    // 工业土地，服务业土地，水价，就业，地表水，地下水
                    // 将县区顺序调整成系统配置文件中县区顺序
                    for (int ii = 0; ii < _countryname.Length; ii++)
                    {
                        for (int kk = 0; kk < _realEcoModelOutput.GetLength(2); kk++)
                        {
                            _EcoModelResultChgRate[ii, kk] = _tempEcoModelResultChgRate[_revindex[ii], kk];
                        }
                    }
                    // 将变化率转换为实际值
                    // 某一年10个县区农业产值,第一年为基准值，第二年之后的计算以上一年为基准：第二年= 第一年*（1+变化率/100）
                    if (yr == 0)
                    {
                        // 第一年，基准值
                        // 县区顺序已经改变成系统默认的县区顺序
                        for (int ii = 0; ii < _countryname.Length; ii++)
                        {
                            _realEcoModelOutput[yr, ii, 0] = System.Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 9].Trim());     //农业产值
                            _realEcoModelOutput[yr, ii, 1] = System.Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 13].Trim());    // 工业产值
                            _realEcoModelOutput[yr, ii, 2] = System.Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 14].Trim());    // 服务业产值
                            _realEcoModelOutput[yr, ii, 3] = System.Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 6].Trim());    // 农业土地
                            _realEcoModelOutput[yr, ii, 4] = System.Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 15].Trim());    // 工业土地
                            _realEcoModelOutput[yr, ii, 5] = System.Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 16].Trim());    // 服务业土地
                            _realEcoModelOutput[yr, ii, 6] = System.Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 18].Trim());    // 水价
                            _realEcoModelOutput[yr, ii, 7] = System.Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 23].Trim()) * 100.0;    // 就业, 将小数转变为百分比
                            _realEcoModelOutput[yr, ii, 8] = System.Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 21].Trim());    // 经济地表需水
                            _realEcoModelOutput[yr, ii, 9] = System.Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 22].Trim());    // 经济地下需水
                            _realEcoModelOutput[yr, ii, 10] = System.Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 19].Trim());    // 非农业用水

                            for(int kk = 0;kk < 11;kk++)
                            {
                                mEcoModelChgRate[yr,ii,kk] = _EcoModelResultChgRate[ii,kk];
                            }
                            //--------------   计算流域GDP，耕地面积，农业产值，就业人口，地表经济需水，地下经济需水  -----------
                            _rbGDP[yr] = _rbGDP[yr] + _realEcoModelOutput[yr, ii, 0] + _realEcoModelOutput[yr, ii, 1] + _realEcoModelOutput[yr, ii, 2];
                            _rbFLA[yr] = _rbFLA[yr] + _realEcoModelOutput[yr, ii, 3];
                            _rbArgValue[yr] = _rbArgValue[yr] + _realEcoModelOutput[yr, ii, 0];
                            //_rbIndValue[yr] = _rbIndValue[yr] + _realEcoModelOutput[yr, ii, 1];
                            //_rbSerValue[yr] = _rbSerValue[yr] + _realEcoModelOutput[yr, ii, 2];
                            _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 1] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 1] +
                                                                                            _realEcoModelOutput[yr, ii, 1];         // 工业产值
                            _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 2] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 2] +
                                                                                            _realEcoModelOutput[yr, ii, 2];         // 服务业产值
                            _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 4] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 5] +
                                                                                            _realEcoModelOutput[yr, ii, 4];       // 工业用地
                            _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 5] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 5] +
                                                                                            _realEcoModelOutput[yr, ii, 5];         // 服务业用地
                            //_rbEmpPop[yr] = _rbEmpPop[yr] + Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 5]) * GlobalVars.LaborPopPorp * (_realEcoModelOutput[yr, ii, 7] / 100.0);
                            _rbSurWD[yr] = _rbSurWD[yr] + _realEcoModelOutput[yr, ii, 8];
                            _rbGrdWD[yr] = _rbGrdWD[yr] + _realEcoModelOutput[yr, ii, 9];
                        }
                    }
                    else
                    {

                        for (int ii = 0; ii < _countryname.Length; ii++)
                        {
                            _realEcoModelOutput[yr, ii, 0] = _realEcoModelOutput[yr - 1, ii, 0] * (1 + _EcoModelResultChgRate[ii, 0] / 100.0);
                            _realEcoModelOutput[yr, ii, 1] = _realEcoModelOutput[yr - 1, ii, 1] * (1 + _EcoModelResultChgRate[ii, 1] / 100.0);
                            _realEcoModelOutput[yr, ii, 2] = _realEcoModelOutput[yr - 1, ii, 2] * (1 + _EcoModelResultChgRate[ii, 2] / 100.0);
                            _realEcoModelOutput[yr, ii, 3] = _realEcoModelOutput[yr - 1, ii, 3] * (1 + _EcoModelResultChgRate[ii, 3] / 100.0);
                            _realEcoModelOutput[yr, ii, 4] = _realEcoModelOutput[yr - 1, ii, 4] * (1 + _EcoModelResultChgRate[ii, 4] / 100.0);
                            _realEcoModelOutput[yr, ii, 5] = _realEcoModelOutput[yr - 1, ii, 5] * (1 + _EcoModelResultChgRate[ii, 5] / 100.0);
                            _realEcoModelOutput[yr, ii, 6] = _realEcoModelOutput[yr - 1, ii, 6] * (1 + _EcoModelResultChgRate[ii, 6] / 100.0);
                            // 利用Okun定律纠正计算很大的就业率，条件是当年就业率大于0.7时，使用Okun定律计算
                            double _gdp1 = 0, _gdp2 = 0;
                            _gdp1 = _realEcoModelOutput[yr - 1, ii, 0] + _realEcoModelOutput[yr - 1, ii, 1] + _realEcoModelOutput[yr - 1, ii, 2];       // 上一年GDP
                            _gdp2 = _realEcoModelOutput[yr, ii, 0] + _realEcoModelOutput[yr, ii, 1] + _realEcoModelOutput[yr, ii, 2];                   // 当年GDP
                            double _gdpr = (_gdp2 - _gdp1) / _gdp1;         // GDP年变化
                            if (_EcoModelResultChgRate[ii, 7] > 1.0)
                            {
                                _EcoModelResultChgRate[ii, 7] = GlobalFunctions.OkunLawEmp(_gdpr, 0.01);          // 利用Okun定律纠正就业率的变化率
                            }
                            // 约束就业率小于100
                            if (_realEcoModelOutput[yr, ii, 7] > 100.0)
                            {
                                _realEcoModelOutput[yr, ii, 7] = 100.0;
                            }
                            _realEcoModelOutput[yr, ii, 7] = _realEcoModelOutput[yr - 1, ii, 7] * (1 + _EcoModelResultChgRate[ii, 7] / 100.0);
                            _realEcoModelOutput[yr, ii, 8] = _realEcoModelOutput[yr - 1, ii, 8] * (1 + _EcoModelResultChgRate[ii, 8] / 100.0);
                            _realEcoModelOutput[yr, ii, 9] = _realEcoModelOutput[yr - 1, ii, 9] * (1 + _EcoModelResultChgRate[ii, 9] / 100.0);
                            _realEcoModelOutput[yr, ii, 10] = _realEcoModelOutput[yr - 1, ii, 10] * (1 + _EcoModelResultChgRate[ii, 10] / 100.0);    // 非农业用水变化率
                            if (_realEcoModelOutput[yr, ii, 10] < 0)
                            {
                                _realEcoModelOutput[yr, ii, 10] = 0.0;
                            }
                            // 将经济模型输出的所有变化率存储到全局变量中
                             for(int kk = 0;kk < 11;kk++)
                            {
                                mEcoModelChgRate[yr,ii,kk] = _EcoModelResultChgRate[ii,kk];
                            }

                            //--------------   计算流域GDP，耕地面积，农业产值，就业人口，地表经济需水，地下经济需水  -----------
                            _rbGDP[yr] = _rbGDP[yr] + _realEcoModelOutput[yr, ii, 0] + _realEcoModelOutput[yr, ii, 1] + _realEcoModelOutput[yr, ii, 2];
                            _rbFLA[yr] = _rbFLA[yr] + _realEcoModelOutput[yr, ii, 3];
                            _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 1] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 1] +
                                                                                            _realEcoModelOutput[yr, ii, 1];         // 工业产值
                            _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 2] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 2] +
                                                                                            _realEcoModelOutput[yr, ii, 2];         // 服务业产值
                            _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 4] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 5] +
                                                                                            _realEcoModelOutput[yr, ii, 4];       // 工业用地
                            _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 5] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 5] +
                                                                                            _realEcoModelOutput[yr, ii, 5];         // 服务业用地
                            //_rbEmpPop[yr] = _rbEmpPop[yr] + Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 5]) * GlobalVars.LaborPopPorp * (_realEcoModelOutput[yr, ii, 7] / 100.0);
                            _rbArgValue[yr] = _rbArgValue[yr] + _realEcoModelOutput[yr, ii, 0];
                            _rbSurWD[yr] = _rbSurWD[yr] + _realEcoModelOutput[yr, ii, 8];
                            _rbGrdWD[yr] = _rbGrdWD[yr] + _realEcoModelOutput[yr, ii, 9];
                        }
                    }

                    _wasttime = (2012 + yr).ToString() + "年：对result.xls文件中的数据进行加权处理，得到每个县区的结果成功！" + System.DateTime.Now.ToString();
                    Console.WriteLine(_wasttime);

                    //---- 计算非农业用水量----=总用水量-农业用水量
                    for (int county = 0; county < GlobalVars.CountyName.Length - 1; county++)
                    {

                        if (yr == 0)
                        {
                            //第一年使用基准值
                            _realEcoModelOutput[yr, county, 10] = Double.Parse(GlobalVars.ModelBasicValue[county + 1, 19]);
                        }
                        else
                        {
                            //_realEcoModelOutput[yr, county, 10] = (_realEcoModelOutput[yr, county, 8] + _realEcoModelOutput[yr, county, 9]) *
                                                                    //(1 - Double.Parse(mWaterCoefficient[county + 1, 2]));         // 经济模型输出的用水量-农业用水量
                        }
                        // 计算流域尺度非农业需水
                        _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 10] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 10] + _realEcoModelOutput[yr, county, 10];
                    }
                    //-------------------------------------------------------------------------------
                    //------------  分县区计算中游生态水文模型、社会模型和指标模型---------------
                    //-------------------------------------------------------------------------------
                    for (int county = 0; county < GlobalVars.CountyName.Length - 1; county++)
                    {

                        // economic model run
                        //_ecomodelCMD = @"C:\windows\system32\cmd.exe /c term -cmf Assessment model\term.cmf";
                        //_ecomodelResult = mGFunc.Execute(_ecomodelCMD, 1);
                        // eco-hydrological model run
                        //---------------------------------------------------------
                        // eco-hydrologcial model of midreaches
                        //---------------------------------------------------------
                        // 修改生态水文模型输入文件，input.txt
                        string[] _inputpara = new string[2];
                        _inputpara[0] = (county + 1).ToString();
                        _inputpara[1] = _midPreci[county, yr].ToString("F7") + "," + _midTemp[county, yr].ToString("F7") + "," +            // 降水变化率(小数）,温度变化率（摄氏度），
                                        mYingluoxia[yr].ToString("F7") + "," + mOtherRivers[yr].ToString("F7") + "," +                      // 莺落峡流量（亿立方），其他河流流量（亿立方）
                                        GlobalVars.GovernScenario[GlobalVars.CountyName.Length - 1, 3];                             // 正义峡下泄量（亿立方）
                        // 获取10个县区的非农业用水量，亿立方米
                        for (int cc = 0; cc < GlobalVars.CountyName.Length - 1; cc++)
                        {
                            if (yr == 0)
                            {
                                // 第一年使用情景参数
                                _inputpara[1] = _inputpara[1] + "," + (System.Double.Parse(GlobalVars.ModelBasicValue[cc + 1, 19])).ToString();
                            }
                            else
                            {
                                // 非第一年使用经济模型输出
                                _inputpara[1] = _inputpara[1] + "," + (_realEcoModelOutput[yr,cc, 10]).ToString();

                            }
                        }

                        // 获取10个县区的耕地面积变化率（小数），第一年来自于情景，之后来自于经济模型输出
                        for (int cc = 0; cc < GlobalVars.CountyName.Length - 1; cc++)
                        {
                            //double _tempWaterUse = 0;
                            //_tempWaterUse = Double.Parse(GlobalVars.ModelBasicValue[cc + 1, 2]) *                                   // 地表用水基准值
                            //                Math.Pow(1 + Double.Parse(GlobalVars.EconomicScenario[cc, 4]), yr) +                // 地表用水年变化率
                            //                Double.Parse(GlobalVars.ModelBasicValue[cc + 1, 3]) *                                   // 地下用水基准值
                            //                Math.Pow(1 + Double.Parse(GlobalVars.EconomicScenario[cc, 5]), yr);                 // 地下用水年变化率
                            //_inputpara[1] = _inputpara[1] + "," + _tempWaterUse;
                            if (yr == 0)
                            {
                                // 第一年使用情景参数
                                _inputpara[1] = _inputpara[1] + "," + (System.Double.Parse(GlobalVars.LandScenario[cc, 1]) / 100.0).ToString();
                            }
                            else
                            {
                                // 非第一年使用经济模型输出
                                _inputpara[1] = _inputpara[1] + "," + (_EcoModelResultChgRate[cc, 3] / 100.0).ToString();

                            }
                        }
                        _inputPath = mCurrpath + @"\MidEcoHydroModel\para\input.txt";
                        GlobalFunctions.WriteTextFile(_inputPath, _inputpara);

                        _wasttime = (2012 + yr).ToString() + "年：修改中游生态水文模型输入成功！" + System.DateTime.Now.ToString();
                        Console.WriteLine(_wasttime);

                        // 运行生态水文模型
                        _cmdPath = @"C:\windows\system32\cmd.exe";
                        //string _inputPath = mCurrpath + @"\Economic model\term.cmf";
                        string _runEcoHydroModel = @"/C cd " + System.AppDomain.CurrentDomain.BaseDirectory + @"\MidEcoHydroModel\  && javasvm.exe";      // @"cd Economic model && C:\windows\system32\cmd.exe /c term -cmf term.cmf";
                        //string _cmd = mCurrpath + @"\MidEcoHydroModel\javasvm.exe";
                        string _ecohydroR = GlobalFunctions.ExcuteExeFile(_runEcoHydroModel, _cmdPath, 1);

                        _wasttime = (2012 + yr).ToString() + "年：运行中游生态水文模型成功！" + System.DateTime.Now.ToString();
                        Console.WriteLine(_wasttime);

                        // 读取模型运行结果
                        _inputPath = mCurrpath + @"\MidEcoHydroModel\para\output.txt";
                        System.Collections.ArrayList _ecohdroRes = GlobalFunctions.ReadTextFile(_inputPath);
                        string[] _tempconver = GlobalFunctions.ReadListofSynergy(_ecohdroRes[0].ToString(), 3, ",");

                        // 第一年使用基准值
                        if (yr == 0)
                        {
                            _MidEcoHydroModelOutput[yr, county, 0] = Double.Parse(GlobalVars.ModelBasicValue[county + 1, 2]);
                            _MidEcoHydroModelOutput[yr, county, 1] = Double.Parse(GlobalVars.ModelBasicValue[county + 1, 3]);
                            _MidEcoHydroModelOutput[yr, county, 2] = Double.Parse(GlobalVars.ModelBasicValue[county + 1, 20]);
                        }
                        else
                        {
                            _MidEcoHydroModelOutput[yr, county, 0] = Double.Parse(_tempconver[0]);
                            _MidEcoHydroModelOutput[yr, county, 1] = Double.Parse(_tempconver[1]);
                            _MidEcoHydroModelOutput[yr, county, 2] = Double.Parse(_tempconver[2]);
                            // 将生态水文模型输出的总供水量转换为经济供水量，即=总供水量*经济用水系数
                            //_MidEcoHydroModelOutput[yr, county, 0] = _MidEcoHydroModelOutput[yr, county, 0] * Double.Parse(mWaterCoefficient[county + 1, 1]);
                            //_MidEcoHydroModelOutput[yr, county, 1] = _MidEcoHydroModelOutput[yr, county, 1] * Double.Parse(mWaterCoefficient[county + 1, 1]);
                        }

                        _wasttime = (2012 + yr).ToString() + "年：获取中游生态水文模型运行结果成功！" + System.DateTime.Now.ToString();
                        Console.WriteLine(_wasttime);

                        //-------------  计算流域地表，地下经济可供水量  --------------------------
                        _rbSurWS[yr] = _rbSurWS[yr] + _MidEcoHydroModelOutput[yr, county, 0];
                        _rbGrdWS[yr] = _rbGrdWS[yr] + _MidEcoHydroModelOutput[yr, county, 1];
                        _rbArgSurWS[yr] = _rbArgSurWS[yr] + _MidEcoHydroModelOutput[yr, county, 0] * Double.Parse(mWaterCoefficient[county + 1, 2]);        // 流域农业经济地表供水
                        _rbArgGrdWS[yr] = _rbArgGrdWS[yr] + _MidEcoHydroModelOutput[yr, county, 1] * Double.Parse(mWaterCoefficient[county + 1, 2]);        // 流域农业经济地下供水
                        _rbArgET[yr] = _rbArgET[yr] + _MidEcoHydroModelOutput[yr, county, 2];
                        _rbTotArgET[yr] = _rbTotArgET[yr] + _MidEcoHydroModelOutput[yr, county, 2] * Double.Parse(mWaterCoefficient[county + 1, 3]);        // 流域总的农业ET

                        //-------------------------------------------------------------------
                        //社会模型，存储的是情景设置中的县区顺序
                        //-------------------------------------------------------------------
                        if (yr == 0)
                        {
                            // 取基准值数据
                            _populationData[yr, county] = Double.Parse(GlobalVars.ModelBasicValue[county + 1, 5]);
                            _urbanRateData[yr, county] = _SocioBasicValue[county, 7];
                        }
                        else
                        {
                            _populationData[yr, county] = SocialModels.PopulationModel(System.Double.Parse(GlobalVars.GovernScenario[county, 1].Trim()), yr,
                                _SocioBasicValue[county, 1], _SocioBasicValue[county, 0]); //有问题！已解决
                            _urbanRateData[yr, county] = SocialModels.UrbanizationRate(_SocioBasicValue[county, 3],
                                System.Double.Parse(GlobalVars.GovernScenario[county, 2].Trim()), yr);
                        }
                        //-------------------  计算流域人口数量，城镇人口数量   -----------------------
                        _rbPop[yr] = _rbPop[yr] + _populationData[yr, county];
                        _rbUrbPop[yr] = _rbUrbPop[yr] + _populationData[yr, county] * _urbanRateData[yr, county] / 100.0;           // 城镇人口=人口总数*城镇化率
                        //if (yr > 0)
                        //{
                        _rbEmpPop[yr] = _rbEmpPop[yr] + _populationData[yr, county] * GlobalVars.LaborPopPorp * (_realEcoModelOutput[yr, county, 7] / 100.0);       // 就业人口数量，将百分数化为小数
                        //}

                        _wasttime = (2012 + yr).ToString() + "年：社会模型运行成功！" + System.DateTime.Now.ToString();
                        Console.WriteLine(_wasttime);

                        //---------------------------------------------------------------------
                        // 土地利用模型，存储的是情景设置中的县区顺序，第一年使用基准值
                        //---------------------------------------------------------------------
                        double _gdp = _realEcoModelOutput[yr, county, 0] + _realEcoModelOutput[yr, county, 1] + _realEcoModelOutput[yr, county, 2];
                        if (yr == 0)
                        {
                            _forestAreaData[yr, county] = Double.Parse(GlobalVars.ModelBasicValue[county + 1, 8]);
                            _grassAreaData[yr, county] = Double.Parse(GlobalVars.ModelBasicValue[county + 1, 7]);
                        }
                        else
                        {
                            _forestAreaData[yr, county] = LanduseModels.ForestAreaPre(county + 1, _urbanRateData[yr, county] / 100.0, _gdp, _midTemp[county, yr]) * 10000.0;              // 单位转换：模型输出万公顷，转换为公顷
                            _grassAreaData[yr, county] = LanduseModels.GrasslandAreaPre(county + 1, _urbanRateData[yr, county] / 100.0, _gdp, _midPreci[county, yr]) * 10000.0;              // 单位转换：模型输出万公顷，转换为公顷
                        }
                        _gradedAreaData[yr, county] = System.Double.Parse(GlobalVars.ModelBasicValue[county + 1, 4]) - _forestAreaData[yr, county] -
                                                        _grassAreaData[yr, county] - _realEcoModelOutput[yr, county, 3] - _realEcoModelOutput[yr, county, 4] -
                                                        _realEcoModelOutput[yr, county, 5];             // 退化土地面积=总面积-森林面积-草地面积-农业用地-工业用地-服务业用地
                        //-------------------  计算流域森林面积，草地面积,退化土地面积  ------------------------
                        _rbFstArea[yr] = _rbFstArea[yr] + _forestAreaData[yr, county];
                        _rbGrsArea[yr] = _rbGrsArea[yr] + _grassAreaData[yr, county];
                        _rbDegradedArea[yr] = _rbDegradedArea[yr] + _gradedAreaData[yr, county];

                        _wasttime = (2012 + yr).ToString() + "年：土地利用模型运行成功！" + System.DateTime.Now.ToString();
                        Console.WriteLine(_wasttime);

                    }
                    //----------------------------------------------------------------------------------------------------
                    // 运行经济模型，利用中游生态水文模型输出的地表供水和地下供水修改之前的经济模型输入的地表和地下供水
                    // 然后重新对经济模型的参数进行冲击，
                    //----------------------------------------------------------------------------------------------------
                    //if (yr == 0)
                    //{
                    //    for (int county = 0; county < mCountyNum; county++)
                    //    {
                    //        _surWaterChgRate[county] = 0;
                    //        _groWaterChgRate[county] = 0;
                    //    }
                    //}
                    //else
                    //{
                    //    for (int county = 1; county < mCountyNum; county++)
                    //    {
                    //        // surface water change rate
                    //        _surWaterChgRate[county] = (_MidEcoHydroModelOutput[yr, county, 0] - _MidEcoHydroModelOutput[yr - 1, county, 0]) / 
                    //                                            _MidEcoHydroModelOutput[yr - 1, county, 0];
                    //        // groundwater chagne rate
                    //        _groWaterChgRate[county] = (_MidEcoHydroModelOutput[yr, county, 1] - _MidEcoHydroModelOutput[yr - 1, county, 1]) /
                    //                                            _MidEcoHydroModelOutput[yr - 1, county, 1];
                    //        // agricultural tech progress rate
                    //        //_argTechChgRate[county] = System.Double.Parse(GlobalVars.EconomicScenario[county, 1].Trim());
                    //        //// industrial tech progress rate
                    //        //_indTechChgRate[county] = System.Double.Parse(GlobalVars.EconomicScenario[county, 2].Trim());
                    //        //// service tech progress rate 
                    //        //_sevTechChgRate[county] = System.Double.Parse(GlobalVars.EconomicScenario[county, 3].Trim());

                    //    }
                    //}
                    // 修改经济模型输入文件term.cmf
                    // 县区顺序：Ganzhou-0,Sunan-8, Minle-1,Linze-2, Gaotai-3,Shandan-4, Suzhou-7, Jintai-6,Jiayuguan-5, Ejinaq-9,Qilian-8
                    //int[] _index = new int[11] { 0, 8, 1, 2, 3, 4, 7, 6, 5, 9, 8 };
                    //string[] _couName = new string[11] { "GanZhou", "SuNan", "MinLe", "LinZe", "GaoTai", "ShanDan", "SuZhou", "JinTai", "JiaYuGuan", "EJNAQ", "QiLian" };
                    //string _cmdPath = @"C:\windows\system32\cmd.exe";
                    //string _inputPath = mCurrpath + @"\Economic model\term.cmf";
                    //string _runEconomicModel = "/C cd " + mCurrpath + @"\Economic model\" + " && term -cmf term.cmf";      // @"cd Economic model && C:\windows\system32\cmd.exe /c term -cmf term.cmf";
                    //string _runTransHAR = "/C cd " + mCurrpath + @"\Economic model\" + "&& sltoht -map=header.map term.sl4 results.sol";      // @"cd Economic model && c:\windows\system32\cmd.exe /c sltoht -map=header.map term.sl4 results.sol";
                    //string _runHARtoCSV = "/C cd " + mCurrpath + @"\Economic model\ && Har2xls results.sol results.xls";
                    //for (int county = 0; county < mCountyNum + 1; county++)
                    //{
                    //    // 冲击地表水
                    //    _economicInputPara[county] = "shock xswt(IND,\"" + _couName[county] + "\") = uniform " + _surWaterChgRate[_index[county]].ToString() + ";";
                    //    // 冲击地下水
                    //    _economicInputPara[mCountyNum + 1 + county] = "shock xuwt(IND,\"" + _couName[county] + "\") = uniform " + _groWaterChgRate[_index[county]].ToString() + ";";
                    //    // 冲击农业技术进步率，负为正，正为负
                    //    _economicInputPara[2 * (mCountyNum + 1) + county] = "shock aprim(AGR,\"" + _couName[county] + "\") = uniform " + _argTechChgRate[_index[county]].ToString() + ";";
                    //    // 冲击工业技术进步率，负为正，正为负
                    //    _economicInputPara[3 * (mCountyNum + 1) + county] = "shock aprim(INDTR,\"" + _couName[county] + "\") = uniform " + _indTechChgRate[_index[county]].ToString() + ";";
                    //    // 冲击服务业技术进步率，负为正，正为负
                    //    _economicInputPara[4 * (mCountyNum + 1) + county] = "shock aprim(SER,\"" + _couName[county] + "\") = uniform " + _sevTechChgRate[_index[county]].ToString() + ";";
                    //}
                    ////-------------------------------------------------------------------------------
                    //// economic model run,经济模型一次冲击一个流域11各县的数据，作为土地利用模型使用
                    ////-------------------------------------------------------------------------------
                    //// 修改经济模型输入文件
                    //GlobalFunctions.ReWriteEcoTerm(_inputPath, _economicInputPara);

                    //_wasttime = (2012 + yr).ToString() + "年：经济模型输入参数修改运行成功！" + System.DateTime.Now.ToString();
                    //textBoxResult.Text = _wasttime;
                    //textBoxResult.Refresh();

                    //// execute economic model
                    //// 运行经济模型
                    //_ecomodel = GlobalFunctions.ExecuteLi(_runEconomicModel, _cmdPath, 1);

                    //_wasttime = (2012 + yr).ToString() + "年：经济模型运行成功！" + System.DateTime.Now.ToString();
                    //textBoxResult.Text = _wasttime;
                    //textBoxResult.Refresh();

                    //// 将经济模型运行结果转换为HAR文件，
                    //_har = GlobalFunctions.ExecuteLi(_runTransHAR, _cmdPath, 1);

                    //_wasttime = (2012 + yr).ToString() + "年：经济模型运行结果转换为HAR文件运行成功！" + System.DateTime.Now.ToString();
                    //textBoxResult.Text = _wasttime;
                    //textBoxResult.Refresh();

                    //// 将HAR文件转换为CSV文件，存储解决模型输出，
                    //_csv = GlobalFunctions.ExecuteLi(_runHARtoCSV, _cmdPath, 1);
                    //// 读取Result.xls文件中的数据，利用percent.xls文件中的加权系统，对result.xls文件中的数据进行加权处理，得到每个县区的结果

                    //_wasttime = (2012 + yr).ToString() + "年：将HAR文件转换为CSV文件，存储解决模型输出运行成功！" + System.DateTime.Now.ToString();
                    //textBoxResult.Text = _wasttime;
                    //textBoxResult.Refresh();

                    ////double[, ,] _resData;       // 经济模型输出文件result.xls
                    ////double[, ,] _perData;       // 经济模型输出文件percent.xls
                    //_fullfile = mCurrpath + @"\Economic model\results.xls";
                    //_resData = ReadResultEcoModel(_fullfile);
                    //_fullfile = mCurrpath + @"\Economic model\percent.xls";
                    //_perData = ReadPercentEcoModel(_fullfile);
                    //_EcoModelResultChgRate = null;        // 0：县区；1：输出类别（工业产值，农业产值，服务业产值，农业土地，
                    //// 工业土地，服务业土地，水价，就业，地表水，地下水
                    ////string[] _countryname = new string[10];
                    ////for (int i = 0; i < _couName.Length - 1; i++)
                    ////{
                    ////    _countryname[i] = _couName[i];
                    ////}
                    //_EcoModelResultChgRate = EconomicModel.ModelOutput(_resData, _perData, _countryname);
                    // 将变化率转换为实际值
                    // 某一年10个县区农业产值,第一年为基准值，第二年之后的计算以上一年为基准：第二年= 第一年*（1+变化率/100）
                    //if (yr == 0)
                    //{
                    //    // 第一年，基准值
                    //    for (int ii = 0; ii < _countryname.Length; ii++)
                    //    {
                    //        _realEcoModelOutput[yr, ii, 0] = System.Double.Parse(GlobalVars.ModelBasicValue[_index[ii] + 1, 9].Trim());     //农业产值
                    //        _realEcoModelOutput[yr, ii, 1] = System.Double.Parse(GlobalVars.ModelBasicValue[_index[ii] + 1, 13].Trim());    // 工业产值
                    //        _realEcoModelOutput[yr, ii, 2] = System.Double.Parse(GlobalVars.ModelBasicValue[_index[ii] + 1, 14].Trim());    // 服务业产值
                    //        _realEcoModelOutput[yr, ii, 3] = System.Double.Parse(GlobalVars.ModelBasicValue[_index[ii] + 1, 6].Trim());    // 农业土地
                    //        _realEcoModelOutput[yr, ii, 4] = System.Double.Parse(GlobalVars.ModelBasicValue[_index[ii] + 1, 15].Trim());    // 工业土地
                    //        _realEcoModelOutput[yr, ii, 5] = System.Double.Parse(GlobalVars.ModelBasicValue[_index[ii] + 1, 16].Trim());    // 服务业土地
                    //        _realEcoModelOutput[yr, ii, 6] = System.Double.Parse(GlobalVars.ModelBasicValue[_index[ii] + 1, 18].Trim());    // 水价
                    //        _realEcoModelOutput[yr, ii, 7] = System.Double.Parse(GlobalVars.ModelBasicValue[_index[ii] + 1, 13].Trim());    // 就业
                    //        _realEcoModelOutput[yr, ii, 8] = System.Double.Parse(GlobalVars.ModelBasicValue[_index[ii] + 1, 2].Trim());    // 经济地表需水
                    //        _realEcoModelOutput[yr, ii, 9] = System.Double.Parse(GlobalVars.ModelBasicValue[_index[ii] + 1, 3].Trim());    // 经济地下需水
                    //        _realEcoModelOutput[yr, ii, 10] = System.Double.Parse(GlobalVars.ModelBasicValue[_index[ii] + 1, 19].Trim());    // 非农业用水

                    //        for (int kk = 0; kk < 11; kk++)
                    //        {
                    //            mEcoModelChgRate[yr, ii, kk] = 0.0;
                    //        }


                    //        //--------------   计算流域GDP，耕地面积，农业产值，就业人口，地表经济需水，地下经济需水  -----------
                    //        _rbGDP = _rbGDP + _realEcoModelOutput[yr, ii, 0] + _realEcoModelOutput[yr, ii, 1] + _realEcoModelOutput[yr, ii, 2];
                    //        _rbFLA = _rbFLA + _realEcoModelOutput[yr, ii, 3];
                    //        _rbArgValue = _rbArgValue + _realEcoModelOutput[yr, ii, 0];
                    //        _rbIndValue = _rbIndValue + _realEcoModelOutput[yr, ii, 1];
                    //        _rbSerValue = _rbSerValue + _realEcoModelOutput[yr, ii, 2];
                    //        _rbEmpPop = _rbEmpPop + Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 5]) * GlobalVars.LaborPopPorp * _realEcoModelOutput[yr, ii, 7];
                    //        _rbSurWD = _rbSurWD + _realEcoModelOutput[yr, ii, 8];
                    //        _rbGrdWD = _rbGrdWD + _realEcoModelOutput[yr, ii, 9];
                    //    }
                    //}
                    //else
                    //{
                    //    for (int ii = 0; ii < _countryname.Length; ii++)
                    //    {
                    //        _realEcoModelOutput[yr, ii, 0] = _realEcoModelOutput[yr - 1, ii, 0] * (1 + _EcoModelResultChgRate[ii, 0] / 100.0);
                    //        _realEcoModelOutput[yr, ii, 1] = _realEcoModelOutput[yr - 1, ii, 1] * (1 + _EcoModelResultChgRate[ii, 1] / 100.0);
                    //        _realEcoModelOutput[yr, ii, 2] = _realEcoModelOutput[yr - 1, ii, 2] * (1 + _EcoModelResultChgRate[ii, 2] / 100.0);
                    //        _realEcoModelOutput[yr, ii, 3] = _realEcoModelOutput[yr - 1, ii, 3] * (1 + _EcoModelResultChgRate[ii, 3] / 100.0);
                    //        _realEcoModelOutput[yr, ii, 4] = _realEcoModelOutput[yr - 1, ii, 4] * (1 + _EcoModelResultChgRate[ii, 4] / 100.0);
                    //        _realEcoModelOutput[yr, ii, 5] = _realEcoModelOutput[yr - 1, ii, 5] * (1 + _EcoModelResultChgRate[ii, 5] / 100.0);
                    //        _realEcoModelOutput[yr, ii, 6] = _realEcoModelOutput[yr - 1, ii, 6] * (1 + _EcoModelResultChgRate[ii, 6] / 100.0);
                    //        _realEcoModelOutput[yr, ii, 7] = _realEcoModelOutput[yr - 1, ii, 7] * (1 + _EcoModelResultChgRate[ii, 7] / 100.0);
                    //        _realEcoModelOutput[yr, ii, 8] = _realEcoModelOutput[yr - 1, ii, 8] * (1 + _EcoModelResultChgRate[ii, 8] / 100.0);
                    //        _realEcoModelOutput[yr, ii, 9] = _realEcoModelOutput[yr - 1, ii, 9] * (1 + _EcoModelResultChgRate[ii, 9] / 100.0);
                    //        //_realEcoModelOutput[yr, ii, 10] = _realEcoModelOutput[yr - 1, ii, 10] * (1 + _EcoModelResultChgRate[ii, 10] / 100.0);    // 非农业用水不通过变化率计算，而是利用地表+地下用水-农业用水（水文模型输出）

                    //        for (int kk = 0; kk < 11; kk++)
                    //        {
                    //            mEcoModelChgRate[yr, ii, kk] = _EcoModelResultChgRate[ii, kk];
                    //        }
                    //        //--------------   计算流域GDP，耕地面积，农业产值，就业人口，地表经济需水，地下经济需水  -----------
                    //        _rbGDP = _rbGDP + _realEcoModelOutput[yr, ii, 0] + _realEcoModelOutput[yr, ii, 1] + _realEcoModelOutput[yr, ii, 2];
                    //        _rbFLA = _rbFLA + _realEcoModelOutput[yr, ii, 3];
                    //        _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 4] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 5] +
                    //                                                                        _realEcoModelOutput[yr, ii, 4];       // 工业用地
                    //        _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 5] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 5] + 
                    //                                                                        _realEcoModelOutput[yr, ii, 5];         // 服务业用地
                    //        _rbArgValue = _rbArgValue + _realEcoModelOutput[yr, ii, 0];
                    //        _rbEmpPop = _rbEmpPop + Double.Parse(GlobalVars.ModelBasicValue[ii + 1, 5]) * GlobalVars.LaborPopPorp * _realEcoModelOutput[yr, ii, 7];
                    //        _rbSurWD = _rbSurWD + _realEcoModelOutput[yr, ii, 8];
                    //        _rbGrdWD = _rbGrdWD + _realEcoModelOutput[yr, ii, 9];
                    //    }
                    //}

                    //// 将县区顺序调整成系统配置文件中县区顺序
                    //for (int ii = 0; ii < _countryname.Length; ii++)
                    //{
                    //    for (int kk = 0; kk < _realEcoModelOutput.GetLength(2); kk++)
                    //    {
                    //        _realEcoModelOutput[yr, _index[ii], kk] = _realEcoModelOutput[yr, ii, kk];
                    //        mEcoModelChgRate[yr, _index[ii], kk] = mEcoModelChgRate[yr, ii, kk];
                    //    }
                    //}
                    _wasttime = (2012 + yr).ToString() + "年：将经济模型输出转变为实际的经济指标运行成功！" + System.DateTime.Now.ToString();
                    Console.WriteLine(_wasttime);

                    //-----------------  再次计算社会模型与土地利用模型，检验生态水文模型之前，增加土地利用模型对整个集成模型的影响  -----------
                    for (int county = 0; county < GlobalVars.CountyName.Length - 1; county++)
                    {
                        //---------------------------------------------------------------------
                        // 土地利用模型，存储的是情景设置中的县区顺序
                        //---------------------------------------------------------------------
                        double _gdp = _realEcoModelOutput[yr, county, 0] + _realEcoModelOutput[yr, county, 1] + _realEcoModelOutput[yr, county, 2];
                        //_forestAreaData[yr, county] = LanduseModels.ForestAreaPre(county + 1, _urbanRateData[yr, county], _gdp, _midTemp[county, yr]);
                        //_grassAreaData[yr, county] = LanduseModels.GrasslandAreaPre(county + 1, _urbanRateData[yr, county], _gdp, _midPreci[county, yr]);
                        //_gradedAreaData[yr, county] = System.Double.Parse(GlobalVars.ModelBasicValue[county + 1, 4]) - _forestAreaData[yr, county] -
                        //                                _grassAreaData[yr, county] - _realEcoModelOutput[yr, county, 3] - _realEcoModelOutput[yr, county, 4] -
                        //                                _realEcoModelOutput[yr, county, 5];             // 退化土地面积=总面积-森林面积-草地面积-农业用地-工业用地-服务业用地
                        //_rbDegradedArea[yr] = _rbDegradedArea[yr] + _gradedAreaData[yr, county];             // 流域退化土地面积

                        ////---- 计算非农业用水量----=总用水量-农业用水量
                        //_realEcoModelOutput[yr, county, 10] = (_realEcoModelOutput[yr, county, 8] + _realEcoModelOutput[yr, county, 9]) *
                        //                                        (1 - double.Parse(mWaterCoefficient[county + 1, 2]));         // 总经济需水量*农业用水系数
                        //_realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 10] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 10] +
                        //                                                                _realEcoModelOutput[yr, county, 10];
                        _wasttime = (2012 + yr).ToString() + "年，" + GlobalVars.CountyName[county] + "县：土地利用模型运行成功！" + System.DateTime.Now.ToString();
                        Console.WriteLine(_wasttime);

                        //---------------------------------------
                        // indicators' models run
                        //---------------------------------------
                        mIndiWP[county, yr] = mIndiModels.WaterProductivity(_gdp, _MidEcoHydroModelOutput[yr, county, 0],                           // 水生产力
                                                                        _MidEcoHydroModelOutput[yr, county, 1]);
                        mIndiWS[county, yr] = mIndiModels.WaterStress((_realEcoModelOutput[yr, county, 8] + _realEcoModelOutput[yr, county, 9]),    // 水压力
                                                                        (_MidEcoHydroModelOutput[yr, county, 0] + _MidEcoHydroModelOutput[yr, county, 1]));
                        mIndiDrinkWater[county,yr] = SocialModels.SafeDrinkWaterPop2(_SocioBasicValue[county,5],                                    // 安全饮用水人口比例
                                                                        Double.Parse(GlobalVars.EconomicScenario[county,7]) / 100.0,yr) / 
                                                                        Double.Parse(GlobalVars.ModelBasicValue[county + 1,5]);
                        if (mIndiDrinkWater[county, yr] > 100.0)
                        {
                            mIndiDrinkWater[county, yr] = 100.0;
                        }
                        mIndiGW[county, yr] = _MidEcoHydroModelOutput[yr, county, 1];                                                               // 地下水开采量
                        mIndiFCR[county, yr] = _forestAreaData[yr, county] / Double.Parse(GlobalVars.ModelBasicValue[county + 1, 4]);                   // 森林覆盖率
                        mIndiGrassCR[county, yr] = _grassAreaData[yr, county] / Double.Parse(GlobalVars.ModelBasicValue[county + 1, 4]);                // 草地覆盖率
                        mIndiGCI[county, yr] = (_forestAreaData[yr, county] + _grassAreaData[yr, county] + _realEcoModelOutput[yr, county, 3]) /    // 绿色覆盖指数=（森林面积+草地面积+耕地面积）/总面积
                                              Double.Parse(GlobalVars.ModelBasicValue[county + 1, 4]);
                        mIndiGDPpc[county, yr] = mIndiModels.GDPperCap(_gdp, Double.Parse(GlobalVars.ModelBasicValue[county + 1, 5]));                  // 人均GDP
                        double _employee = Double.Parse(GlobalVars.ModelBasicValue[county + 1, 5]) * GlobalVars.LaborPopPorp * _realEcoModelOutput[yr, county, 7] / 100.0;        // 就业人口数量=总人口数量*劳动力人口比例（甘肃统计年鉴2016）*就业率
                        mIndiGDPpe[county, yr] = mIndiModels.GDPperCap(_gdp, _employee);                                                            // 就业人口人均GDP
                        mIndiUR[county, yr] = _urbanRateData[yr, county];                                                                           // 城市化率
                        mIndiAWP[county, yr] = _realEcoModelOutput[yr, county, 0] / 
                                            ((_MidEcoHydroModelOutput[yr, county, 0] + _MidEcoHydroModelOutput[yr, county, 1]) *                    // 农业水生产力=农业产值/（地表供水+地下供水）*农业用水比例（这里多除了经济用水比例，是因为之前计算出地表和地下供水后乘了经济用水比例）
                                            (Double.Parse(mWaterCoefficient[county + 1, 2]) / Double.Parse(mWaterCoefficient[county + 1,1])));
                        mIndiAWUE[county, yr] = (_MidEcoHydroModelOutput[yr, county, 2] * Double.Parse(mWaterCoefficient[county + 1, 3])) /
                                            ((_MidEcoHydroModelOutput[yr, county, 0] + _MidEcoHydroModelOutput[yr, county, 1]) *                    // 农业水利用效率=作物总蒸腾量/农业总用水量，即耕地面积ET*作物蒸腾系数/（地表供水+地下供水）*农业用水比例（这里多除了经济用水比例，是因为之前计算出地表和地下供水后乘了经济用水比例）
                                            (Double.Parse(mWaterCoefficient[county + 1, 2]) / Double.Parse(mWaterCoefficient[county + 1, 1])));
                        mIndiFLA[county, yr] = _realEcoModelOutput[yr, county, 3];                                                                  // 耕地面积
                        mIndiGradArea[county, yr] = _gradedAreaData[yr, county];                                                                    // 退化土地面积

                        //-----------  计算流域尺度安全饮用水人口数量   -----------------------
                        _rbSafeWPop[yr] = _rbSafeWPop[yr] + SocialModels.SafeDrinkWaterPop2(_SocioBasicValue[county, 5],                                    // 安全饮用水人口比例
                                                                        Double.Parse(GlobalVars.EconomicScenario[county, 7]) / 100.0, yr);


                        mSDGsOutput[1 + (yr * 11) + county, 0] = (2012 + yr).ToString();                               // 年份
                        mSDGsOutput[1 + (yr * 11) + county, 1] = GlobalVars.CountyName[county];                         // 县区名称
                        mSDGsOutput[1 + (yr * 11) + county, 2] = mIndiWP[county,yr].ToString();                         // 水生产力
                        mSDGsOutput[1 + (yr * 11) + county, 3] = mIndiWS[county,yr].ToString();                         // 水压力
                        mSDGsOutput[1 + (yr * 11) + county, 4] = mIndiDrinkWater[county,yr].ToString();                 // 安全饮用水人口比例
                        mSDGsOutput[1 + (yr * 11) + county, 5] =  mIndiGW[county,yr].ToString();                        // 地下水开采量
                        mSDGsOutput[1 + (yr * 11) + county, 6] = mIndiFCR[county,yr].ToString();                        // 森林覆盖率
                        mSDGsOutput[1 + (yr * 11) + county, 7] = mIndiGrassCR[county,yr].ToString();                    // 草地覆盖率
                        mSDGsOutput[1 + (yr * 11) + county, 8] = mIndiGCI[county,yr].ToString();                        // 绿色覆盖指数
                        mSDGsOutput[1 + (yr * 11) + county, 9] = mIndiGDPpc[county,yr].ToString();                      // 人均GDP
                        mSDGsOutput[1 + (yr * 11) + county, 10] = mIndiGDPpe[county,yr].ToString();                      // 就业人口人均GDP
                        mSDGsOutput[1 + (yr * 11) + county, 11] = mIndiUR[county,yr].ToString();                         // 城市化率
                        mSDGsOutput[1 + (yr * 11) + county, 12] = mIndiAWP[county,yr].ToString();                        // 农业水生产力
                        mSDGsOutput[1 + (yr * 11) + county, 13] = mIndiAWUE[county,yr].ToString();                       // 农业水利用效率
                        mSDGsOutput[1 + (yr * 11) + county, 14] = mIndiFLA[county,yr].ToString();                        // 耕地面积
                        mSDGsOutput[1 + (yr * 11) + county, 15] = mIndiGradArea[county,yr].ToString();                   // 退化土地面积


                        _wasttime = (2012 + yr).ToString() + "年，" + GlobalVars.CountyName[county] + "县：指标模型运行成功！" + System.DateTime.Now.ToString();
                        Console.WriteLine(_wasttime);
                    }


                    ///--------------------------------------------------------
                    ///   流域可持续发展指标计算，将县区指标转换到流域尺度上
                    ///--------------------------------------------------------
                    mIndiWP[GlobalVars.CountyName.Length - 1, yr] = _rbGDP[yr] / (_rbSurWS[yr] + _rbGrdWS[yr]);                           // 水生产力
                    mIndiWS[GlobalVars.CountyName.Length - 1, yr] = (_rbSurWD[yr] + _rbGrdWD[yr]) / (_rbSurWS[yr] + _rbGrdWS[yr]);    // 水压力
                    mIndiDrinkWater[GlobalVars.CountyName.Length - 1, yr] = _rbSafeWPop[yr] * 100.0 / _rbPop[yr];                                    // 安全饮用水人口比例
                    if (mIndiDrinkWater[GlobalVars.CountyName.Length - 1, yr] > 100.0)
                    {
                        mIndiDrinkWater[GlobalVars.CountyName.Length - 1, yr] = 100.0;
                    }
                    mIndiGW[GlobalVars.CountyName.Length - 1, yr] = _rbGrdWS[yr];                                                               // 地下水开采量
                    mIndiFCR[GlobalVars.CountyName.Length - 1, yr] = _rbFstArea[yr] / _rbTotalArea;                   // 森林覆盖率
                    mIndiGrassCR[GlobalVars.CountyName.Length - 1, yr] = _rbGrsArea[yr] / _rbTotalArea;                // 草地覆盖率
                    mIndiGCI[GlobalVars.CountyName.Length - 1, yr] = (_rbFstArea[yr] + _rbGrsArea[yr] + _rbFLA[yr]) / _rbTotalArea;    // 绿色覆盖指数=（森林面积+草地面积+耕地面积）/总面积
                    mIndiGDPpc[GlobalVars.CountyName.Length - 1, yr] = mIndiModels.GDPperCap(_rbGDP[yr], _rbPop[yr]);                  // 人均GDP
                    //double _employee = Double.Parse(GlobalVars.ModelBasicValue[county, 5]) * GlobalVars.LaborPopPorp * _realEcoModelOutput[yr, county, 7];        // 就业人口数量=总人口数量*劳动力人口比例（甘肃统计年鉴2016）*就业率
                    mIndiGDPpe[GlobalVars.CountyName.Length - 1, yr] = mIndiModels.GDPperCap(_rbGDP[yr], _rbEmpPop[yr]);                                                            // 就业人口人均GDP
                    mIndiUR[GlobalVars.CountyName.Length - 1, yr] = _rbUrbPop[yr] * 100.0 / _rbPop[yr];                                                                           // 城市化率
                    mIndiAWP[GlobalVars.CountyName.Length - 1, yr] = _rbArgValue[yr] / ((_rbArgSurWS[yr] + _rbArgGrdWS[yr]));// 农业水生产力=农业产值/（地表供水+地下供水）*农业用水比例（这里多除了经济用水比例，是因为之前计算出地表和地下供水后乘了经济用水比例）
                    mIndiAWUE[GlobalVars.CountyName.Length - 1, yr] = (_rbTotArgET[yr]) / ((_rbArgSurWS[yr] + _rbArgGrdWS[yr]));// 农业水利用效率=作物总蒸腾量/农业总用水量，即耕地面积ET*作物蒸腾系数/（地表供水+地下供水）*农业用水比例（这里多除了经济用水比例，是因为之前计算出地表和地下供水后乘了经济用水比例）
                    mIndiFLA[GlobalVars.CountyName.Length - 1, yr] = _rbFLA[yr];                                                                  // 耕地面积
                    mIndiGradArea[GlobalVars.CountyName.Length - 1, yr] = _rbDegradedArea[yr];                                                                    // 退化土地面积

                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 0] = (2012 + yr).ToString();                               // 年份
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 1] = GlobalVars.CountyName[GlobalVars.CountyName.Length - 1];                         // 县区名称
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 2] = mIndiWP[GlobalVars.CountyName.Length - 1, yr].ToString();                         // 水生产力
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 3] = mIndiWS[GlobalVars.CountyName.Length - 1, yr].ToString();                         // 水压力
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 4] = mIndiDrinkWater[GlobalVars.CountyName.Length - 1, yr].ToString();                 // 安全饮用水人口比例
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 5] = mIndiGW[GlobalVars.CountyName.Length - 1, yr].ToString();                        // 地下水开采量
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 6] = mIndiFCR[GlobalVars.CountyName.Length - 1, yr].ToString();                        // 森林覆盖率
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 7] = mIndiGradArea[GlobalVars.CountyName.Length - 1, yr].ToString();                    // 退化土地面积
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 8] = mIndiGCI[GlobalVars.CountyName.Length - 1, yr].ToString();                        // 绿色覆盖指数
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 9] = mIndiGDPpc[GlobalVars.CountyName.Length - 1, yr].ToString();                      // 人均GDP
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 10] = mIndiGDPpe[GlobalVars.CountyName.Length - 1, yr].ToString();                      // 就业人口人均GDP
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 11] = mIndiUR[GlobalVars.CountyName.Length - 1, yr].ToString();                         // 城市化率
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 12] = mIndiAWP[GlobalVars.CountyName.Length - 1, yr].ToString();                        // 农业水生产力
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 13] = mIndiAWUE[GlobalVars.CountyName.Length - 1, yr].ToString();                       // 农业水利用效率
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 14] = mIndiFLA[GlobalVars.CountyName.Length - 1, yr].ToString();                        // 耕地面积
                    mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 15] = mIndiGrassCR[GlobalVars.CountyName.Length - 1, yr].ToString();                   // 草地覆盖率


                    _wasttime = (2012 + yr).ToString() + "年，流域尺度：指标模型运行成功！" + System.DateTime.Now.ToString();
                    Console.WriteLine(_wasttime);


                    _progressV = (int)(_progressB * yr) + 6;
                    if (_progressV > 100)
                    {
                        _progressV = 100;
                    }
                    if (yr == mSimlong - 1 && _progressV < 100)
                    {
                        _progressV = 100;
                    }
                }
                //------------------------ 集成模型计算结束时间  -----------------------

                //---------------------------------------------------------------------------
                // 输入模型运算结果到CSV文件中，输出数据列表：
                // 年份，县区名称，莺落峡流量（亿立方米），其他河流流量（亿立方米），地表引水量（亿立方米），地下抽水量（亿立方米），耕地面积ET（亿立方米），
                // 农业产值变化率，工业产值变化率，服务业产值变化率，耕地面积变化率，城镇工业用地变化率，服务业用地变化率，水价变化率，就业率变化率，
                // 地表需水量变化率，地下需水量变化率，非农业需水量变化率，农业产值（亿元）,工业产值（亿元）,服务业产值（亿元）,耕地面积（公顷），
                // 城镇工业用地面积（公顷），服务业用地面积（公顷），水价（元/立方米），就业率，地表需水量（亿立方米），地下需水量（亿立方米），
                // 非农业需水量（亿立方米），人口总数（万人），城镇化人口数量（万人），城镇化率（%），温度（），降水（厘米），林地面积（公顷），
                // 草地面积（公顷），湿地面积（公顷），退化土地面积（公顷）
                //---------------------------------------------------------------------------
                string[,] _modeloutput = new string[mSimlong * GlobalVars.CountyName.Length + 1, mModelOuputVarsNum];
                string[,] _rbModelOutput = new string[mSimlong + 1, mModelOuputVarsNum];            // 输出流域尺度模型结果变量
                string[] _modeloutputTitle = new string[39] {"年份","县区名称","莺落峡流量（亿立方米）","其他河流流量（亿立方米）","地表引水量（亿立方米）","地下抽水量（亿立方米）","耕地面积ET（亿立方米）",
                                                            "农业产值变化率","工业产值变化率","服务业产值变化率","耕地面积变化率","城镇工业用地变化率","服务业用地变化率","水价变化率","就业率变化率",
                                                            "地表需水量变化率","地下需水量变化率","非农业需水量变化率","农业产值（亿元）","工业产值（亿元）","服务业产值（亿元）","耕地面积（公顷）",
                                                            "城镇工业用地面积（公顷）","服务业用地面积（公顷）","水价（元/立方米）","就业率","地表需水量（亿立方米）","地下需水量（亿立方米）",
                                                            "非农业需水量（亿立方米）","人口总数（万人）","城镇化人口数量（万人）","城镇化率（%）","温度（）","降水（厘米）","林地面积（公顷）",
                                                            "草地面积（公顷）","湿地面积（公顷）","退化土地面积（公顷）" ,"GDP"};
                for (int i = 0; i < _modeloutputTitle.Length; i++)
                {
                    mIntegModelOut[0, i] = _modeloutputTitle[i];
                    _rbModelOutput[0, i] = _modeloutputTitle[i];
                }
                for (int yr = 0; yr < mSimlong; yr++)
                {
                    for (int ccy = 0; ccy < GlobalVars.CountyName.Length - 1; ccy++)
                    {
                        // 县区数据列表
                        mIntegModelOut[1 + (yr * 11) + ccy, 0] = (2012 + yr).ToString();                                   // 年份
                        mIntegModelOut[1 + (yr * 11) + ccy, 1] = GlobalVars.CountyName[ccy];                               // 县区名称
                        mIntegModelOut[1 + (yr * 11) + ccy, 2] = mYingluoxia[yr].ToString();                               // 莺落峡流量
                        mIntegModelOut[1 + (yr * 11) + ccy, 3] = mOtherRivers[yr].ToString();                              // 其他河流流量
                        mIntegModelOut[1 + (yr * 11) + ccy, 4] = _MidEcoHydroModelOutput[yr, ccy, 0].ToString();           // 地表水引水量
                        mIntegModelOut[1 + (yr * 11) + ccy, 5] = _MidEcoHydroModelOutput[yr, ccy, 1].ToString();           // 地下水抽水量
                        mIntegModelOut[1 + (yr * 11) + ccy, 6] = _MidEcoHydroModelOutput[yr, ccy, 2].ToString();           // 耕地面积ET
                        mIntegModelOut[1 + (yr * 11) + ccy, 7] = mEcoModelChgRate[yr, ccy, 0].ToString();                  // 农业产值变化率
                        mIntegModelOut[1 + (yr * 11) + ccy, 8] = mEcoModelChgRate[yr, ccy, 1].ToString();                  // 工业产值变化率
                        mIntegModelOut[1 + (yr * 11) + ccy, 9] = mEcoModelChgRate[yr, ccy, 2].ToString();                   // 服务业产值变化率
                        mIntegModelOut[1 + (yr * 11) + ccy, 10] = mEcoModelChgRate[yr, ccy, 3].ToString();                   // 耕地面积变化率
                        mIntegModelOut[1 + (yr * 11) + ccy, 11] = mEcoModelChgRate[yr, ccy, 4].ToString();                   // 城镇工业用地变化率
                        mIntegModelOut[1 + (yr * 11) + ccy, 12] = mEcoModelChgRate[yr, ccy, 5].ToString();                   // 服务业用地变化率
                        mIntegModelOut[1 + (yr * 11) + ccy, 13] = mEcoModelChgRate[yr, ccy, 6].ToString();                   // 水价变化率
                        mIntegModelOut[1 + (yr * 11) + ccy, 14] = mEcoModelChgRate[yr, ccy, 7].ToString();                   // 就业率变化率
                        mIntegModelOut[1 + (yr * 11) + ccy, 15] = mEcoModelChgRate[yr, ccy, 8].ToString();                   // 经济地表需水变化率
                        mIntegModelOut[1 + (yr * 11) + ccy, 16] = mEcoModelChgRate[yr, ccy, 9].ToString();                   // 经济地下需水变化率
                        mIntegModelOut[1 + (yr * 11) + ccy, 17] = mEcoModelChgRate[yr, ccy, 10].ToString();                   // 非农业需水变化率
                        mIntegModelOut[1 + (yr * 11) + ccy, 18] = _realEcoModelOutput[yr, ccy, 0].ToString();                  // 农业产值
                        mIntegModelOut[1 + (yr * 11) + ccy, 19] = _realEcoModelOutput[yr, ccy, 1].ToString();                  // 工业产值
                        mIntegModelOut[1 + (yr * 11) + ccy, 20] = _realEcoModelOutput[yr, ccy, 2].ToString();                   // 服务业产值
                        mIntegModelOut[1 + (yr * 11) + ccy, 21] = _realEcoModelOutput[yr, ccy, 3].ToString();                   // 耕地面积
                        mIntegModelOut[1 + (yr * 11) + ccy, 22] = _realEcoModelOutput[yr, ccy, 4].ToString();                   // 城镇工业用地
                        mIntegModelOut[1 + (yr * 11) + ccy, 23] = _realEcoModelOutput[yr, ccy, 5].ToString();                   // 服务业用地
                        mIntegModelOut[1 + (yr * 11) + ccy, 24] = _realEcoModelOutput[yr, ccy, 6].ToString();                   // 水价
                        mIntegModelOut[1 + (yr * 11) + ccy, 25] = _realEcoModelOutput[yr, ccy, 7].ToString();                   // 就业率
                        mIntegModelOut[1 + (yr * 11) + ccy, 26] = _realEcoModelOutput[yr, ccy, 8].ToString();                   // 经济地表需水
                        mIntegModelOut[1 + (yr * 11) + ccy, 27] = _realEcoModelOutput[yr, ccy, 9].ToString();                   // 经济地下需水
                        mIntegModelOut[1 + (yr * 11) + ccy, 28] = _realEcoModelOutput[yr, ccy, 10].ToString();                   // 非农业需水
                        mIntegModelOut[1 + (yr * 11) + ccy, 29] = _populationData[yr, ccy].ToString();                          // 人口数量
                        mIntegModelOut[1 + (yr * 11) + ccy, 30] = (_populationData[yr, ccy] * _urbanRateData[yr, ccy] / 100.0).ToString();  // 城镇人口数量
                        mIntegModelOut[1 + (yr * 11) + ccy, 31] = _urbanRateData[yr, ccy].ToString();                           // 城镇化率
                        mIntegModelOut[1 + (yr * 11) + ccy, 32] = mMidRealTemp[yr, ccy].ToString();                             // 温度
                        mIntegModelOut[1 + (yr * 11) + ccy, 33] = mMidRealpric[yr, ccy].ToString();                             // 降水
                        mIntegModelOut[1 + (yr * 11) + ccy, 34] = _forestAreaData[yr, ccy].ToString();                          // 林地面积
                        mIntegModelOut[1 + (yr * 11) + ccy, 35] = _grassAreaData[yr, ccy].ToString();                           // 草地面积
                        mIntegModelOut[1 + (yr * 11) + ccy, 36] = "0";                                                          // 湿地面积
                        mIntegModelOut[1 + (yr * 11) + ccy, 37] = _gradedAreaData[yr, ccy].ToString();                          // 退化土地面积
                        mIntegModelOut[1 + (yr * 11) + ccy, 38] = (_realEcoModelOutput[yr, ccy, 0] + 
                                                                    _realEcoModelOutput[yr, ccy, 1] + 
                                                                    _realEcoModelOutput[yr, ccy, 2]).ToString();                // GDP

                    }
                    // 流域数据列表
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 0] = (2012 + yr).ToString();                                   // 年份
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 1] = GlobalVars.CountyName[GlobalVars.CountyName.Length - 1];                               // 县区名称
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 2] = mYingluoxia[yr].ToString();                               // 莺落峡流量
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 3] = mOtherRivers[yr].ToString();                              // 其他河流流量
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 4] = _rbSurWS[yr].ToString();           // 地表水引水量
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 5] = _rbGrdWS[yr].ToString();           // 地下水抽水量
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 6] = _rbTotArgET[yr].ToString();           // 耕地面积ET
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 7] = mEcoModelChgRate[yr, GlobalVars.CountyName.Length - 1, 0].ToString();                  // 农业产值变化率
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 8] = mEcoModelChgRate[yr, GlobalVars.CountyName.Length - 1, 1].ToString();                  // 工业产值变化率
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 9] = mEcoModelChgRate[yr, GlobalVars.CountyName.Length - 1, 2].ToString();                   // 服务业产值变化率
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 10] = mEcoModelChgRate[yr, GlobalVars.CountyName.Length - 1, 3].ToString();                   // 耕地面积变化率
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 11] = mEcoModelChgRate[yr, GlobalVars.CountyName.Length - 1, 4].ToString();                   // 城镇工业用地变化率
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 12] = mEcoModelChgRate[yr, GlobalVars.CountyName.Length - 1, 5].ToString();                   // 服务业用地变化率
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 13] = mEcoModelChgRate[yr, GlobalVars.CountyName.Length - 1, 6].ToString();                   // 水价变化率
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 14] = mEcoModelChgRate[yr, GlobalVars.CountyName.Length - 1, 7].ToString();                   // 就业率变化率
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 15] = mEcoModelChgRate[yr, GlobalVars.CountyName.Length - 1, 8].ToString();                   // 经济地表需水变化率
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 16] = mEcoModelChgRate[yr, GlobalVars.CountyName.Length - 1, 9].ToString();                   // 经济地下需水变化率
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 17] = mEcoModelChgRate[yr, GlobalVars.CountyName.Length - 1, 10].ToString();                   // 非农业需水变化率
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 18] = _rbArgValue[yr].ToString();                  // 农业产值
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 19] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 1].ToString();    // _rbIndValue[yr].ToString();                  // 工业产值
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 20] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 2].ToString();    // _rbSerValue[yr].ToString();                   // 服务业产值
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 21] = _rbFLA[yr].ToString();                   // 耕地面积
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 22] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 4].ToString();                   // 城镇工业用地
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 23] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 5].ToString();                   // 服务业用地
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 24] = _realEcoModelOutput[yr, 0, 6].ToString();                   // 水价，取第一个县的水价为流域水价
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 25] = (_rbEmpPop[yr] * 100.0 / (_rbPop[yr] * GlobalVars.LaborPopPorp)).ToString();                   // 就业率
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 26] = _rbSurWD[yr].ToString();                   // 经济地表需水
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 27] = _rbGrdWD[yr].ToString();                   // 经济地下需水
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 28] = _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 10].ToString();                   // 非农业需水
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 29] = _rbPop[yr].ToString();                          // 人口数量
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 30] = _rbUrbPop[yr].ToString();  // 城镇人口数量
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 31] = (_rbUrbPop[yr] * 100.0 / _rbPop[yr]).ToString();                           // 城镇化率
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 32] = mMidRealTemp[yr, 0].ToString();                             // 温度
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 33] = mMidRealpric[yr, 0].ToString();                             // 降水
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 34] = _rbFstArea[yr].ToString();                          // 林地面积
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 35] = _rbGrsArea[yr].ToString();                           // 草地面积
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 36] = "0";                                                          // 湿地面积
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 37] = _rbDegradedArea[yr].ToString();                          // 退化土地面积
                    mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, 38] = (_rbArgValue[yr] + 
                                                                                        _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 1] + 
                                                                                        _realEcoModelOutput[yr, GlobalVars.CountyName.Length - 1, 2]).ToString();       //GDP
                }
                string _modelouputPath = GlobalVars.ProgramDirectory;
                mGFunc.WriteGlobleCSVFile2(_modelouputPath, "ModelOuput", mIntegModelOut, ",");

                //Spire.Xls.Workbook _workbookModel = new Spire.Xls.Workbook();
                //_workbookModel.LoadFromFile(_modelouputPath + @"\ModelOuput.CSV",",");
                //_workbookModel.SaveToFile(_modelouputPath + @"\ModelOuput.xls", Spire.Xls.ExcelVersion.Version97to2003);                  


                // 输出流域尺度模型计算结果
                for (int yr = 0; yr < mSimlong; yr++)
                {
                    for (int jj = 0; jj < mModelOuputVarsNum; jj++)
                    {
                        _rbModelOutput[yr + 1, jj] = mIntegModelOut[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, jj];
                    }
                }
                mGFunc.WriteGlobleCSVFile2(_modelouputPath, "RB_ModelOutput", _rbModelOutput, ",");

                //Spire.Xls.Workbook _workbookRBModel = new Spire.Xls.Workbook();
                //_workbookRBModel.LoadFromFile(_modelouputPath + @"\RB_ModelOutput.CSV", ",");
                //_workbookRBModel.SaveToFile(_modelouputPath + @"\RB_ModelOutput.xls", Spire.Xls.ExcelVersion.Version97to2003);                  

                //---------------------------------------------------------------------------
                // 输出指标计算结果到CSV文件中，表头结构：日期，县区，水生产力，水压力，安全饮用水人口比例，地下水开采量，森林覆盖率，草地覆盖率,
                // 绿色覆盖指数，人均GDP，就业人口人均GDP，城市化率，农业水生产力，农业水利用效率，耕地面积
                //---------------------------------------------------------------------------
                // 初始化表头
                string[] _indicatortitle = new string[16] {"日期","县区","水生产力（元/立方米）","水压力","安全饮用水人口比例",
                                                        "地下水开采量（亿立方米）","森林覆盖率","退化土地面积（公顷）","绿色覆盖指数",
                                                        "人均GDP（元）","就业人口人均GDP（元）","城市化率","农业水生产力（元/立方米）",
                                                        "农业水利用效率","耕地面积（公顷）","草地覆盖率"};
                string[,] _rbSDGsIndiOutput = new string[mSimlong + 1, 16];
                for (int ii = 0; ii < _indicatortitle.Length; ii++)
                {
                    mSDGsOutput[0, ii] = _indicatortitle[ii];
                    _rbSDGsIndiOutput[0, ii] = _indicatortitle[ii];
                }
                mGFunc.WriteGlobleCSVFile2(_modelouputPath, "SDGsOutput", mSDGsOutput, ",");

                //Spire.Xls.Workbook _workbookSDGs = new Spire.Xls.Workbook();
                //_workbookSDGs.LoadFromFile(_modelouputPath + @"\SDGsOutput.CSV", ",");
                //_workbookSDGs.SaveToFile(_modelouputPath + @"\SDGsOutput.xls", Spire.Xls.ExcelVersion.Version97to2003);                  


                // 输出流域尺度可持续发展指标值
                for (int yr = 0; yr < mSimlong; yr++)
                {
                    for (int jj = 0; jj < _indicatortitle.Length; jj++)
                    {
                        _rbSDGsIndiOutput[yr + 1, jj] = mSDGsOutput[1 + (yr * 11) + GlobalVars.CountyName.Length - 1, jj];
                    }
                }
                mGFunc.WriteGlobleCSVFile2(_modelouputPath, "RB_SDGsOutput", _rbSDGsIndiOutput, ",");

                //Spire.Xls.Workbook _workbookRBSDGs = new Spire.Xls.Workbook();
                //_workbookRBSDGs.LoadFromFile(_modelouputPath + @"\RB_SDGsOutput.CSV", ",");
                //_workbookRBSDGs.SaveToFile(_modelouputPath + @"\RB_SDGsOutput.xls", Spire.Xls.ExcelVersion.Version97to2003);                  


                //---------------------------------------------------
                // SDGs assessment model run for each county
                //---------------------------------------------------
                // init assessment model parameters file
                // 修改目标权重文件
                string[] _goalweight = new string[GlobalVars.GoalWeight.Length];
                string _assessfile = "";
                //----   get python program file path according to weight type (e.g., subjective or objective)  ----------
                GlobalVars.PythonPath = @"C:\python27\python.exe";          // @"C:\Users\Gyc\PycharmProjects\untitled\venv\Scripts\python.exe";
                if (GlobalVars.WeightType == 0)
                {
                    // 主观评价方法
                    // subjective
                    GlobalVars.AssessModelPath = mCurrpath + @"\AssessmentModel\Subjective\fast_test_subjective_classification.py";  //mCurrpath + @"\Assessment Model\Subjective\fast_test_subjective_classification.py";//这里是python的文件名字
                    // 目标权重输入文件修改
                    string _tempgoal = GlobalVars.GoalWeight[0].ToString();
                    for (int i = 1; i < GlobalVars.GoalWeight.Length; i++)
                    {
                        _tempgoal = _tempgoal + " " + GlobalVars.GoalWeight[i].ToString();
                    }
                    _goalweight[0] = _tempgoal;
                    _assessfile = mCurrpath + @"\Assessmentmodel\Subjective\goalweight.txt";
                    GlobalFunctions.WriteTextFile(_assessfile, _goalweight);

                    _wasttime = "主观评价模型目标权重输入文件修改运行成功！";
                    Console.WriteLine(_wasttime);

                    // 指标个数输入文件修改
                    int _counSDg = 0, _sdgTotalIndi = 1;        // 总指标个数，add temperature  as inverse indicator, so _sdgTotalIndi starts from 1
                    bool _sdgAdjust = false;
                    string _sdgIndNum = "";
                    System.Collections.ArrayList _sdgTempIndWeight = new System.Collections.ArrayList();
                    System.Collections.ArrayList _sdgIndType = new System.Collections.ArrayList();
                    for (int cc = 1; cc < GlobalVars.SDGs.GetLength(0); cc++)
                    {
                        if (GlobalVars.SDGs[cc, 0] == "1" && GlobalVars.SDGs[cc,2] == "true")
                        {
                            _sdgIndNum = _sdgIndNum + " " + _counSDg.ToString();
                            _sdgTotalIndi = _sdgTotalIndi + _counSDg;
                            _counSDg = 0;
                        }
                        else if (GlobalVars.SDGs[cc, 0] == "3" && GlobalVars.SDGs[cc, 2] == "true")
                        {
                            _sdgTempIndWeight.Add(GlobalVars.SDGs[cc,3]);
                            _sdgIndType.Add(GlobalVars.SDGs[cc, 4]);
                            _counSDg++;
                        }
                    }
                    _sdgIndNum = (_sdgIndNum + " " + (_counSDg + 1).ToString()).Trim();             // 增加一个温度评价指标
                    _sdgTotalIndi = _sdgTotalIndi + _counSDg;
                    string[] _sdgIndicatorNum = new string[1];
                    _sdgIndicatorNum[0] = _sdgIndNum;
                    _assessfile = mCurrpath + @"\Assessmentmodel\Subjective\goals_number.txt";
                    GlobalFunctions.WriteTextFile(_assessfile, _sdgIndicatorNum);

                    _wasttime = "主观评价模型指标个数输入文件修改运行成功！";
                    Console.WriteLine(_wasttime);

                    // 指标权重文件修改
                    string[] _sdgIndWeight = new string[_sdgTotalIndi];
                    for (int cc = 0; cc < _sdgTotalIndi - 1; cc++)
                    {
                        _sdgIndWeight[cc] = _sdgTempIndWeight[cc].ToString();
                    }
                    _sdgIndWeight[_sdgTotalIndi - 1] = (Double.Parse(_sdgTempIndWeight[_sdgTotalIndi - 2].ToString()) * 2.0).ToString();

                    _assessfile = mCurrpath + @"\Assessmentmodel\Subjective\Subjective_weight.txt";
                    GlobalFunctions.WriteTextFile(_assessfile, _sdgIndWeight);


                    // 数据文件输入修改，datacal 001.txt,第一行为指标正逆项属性
                    // 构建数据文件所需数据集，计算的空间尺度为流域
                    string[,] _sdgData = new string[mSimlong + 1,_sdgTotalIndi + 1];
                    string[] _sdgDataFile = new string[mSimlong + 1];
                    _sdgData[0, 0] = "5";
                    for (int i = 0; i < _sdgTotalIndi - 1; i++)
                    {
                        _sdgData[0, i + 1] = _sdgIndType[i].ToString().Trim();
                    }
                    _sdgData[0, _sdgTotalIndi] = "0";          // 温度为逆向指标

                    // 每个县区面积占整个流域面积的比例，作为温度加权的权重
                    double[] _areaweight = new double[GlobalVars.CountyName.Length - 1];
                    for (int county = 0; county < GlobalVars.CountyName.Length - 1; county++)
                    {
                        _areaweight[county] =  Double.Parse(GlobalVars.ModelBasicValue[county + 1, 4]) / mWatershedArea;                              // 流域总面积

                    }


                    for (int yr = 1; yr < mSimlong + 1; yr++)
                    {
                        _sdgData[yr, 0] = (GlobalVars.SimStartYear + yr - 1).ToString();                           // 年份
                        _sdgData[yr, 1] = mIndiWP[GlobalVars.CountyName.Length - 1, yr - 1].ToString();                // 水生产力
                        _sdgData[yr, 2] = mIndiWS[GlobalVars.CountyName.Length - 1, yr - 1].ToString();                // 水压力
                        _sdgData[yr, 3] = mIndiDrinkWater[GlobalVars.CountyName.Length - 1, yr - 1].ToString();        // 安全饮用水人口比例
                        _sdgData[yr, 4] = mIndiGW[GlobalVars.CountyName.Length - 1, yr - 1].ToString();                // 地下水引用量
                        _sdgData[yr, 5] = mIndiFCR[GlobalVars.CountyName.Length - 1, yr - 1].ToString();               // 森林覆盖率
                        _sdgData[yr, 6] = mIndiGradArea[GlobalVars.CountyName.Length - 1, yr - 1].ToString();          // 退化土地面积  mIndiGrassCR[GlobalVars.CountyName.Length - 1, yr].ToString();           // 草地覆盖率
                        _sdgData[yr, 7] = mIndiGCI[GlobalVars.CountyName.Length - 1, yr - 1].ToString();               // 绿色覆盖指数
                        _sdgData[yr, 8] = mIndiGDPpc[GlobalVars.CountyName.Length - 1, yr - 1].ToString();             // 人均GDP
                        _sdgData[yr, 9] = mIndiGDPpe[GlobalVars.CountyName.Length - 1, yr - 1].ToString();             // 就业人口人均GDP
                        _sdgData[yr, 10] = mIndiUR[GlobalVars.CountyName.Length - 1, yr - 1].ToString();               // 城市化率
                        _sdgData[yr, 11] = mIndiAWP[GlobalVars.CountyName.Length - 1, yr - 1].ToString();              // 农业水生产力
                        _sdgData[yr, 12] = mIndiAWUE[GlobalVars.CountyName.Length - 1, yr - 1].ToString();             // 农业水利用效率
                        _sdgData[yr, 13] = mIndiFLA[GlobalVars.CountyName.Length - 1, yr - 1].ToString();              // 耕地面积

                        // 根据县区面积线性加权平均，求流域平均温度
                        double _watershedtemp = 0;
                        for (int coun = 0; coun < GlobalVars.CountyName.Length - 1; coun++)
                        {
                            _watershedtemp = _watershedtemp + _areaweight[coun] * mMidRealTemp[yr - 1, coun];           // 温度,利用县区面积占比作为权重，加权平均
                        }
                        _sdgData[yr, 14] = _watershedtemp.ToString();
                    }
                    for (int i = 0; i < mSimlong + 1; i++)
                    {
                        _sdgDataFile[i] = _sdgData[i,0];
                        for (int j = 1; j < _sdgTotalIndi; j++)
                        {
                            _sdgDataFile[i] = _sdgDataFile[i] + "\t" + _sdgData[i, j];
                        }
                        if (i < mSimlong)
                        {
                            _sdgDataFile[i] = _sdgDataFile[i] + "\t" + _sdgData[i, _sdgTotalIndi] + "\r\n";
                        }
                        else
                        {
                            _sdgDataFile[i] = _sdgDataFile[i] + "\t" + _sdgData[i, _sdgTotalIndi];
                        }
                    }
                    string _data001path = mCurrpath + @"\Assessmentmodel\Subjective\datacal 001.txt";

                    GlobalFunctions.WriteTextFile(_data001path, _sdgDataFile);


                    _wasttime = "主观评价模型指标权重输入文件修改运行成功！";
                    Console.WriteLine(_wasttime);

                    // 运行主观评价模型
                    // run assessment model
                    string _assResult = SDGsAssessModel.RunPythonScript(GlobalVars.AssessModelPath, GlobalVars.PythonPath, 1);

                    _wasttime = "主观评价模型运行成功！";
                    Console.WriteLine(_wasttime);

                    // 读取评价模型输出结果,生成评价模型文件，CSV
                    string _assessrespath = mCurrpath + @"\Assessmentmodel\Subjective\result.txt";
                    string[,] _assessIndicator = new string[mSimlong + 1, GlobalVars.GoalWeight.Length + 2];    // title, 0:year, 1:水资源可持续发展指数，2：生态系统可持续发展指数；3：经济可持续发展指数；4：流域综合可持续发展指数
                    string[,] _assessIndGrade = new string[mSimlong + 1, GlobalVars.GoalWeight.Length + 2];     // title, 0:year, 1:水资源可持续发展等级，2：生态系统可持续发展等级；3：经济可持续发展等级；4：流域综合可持续发展等级
                    System.Collections.ArrayList _assessResult = new System.Collections.ArrayList();
                    _assessResult = GlobalFunctions.ReadTextFile(_assessrespath);
                    _assessIndicator[0, 0] = "年份";
                    _assessIndicator[0, 1] = "水资源可持续发展指数";
                    _assessIndicator[0, 2] = "生态系统可持续发展指数";
                    _assessIndicator[0, 3] = "社会经济可持续发展指数";
                    _assessIndicator[0, 4] = "流域综合可持续发展指数";
                    _assessIndGrade[0, 0] = "年份";
                    _assessIndGrade[0, 1] = "水资源可持续发展等级";
                    _assessIndGrade[0, 2] = "生态系统可持续发展等级";
                    _assessIndGrade[0, 3] = "社会经济可持续发展等级";
                    _assessIndGrade[0, 4] = "流域综合可持续发展等级";
                    string[] _reviseResult = new string[8];
                    int _ccindex = -1;
                    for (int ii = 0; ii < _assessResult.Count; ii++)
                    {
                        if (_assessResult[ii].ToString().Contains("goal") || _assessResult[ii].ToString().Contains("total"))
                        {
                            _ccindex++;
                            _reviseResult[_ccindex] = _assessResult[ii].ToString();
                        }
                        else
                        {
                            _reviseResult[_ccindex] = _reviseResult[_ccindex] + "  " + _assessResult[ii].ToString();
                        }
                    }
                    // 水资源可持续发展指数
                    string[] _tempWI = GlobalFunctions.GetSDGIndex(_reviseResult[0].ToString(), mSimlong, ' ');     
                    // 生态可持续发展指数
                    string[] _tempEI = GlobalFunctions.GetSDGIndex(_reviseResult[1].ToString(), mSimlong, ' ');
                    // 社会经济可持续发展指数
                    string[] _tempSEI = GlobalFunctions.GetSDGIndex(_reviseResult[2].ToString(), mSimlong, ' ');
                    // 流域综合可持续发展指数
                    string[] _tempRBI = GlobalFunctions.GetSDGIndex(_reviseResult[3].ToString(), mSimlong, ' ');
                    // 水资源可持续发展等级
                    string[] _tempWID = GlobalFunctions.GetSDGIndex(_reviseResult[4].ToString(), mSimlong, ' ');
                    // 生态可持续发展等级
                    string[] _tempEID = GlobalFunctions.GetSDGIndex(_reviseResult[5].ToString(), mSimlong, ' ');
                    // 社会经济可持续发展等级
                    string[] _tempSEID = GlobalFunctions.GetSDGIndex(_reviseResult[6].ToString(), mSimlong, ' ');
                    // 流域综合可持续发展等级
                    string[] _tempRBID = GlobalFunctions.GetSDGIndex(_reviseResult[7].ToString(), mSimlong, ' ');

                    for (int ii = 0; ii < mSimlong; ii++)
                    {
                        _assessIndicator[1 + ii, 0] = (GlobalVars.SimStartYear + ii).ToString();            // 年份
                        _assessIndicator[1 + ii, 1] = _tempWI[ii];            // 水资源可持续发展指数
                        _assessIndicator[1 + ii, 2] = _tempEI[ii];            // 生态系统可持续发展指数
                        _assessIndicator[1 + ii, 3] = _tempSEI[ii];            // 社会经济可持续发展指数
                        _assessIndicator[1 + ii, 4] = _tempRBI[ii];            // 流域综合可持续发展指数

                        _assessIndGrade[1 + ii, 0] = (GlobalVars.SimStartYear + ii).ToString();            // 年份
                        _assessIndGrade[1 + ii, 1] = _tempWID[ii];            // 水资源可持续发展等级
                        _assessIndGrade[1 + ii, 2] = _tempEID[ii];            // 生态系统可持续发展等级
                        _assessIndGrade[1 + ii, 3] = _tempSEID[ii];            // 社会经济可持续发展等级
                        _assessIndGrade[1 + ii, 4] = _tempRBID[ii];            // 流域综合可持续发展等级
                    }
                    // 生成可持续发展指数文件
                    mGFunc.WriteGlobleCSVFile2(_modelouputPath, "SDGsIndex", _assessIndicator, ",");

                    //Spire.Xls.Workbook _workbook = new Spire.Xls.Workbook();
                    //_workbook.LoadFromFile(_modelouputPath + @"\SDGsIndex.CSV", ",");
                    //_workbook.SaveToFile(_modelouputPath + @"\SDGsIndex.xls", Spire.Xls.ExcelVersion.Version97to2003);                  
                    
                    
                    // 生成可持续发展等级文件
                    mGFunc.WriteGlobleCSVFile2(_modelouputPath, "SDGsIndicatorGrade", _assessIndGrade, ",");

                    //Spire.Xls.Workbook _workbook1 = new Spire.Xls.Workbook();
                    //_workbook1.LoadFromFile(_modelouputPath + @"\SDGsIndicatorGrade.CSV", ",");
                    //_workbook1.SaveToFile(_modelouputPath + @"\SDGsIndicatorGrade.xls", Spire.Xls.ExcelVersion.Version97to2003);                  

                }

                else if (GlobalVars.WeightType == 1)
                {
                    // 客观评价方法
                    // subjective
                    GlobalVars.AssessModelPath = mCurrpath + @"\AssessmentModel\Objective\fast_test_objective_classification.py";  //mCurrpath + @"\Assessment Model\Subjective\fast_test_subjective_classification.py";//这里是python的文件名字
                    // 目标权重输入文件修改
                    string _tempgoal = GlobalVars.GoalWeight[0].ToString();
                    for (int i = 1; i < GlobalVars.GoalWeight.Length; i++)
                    {
                        _tempgoal = _tempgoal + " " + GlobalVars.GoalWeight[i].ToString();
                    }
                    _goalweight[0] = _tempgoal;
                    _assessfile = mCurrpath + @"\Assessmentmodel\Objective\goalweight.txt";
                    GlobalFunctions.WriteTextFile(_assessfile, _goalweight);

                    _wasttime = "客观评价模型目标权重输入文件修改运行成功！";
                    Console.WriteLine(_wasttime);

                    // 指标个数输入文件修改
                    int _counSDg = 0, _sdgTotalIndi = 1;        // 总指标个数,add temperature  as inverse indicator, so _sdgTotalIndi starts from 1
                    bool _sdgAdjust = false;
                    string _sdgIndNum = "";
                    System.Collections.ArrayList _sdgTempIndWeight = new System.Collections.ArrayList();
                    System.Collections.ArrayList _sdgIndType = new System.Collections.ArrayList();
                    for (int cc = 1; cc < GlobalVars.SDGs.GetLength(0); cc++)
                    {
                        if (GlobalVars.SDGs[cc, 0] == "1" && GlobalVars.SDGs[cc, 2] == "true")
                        {
                            _sdgIndNum = _sdgIndNum + " " + _counSDg.ToString();
                            _sdgTotalIndi = _sdgTotalIndi + _counSDg;
                            _counSDg = 0;
                        }
                        else if (GlobalVars.SDGs[cc, 0] == "3" && GlobalVars.SDGs[cc, 2] == "true")
                        {
                            _sdgTempIndWeight.Add(GlobalVars.SDGs[cc, 3]);
                            _sdgIndType.Add(GlobalVars.SDGs[cc, 4]);
                            _counSDg++;
                        }
                    }
                    _sdgIndNum = (_sdgIndNum + " " + (_counSDg + 1).ToString()).Trim();             // 增加一个温度评价指标
                    _sdgTotalIndi = _sdgTotalIndi + _counSDg;
                    string[] _sdgIndicatorNum = new string[1];
                    _sdgIndicatorNum[0] = _sdgIndNum;
                    _assessfile = mCurrpath + @"\Assessmentmodel\Objective\goals_number.txt";
                    GlobalFunctions.WriteTextFile(_assessfile, _sdgIndicatorNum);

                    _wasttime = "客观评价模型指标个数输入文件修改运行成功！";
                    Console.WriteLine(_wasttime);

                    // 指标权重文件修改
                    //string[] _sdgIndWeight = new string[_sdgTotalIndi];
                    //for (int cc = 0; cc < _sdgTotalIndi; cc++)
                    //{
                    //    _sdgIndWeight[cc] = _sdgTempIndWeight[cc].ToString();
                    //}
                    //_assessfile = mCurrpath + @"\Assessmentmodel\Subjective\Subjective_weight.txt";
                    //GlobalFunctions.WriteTextFile(_assessfile, _sdgIndWeight);


                    // 数据文件输入修改，datacal 001.txt,第一行为指标正逆项属性
                    // 构建数据文件所需数据集，计算的空间尺度为流域
                    string[,] _sdgData = new string[mSimlong + 1, _sdgTotalIndi + 1];
                    string[] _sdgDataFile = new string[mSimlong + 1];
                    _sdgData[0, 0] = "5";
                    for (int i = 0; i < _sdgTotalIndi - 1; i++)
                    {
                        _sdgData[0, i + 1] = _sdgIndType[i].ToString().Trim();
                    }
                    _sdgData[0, _sdgTotalIndi] = "0";          // 温度为逆向指标

                    // 每个县区面积占整个流域面积的比例，作为温度加权的权重
                    double[] _areaweight = new double[GlobalVars.CountyName.Length - 1];
                    for (int county = 0; county < GlobalVars.CountyName.Length - 1; county++)
                    {
                        _areaweight[county] = Double.Parse(GlobalVars.ModelBasicValue[county + 1, 4]) / mWatershedArea;                              // 流域总面积

                    }


                    for (int yr = 1; yr < mSimlong + 1; yr++)
                    {
                        _sdgData[yr, 0] = (GlobalVars.SimStartYear + yr - 1).ToString();                           // 年份
                        _sdgData[yr, 1] = mIndiWP[GlobalVars.CountyName.Length - 1, yr - 1].ToString();                // 水生产力
                        _sdgData[yr, 2] = mIndiWS[GlobalVars.CountyName.Length - 1, yr - 1].ToString();                // 水压力
                        _sdgData[yr, 3] = mIndiDrinkWater[GlobalVars.CountyName.Length - 1, yr - 1].ToString();        // 安全饮用水人口比例
                        _sdgData[yr, 4] = mIndiGW[GlobalVars.CountyName.Length - 1, yr - 1].ToString();                // 地下水引用量
                        _sdgData[yr, 5] = mIndiFCR[GlobalVars.CountyName.Length - 1, yr - 1].ToString();               // 森林覆盖率
                        _sdgData[yr, 6] = mIndiGradArea[GlobalVars.CountyName.Length - 1, yr - 1].ToString();          // 退化土地面积  mIndiGrassCR[GlobalVars.CountyName.Length - 1, yr].ToString();           // 草地覆盖率
                        _sdgData[yr, 7] = mIndiGCI[GlobalVars.CountyName.Length - 1, yr - 1].ToString();               // 绿色覆盖指数
                        _sdgData[yr, 8] = mIndiGDPpc[GlobalVars.CountyName.Length - 1, yr - 1].ToString();             // 人均GDP
                        _sdgData[yr, 9] = mIndiGDPpe[GlobalVars.CountyName.Length - 1, yr - 1].ToString();             // 就业人口人均GDP
                        _sdgData[yr, 10] = mIndiUR[GlobalVars.CountyName.Length - 1, yr - 1].ToString();               // 城市化率
                        _sdgData[yr, 11] = mIndiAWP[GlobalVars.CountyName.Length - 1, yr - 1].ToString();              // 农业水生产力
                        _sdgData[yr, 12] = mIndiAWUE[GlobalVars.CountyName.Length - 1, yr - 1].ToString();             // 农业水利用效率
                        _sdgData[yr, 13] = mIndiFLA[GlobalVars.CountyName.Length - 1, yr - 1].ToString();              // 耕地面积


                        // 根据县区面积线性加权平均，求流域平均温度
                        double _watershedtemp = 0;
                        for (int coun = 0; coun < GlobalVars.CountyName.Length - 1; coun++)
                        {
                            _watershedtemp = _watershedtemp + _areaweight[coun] * mMidRealTemp[yr - 1, coun];           // 温度,利用县区面积占比作为权重，加权平均
                        }
                        _sdgData[yr, 14] = _watershedtemp.ToString();
                    }
                    for (int i = 0; i < mSimlong + 1; i++)
                    {
                        _sdgDataFile[i] = _sdgData[i, 0];
                        for (int j = 1; j < _sdgTotalIndi; j++)
                        {
                            _sdgDataFile[i] = _sdgDataFile[i] + "\t" + _sdgData[i, j];
                        }
                        if (i < mSimlong)
                        {
                            _sdgDataFile[i] = _sdgDataFile[i] + "\t" + _sdgData[i, _sdgTotalIndi] + "\r\n"; ;
                        }
                        else
                        {
                            _sdgDataFile[i] = _sdgDataFile[i] + "\t" + _sdgData[i, _sdgTotalIndi];
                        }
                    }
                    string _data001path = mCurrpath + @"\Assessmentmodel\Objective\datacal 001.txt";

                    GlobalFunctions.WriteTextFile(_data001path, _sdgDataFile);


                    _wasttime = "客观评价模型指标权重输入文件修改运行成功！";
                    Console.WriteLine(_wasttime);

                    // 运行主观评价模型
                    // run assessment model
                    string _assResult = SDGsAssessModel.RunPythonScript(GlobalVars.AssessModelPath, GlobalVars.PythonPath, 1);

                    _wasttime = "客观评价模型运行成功！";
                    Console.WriteLine(_wasttime);

                    // 读取评价模型输出结果,生成评价模型文件，CSV
                    string _assessrespath = mCurrpath + @"\Assessmentmodel\Objective\result.txt";
                    string[,] _assessIndicator = new string[mSimlong + 1, GlobalVars.GoalWeight.Length + 2];    // title, 0:year, 1:水资源可持续发展指数，2：生态系统可持续发展指数；3：经济可持续发展指数；4：流域综合可持续发展指数
                    string[,] _assessIndGrade = new string[mSimlong + 1, GlobalVars.GoalWeight.Length + 2];     // title, 0:year, 1:水资源可持续发展等级，2：生态系统可持续发展等级；3：经济可持续发展等级；4：流域综合可持续发展等级
                    System.Collections.ArrayList _assessResult = new System.Collections.ArrayList();
                    _assessResult = GlobalFunctions.ReadTextFile(_assessrespath);
                    _assessIndicator[0, 0] = "年份";
                    _assessIndicator[0, 1] = "水资源可持续发展指数";
                    _assessIndicator[0, 2] = "生态系统可持续发展指数";
                    _assessIndicator[0, 3] = "社会经济可持续发展指数";
                    _assessIndicator[0, 4] = "流域综合可持续发展指数";
                    _assessIndGrade[0, 0] = "年份";
                    _assessIndGrade[0, 1] = "水资源可持续发展等级";
                    _assessIndGrade[0, 2] = "生态系统可持续发展等级";
                    _assessIndGrade[0, 3] = "社会经济可持续发展等级";
                    _assessIndGrade[0, 4] = "流域综合可持续发展等级";
                    string[] _reviseResult = new string[8];
                    int _ccindex = -1;
                    for (int ii = 0; ii < _assessResult.Count; ii++)
                    {
                        if (_assessResult[ii].ToString().Contains("goal") || _assessResult[ii].ToString().Contains("total"))
                        {
                            _ccindex++;
                            _reviseResult[_ccindex] = _assessResult[ii].ToString();
                        }
                        else
                        {
                            _reviseResult[_ccindex] = _reviseResult[_ccindex] + "  " + _assessResult[ii].ToString();
                        }
                    }
                    // 水资源可持续发展指数
                    string[] _tempWI = GlobalFunctions.GetSDGIndex(_reviseResult[0].ToString(), mSimlong, ' ');
                    // 生态可持续发展指数
                    string[] _tempEI = GlobalFunctions.GetSDGIndex(_reviseResult[1].ToString(), mSimlong, ' ');
                    // 社会经济可持续发展指数
                    string[] _tempSEI = GlobalFunctions.GetSDGIndex(_reviseResult[2].ToString(), mSimlong, ' ');
                    // 流域综合可持续发展指数
                    string[] _tempRBI = GlobalFunctions.GetSDGIndex(_reviseResult[3].ToString(), mSimlong, ' ');
                    // 水资源可持续发展等级
                    string[] _tempWID = GlobalFunctions.GetSDGIndex(_reviseResult[4].ToString(), mSimlong, ' ');
                    // 生态可持续发展等级
                    string[] _tempEID = GlobalFunctions.GetSDGIndex(_reviseResult[5].ToString(), mSimlong, ' ');
                    // 社会经济可持续发展等级
                    string[] _tempSEID = GlobalFunctions.GetSDGIndex(_reviseResult[6].ToString(), mSimlong, ' ');
                    // 流域综合可持续发展等级
                    string[] _tempRBID = GlobalFunctions.GetSDGIndex(_reviseResult[7].ToString(), mSimlong, ' ');

                    for (int ii = 0; ii < mSimlong; ii++)
                    {
                        _assessIndicator[1 + ii, 0] = (GlobalVars.SimStartYear + ii).ToString();            // 年份
                        _assessIndicator[1 + ii, 1] = _tempWI[ii];            // 水资源可持续发展指数
                        _assessIndicator[1 + ii, 2] = _tempEI[ii];            // 生态系统可持续发展指数
                        _assessIndicator[1 + ii, 3] = _tempSEI[ii];            // 社会经济可持续发展指数
                        _assessIndicator[1 + ii, 4] = _tempRBI[ii];            // 流域综合可持续发展指数

                        _assessIndGrade[1 + ii, 0] = (GlobalVars.SimStartYear + ii).ToString();            // 年份
                        _assessIndGrade[1 + ii, 1] = _tempWID[ii];            // 水资源可持续发展等级
                        _assessIndGrade[1 + ii, 2] = _tempEID[ii];            // 生态系统可持续发展等级
                        _assessIndGrade[1 + ii, 3] = _tempSEID[ii];            // 社会经济可持续发展等级
                        _assessIndGrade[1 + ii, 4] = _tempRBID[ii];            // 流域综合可持续发展等级
                    }
                    // 生成可持续发展指数文件
                    mGFunc.WriteGlobleCSVFile2(_modelouputPath, "SDGsIndex", _assessIndicator, ",");

                    //Spire.Xls.Workbook _workbook = new Spire.Xls.Workbook();
                    //_workbook.LoadFromFile(_modelouputPath + @"\SDGsIndex.CSV", ",");
                    //_workbook.SaveToFile(_modelouputPath + @"\SDGsIndex.xls", Spire.Xls.ExcelVersion.Version97to2003);                  


                    // 生成可持续发展等级文件
                    mGFunc.WriteGlobleCSVFile2(_modelouputPath, "SDGsIndicatorGrade", _assessIndGrade, ",");

                    //Spire.Xls.Workbook _workbook1 = new Spire.Xls.Workbook();
                    //_workbook1.LoadFromFile(_modelouputPath + @"\SDGsIndicatorGrade.CSV", ",");
                    //_workbook1.SaveToFile(_modelouputPath + @"\SDGsIndicatorGrade.xls", Spire.Xls.ExcelVersion.Version97to2003);    
                }
                // read assessment model result file


                _wasttime = "End time:" + System.DateTime.Now.ToString();
                Console.WriteLine(_wasttime);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.StackTrace);
                Console.WriteLine(err.Message, "模型模拟");
            }
        }


        //private delegate void DoProgressDelegate(object number);
        /// <summary>
        /// 进行循环过程
        /// </summary>
        /// <param name="num">进度条的最大值</param>
        //private void DoProgressBar(object num)
        //{
        //    if (pgbSimulation.InvokeRequired)
        //    {
        //        DoProgressDelegate d = DoProgressBar;
        //        pgbSimulation.Invoke(d, num);
        //    }
        //    else
        //    {
        //        pgbSimulation.Maximum = (int)num;
        //        for (int i = 0; i < (int)num; i++)
        //        {
        //            pgbSimulation.Value = i;
        //            Application.DoEvents();
        //        }
        //    }
        //}
        /// <summary>
        /// 读取经济模型输出文件result.xls
        /// </summary>
        /// <param name="fullpath"></param>
        /// <returns>将result文件中的数据输出到三维数组中：0：数据类别（产值，土地，价格等）；1：产业（48个产业）；2：县区</returns>
        private double[, ,] ReadResultEcoModel(string fullpath)
        {
            try
            {
                double[, ,] _res = new double[6,48,10];
                if (System.IO.File.Exists(fullpath))
                {
                    System.Object[,] _temp;
                    // 产值数据
                    _temp = mGFunc.ReadExcelData(fullpath, "XTOT", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[0, i, j] = System.Double.Parse(_temp[8 + i, 2 + j].ToString().Trim());
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
                                _res[1, i, j] = System.Double.Parse(_temp[8 + i, 2 + j].ToString().Trim());
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
                                _res[2, i, j] = System.Double.Parse(_temp[8 + i, 2 + j].ToString().Trim());
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
                                _res[3, i, j] = System.Double.Parse(_temp[8 + i, 2 + j].ToString().Trim());
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
                                _res[4, i, j] = System.Double.Parse(_temp[8 + i, 2 + j].ToString().Trim());
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
                                _res[5, i, j] = System.Double.Parse(_temp[8 + i, 2 + j].ToString().Trim());
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("The system cannot find the result.xls file!", "读取经济模型输出文件result");
                }
                return _res;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message, "");
                return null;
            }
        }

        /// <summary>
        /// 读取经济模型输出文件percent.xls
        /// </summary>
        /// <param name="fullpath"></param>
        /// <returns>将percent文件中的数据输出到三维数组中：0：数据类别（产值，土地，价格等）；1：产业（48个产业）；2：县区</returns>
        private double[, ,] ReadPercentEcoModel(string fullpath)
        {
            try
            {
                double[, ,] _res = new double[6, 48, 10];
                if (System.IO.File.Exists(fullpath))
                {
                    System.Object[,] _temp;
                    // 产值数据
                    _temp = mGFunc.ReadExcelData(fullpath, "产值", 14);
                    if (_temp != null)
                    {
                        for (int i = 0; i < 48; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                _res[0, i, j] = System.Double.Parse(_temp[1 + i, 1 + j].ToString().Trim());
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
                                _res[1, i, j] = System.Double.Parse(_temp[1 + i, 1 + j].ToString().Trim());
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
                                _res[2, i, j] = System.Double.Parse(_temp[1 + i, 1 + j].ToString().Trim());
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
                                _res[3, i, j] = System.Double.Parse(_temp[1 + i, 1 + j].ToString().Trim());
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
                                _res[4, i, j] = System.Double.Parse(_temp[1 + i, 1 + j].ToString().Trim());
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
                                _res[5, i, j] = System.Double.Parse(_temp[1 + i, 1 + j].ToString().Trim());
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("The system cannot find the result.xls file!", "读取经济模型输出文件percent");
                }
                return _res;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message, "");
                return null;
            }
        }
       
    }
}
