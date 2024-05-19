﻿using LoaderCalculator.Data.Ndms;
using StructureHelperCommon.Infrastructures.Enums;
using StructureHelperCommon.Infrastructures.Exceptions;
using StructureHelperCommon.Models;
using StructureHelperCommon.Models.Calculators;
using StructureHelperCommon.Models.Forces;
using StructureHelperCommon.Models.Loggers;
using StructureHelperCommon.Services.Forces;
using StructureHelperLogics.NdmCalculations.Analyses.ByForces;
using StructureHelperLogics.NdmCalculations.Primitives;
using StructureHelperLogics.Services.NdmPrimitives;

namespace StructureHelperLogics.NdmCalculations.Cracking
{
    public class TupleCrackCalculator : ICalculator
    {
        private static readonly ILengthBetweenCracksLogic lengthLogic = new LengthBetweenCracksLogicSP63();
        private TupleCrackResult result;
        private ICrackedSectionTriangulationLogic triangulationLogic;
        private List<RebarPrimitive>? rebarPrimitives;
        private IEnumerable<INdm> crackableNdms;
        private IEnumerable<INdm> crackedNdms;
        private IEnumerable<INdm> elasticNdms;
        private CrackForceResult crackForceResult;
        private StrainTuple longDefaultStrainTuple;
        private StrainTuple shortDefaultStrainTuple;
        private double longLength;
        private double shortLength;

        public string Name { get; set; }
        public TupleCrackInputData InputData { get; set; }
        public IResult Result => result;

        public IShiftTraceLogger? TraceLogger { get; set; }

        public void Run()
        {
            PrepareNewResult();
            TraceLogger?.AddMessage(LoggerStrings.CalculatorType(this), TraceLogStatuses.Service);

            try
            {
                TraceLogger?.AddMessage($"Calculation of crack width by force combination");
                ProcessCalculations();
                TraceLogger?.AddMessage(LoggerStrings.CalculationHasDone);

            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Description += ex;
                TraceLogger?.AddMessage(LoggerStrings.CalculationError + ex, TraceLogStatuses.Error);
            }

        }

        private void PrepareNewResult()
        {
            result = new()
            {
                IsValid = true,
                Description = string.Empty,
                InputData = InputData
            };
        }

        private void ProcessCalculations()
        {
            CheckInputData();
            Triangulate();
            longDefaultStrainTuple = CalcStrainMatrix(InputData.LongTermTuple as ForceTuple, crackableNdms);
            shortDefaultStrainTuple = CalcStrainMatrix(InputData.LongTermTuple as ForceTuple, crackableNdms);
            var longElasticStrainTuple = CalcStrainMatrix(InputData.LongTermTuple as ForceTuple, elasticNdms);
            var shortElasticStrainTuple = CalcStrainMatrix(InputData.ShortTermTuple as ForceTuple, elasticNdms);
            if (result.IsValid == false) { return; }
            if (InputData.UserCrackInputData.SetLengthBetweenCracks == true)
            {
                longLength = InputData.UserCrackInputData.LengthBetweenCracks;
                shortLength = InputData.UserCrackInputData.LengthBetweenCracks;                
                TraceLogger?.AddMessage($"User value of length between cracks Lcrc = {longLength}");
            }
            else
            {
                longLength = GetLengthBetweenCracks(longElasticStrainTuple);
                shortLength = GetLengthBetweenCracks(shortElasticStrainTuple);
            }
            CalcCrackForce();
            foreach (var rebar in rebarPrimitives)
            {
                RebarCrackCalculatorInputData rebarCalculatorData = GetRebarCalculatorInputData(rebar);
                var calculator = new RebarCrackCalculator
                {
                    InputData = rebarCalculatorData,
                    TraceLogger = TraceLogger?.GetSimilarTraceLogger(50)
                };
                calculator.Run();
                var rebarResult = calculator.Result as RebarCrackResult;
                result.RebarResults.Add(rebarResult);
            }
            result.LongTermResult = new()
            {
                CrackWidth = result.RebarResults.Max(x => x.LongTermResult.CrackWidth),
                UltimateCrackWidth = InputData.UserCrackInputData.UltimateLongCrackWidth
            };
            result.ShortTermResult = new()
            {
                CrackWidth = result.RebarResults.Max(x => x.ShortTermResult.CrackWidth),
                UltimateCrackWidth = InputData.UserCrackInputData.UltimateShortCrackWidth
            };
        }

        private RebarCrackCalculatorInputData GetRebarCalculatorInputData(RebarPrimitive rebar)
        {
            var longRebarData = new RebarCrackInputData()
            {
                CrackableNdmCollection = crackableNdms,
                CrackedNdmCollection = crackedNdms,
                ForceTuple = InputData.LongTermTuple as ForceTuple,
                Length = longLength
            };
            var shortRebarData = new RebarCrackInputData()
            {
                CrackableNdmCollection = crackableNdms,
                CrackedNdmCollection = crackedNdms,
                ForceTuple = InputData.ShortTermTuple as ForceTuple,
                Length = shortLength
            };
            var rebarCalculatorData = new RebarCrackCalculatorInputData()
            {
                RebarPrimitive = rebar,
                LongRebarData = longRebarData,
                ShortRebarData = shortRebarData,
                UserCrackInputData = InputData.UserCrackInputData
            };
            return rebarCalculatorData;
        }

        private StrainTuple CalcStrainMatrix(ForceTuple forceTuple, IEnumerable<INdm> ndms)
        {
            IForceTupleInputData inputData = new ForceTupleInputData()
            {
                NdmCollection = ndms,
                Tuple = forceTuple
            };
            IForceTupleCalculator calculator = new ForceTupleCalculator()
            {
                InputData = inputData,
                //TraceLogger = TraceLogger?.GetSimilarTraceLogger(50)
            };
            calculator.Run();
            var forceResult = calculator.Result as IForcesTupleResult;
            if (forceResult.IsValid == false)
            {
                result.IsValid = false;
                result.Description += forceResult.Description;
            }
            var strain = TupleConverter.ConvertToStrainTuple(forceResult.LoaderResults.StrainMatrix);
            return strain;
        }

        private double GetLengthBetweenCracks(StrainTuple strainTuple)
        {
            var logic = new LengthBetweenCracksLogicSP63()
            {
                NdmCollection = elasticNdms,
                TraceLogger = TraceLogger
            };
            logic.StrainMatrix = TupleConverter.ConvertToLoaderStrainMatrix(strainTuple);
            return logic.GetLength();
        }

        private void Triangulate()
        {
            triangulationLogic = new CrackedSectionTriangulationLogic(InputData.NdmPrimitives)
            {
                //TraceLogger = TraceLogger?.GetSimilarTraceLogger(50)
            };
            rebarPrimitives = triangulationLogic.GetRebarPrimitives();
            crackableNdms = triangulationLogic.GetNdmCollection();
            crackedNdms = triangulationLogic.GetCrackedNdmCollection();
            elasticNdms = triangulationLogic.GetElasticNdmCollection();
        }

        private void CalcCrackForce()
        {
            var calculator = new CrackForceCalculator();
            calculator.EndTuple = InputData.LongTermTuple;
            calculator.NdmCollection = crackableNdms;
            calculator.Run();
            crackForceResult = calculator.Result as CrackForceResult;
        }

        private void CheckInputData()
        {
            if (InputData.NdmPrimitives is null || InputData.NdmPrimitives.Count == 0)
            {
                throw new StructureHelperException(ErrorStrings.DataIsInCorrect + ": input data doesn't have any primitives");
            }    
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
