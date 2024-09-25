using LoaderCalculator.Data.Materials;
using StructureHelperCommon.Infrastructures.Enums;
using StructureHelperCommon.Models.Materials.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureHelperLogics.Models.Materials
{
    class StudyElasticMaterial : IElasticMaterial
    {
        private IElasticMaterialLogic elasticMaterialLogic => new ElasticMaterialLogic();
        public double Modulus { get; set; }
        public double CompressiveStrength { get; set; }
        public double TensileStrength { get; set; }
        public List<IMaterialSafetyFactor> SafetyFactors { get; }
        public StudyElasticMaterial()
        {
            SafetyFactors = new List<IMaterialSafetyFactor>();
        }
        public IMaterial GetLoaderMaterial(LimitStates limitState, CalcTerms calcTerm)
        {
            var material = elasticMaterialLogic.GetLoaderMaterial(this, limitState, calcTerm);
            return material;
        }
        public object Clone()
        {
            var newItem = new StudyElasticMaterial();
            var updateStrategy = new ElasticUpdateStrategy();
            updateStrategy.Update(newItem, this);
            return newItem;
        }

        public IMaterial GetCrackedLoaderMaterial(LimitStates limitState, CalcTerms calcTerm)
        {
            return GetLoaderMaterial(limitState, calcTerm);
        }


    }
}
