namespace GasOptimizationLib.Models;

public class GeneralParameters
{
    public double CokeCost { get; set; } //Цена кокса
    public double CokeStock { get; set; } //Запас кокса
    public double GasCost { get; set; } //Цена газа
    public double GasStock { get; set; } //Запас газа
    public double CastIronNeed { get; set; } //Запрос по чугуну
}