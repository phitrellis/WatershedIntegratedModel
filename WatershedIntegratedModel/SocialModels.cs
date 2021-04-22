using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WatershedIntegratedModel
{
    public class SocialModels
    {
        public SocialModels()
        {
        }

        /// <summary>
        /// 人口增长模型
        /// </summary>
        /// <param name="conpara">人口增长控制参数</param>
        /// <param name="year">simulated year</param>
        /// <param name="x0">population of base year</param>
        /// <param name="xm">maximum population</param>
        /// <returns></returns>
        public static double PopulationModel(double conpara,double year,double x0,double xm)
        {
            try
            {
                double _result = 0;

                _result = xm / (1 + (xm / x0 - 1) * Math.Pow(Math.E, (-1) * conpara * year));

                return _result;
            }
            catch (Exception err)
            {
                return 0;
            }
        }

        /// <summary>
        /// Urbanization rate, %
        /// </summary>
        /// <param name="lamda"></param>
        /// <param name="paraK"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public static double UrbanizationRate(double lamda, double paraK, double year)
        {
            try
            {
                double _result = 0;

                _result = (1 / (1 + lamda * Math.Pow(Math.E, paraK * (1 + year)))) * 100;

                return _result;
            }
            catch (Exception err)
            {
                return 0;
            }
        }

        /// <summary>
        /// 安全饮用水人口比例计算,根据国家政策：达到100%的年份
        /// </summary>
        /// <param name="basicvalue">2012年基准比例</param>
        /// <param name="goalvalue">目标值</param>
        /// <param name="goalyrnum">达到一定比例所需的时间</param>
        /// <returns></returns>
        public static double[] SafeDrinkWaterPop(double basicvalue, double goalvalue, int goalyrnum)
        {
            try
            {
                double[] _result = new double[goalyrnum];
                double _annAveInc = Math.Pow(goalvalue / basicvalue, 1 / goalyrnum) - 1;

                for (int i = 0; i < goalyrnum; i++)
                {
                    if (i != goalyrnum - 1)
                    {
                        _result[i] = basicvalue * Math.Pow(1 + _annAveInc, i);
                    }
                    else
                    {
                        _result[i] = goalvalue;
                    }
                }

                return _result;
            }
            catch (Exception err)
            {
                return null;
            }
        }

        /// <summary>
        /// 安全饮用水人口计算,根据基准值和年平均增长率计算,
        /// </summary>
        /// <param name="basicvalue">安全饮用水人口基准值</param>
        /// <param name="incRate">年平均增长率</param>
        /// <returns></returns>
        public static double SafeDrinkWaterPop2(double basicvalue, double incRate, int yr)
        {
            return basicvalue * Math.Pow(1 + incRate, yr);
        }

        /// <summary>
        /// 人均GDP
        /// </summary>
        /// <returns></returns>
        public static double GDPperCapita(double gdp,double pop)
        {
            if (pop > 0)
            {
                return gdp / pop;
            }
            else
            {
                return 0;
            }
        }
    }
}
