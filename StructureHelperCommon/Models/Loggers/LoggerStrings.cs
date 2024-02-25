﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureHelperCommon.Models.Loggers
{
    public static class LoggerStrings
    {
        public static string DimensionLess => "(dimensionless)";
        public static string MethodBasedOn => "Method of calculation based on ";
        public static string CalculationHasDone => "Calculation has done succesfully";
        public static string CalculatorType(object obj) => string.Format("Calculator type: {0}", obj.GetType());
    }
}
