using S.ModernManagementMethods.Infrastructure;

namespace S.ModernManagementMethods.ViewModels;

public class FurnaceViewModel : ViewModelBase
    {
        private int _index;
        private double _gasUsage;
        private double _minimalGasUsage;
        private double _maximalGasUsage;
        private double _cokeCoalUsage;
        private double _castironProductivity;
        private double _burningTemperature;
        private double _minimalBurningTemperature;
        private double _maximalBurningTemperature;
        private double _cokeReplacementKoefficient;
        private double _productivityChangeByGasChange;
        private double _productivityChangeByCokeChange;
        private double _temperatureChangeByGasChange;

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public double GasUsage
        {
            get => _gasUsage;
            set => SetProperty(ref _gasUsage, value);
        }

        public double MinimalGasUsage
        {
            get => _minimalGasUsage;
            set => SetProperty(ref _minimalGasUsage, value);
        }

        public double MaximalGasUsage
        {
            get => _maximalGasUsage;
            set => SetProperty(ref _maximalGasUsage, value);
        }

        public double CokeCoalUsage
        {
            get => _cokeCoalUsage;
            set => SetProperty(ref _cokeCoalUsage, value);
        }

        public double CastironProductivity
        {
            get => _castironProductivity;
            set => SetProperty(ref _castironProductivity, value);
        }

        public double BurningTemperature
        {
            get => _burningTemperature;
            set => SetProperty(ref _burningTemperature, value);
        }

        public double MinimalBurningTemperature
        {
            get => _minimalBurningTemperature;
            set => SetProperty(ref _minimalBurningTemperature, value);
        }

        public double MaximalBurningTemperature
        {
            get => _maximalBurningTemperature;
            set => SetProperty(ref _maximalBurningTemperature, value);
        }

        public double CokeReplacementKoefficient
        {
            get => _cokeReplacementKoefficient;
            set => SetProperty(ref _cokeReplacementKoefficient, value);
        }

        public double ProductivityChangeByGasChange
        {
            get => _productivityChangeByGasChange;
            set => SetProperty(ref _productivityChangeByGasChange, value);
        }

        public double ProductivityChangeByCokeChange
        {
            get => _productivityChangeByCokeChange;
            set => SetProperty(ref _productivityChangeByCokeChange, value);
        }

        public double TemperatureChangeByGasChange
        {
            get => _temperatureChangeByGasChange;
            set => SetProperty(ref _temperatureChangeByGasChange, value);
        }
    }