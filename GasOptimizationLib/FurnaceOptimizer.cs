using GasOptimizationLib.Models;
using Google.OrTools.LinearSolver;

namespace GasOptimizationLib;

public class FurnaceOptimizer
{
    public OutputData Solve(InputData inputData)
    {
        var solver = Solver.CreateSolver("GLOP");
        if (solver == null)
            return CreateErrorOutput("SOLVER_NOT_AVAILABLE", "GLOP solver not found. Install Google.OrTools package.");

        var furnaces = inputData.FurnaceParameters;
        var general = inputData.GeneralParameters;
        int n = furnaces.Length;

        // 1. Создаём переменные с широкими границами (ограничения добавим отдельно)
        var gasVars = new Variable[n];
        for (int i = 0; i < n; i++)
        {
            gasVars[i] = solver.MakeNumVar(0, double.PositiveInfinity, $"gas_{i}");
        }

        // 2. Накопители для глобальных ограничений и целевой функции
        LinearExpr totalGas = new LinearExpr();
        LinearExpr totalCoke = new LinearExpr();
        LinearExpr totalProduction = new LinearExpr();
        LinearExpr totalSave = new LinearExpr();

        // 3. Добавляем ограничения и формируем выражения для каждой печи
        for (int i = 0; i < n; i++)
        {
            var fp = furnaces[i];
            var gVar = gasVars[i];

            // Суммарный газ
            totalGas += gVar;

            // Локальные ограничения газа
            solver.Add(gVar >= fp.MinimalGasUsage);
            solver.Add(gVar <= fp.MaximalGasUsage);

            // Расход кокса
            var cokeUsage = fp.CokeCoalUsage + 0.001 * (fp.GasUsage - gVar) * fp.CokeReplacementKoefficient;
            totalCoke += cokeUsage;

            // Температура горения
            var burningTemp = fp.BurningTemperature + fp.TemperatureChangeByGasChange * (gVar - fp.GasUsage);
            solver.Add(burningTemp >= fp.MinimalBurningTemperature);
            solver.Add(burningTemp <= fp.MaximalBurningTemperature);

            // Производство чугуна
            var productivity = fp.CastironProductivity 
                             + (gVar - fp.GasUsage) * fp.ProductivityChangeByGasChange 
                             - (gVar - fp.GasUsage) * fp.CokeReplacementKoefficient * fp.ProductivityChangeByCokeChange;
            totalProduction += productivity;

            // Экономия (прибыль)
            var profit = (fp.CokeReplacementKoefficient * general.CokeCost - general.GasCost) * gVar;
            totalSave += profit;
        }

        // 4. Глобальные ограничения
        solver.Add(totalGas <= general.GasStock);
        solver.Add(totalCoke <= general.CokeStock);
        solver.Add(totalProduction >= general.CastIronNeed);

        // 5. Целевая функция: Максимизация экономии
        solver.Maximize(totalSave);

        // 6. Запуск решателя
        var status = solver.Solve();

        if (status != Solver.ResultStatus.OPTIMAL && status != Solver.ResultStatus.FEASIBLE)
        {
            return new OutputData
            {
                Success = false,
                SolverStatus = status.ToString(),
                Message = "Решатель не нашёл допустимого решения. Проверьте входные ограничения и запасы.",
                SolvedFurnaces = Array.Empty<SolvedFurnace>()
            };
        }

        // 7. Сбор результатов
        var results = new SolvedFurnace[n];
        double totalSaveVal = 0, totalGasVal = 0, totalCokeVal = 0, totalProdVal = 0;

        for (int i = 0; i < n; i++)
        {
            var fp = furnaces[i];
            double solvedGas = gasVars[i].SolutionValue();

            // Пересчёт всех показателей по точным формулам
            double solvedTemp = fp.BurningTemperature + fp.TemperatureChangeByGasChange * (solvedGas - fp.GasUsage);
            double solvedCoke = fp.CokeCoalUsage + 0.001 * (fp.GasUsage - solvedGas) * fp.CokeReplacementKoefficient;
            double solvedProd = fp.CastironProductivity 
                              + (solvedGas - fp.GasUsage) * fp.ProductivityChangeByGasChange 
                              - (solvedGas - fp.GasUsage) * fp.CokeReplacementKoefficient * fp.ProductivityChangeByCokeChange;
            double solvedMoneySave = (fp.CokeReplacementKoefficient * general.CokeCost - general.GasCost) * solvedGas;

            totalSaveVal += solvedMoneySave;
            totalGasVal += solvedGas;
            totalCokeVal += solvedCoke;
            totalProdVal += solvedProd;

            results[i] = new SolvedFurnace
            {
                FurnaceIndex = i,
                SolvedGasUsage = solvedGas,
                SolvedBurnTemperature = solvedTemp,
                SolvedCokeCoalUsage = solvedCoke,
                SolvedCastironProductivity = solvedProd,
                SolvedMoneySave = solvedMoneySave,
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
            Message = "Оптимизация завершена успешно",
            TotalMoneySave = totalSaveVal,
            TotalGasUsed = totalGasVal,
            TotalCokeUsed = totalCokeVal,
            TotalProduction = totalProdVal,
            SolvedFurnaces = results
        };
    }

    private static OutputData CreateErrorOutput(string status, string message) =>
        new() { Success = false, SolverStatus = status, Message = message, SolvedFurnaces = Array.Empty<SolvedFurnace>() };
}