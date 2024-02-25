﻿using StructureHelperCommon.Models.Forces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureHelperCommon.Models.Sections
{
    public class AccidentalEccentricityLogic : IAccidentalEccentricityLogic
    {
        private double lengthFactor;
        private double sizeFactor;
        private double minEccentricity;

        public ICompressedMember CompressedMember { get; set; }
        public double SizeX { get; set; }
        public double SizeY { get; set; }
        public IForceTuple InitialForceTuple { get; set; }
        public IShiftTraceLogger? TraceLogger { get; set; }
        public AccidentalEccentricityLogic()
        {
            lengthFactor = 1d / 600d;
            sizeFactor = 1d / 30d;
            minEccentricity = 0.01d;
        }
        public ForceTuple GetForceTuple()
        {
            var lengthEccetricity = CompressedMember.GeometryLength * lengthFactor;
            TraceLogger?.AddMessage(string.Format("Accidental eccentricity by length ea = {0}", lengthEccetricity));
            var sizeXEccetricity = SizeX * sizeFactor;
            TraceLogger?.AddMessage(string.Format("Accidental eccentricity by SizeX = {0} ea = {1}", SizeX, sizeXEccetricity));
            var sizeYEccetricity = SizeY * sizeFactor;
            TraceLogger?.AddMessage(string.Format("Accidental eccentricity by SizeY = {0} ea = {1}", SizeY, sizeYEccetricity));
            TraceLogger?.AddMessage(string.Format("Minimum accidental eccentricity ea = {0}", minEccentricity));
            var xEccentricity = Math.Abs(InitialForceTuple.My / InitialForceTuple.Nz);
            TraceLogger?.AddMessage(string.Format("Actual eccentricity e0,x = {0}", xEccentricity));
            var yEccentricity = Math.Abs(InitialForceTuple.Mx / InitialForceTuple.Nz);
            TraceLogger?.AddMessage(string.Format("Actual eccentricity e0,y = {0}", yEccentricity));

            var xFullEccentricity = new List<double>()
            {
                lengthEccetricity,
                sizeXEccetricity,
                minEccentricity,
                xEccentricity
            }
            .Max();
            string mesEx = string.Format("Eccentricity e,x = max({0}; {1}; {2}; {3}) = {4}",
                lengthEccetricity, sizeXEccetricity,
                minEccentricity, xEccentricity,
                xFullEccentricity);
            TraceLogger?.AddMessage(mesEx);
            var yFullEccentricity = new List<double>()
            {
                lengthEccetricity,
                sizeYEccetricity,
                minEccentricity,
                yEccentricity
            }
            .Max();
            string mesEy = string.Format("Eccentricity e,y = max({0}; {1}; {2}; {3}) = {4}",
                lengthEccetricity, sizeYEccetricity,
                minEccentricity, yEccentricity,
                yFullEccentricity);
            TraceLogger?.AddMessage(mesEy);
            var mx = InitialForceTuple.Nz * yFullEccentricity * Math.Sign(InitialForceTuple.Mx);
            var my = InitialForceTuple.Nz * xFullEccentricity * Math.Sign(InitialForceTuple.My);
            TraceLogger?.AddMessage(string.Format("Bending moment arbitrary X-axis Mx = {0} * {1} = {2}", InitialForceTuple.Nz, yFullEccentricity, mx), TraceLogStatuses.Debug);
            TraceLogger?.AddMessage(string.Format("Bending moment arbitrary Y-axis My = {0} * {1} = {2}", InitialForceTuple.Nz, xFullEccentricity, my), TraceLogStatuses.Debug);

            var newTuple = new ForceTuple()
            {
                Mx = mx,
                My = my,
                Nz = InitialForceTuple.Nz,
                Qx = InitialForceTuple.Qx,
                Qy = InitialForceTuple.Qy,
                Mz = InitialForceTuple.Mz,
            };
            TraceLogger?.AddEntry(new TraceTablesFactory().GetByForceTuple(newTuple));
            return newTuple;
        }
    }
}
