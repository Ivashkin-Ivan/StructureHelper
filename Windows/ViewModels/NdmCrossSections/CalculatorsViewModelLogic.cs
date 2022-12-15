﻿using StructureHelper.Infrastructure;
using StructureHelper.Windows.CalculationWindows.CalculatorsViews.ForceCalculatorViews;
using StructureHelper.Windows.ViewModels.Calculations.Calculators;
using StructureHelperLogics.Models.CrossSections;
using StructureHelperLogics.NdmCalculations.Analyses;
using StructureHelperLogics.NdmCalculations.Analyses.ByForces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace StructureHelper.Windows.ViewModels.NdmCrossSections
{
    public class CalculatorsViewModelLogic : ViewModelBase, ICalculatorsViewModelLogic
    {
        private readonly ICrossSectionRepository repository;

        public INdmCalculator SelectedItem { get; set; }
        public ObservableCollection<INdmCalculator> Items
        {
            get
            {
                var collection = new ObservableCollection<INdmCalculator>();
                foreach (var item in repository.CalculatorsList)
                {
                    collection.Add(item);
                }
                return collection;
            }
        }

        private RelayCommand addCalculatorCommand;
        public RelayCommand Add
        {
            get
            {
                return addCalculatorCommand ??
                    (
                    addCalculatorCommand = new RelayCommand(o =>
                    {
                        AddCalculator();
                        OnPropertyChanged(nameof(Items));
                    }));
            }
        }
        private void AddCalculator()
        {
            var item = new ForceCalculator() { Name = "New force calculator" };
            repository.CalculatorsList.Add(item);
        }
        private RelayCommand editCalculatorCommand;
        public RelayCommand Edit
        {
            get
            {
                return editCalculatorCommand ??
                    (
                    editCalculatorCommand = new RelayCommand(o =>
                    {
                        EditCalculator();
                        OnPropertyChanged(nameof(Items));
                    }, o => SelectedItem != null));
            }
        }
        private void EditCalculator()
        {
            if (SelectedItem is ForceCalculator)
            {
                var calculator = SelectedItem as ForceCalculator;
                var vm = new ForceCalculatorViewModel(repository.Primitives, repository.ForceCombinationLists, calculator);

                var wnd = new ForceCalculatorView(vm);
                wnd.ShowDialog();
            }
        }
        private RelayCommand deleteCalculatorCommand;
        private RelayCommand runCommand;
        public RelayCommand Delete
        {
            get
            {
                return deleteCalculatorCommand ??
                    (
                    deleteCalculatorCommand = new RelayCommand(o =>
                    {
                        DeleteCalculator();
                    }, o => SelectedItem != null));
            }
        }

        public RelayCommand Run
        {
            get
            {
                return runCommand ??
                (
                runCommand = new RelayCommand(o =>
                {
                    (SelectedItem as INdmCalculator).Run();
                }, o => SelectedItem != null));
            }
        }

        private void DeleteCalculator()
        {
            var dialogResult = MessageBox.Show("Delete calculator?", "Please, confirm deleting", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                repository.CalculatorsList.Remove(SelectedItem as INdmCalculator);
                OnPropertyChanged(nameof(Items));
            }
        }
        public CalculatorsViewModelLogic(ICrossSectionRepository repository)
        {
            this.repository = repository;
        }
    }
}
