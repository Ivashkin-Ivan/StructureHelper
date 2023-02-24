﻿using StructureHelper.Models.Materials;
using StructureHelperLogics.Models.Primitives;
using StructureHelperLogics.NdmCalculations.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace StructureHelperLogics.Models.Templates.CrossSections.RCs
{
    public class CircleGeometryLogic : IRCGeometryLogic
    {
        ICircleBeamTemplate template;
        
        
        public IEnumerable<IHeadMaterial> HeadMaterials { get; set; }

        public CircleGeometryLogic(ICircleBeamTemplate template)
        {
            this.template = template;
        }

        public IEnumerable<INdmPrimitive> GetNdmPrimitives()
        {
            List<INdmPrimitive> primitives = new List<INdmPrimitive>();
            primitives.AddRange(GetConcretePrimitives());
            primitives.AddRange(GetReinfrocementPrimitives());
            return primitives;
        }

        private IEnumerable<INdmPrimitive> GetConcretePrimitives()
        {
            var diameter = template.SectionDiameter;
            var concreteMaterial = HeadMaterials.ToList()[0];
            var primitives = new List<INdmPrimitive>();
            var rectangle = new CirclePrimitive() { Diameter = diameter, Name = "Concrete block", HeadMaterial = concreteMaterial };
            primitives.Add(rectangle);
            return primitives;
        }

        private IEnumerable<INdmPrimitive> GetReinfrocementPrimitives()
        {
            var reinforcementMaterial = HeadMaterials.ToList()[1];
            var radius = template.SectionDiameter / 2 - template.CoverGap;
            var dAngle = 2d * Math.PI / template.BarQuantity;
            var barArea = Math.PI* template.BarDiameter* template.BarDiameter / 4d;
            var primitives = new List<INdmPrimitive>();
            for (int i = 0; i < template.BarQuantity; i++)
            {
                var angle = i * dAngle;
                var x = radius * Math.Sin(angle);
                var y = radius * Math.Cos(angle);
                var point = new PointPrimitive() { CenterX = x, CenterY = y, Area = barArea, Name = "Left bottom point", HeadMaterial = reinforcementMaterial };
                primitives.Add(point);
            }
            return primitives;
        }
    }
}
