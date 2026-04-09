using System.Windows;
using System.Windows.Input;
using S.ModernManagementMethods.Infrastructure;

namespace S.ModernManagementMethods.ViewModels;

public class FurnaceDialogViewModel : ViewModelBase
    {
        private FurnaceViewModel? _furnace;
        
        public string GasUsage { get; set; } = string.Empty;
        public string MinimalGasUsage { get; set; } = string.Empty;
        public string MaximalGasUsage { get; set; } = string.Empty;
        public string CokeCoalUsage { get; set; } = string.Empty;
        public string CokeReplacementKoefficient { get; set; } = string.Empty;
        public string CastironProductivity { get; set; } = string.Empty;
        public string BurningTemperature { get; set; } = string.Empty;
        public string MinimalBurningTemperature { get; set; } = string.Empty;
        public string MaximalBurningTemperature { get; set; } = string.Empty;
        public string ProductivityChangeByGasChange { get; set; } = string.Empty;
        public string ProductivityChangeByCokeChange { get; set; } = string.Empty;
        public string TemperatureChangeByGasChange { get; set; } = string.Empty;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public FurnaceViewModel? Furnace => _furnace;

        public FurnaceDialogViewModel(FurnaceViewModel? existingFurnace, int newIndex)
        {
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            if (existingFurnace != null)
            {
                FillFields(existingFurnace);
            }
            else
            {
                SetDefaultValues(newIndex);
            }
        }

        private void SetDefaultValues(int index)
        {
            GasUsage = "15000";
            MinimalGasUsage = "10000";
            MaximalGasUsage = "20000";
            CokeCoalUsage = "60.0";
            CokeReplacementKoefficient = "0.7";
            CastironProductivity = "140.0";
            BurningTemperature = "1950";
            MinimalBurningTemperature = "1900";
            MaximalBurningTemperature = "2100";
            ProductivityChangeByGasChange = "-0.0007";
            ProductivityChangeByCokeChange = "-0.003";
            TemperatureChangeByGasChange = "-0.03";
        }

        private void FillFields(FurnaceViewModel f)
        {
            GasUsage = f.GasUsage.ToString();
            MinimalGasUsage = f.MinimalGasUsage.ToString();
            MaximalGasUsage = f.MaximalGasUsage.ToString();
            CokeCoalUsage = f.CokeCoalUsage.ToString();
            CokeReplacementKoefficient = f.CokeReplacementKoefficient.ToString();
            CastironProductivity = f.CastironProductivity.ToString();
            BurningTemperature = f.BurningTemperature.ToString();
            MinimalBurningTemperature = f.MinimalBurningTemperature.ToString();
            MaximalBurningTemperature = f.MaximalBurningTemperature.ToString();
            ProductivityChangeByGasChange = f.ProductivityChangeByGasChange.ToString();
            ProductivityChangeByCokeChange = f.ProductivityChangeByCokeChange.ToString();
            TemperatureChangeByGasChange = f.TemperatureChangeByGasChange.ToString();
        }

        private bool CanSave(object? parameter) => true;

        private void Save(object? parameter)
        {
            try
            {
                _furnace = new FurnaceViewModel
                {
                    Index = GetIndexFromTitle(),
                    GasUsage = double.Parse(GasUsage),
                    MinimalGasUsage = double.Parse(MinimalGasUsage),
                    MaximalGasUsage = double.Parse(MaximalGasUsage),
                    CokeCoalUsage = double.Parse(CokeCoalUsage),
                    CokeReplacementKoefficient = double.Parse(CokeReplacementKoefficient),
                    CastironProductivity = double.Parse(CastironProductivity),
                    BurningTemperature = double.Parse(BurningTemperature),
                    MinimalBurningTemperature = double.Parse(MinimalBurningTemperature),
                    MaximalBurningTemperature = double.Parse(MaximalBurningTemperature),
                    ProductivityChangeByGasChange = double.Parse(ProductivityChangeByGasChange),
                    ProductivityChangeByCokeChange = double.Parse(ProductivityChangeByCokeChange),
                    TemperatureChangeByGasChange = double.Parse(TemperatureChangeByGasChange)
                };

                // Если редактируем существующую, сохраняем её Index
                if (Application.Current.MainWindow is Views.FurnaceDialogWindow window && 
                    window.Tag is int existingIndex)
                {
                    _furnace.Index = existingIndex;
                }

                var dialogWindow = Application.Current.Windows
                    .OfType<Views.FurnaceDialogWindow>()
                    .FirstOrDefault(w => w.DataContext == this);
            
                if (dialogWindow != null)
                    dialogWindow.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в данных: {ex.Message}", "Ошибка ввода", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel(object? parameter)
        {
            var dialogWindow = Application.Current.Windows
                .OfType<Views.FurnaceDialogWindow>()
                .FirstOrDefault(w => w.DataContext == this);
        
            if (dialogWindow != null)
                dialogWindow.DialogResult = false;
        }
        
        private int GetIndexFromTitle()
        {
            if (Application.Current.MainWindow != null)
            {
                var title = Application.Current.MainWindow.Title;
                var parts = title.Split('#');
                if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out var idx))
                    return idx;
            }
            return 1;
        }
    }