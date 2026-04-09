namespace GasOptimizationLib.Models;

public class FurnaceParameters
{
    public double GasUsage { get; set; } //Расход природного газа
    public double MinimalGasUsage { get; set; } //Минимальный расход
    public double MaximalGasUsage { get; set; } //Максимальный раход
    
    public double CokeCoalUsage { get; set; } //Базовый расход коксового угля
    public double CokeReplacementKoefficient { get; set; } //Эквивалент замены кокса

    public double CastironProductivity { get; set; } //Производительность по чугуну в базовом периоде
    
    public double BurningTemperature { get; set; } //Температура горения
    public double MinimalBurningTemperature { get; set; } //Минимальная температура горения
    public double MaximalBurningTemperature { get; set; } //Максимальная температура горения
    
    public double ProductivityChangeByGasChange { get; set; } //Изменение производства чугуна при изменении ПГ
    public double ProductivityChangeByCokeChange { get; set; } //Изменение производительности при замене кокса
    public double TemperatureChangeByGasChange { get; set; } //Изменение температуры горения при изменении расхода газа
}