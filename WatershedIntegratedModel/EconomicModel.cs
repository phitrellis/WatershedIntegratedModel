using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WatershedIntegratedModel
{
    public class EconomicModel
    {
        public EconomicModel()
        {
        }

        /// <summary>
        /// 将CGE模型输出结果转换成能够被指标模型使用的形式
        /// </summary>
        /// <param name="result">CGE模型输出</param>
        /// <param name="percent">转换权重</param>
        /// <returns></returns>
        public static double[,] ModelOutput(double[,,] result, double[,,] percent, string[] countyname)
        {
            try
            {
                double[,] _output = new double[countyname.Length, 11];
                for (int county = 0; county < countyname.Length; county++)
                {
                    // 农业产值(1-7)
                    for (int i = 0; i < 6; i++)
                    {
                        _output[county, 0] = _output[county, 0] + result[0, i, county] * percent[0, i, county];
                    }
                    // 工业产值(8-32)
                    for (int i = 7; i < 31; i++)
                    {
                        _output[county, 1] = _output[county, 1] + result[0, i, county] * percent[0, i, county];
                    }

                    // 服务业产值(33-48)
                    for (int i = 32; i < 47; i++)
                    {
                        _output[county, 2] = _output[county, 2] + result[0, i, county] * percent[0, i, county];
                    }

                    // 农业土地
                    for (int i = 0; i < 6; i++)
                    {
                        _output[county, 3] = _output[county, 3] + result[1, i, county] * percent[1, i, county];
                    }

                    // 工业土地
                    for (int i = 7; i < 31; i++)
                    {
                        _output[county, 4] = _output[county, 4] + result[1, i, county] * percent[1, i, county];
                    }

                    // 服务业土地
                    for (int i = 32; i < 47; i++)
                    {
                        _output[county, 5] = _output[county, 5] + result[1, i, county] * percent[1, i, county];
                    }

                    // 水价
                    for (int i = 0; i < 47; i++)
                    {
                        _output[county, 6] = _output[county, 6] + result[2, i, county] * percent[2, i, county];
                    }

                    // 就业
                    for (int i = 0; i < 47; i++)
                    {
                        _output[county, 7] = _output[county, 7] + result[3, i, county] * percent[3, i, county];
                    }

                    // 地表水
                    for (int i = 0; i < 47; i++)
                    {
                        _output[county, 8] = _output[county, 8] + result[4, i, county] * percent[4, i, county];
                    }

                    // 地下水
                    for (int i = 0; i < 47; i++)
                    {
                        _output[county, 9] = _output[county, 9] + result[5, i, county] * percent[5, i, county];
                    }
                    // 非农业用水量,增加了10个县区的非农业用水量，经济模型数据表中8-48产业用水量之和
                    for (int i = 7; i < 47; i++)
                    {
                        _output[county, 10] = _output[county, 10] + result[4, 1, county] * percent[4, 1, county];       // 8-48个产业地表水用水量
                        _output[county, 10] = _output[county, 10] + result[5, 1, county] * percent[5, 1, county];       // 8-48个产业地下水用水量
                    }


                }
                return _output;
            }
            catch (Exception err)
            {
                return null;
            }
        }

        /// <summary>
        /// 经济模型输入参数地表供水变化率和地下供水变化率阈值约束
        /// </summary>
        /// <param name="orgdata"></param>
        /// <returns></returns>
        public static double WaterThresholdCon(double orgdata)
        {
            double _result = 0;
            if (orgdata < -20)
            {
                _result = -20;
            }
            else if (orgdata > 20)
            {
                _result = 20;
            }
            else
            {
                _result = orgdata;
            }
            return _result;
        }
        /// <summary>
        /// 经济模型输入参数技术进步率变化率阈值约束
        /// </summary>
        /// <param name="orgdata"></param>
        /// <returns></returns>
        public static double TechAdvThsCon(double orgdata)
        {
            double _result = 0;
            if (orgdata < 0.0000001)
            {
                _result = 0.0000001;
            }
            else if (orgdata > 10)
            {
                _result = 10;
            }
            else
            {
                _result = orgdata;
            }
            return _result;
        }
    }
}
