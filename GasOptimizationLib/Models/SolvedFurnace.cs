namespace GasOptimizationLib.Models;

public class SolvedFurnace
{
    public int FurnaceIndex { get; set; }

    // === Решённые и рассчитанные значения ===
    public double SolvedGasUsage { get; set; }
    public double SolvedBurnTemperature { get; set; }
    public double SolvedCokeCoalUsage { get; set; }
    public double SolvedCastironProductivity { get; set; }
    public double SolvedMoneySave { get; set; }

    // === Исходные параметры (для сверки и отладки) ===
    public double BaseGasUsage { get; set; }
    public double MinimalGasUsage { get; set; }
    public double MaximalGasUsage { get; set; }
    public double MinimalBurningTemperature { get; set; }
    public double MaximalBurningTemperature { get; set; }

    // === Вычисляемые флаги валидации ограничений ===
    public bool IsGasUsageValid => 
        SolvedGasUsage >= MinimalGasUsage - 1e-6 && SolvedGasUsage <= MaximalGasUsage + 1e-6;

    public bool IsTemperatureValid => 
        SolvedBurnTemperature >= MinimalBurningTemperature - 1e-6 && 
        SolvedBurnTemperature <= MaximalBurningTemperature + 1e-6;

    public bool IsCokeUsageNonNegative => SolvedCokeCoalUsage >= -1e-6;

    // Конструктор для удобного создания с авто-расчётом флагов
    public SolvedFurnace() { }
}