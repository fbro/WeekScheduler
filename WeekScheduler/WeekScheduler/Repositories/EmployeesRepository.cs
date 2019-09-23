using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace WeekScheduler.Repositories
{
    public class EmployeesRepository
    {
        private Random rng = new Random();

        public string GetFreeColor(List<string> RGBs)
        {
            var nRGB = "000000";
            var count = 0;
            while (count < 100)
            {
                var foundNewRGB = true;
                var pR = rng.Next(5, 200).ToString("X");
                if (pR.Length == 1)
                    pR = "0" + pR;
                var pG = rng.Next(5, 200).ToString("X");
                if (pG.Length == 1)
                    pG = "0" + pG;
                var pB = rng.Next(5, 200).ToString("X");
                if (pB.Length == 1)
                    pB = "0" + pB;
                var pRGB = pR + pG + pB;
                foreach (var RGB in RGBs)
                {
                    count++;
                    if (RGB.Equals(pRGB))
                    {
                        foundNewRGB = false;
                        break;
                    }
                }
                if (foundNewRGB)
                {
                    nRGB = pRGB;
                    break;
                }
            }
            return nRGB;
        }
    }
}
