﻿using LoaderCalculator;
using LoaderCalculator.Data.Matrix;
using LoaderCalculator.Data.Ndms;
using LoaderCalculator.Data.SourceData;
using StructureHelperCommon.Infrastructures.Enums;
using StructureHelperCommon.Models.Forces;
using StructureHelperCommon.Models.Shapes;
using StructureHelperCommon.Services.Forces;
using StructureHelperLogics.NdmCalculations.Primitives;
using StructureHelperLogics.Services.NdmPrimitives;
using System;
using System.Collections.Generic;
using System.Threading;

namespace StructureHelperLogics.NdmCalculations.Analyses.ByForces
{
    public class ForceCalculator : IForceCalculator
    {
        public string Name { get; set; }
        public double IterationAccuracy { get; set; }
        public int MaxIterationCount { get; set; }
        public List<LimitStates> LimitStatesList { get; }
        public List<CalcTerms> CalcTermsList { get; }
        public List<IForceCombinationList> ForceCombinationLists { get; }
        public List<INdmPrimitive> Primitives { get; }
        public INdmResult Result { get; private set; }


        public void Run()
        {
            var checkResult = CheckInputData();
            if (checkResult != "")
            {
                Result = new ForcesResults() { IsValid = false, Desctription = checkResult };
                return;
            }
            else { CalculateResult(); }
        }

        private void CalculateResult()
        {
            var ndmResult = new ForcesResults() { IsValid = true };
            foreach (var combination in ForceCombinationLists)
            {
                foreach (var tuple in combination.DesignForces)
                {
                    var limitState = tuple.LimitState;
                    var calcTerm = tuple.CalcTerm;
                    if (LimitStatesList.Contains(limitState) & CalcTermsList.Contains(calcTerm))
                    {
                        var ndms = NdmPrimitivesService.GetNdms(Primitives, limitState, calcTerm);
                        IPoint2D point2D;
                        if (combination.SetInGravityCenter == true)
                        {
                            var loaderPoint = LoaderCalculator.Logics.Geometry.GeometryOperations.GetGravityCenter(ndms);
                            point2D = new Point2D() { X = loaderPoint[0], Y = loaderPoint[1] };
                        }
                        else point2D = combination.ForcePoint;
                        var newTuple = TupleService.MoveTupleIntoPoint(tuple.ForceTuple, point2D);
                        var result = GetPrimitiveStrainMatrix(ndms, newTuple);
                        result.DesignForceTuple.LimitState = limitState;
                        result.DesignForceTuple.CalcTerm = calcTerm;
                        result.DesignForceTuple.ForceTuple = newTuple;
                        ndmResult.ForcesResultList.Add(result);
                    }
                }
            }
            Result = ndmResult;
        }

        private string CheckInputData()
        {
            string result = "";
            if (Primitives.Count == 0) { result += "Calculator does not contain any primitives \n"; }
            if (ForceCombinationLists.Count == 0) { result += "Calculator does not contain any forces \n"; }
            if (LimitStatesList.Count == 0) { result += "Calculator does not contain any limit states \n"; }
            if (CalcTermsList.Count == 0) { result += "Calculator does not contain any duration \n"; }
            return result;
        }

        public ForceCalculator()
        {
            ForceCombinationLists = new List<IForceCombinationList>();
            Primitives = new List<INdmPrimitive>();
            IterationAccuracy = 0.001d;
            MaxIterationCount = 1000;
            LimitStatesList = new List<LimitStates>() { LimitStates.ULS, LimitStates.SLS };
            CalcTermsList = new List<CalcTerms>() { CalcTerms.ShortTerm, CalcTerms.LongTerm };
        }

        private ForcesResult GetPrimitiveStrainMatrix(IEnumerable<INdm> ndmCollection, IForceTuple tuple)
        {
            var mx = tuple.Mx;
            var my = tuple.My;
            var nz = tuple.Nz;

            try
            {
                var loaderData = new LoaderOptions
                {
                    Preconditions = new Preconditions
                    {
                        ConditionRate = IterationAccuracy,
                        MaxIterationCount = MaxIterationCount,
                        StartForceMatrix = new ForceMatrix { Mx = mx, My = my, Nz = nz }
                    },
                    NdmCollection = ndmCollection
                };
                var calculator = new Calculator();
                calculator.Run(loaderData, new CancellationToken());
                var calcResult = calculator.Result;
                if (calcResult.AccuracyRate <= IterationAccuracy)
                {
                    return new ForcesResult() { IsValid = true, Desctription = "Analysis is done succsefully", LoaderResults = calcResult };
                }
                else
                {
                    return new ForcesResult() { IsValid = false, Desctription = "Required accuracy rate has not achived", LoaderResults = calcResult };
                }

            }
            catch (Exception ex)
            {
                var result = new ForcesResult() { IsValid = false };
                if (ex.Message == "Calculation result is not valid: stiffness matrix is equal to zero") { result.Desctription = "Stiffness matrix is equal to zero \nProbably section was collapsed"; }
                else { result.Desctription = $"Error is appeared due to analysis. Error: {ex}"; }
                return result;
            }

        }

        public object Clone()
        {
            IForceCalculator calculator = new ForceCalculator();
            calculator.LimitStatesList.Clear();
            calculator.LimitStatesList.AddRange(LimitStatesList);
            calculator.CalcTermsList.Clear();
            calculator.CalcTermsList.AddRange(CalcTermsList);
            calculator.IterationAccuracy = IterationAccuracy;
            calculator.MaxIterationCount = MaxIterationCount;
            calculator.Primitives.AddRange(Primitives);
            calculator.ForceCombinationLists.AddRange(ForceCombinationLists);
            return calculator;
        }
    }
}
