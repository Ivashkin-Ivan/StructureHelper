﻿using LoaderCalculator.Data.Materials;
using LoaderCalculator.Data.Materials.MaterialBuilders;
using LoaderCalculator.Data.Matrix;
using LoaderCalculator.Data.Ndms;
using LoaderCalculator.Data.Ndms.Transformations;
using LoaderCalculator.Data.Planes;
using LoaderCalculator.Data.SourceData;
using LoaderCalculator.Infrastructure;
using LoaderCalculator.Tests.Infrastructures.Logics;
using LoaderCalculator.Triangulations;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StructureHelperLogics.NdmCalculations.Triangulations;
using StructureHelperLogics.Data.Shapes;

namespace StructureHelperTests.UnitTests.Ndms.Triangulations
{
    public class RectangleTriangulationTest
    {
        //Участок по центру
        [TestCase(0d, 0d, 1.0d, 1.0d, 0d, 0.02d, 1, 50 * 50, -0.49d, -0.49d)]
        //Участок со смещением от центра
        [TestCase(2d, 2d, 1.0d, 1.0d, 0d, 0.02d, 1, 50 * 50, 1.51d, 1.51d)]
        [TestCase(2d, 1d, 1.0d, 1.0d, 0d, 0.02d, 1, 50 * 50, 1.51d, 0.51d)]
        //Участок с поворотом на 1 радиан
        [TestCase(0d, 0d, 1.0d, 1.0d, 1d, 0.02d, 1, 50 * 50, 0.14757265268048089d, -0.67706891243125777d)]
        //Участок со смещением и поворотом на 1 радиан
        [TestCase(2d, 2d, 1.0d, 1.0d, 1d, 0.02d, 1, 50 * 50, -0.45476470519903267d, 2.0864776689208147d)]
        public void Run_ShouldPass (double centerX, double centerY, double width, double height, double angle, double ndmMaxSize, int ndmMinDivision, int expectedCount, double expectedFirstCenterX, double expectedFirstCenterY)
        {
            //Arrange
            IMaterial material = new Material();
            ICenter center = new Center() { CenterX = centerX, CenterY = centerY };
            IRectangle rectangle = new Rectangle() { Width = width, Height = height, Angle = angle };
            IRectangleTriangulationOptions options = new RectangleTriangulationOptions(center, rectangle, ndmMaxSize, ndmMinDivision);
            IRectangleTriangulationLogic logic = new StructureHelperLogics.NdmCalculations.Triangulations.RectangleTriangulationLogic(options);
            //Act
            var result = logic.GetNdmCollection(material);
            //Assert
            Assert.NotNull(result);
            Assert.AreEqual(expectedCount, result.Count());
            var firstNdm = result.First();
            Assert.AreEqual(expectedFirstCenterX, firstNdm.CenterX);
            Assert.AreEqual(expectedFirstCenterY, firstNdm.CenterY);
        }
    }
}
