using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using GasOptimizationLib.Models;
using S.ModernManagementMethods.Infrastructure;

namespace S.ModernManagementMethods.ViewModels;

public class ResultsViewModel : ViewModelBase
    {
        private readonly OutputData _result;
        private readonly GeneralParameters _generalParams;

        public string TotalMoneySave { get; }
        public string TotalGasUsed { get; }
        public string TotalCokeUsed { get; }
        public string TotalProduction { get; }
        public string SolverStatus { get; }
        public string Message { get; }

        public ObservableCollection<SolvedFurnaceViewModel> SolvedFurnaces { get; }

        public ICommand CloseCommand { get; }
        public ICommand ExportCommand { get; }

        public ResultsViewModel(OutputData result, GeneralParameters generalParams)
        {
            _result = result;
            _generalParams = generalParams;

            TotalMoneySave = $"{result.TotalMoneySave:F2}";
            TotalGasUsed = $"{result.TotalGasUsed:F2}";
            TotalCokeUsed = $"{result.TotalCokeUsed:F2}";
            TotalProduction = $"{result.TotalProduction:F2}";
            SolverStatus = result.SolverStatus ?? string.Empty;
            Message = result.Message ?? string.Empty;

            SolvedFurnaces = new ObservableCollection<SolvedFurnaceViewModel>(
                result.SolvedFurnaces.Select(f => new SolvedFurnaceViewModel(f))
            );

            CloseCommand = new RelayCommand(Close);
            ExportCommand = new RelayCommand(Export);
        }

        private void Close(object? parameter)
        {
            var resultsWindow = Application.Current.Windows
                .OfType<Views.ResultsWindow>()
                .FirstOrDefault(w => w.DataContext == this);
        
            if (resultsWindow != null)
                resultsWindow.DialogResult = true;
        }

        private void Export(object? parameter)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv",
                FileName = $"optimization_results_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                ExportToCsv(dialog.FileName);
                MessageBox.Show("Данные экспортированы успешно!", "Экспорт", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportToCsv(string fileName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("№ печи;Расход газа (м³/ч);Расход кокса (т/ч);Производство (т/ч);Температура (°C);Экономия (руб/ч)");
            
            foreach (var furnace in SolvedFurnaces)
            {
                sb.AppendLine($"{furnace.FurnaceIndex};" +
                             $"{furnace.SolvedGasUsage:F2};" +
                             $"{furnace.SolvedCokeCoalUsage:F2};" +
                             $"{furnace.SolvedCastironProductivity:F2};" +
                             $"{furnace.SolvedBurnTemperature:F2};" +
                             $"{furnace.SolvedMoneySave:F2}");
            }

            sb.AppendLine();
            sb.AppendLine("ИТОГО:");
            sb.AppendLine($"Общая экономия: {TotalMoneySave} руб/ч");
            sb.AppendLine($"Общий расход газа: {TotalGasUsed} м³/ч");
            sb.AppendLine($"Общий расход кокса: {TotalCokeUsed} т/ч");
            sb.AppendLine($"Общее производство: {TotalProduction} т/ч");

            System.IO.File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
        }
    }

    public class SolvedFurnaceViewModel : ViewModelBase
    {
        private readonly SolvedFurnace _model;

        public int FurnaceIndex => _model.FurnaceIndex;
        public string SolvedGasUsage => $"{_model.SolvedGasUsage:F2}";
        public string SolvedCokeCoalUsage => $"{_model.SolvedCokeCoalUsage:F2}";
        public string SolvedCastironProductivity => $"{_model.SolvedCastironProductivity:F2}";
        public string SolvedBurnTemperature => $"{_model.SolvedBurnTemperature:F2}";
        public string SolvedMoneySave => $"{_model.SolvedMoneySave:F2}";
        public bool IsGasUsageValid => _model.IsGasUsageValid;
        public bool IsTemperatureValid => _model.IsTemperatureValid;

        public SolvedFurnaceViewModel(SolvedFurnace model)
        {
            _model = model;
        }
    }