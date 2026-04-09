using GasOptimizationLib.Models;
using Google.OrTools.LinearSolver;

namespace GasOptimizationLib;

public class FurnaceOptimizer
{
    public OutputData Solve(InputData inputData)
    {
        var solver = Solver.CreateSolver("GLOP");
        if (solver == null)
            return new OutputData 
            { 
                Success = false, 
                SolverStatus = "SOLVER_NOT_AVAILABLE",
                Message = "GLOP solver not found. Install Google.OrTools NuGet package."
            };

        var general = inputData.GeneralParameters;
        var furnaces = inputData.FurnaceParameters;
        int n = furnaces.Length;

        if (n == 0)
            return new OutputData 
            { 
                Success = true, 
                SolverStatus = "NO_FURNACES",
                Message = "Input contains no furnaces to optimize.",
                SolvedFurnaces = Array.Empty<SolvedFurnace>()
            };

        // === 1. Переменные решения: SolvedGasUsage[i] ===
        var solvedGasVars = new Variable[n];
        for (int i = 0; i < n; i++)
        {
            var fp = furnaces[i];
            solvedGasVars[i] = solver.MakeNumVar(
                fp.MinimalGasUsage, 
                fp.MaximalGasUsage, 
                $"solved_gas_{i}");
        }

        // === 2. Целевая функция: Максимизация суммы SolvedMoneySave ===
        // SolvedMoneySave = (CokeReplacementKoefficient * CokeCost - GasCost) * SolvedGasUsage
        var objective = solver.Objective();
        for (int i = 0; i < n; i++)
        {
            var fp = furnaces[i];
            double coefficient = (fp.CokeReplacementKoefficient * general.CokeCost - general.GasCost);
            objective.SetCoefficient(solvedGasVars[i], coefficient);
        }
        objective.SetMaximization();

        // === 3. Ограничения ===

        // 3.1. Σ SolvedGasUsage <= GasStock
        var gasStockConstraint = solver.MakeConstraint(0, general.GasStock, "total_gas_limit");
        for (int i = 0; i < n; i++)
            gasStockConstraint.SetCoefficient(solvedGasVars[i], 1.0);

        // 3.2. Σ SolvedCokeCoalUsage <= CokeStock
        // Формула: CokeCoalUsage + 0.001 * (GasUsage - SolvedGasUsage) * Koeff
        // => const_part + (-0.001 * Koeff) * SolvedGasUsage
        var cokeStockConstraint = solver.MakeConstraint(0, general.CokeStock, "total_coke_limit");
        double totalCokeConstant = 0;
        for (int i = 0; i < n; i++)
        {
            var fp = furnaces[i];
            double constPart = fp.CokeCoalUsage + 0.001 * fp.GasUsage * fp.CokeReplacementKoefficient;
            double coeffForVar = -0.001 * fp.CokeReplacementKoefficient;
            
            totalCokeConstant += constPart;
            cokeStockConstraint.SetCoefficient(solvedGasVars[i], coeffForVar);
        }
        // Переносим константную часть в правую часть неравенства:
        // Σ(coeff * x) <= CokeStock - totalCokeConstant
        cokeStockConstraint.SetBounds(0, general.CokeStock - totalCokeConstant);

        // 3.3. Ограничения температуры для каждой печи
        // SolvedBurnTemperature = BurningTemperature + TempCoeff * (SolvedGasUsage - GasUsage)
        // MinTemp <= ... <= MaxTemp
        for (int i = 0; i < n; i++)
        {
            var fp = furnaces[i];
            double tempCoeff = fp.TemperatureChangeByGasChange;
            double baseTempShift = fp.BurningTemperature - tempCoeff * fp.GasUsage;

            if (Math.Abs(tempCoeff) < 1e-9)
            {
                // Температура не зависит от газа — проверяем статическое условие
                if (fp.BurningTemperature < fp.MinimalBurningTemperature - 1e-6 || 
                    fp.BurningTemperature > fp.MaximalBurningTemperature + 1e-6)
                {
                    return new OutputData
                    {
                        Success = false,
                        SolverStatus = "INFEASIBLE_STATIC_TEMP",
                        Message = $"Furnace {i}: base temperature {fp.BurningTemperature} out of bounds [{fp.MinimalBurningTemperature}, {fp.MaximalBurningTemperature}]"
                    };
                }
                continue;
            }

            // Преобразуем: MinTemp <= baseTempShift + tempCoeff * x <= MaxTemp
            double lowerBound = (fp.MinimalBurningTemperature - baseTempShift) / tempCoeff;
            double upperBound = (fp.MaximalBurningTemperature - baseTempShift) / tempCoeff;
            
            // Если коэффициент отрицательный — границы меняются местами
            if (tempCoeff < 0) (lowerBound, upperBound) = (upperBound, lowerBound);

            var tempConstraint = solver.MakeConstraint(lowerBound, upperBound, $"temp_limit_{i}");
            tempConstraint.SetCoefficient(solvedGasVars[i], 1.0);
        }

        // 3.4. Σ SolvedCastironProductivity >= CastIronNeed
        // Формула: BaseProd + (SolvedGas - BaseGas) * ProdGasCoeff - (SolvedGas - BaseGas) * Koeff * ProdCokeCoeff
        // => const_part + (ProdGasCoeff - Koeff * ProdCokeCoeff) * SolvedGasUsage
        var productionConstraint = solver.MakeConstraint(general.CastIronNeed, double.MaxValue, "min_production");
        double totalProdConstant = 0;
        for (int i = 0; i < n; i++)
        {
            var fp = furnaces[i];
            double prodCoeff = fp.ProductivityChangeByGasChange 
                             - fp.CokeReplacementKoefficient * fp.ProductivityChangeByCokeChange;
            double constPart = fp.CastironProductivity 
                             - fp.GasUsage * fp.ProductivityChangeByGasChange 
                             + fp.GasUsage * fp.CokeReplacementKoefficient * fp.ProductivityChangeByCokeChange;
            
            totalProdConstant += constPart;
            productionConstraint.SetCoefficient(solvedGasVars[i], prodCoeff);
        }
        // Переносим константу: Σ(coeff * x) >= CastIronNeed - totalProdConstant
        productionConstraint.SetBounds(general.CastIronNeed - totalProdConstant, double.MaxValue);

        // === 4. Запуск решателя ===
        var status = solver.Solve();

        // === 5. Формирование результата ===
        if (status == Solver.ResultStatus.OPTIMAL || status == Solver.ResultStatus.FEASIBLE)
        {
            var results = new SolvedFurnace[n];
            double totalSave = 0, totalGas = 0, totalCoke = 0, totalProd = 0;

            for (int i = 0; i < n; i++)
            {
                var fp = furnaces[i];
                double solvedGas = solvedGasVars[i].SolutionValue();

                // === Расчёт всех производных полей по точным формулам ===
                double solvedTemp = fp.BurningTemperature 
                                  + fp.TemperatureChangeByGasChange * (solvedGas - fp.GasUsage);

                double solvedCoke = fp.CokeCoalUsage 
                                  + 0.001 * (fp.GasUsage - solvedGas) * fp.CokeReplacementKoefficient;

                double solvedProd = fp.CastironProductivity 
                                  + (solvedGas - fp.GasUsage) * fp.ProductivityChangeByGasChange 
                                  - (solvedGas - fp.GasUsage) * fp.CokeReplacementKoefficient * fp.ProductivityChangeByCokeChange;

                double solvedSave = (fp.CokeReplacementKoefficient * general.CokeCost - general.GasCost) * solvedGas;

                // Накопление итогов
                totalSave += solvedSave;
                totalGas += solvedGas;
                totalCoke += solvedCoke;
                totalProd += solvedProd;

                results[i] = new SolvedFurnace
                {
                    FurnaceIndex = i,
                    SolvedGasUsage = solvedGas,
                    SolvedBurnTemperature = solvedTemp,
                    SolvedCokeCoalUsage = solvedCoke,
                    SolvedCastironProductivity = solvedProd,
                    SolvedMoneySave = solvedSave,
                    // Исходные параметры для валидации
                    BaseGasUsage = fp.GasUsage,
                    MinimalGasUsage = fp.MinimalGasUsage,
                    MaximalGasUsage = fp.MaximalGasUsage,
                    MinimalBurningTemperature = fp.MinimalBurningTemperature,
                    MaximalBurningTemperature = fp.MaximalBurningTemperature
                };
            }

            return new OutputData
            {
                Success = true,
                SolverStatus = status.ToString(),
                Message = "Optimization completed successfully",
                TotalMoneySave = totalSave,
                TotalGasUsed = totalGas,
                TotalCokeUsed = totalCoke,
                TotalProduction = totalProd,
                SolvedFurnaces = results
            };
        }

        return new OutputData
        {
            Success = false,
            SolverStatus = status.ToString() ?? "UNKNOWN",
            Message = $"Solver failed to find feasible solution. Status: {status}",
            SolvedFurnaces = Array.Empty<SolvedFurnace>()
        };
    }
}