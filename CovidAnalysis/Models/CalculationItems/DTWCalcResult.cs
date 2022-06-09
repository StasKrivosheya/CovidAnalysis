using System.Collections.Generic;

namespace CovidAnalysis.Models.CalculationItems
{
    public class DTWCalcResult
    {
        public double Cost { get; set; }

        public List<MatchingElementsModel> MatchingElements { get; set; }
    }
}
