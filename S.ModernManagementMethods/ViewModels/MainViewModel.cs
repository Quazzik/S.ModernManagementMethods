using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GasOptimizationLib;
using GasOptimizationLib.Models;
using S.ModernManagementMethods.Infrastructure;

namespace S.ModernManagementMethods.ViewModels;

public class MainViewModel : ViewModelBase
    {
        private readonly FurnaceOptimizer _optimizer;
        private FurnaceViewModel? _selectedFurnace;
        private string _statusMessage = string.Empty;

        // Общие параметры
        public double CokeCost { get; set; } = 1.8;
        public double GasCost { get; set; } = 0.6;
        public double GasStock { get; set; } = 115000;
        public double CokeStock { get; set; } = 450;
        public double CastIronNeed { get; set; } = 1000;

        public ObservableCollection<FurnaceViewModel> Furnaces { get; }
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public FurnaceViewModel? SelectedFurnace
        {
            get => _selectedFurnace;
            set => SetProperty(ref _selectedFurnace, value);
        }

        // Команды
        public ICommand AddFurnaceCommand { get; }
        public ICommand EditFurnaceCommand { get; }
        public ICommand DeleteFurnaceCommand { get; }
        public ICommand CalculateCommand { get; }

        public MainViewModel()
        {
            _optimizer = new FurnaceOptimizer();
            Furnaces = new ObservableCollection<FurnaceViewModel>();
            
            AddFurnaceCommand = new RelayCommand(AddFurnace, _ => true);
            EditFurnaceCommand = new RelayCommand(EditFurnace, _ => SelectedFurnace != null);
            DeleteFurnaceCommand = new RelayCommand(DeleteFurnace, _ => SelectedFurnace != null);
            CalculateCommand = new RelayCommand(Calculate, _ => Furnaces.Count > 0);

            LoadDefaultData();
        }

        private void LoadDefaultData()
        {
            var defaultData = new[]
            {
                new { GasUsage = 15000.0, CokeCoalUsage = 64.25, CastironProductivity = 146.4, 
                      BurningTemperature = 1938.0, CokeReplacementKoefficient = 0.59,
                      ProductivityChangeByGasChange = -0.0007295, ProductivityChangeByCokeChange = -0.00297,
                      TemperatureChangeByGasChange = -0.0265 },
                new { GasUsage = 17000.0, CokeCoalUsage = 66.76, CastironProductivity = 136.4, 
                      BurningTemperature = 1959.0, CokeReplacementKoefficient = 0.53,
                      ProductivityChangeByGasChange = -0.0006695, ProductivityChangeByCokeChange = -0.00297,
                      TemperatureChangeByGasChange = -0.0356 },
                new { GasUsage = 11000.0, CokeCoalUsage = 56.08, CastironProductivity = 134.3, 
                      BurningTemperature = 2091.0, CokeReplacementKoefficient = 0.85,
                      ProductivityChangeByGasChange = 0.0, ProductivityChangeByCokeChange = -0.002928,
                      TemperatureChangeByGasChange = -0.038 },
                new { GasUsage = 13000.0, CokeCoalUsage = 49.78, CastironProductivity = 122.3, 
                      BurningTemperature = 1990.0, CokeReplacementKoefficient = 0.59,
                      ProductivityChangeByGasChange = -0.00072373, ProductivityChangeByCokeChange = -0.002897,
                      TemperatureChangeByGasChange = -0.0334 },
                new { GasUsage = 12000.0, CokeCoalUsage = 62.92, CastironProductivity = 138.2, 
                      BurningTemperature = 1997.0, CokeReplacementKoefficient = 0.75,
                      ProductivityChangeByGasChange = -0.0007724, ProductivityChangeByCokeChange = -0.00297,
                      TemperatureChangeByGasChange = -0.02984 },
                new { GasUsage = 15000.0, CokeCoalUsage = 60.02, CastironProductivity = 138.8, 
                      BurningTemperature = 1925.0, CokeReplacementKoefficient = 0.79,
                      ProductivityChangeByGasChange = -0.0006872, ProductivityChangeByCokeChange = -0.00297,
                      TemperatureChangeByGasChange = -0.0314 },
                new { GasUsage = 17000.0, CokeCoalUsage = 81.68, CastironProductivity = 191.4, 
                      BurningTemperature = 1974.0, CokeReplacementKoefficient = 0.87,
                      ProductivityChangeByGasChange = -0.0007284, ProductivityChangeByCokeChange = -0.003316,
                      TemperatureChangeByGasChange = -0.0223 }
            };

            int index = 1;
            foreach (var data in defaultData)
            {
                Furnaces.Add(new FurnaceViewModel
                {
                    Index = index++,
                    GasUsage = data.GasUsage,
                    MinimalGasUsage = 10000,
                    MaximalGasUsage = 20000,
                    CokeCoalUsage = data.CokeCoalUsage,
                    CastironProductivity = data.CastironProductivity,
                    BurningTemperature = data.BurningTemperature,
                    MinimalBurningTemperature = 1900,
                    MaximalBurningTemperature = 2100,
                    CokeReplacementKoefficient = data.CokeReplacementKoefficient,
                    ProductivityChangeByGasChange = data.ProductivityChangeByGasChange,
                    ProductivityChangeByCokeChange = data.ProductivityChangeByCokeChange,
                    TemperatureChangeByGasChange = data.TemperatureChangeByGasChange
                });
            }
        }

        private void AddFurnace(object? parameter)
        {
            var dialogViewModel = new FurnaceDialogViewModel(null, Furnaces.Count + 1);
            var dialog = new Views.FurnaceDialogWindow { DataContext = dialogViewModel };
            
            if (dialog.ShowDialog() == true && dialogViewModel.Furnace != null)
            {
                Furnaces.Add(dialogViewModel.Furnace);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void EditFurnace(object? parameter)
        {
            if (SelectedFurnace == null) return;

            var dialogViewModel = new FurnaceDialogViewModel(SelectedFurnace, SelectedFurnace.Index);
            var dialog = new Views.FurnaceDialogWindow { DataContext = dialogViewModel };
            
            if (dialog.ShowDialog() == true && dialogViewModel.Furnace != null)
            {
                var index = Furnaces.IndexOf(SelectedFurnace);
                Furnaces[index] = dialogViewModel.Furnace;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void DeleteFurnace(object? parameter)
        {
            if (SelectedFurnace == null) return;

            var result = MessageBox.Show(
                $"Удалить печь №{SelectedFurnace.Index}?", 
                "Подтверждение",
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Furnaces.Remove(SelectedFurnace);
                int idx = 1;
                foreach (var f in Furnaces)
                    f.Index = idx++;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void Calculate(object? parameter)
        {
            try
            {
                var generalParams = new GeneralParameters
                {
                    CokeCost = CokeCost,
                    GasCost = GasCost,
                    GasStock = GasStock,
                    CokeStock = CokeStock,
                    CastIronNeed = CastIronNeed
                };

                var furnaceParams = Furnaces.Select(f => new FurnaceParameters
                {
                    GasUsage = f.GasUsage,
                    MinimalGasUsage = f.MinimalGasUsage,
                    MaximalGasUsage = f.MaximalGasUsage,
                    CokeCoalUsage = f.CokeCoalUsage,
                    CokeReplacementKoefficient = f.CokeReplacementKoefficient,
                    CastironProductivity = f.CastironProductivity,
                    BurningTemperature = f.BurningTemperature,
                    MinimalBurningTemperature = f.MinimalBurningTemperature,
                    MaximalBurningTemperature = f.MaximalBurningTemperature,
                    ProductivityChangeByGasChange = f.ProductivityChangeByGasChange,
                    ProductivityChangeByCokeChange = f.ProductivityChangeByCokeChange,
                    TemperatureChangeByGasChange = f.TemperatureChangeByGasChange
                }).ToArray();

                var inputData = new InputData
                {
                    GeneralParameters = generalParams,
                    FurnaceParameters = furnaceParams
                };

                StatusMessage = "Выполняется расчёт...";
                var result = _optimizer.Solve(inputData);

                if (result.Success)
                {
                    var resultsViewModel = new ResultsViewModel(result, generalParams);
                    var resultsWindow = new Views.ResultsWindow { DataContext = resultsViewModel };
                    resultsWindow.ShowDialog();
                    StatusMessage = "Расчёт завершён успешно";
                }
                else
                {
                    MessageBox.Show($"Ошибка оптимизации: {result.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusMessage = "Ошибка расчёта";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчёте: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Ошибка расчёта";
            }
        }
    }