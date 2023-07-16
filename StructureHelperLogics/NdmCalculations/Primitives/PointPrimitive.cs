﻿using StructureHelperLogics.Models.Materials;
using StructureHelperCommon.Models.Shapes;
using StructureHelper.Models.Materials;
using System.Collections.Generic;
using LoaderCalculator.Data.Ndms;
using LoaderCalculator.Data.Materials;
using StructureHelperCommon.Infrastructures.Interfaces;
using System;
using StructureHelperLogics.NdmCalculations.Primitives;
using StructureHelperLogics.NdmCalculations.Triangulations;
using StructureHelperLogics.Services.NdmPrimitives;
using StructureHelperCommon.Models.Forces;
using StructureHelperLogics.Models.CrossSections;

namespace StructureHelperLogics.Models.Primitives
{
    public class PointPrimitive : IPointPrimitive
    {
        static readonly PointUpdateStrategy updateStrategy = new();
        public Guid Id { get; }
        public string? Name { get; set; }
        public IPoint2D Center { get; private set; }
        public IHeadMaterial HeadMaterial { get; set; }
        //public double NdmMaxSize { get; set; }
        //public int NdmMinDivision { get; set; }
        public StrainTuple UsersPrestrain { get; private set; }
        public StrainTuple AutoPrestrain { get; private set; }
        public double Area { get; set; }

        public IVisualProperty VisualProperty { get; }
        public bool Triangulate { get; set; }
        public ICrossSection? CrossSection { get; set; }


        public PointPrimitive(Guid id)
        {
            Id = id;
            Name = "New Point";
            Area = 0.0005d;
            Center = new Point2D();
            VisualProperty = new VisualProperty();
            UsersPrestrain = new StrainTuple();
            AutoPrestrain = new StrainTuple();
            Triangulate = true;
        }
        public PointPrimitive() : this (Guid.NewGuid())
        {}
        public PointPrimitive(IHeadMaterial material) : this() { HeadMaterial = material; }

        public IEnumerable<INdm> GetNdms(IMaterial material)
        {
            var options = new PointTriangulationLogicOptions(this);
            IPointTriangulationLogic logic = new PointTriangulationLogic(options);
            return logic.GetNdmCollection(material);
        }

        public object Clone()
        { 
            var primitive = new PointPrimitive();
            updateStrategy.Update(primitive, this);
            return primitive;
        }
    }
}
