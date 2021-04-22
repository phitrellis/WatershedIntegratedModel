using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WatershedIntegratedModel
{
    public class GlobalVars
    {
        private static string[,] mSDGsInfo;
        private static double[] mWeight;
        private static double[] mWeightGoals;
        private static int mWeightType;             // 0:subjective; 1:objective
        private static string[] mCountyName = new string[11] { "甘州", "民乐", "临泽", "高台", "山丹", "嘉峪关", "金塔", "肃州", "肃南", "额济纳旗", "流域" };
        private static string[,] mScenClimate;          // 0,temperature;1,precipitation
        private static string[,] mScenLand;
        private static string[,] mScenEconomic;
        private static string[,] mScenGovern;
        private static double[] mThresholdUpTemp;           // 上游温度阈值，0：最小；1：最大
        private static double[] mTempBasicValue;            // 上游温度基准值，0：干流区；1：非干流区
        private static double[] mThresholdUpPreci;          // 上游降水阈值，0：最小；1：最大
        private static double[] mPreciBasicValue;           // 上游降水初值，0：干流区；1：非干流区
        private static double[,] mClimateCounty;

        private static int mStartYear;
        private static int mEndYear;
        private static string mCurrentDirectory;

        private static string mPythonPath;
        private static string mAssessModelPath;

        private static string[,] mModelBasicValue;           // 模型基准值

        private static double mLaborPop = 0.758;                    // 劳动力人口比例

        public GlobalVars()
        {
        }

        /// <summary>
        /// 劳动力人口比例，即具有劳动力的人口占总人口的比例
        /// </summary>
        public static double LaborPopPorp
        {
            get { return mLaborPop; }
            set { mLaborPop = value; }
        }
        /// <summary>
        /// 各县区气候数据；0：温度（摄氏度）；1：降水（cm）
        /// </summary>
        public static double[,] ClimateBasicValue
        {
            get { return mClimateCounty; }
            set { mClimateCounty = value; }
        }
        /// <summary>
        /// 经济模型运行所需基准值，0：县区；1：类别
        /// </summary>
        public static string[,] ModelBasicValue
        {
            get { return mModelBasicValue; }
            set { mModelBasicValue = value; }
        }
        public static double[] TempBasicValue
        {
            get { return mTempBasicValue; }
            set { mTempBasicValue = value; }
        }
        public static double[] PreciBasicValue
        {
            get { return mPreciBasicValue; }
            set { mPreciBasicValue = value; }
        }
        /// <summary>
        /// 上游温度阈值，0：最小；1：最大
        /// </summary>
        public static double[] ThresholdUpTemp
        {
            get { return mThresholdUpTemp; }
            set { mThresholdUpTemp = value; }
        }
        /// <summary>
        /// 上游降水阈值，0：最小；1：最大
        /// </summary>
        public static double[] ThresholdUpPreci
        {
            get { return mThresholdUpPreci; }
            set { mThresholdUpPreci = value; }
        }
        /// <summary>
        /// Path of assessment model
        /// </summary>
        public static string AssessModelPath
        {
            get { return mAssessModelPath; }
            set { mAssessModelPath = value; }
        }
        /// <summary>
        /// Python file path 
        /// </summary>
        public static string PythonPath
        {
            get { return mPythonPath; }
            set { mPythonPath = value; }
        }
        /// <summary>
        /// SDGs指标信息，0:等级标识；1：目标内容；2：是否被check；3：weight；4：指标正逆向
        /// </summary>
        public static string[,] SDGs
        {
            get { return mSDGsInfo; }
            set { mSDGsInfo = value; }
        }
        public static int SimStartYear
        {
            get { return mStartYear; }
            set { mStartYear = value; }
        }
        public static int SimEndYear
        {
            get { return mEndYear; }
            set { mEndYear = value; }
        }


        /// <summary>
        /// Weight of the indicators
        /// </summary>
        public static double[] IndicatorWeight
        {
            get { return mWeight; }
            set { mWeight = value; }
        }

        /// <summary>
        /// Weight of the goals
        /// </summary>
        public static double[] GoalWeight
        {
            get { return mWeightGoals; }
            set { mWeightGoals = value; }
        }

        /// <summary>
        /// Weight type of SDGs
        /// </summary>
        public static int WeightType
        {
            get { return mWeightType; }
            set { mWeightType = value; }
        }


        /// <summary>
        /// Counties' name in watershed
        /// </summary>
        public static string[] CountyName
        {
            get { return mCountyName; }
            set { mCountyName = value; }
        }

        /// <summary>
        /// Climate scenario: 0.county; 1.Temperature; 2.Precipitation
        /// </summary>
        public static string[,] ClimateScenario
        {
            get { return mScenClimate; }
            set { mScenClimate = value; }
        }

        /// <summary>
        /// Land use scenario: 0.county;1.Farmland increasing rate (%/39 yr);2.Forest increasing (%/39 yr),3.Grassland increasing (%/39 yr),4.Degraded land increasing(%/39 yr)
        /// </summary>
        public static string[,] LandScenario
        {
            get { return mScenLand; }
            set { mScenLand = value; }
        }

        /// <summary>
        /// Economic scenario: 0,county;1.Agricultural technology progress rate (%/39 yr);2.Industrial technology progress rate (%/39 yr);3.Service technology progress rate (%/39 yr)",
        /// 4.Surface water use change rate (%/yr);5.Groundwater water use change rate (%/yr),6.Non-economic water use (108m3),7.Change rate of people drinking safe water (%/yr)
        /// </summary>
        public static string[,] EconomicScenario
        {
            get { return mScenEconomic; }
            set { mScenEconomic = value; }
        }

        /// <summary>
        /// Government management scenario: 0,county;1,population controal ceofficient;2. urbanization control coefficient; 3. transfer water to downstream
        /// </summary>
        public static string[,] GovernScenario
        {
            get { return mScenGovern; }
            set { mScenGovern = value; }
        }

        /// <summary>
        /// 目前打开的项目所在的目录
        /// </summary>
        public static string ProgramDirectory
        {
            get { return mCurrentDirectory; }
            set { mCurrentDirectory = value; }
        }

    }
}
