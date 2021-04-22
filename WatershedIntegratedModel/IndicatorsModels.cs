using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WatershedIntegratedModel
{
    public class IndicatorsModels
    {
        public IndicatorsModels()
        {
        }

        /// <summary>
        /// Water productivity (CNY/m3)
        /// </summary>
        /// <param name="GDP"></param>
        /// <param name="surfacewater">surface water use (m3)</param>
        /// <param name="groundwater">groundwater use (m3)</param>
        /// <param name="economicrate">proportion of economic water use on total water use</param>
        /// <returns></returns>
        public double WaterProductivity(double GDP, double surfacewater, double groundwater)
        {
            if (surfacewater * groundwater > 0)
            {
                return GDP / (surfacewater + groundwater);
            }
            else
            {
                return 0;
            }
        }

        public double WaterStress(double demandwater, double supplywater)
        {
            if (supplywater > 0)
            {
                return demandwater / supplywater;
            }
            else
            {
                return 0;
            }
        }

        public double DrinkSWPop(double population, double totpop)
        {
            if (totpop > 0)
            {
                return population / totpop;
            }
            else
            {
                return 0;
            }
        }

        public double ForestCoverR(double forest, double totarea)
        {
            if (totarea > 0)
            {
                return forest / totarea;
            }
            else
            {
                return 0;
            }
        }
        public double DegradedLandR(double degarea, double totarea)
        {
            if (totarea > 0)
            {
                return degarea / totarea;
            }
            else
            {
                return 0;
            }
        }
        public double GreenCoverI(double greenland, double totarea)
        {
            if (totarea > 0)
            {
                return greenland / totarea;
            }
            else
            {
                return 0;
            }
        }
        public double GDPperCap(double GDP, double pop)
        {
            if (pop > 0)
            {
                return GDP / pop;
            }
            else
            {
                return 0;
            }
        }
        public double UrbanizationRate(double urbanpop, double totpop)
        {
            if (totpop > 0)
            {
                return urbanpop / totpop;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 农业水生产力
        /// </summary>
        /// <param name="argvalue">农业产值（元）</param>
        /// <param name="surfacewater">地表引水量（立方米）</param>
        /// <param name="groundwater">地下引水量（立方米）</param>
        /// <param name="agrwaterratio">农业用水比例（小数）</param>
        /// <returns></returns>
        public double ArgWaterProductivity(double argvalue, double surfacewater, double groundwater, double agrwaterratio)
        {
            if (surfacewater * groundwater * agrwaterratio > 0)
            {
                return argvalue / ((surfacewater + groundwater) * agrwaterratio);
            }
            else
            {
                return 0;
            }
        }
        public double ArgWaterEff(double argET, double argwateruse)
        {
            if (argwateruse > 0)
            {
                return argET / argwateruse;
            }
            else
            {
                return 0;
            }
        }


    }
}
