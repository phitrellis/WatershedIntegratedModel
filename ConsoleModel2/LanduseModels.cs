using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WatershedIntegratedModel
{
    public class LanduseModels
    {
        public LanduseModels()
        {
        }


        /// <summary>
        /// calculation of forest area in future
        /// </summary>
        /// <param name="countrycode"></param>
        /// <param name="urbrate">小数</param>
        /// <param name="gdp">亿元</param>
        /// <param name="temperature">℃</param>
        /// <returns>ha</returns>
        public static double ForestAreaPre(int countrycode, double urbrate, double gdp, double temperature)
        {
            try
            {
                double _result = 0;
                switch (countrycode)
                {
                    case 1:         // Ganzhou
                        _result = 0.063 * urbrate + 0.001 * gdp + 0.039 * temperature + 0.454;
                        break;
                    case 2:         // Minle
                        _result = 0.012 * urbrate + 0.000022 * gdp - 0.000117 * temperature + 0.025;
                        break;
                    case 3:         // Linze
                        _result = 1.129 * urbrate + 0.001 * gdp - 0.033 * temperature + 0.313;
                        break;
                    case 4:         // Gaotai
                        _result = 0.099 * urbrate + 0.002 * gdp + 0.003 * temperature + 0.055;
                        break;
                    case 5:         // Shandan
                        _result = 0.065 * urbrate + 0.001 * gdp - 0.003 * temperature + 0.067;
                        break;
                    case 6:         // Jiayuguan
                        _result = 0.042 * urbrate + 0.000181 * gdp + 0.002 * temperature - 0.048;
                        break;
                    case 7:         // Jinta
                        _result = 0.869 * urbrate + 0.000454 * gdp - 0.006 * temperature + 0.12;
                        break;
                    case 8:         // Suzhou
                        _result = 0.164 * urbrate + 0.00038 * gdp + 0.01 * temperature - 0.129;
                        break;
                    case 9:         // Sunan
                        _result = 0.001 * gdp - 0.003 * temperature + 0.03;
                        break;
                    case 10:        // Erjina
                        _result = 1.594 * urbrate + 0.006 * gdp + 0.056 * temperature + 3.866;
                        break;
                    default:
                        break;
                }
                return _result;
            }
            catch (Exception err)
            {
                return 0;
            }
        }

        /// <summary>
        /// calculation of grassland area
        /// </summary>
        /// <param name="countrysort"></param>
        /// <param name="urbanrate">小数</param>
        /// <param name="gdp">亿元</param>
        /// <param name="precipitation"></param>
        /// <returns>ha</returns>
        public static double GrasslandAreaPre(int countrysort, double urbanrate, double gdp, double precipitation)
        {
            try
            {
                double _result = 0;
                switch (countrysort)
                {
                    case 1:         // Ganzhou
                        _result = 0.494 * urbanrate + 0.002 * gdp + 0.00010 * precipitation + 4.897;
                        break;
                    case 2:         // Minle
                        _result = 2.257 * urbanrate + 0.014 * gdp + 0.00030 * precipitation + 6.49;
                        break;
                    case 3:         // Linze
                        _result = 0.307 * urbanrate - 0.012 * gdp + 0.001 * precipitation + 1.41;
                        break;
                    case 4:         // Gaotai
                        _result = 0.099 * urbanrate + 0.002 * gdp + 0.003 * precipitation + 0.055;
                        break;
                    case 5:         // Shandan
                        _result = 0.629 * urbanrate - 0.013 * gdp - 0.001 * precipitation + 21.408;
                        break;
                    case 6:         // Jiayuguan
                        _result = 0.589 * urbanrate - 0.002 * gdp + 0.000256 * precipitation + 2.263;
                        break;
                    case 7:         // Jinta
                        _result = 0.167 * urbanrate - 0.001 * gdp + 0.000466 * precipitation + 7.984;
                        break;
                    case 8:         // Suzhou
                        _result = 0.678 * urbanrate - 0.008 * gdp + 0.01 * precipitation + 3.932;
                        break;
                    case 9:         // Sunan
                        _result = 0.006 * urbanrate - 0.001 * gdp + 0.000144 * precipitation + 3.92;
                        break;
                    case 10:        // Erjina
                        _result = 13.354 * urbanrate + 0.031 * gdp + 0.066 * precipitation + 28.517;
                        break;
                    default:
                        break;
                }
                return _result;
            }
            catch (Exception err)
            {
                return 0;
            }
        }


        /// <summary>
        /// calculation of the wetland area (ha)
        /// </summary>
        /// <param name="countysort"></param>
        /// <param name="urbanization">%</param>
        /// <param name="gdp">亿元</param>
        /// <param name="precipitation"></param>
        /// <param name="temperature">℃</param>
        /// <returns>ha</returns>
        public static double WetlandAreaPre(int countysort, double urbanization, double gdp, double precipitation, double temperature)
        {
            try
            {
                double _result = 0;
                switch (countysort)
                {
                    case 1:         // Ganzhou
                        _result = 0.01 * urbanization - 0.001 * gdp + 0.011 * temperature + 0.35;
                        break;
                    case 2:         // Minle
                        _result = 0.081 * urbanization - 0.001 * gdp - 0.004 * temperature + 0.258;
                        break;
                    case 3:         // Linze
                        _result = -0.014 * gdp + 0.156 * temperature - 0.926;
                        break;
                    case 4:         // Gaotai
                        _result = 1.03 * urbanization - 0.013 * gdp - 0.051 * temperature + 0.975;
                        break;
                    case 5:         // Shandan
                        _result = -0.015 * urbanization - 0.001 * gdp - 0.002 * temperature + 0.361;
                        break;
                    case 6:         // Jiayuguan
                        _result = 0.006 * urbanization + 0.002 * temperature + 0.006;
                        break;
                    case 7:         // Jinta
                        _result = 0.191 * urbanization + 0.001 * gdp - 0.002 * temperature + 0.24;
                        break;
                    case 8:         // Suzhou
                        _result = -0.189 * urbanization - 0.001 * gdp + 0.002 * precipitation + 0.411;
                        break;
                    case 9:         // Sunan
                        _result = -0.007 * gdp + 0.002 * precipitation + 0.26;
                        break;
                    case 10:        // Erjina
                        _result = 0.007 * urbanization - 0.000022 * gdp - 0.000284 * temperature + 0.191;
                        break;
                    default:
                        break;
                }
                return _result;
            }
            catch (Exception err)
            {
                return 0;
            }
        }

    }
}
