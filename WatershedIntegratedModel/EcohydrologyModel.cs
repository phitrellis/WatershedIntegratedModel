using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WatershedIntegratedModel
{
    public class EcohydrologyModel
    {
        public EcohydrologyModel()
        {
        }

        /// <summary>
        /// 上游生态水文模型-莺落峡
        /// </summary>
        /// <param name="temp">温度（K）</param>
        /// <param name="preci">降水（mm）</param>
        /// <returns>莺落峡径流量（立方米）,其他河流年径流量（立方米）</returns>
        public double UpEcohydroModelYLX(double temp, double preci)
        {
            try
            {
                double _result;

                //莺落峡径流量（立方米）
                _result = 177.825085 * preci * preci + 6988908.56 * temp * temp - 77217.5416 * preci * temp + 26500711 * preci - 3843735530 * temp + 526858754000;

                return _result;
            }
            catch (Exception err)
            {
                return 0;
            }
        }
        /// <summary>
        /// 上游生态水文模型-其他河流
        /// </summary>
        /// <param name="temp">温度（K）</param>
        /// <param name="preci">降水（mm）</param>
        /// <returns>莺落峡径流量（立方米）,其他河流年径流量（立方米）</returns>
        public double UpEcohydroModelOther(double temp, double preci)
        {
            try
            {
                double _result;

                // 其他河流年径流量（立方米）
                _result = -394.249672 * preci * preci - 16284516.7 * temp * temp + 1274068.40 * preci * temp - 341549958 * preci + 7825006480 * temp - 925319379000;

                return _result;
            }
            catch (Exception err)
            {
                return 0;
            }
        }

        /// <summary>
        /// 中游生态水文模型
        /// </summary>
        /// <param name="temp">温度（）</param>
        /// <param name="precip">降水（）</param>
        /// <param name="Yingluoxia">莺落峡径流量（）</param>
        /// <param name="otherriver">其他河流径流量（）</param>
        /// <param name="Zhengyixia">正义峡下泄量（）</param>
        /// <param name="farmland">耕地面积（）</param>
        /// <param name="nonagriwater">非农业经济用水量（）</param>
        /// <returns>地表引水量（），地下取水量（），农业ET（）</returns>
        public double[] MidEcohydroModel(double temp, double precip, double Yingluoxia, double otherriver, double Zhengyixia, double farmland, double nonagriwater)
        {
            try
            {
                double[] _result = new double[3];

                return _result;
            }
            catch (Exception err)
            {
                return null;
            }
        }

    }
}
