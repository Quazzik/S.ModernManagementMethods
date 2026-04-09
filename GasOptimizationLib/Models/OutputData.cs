namespace GasOptimizationLib.Models;

public class OutputData
{
    public bool Success { get; set; }
    public string? SolverStatus { get; set; }
    public string? Message { get; set; }
    
    public double TotalMoneySave { get; set; }
    public double TotalGasUsed { get; set; }
    public double TotalCokeUsed { get; set; }
    public double TotalProduction { get; set; }
    
    public SolvedFurnace[] SolvedFurnaces { get; set; } = Array.Empty<SolvedFurnace>();
}