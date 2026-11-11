using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MDMS.UnitTests;

/// <summary>
/// Autonomous Test Runner: Thực thi trực tiếp toàn bộ Test Cases của MDMSv2
/// Hỗ trợ cả 2 chế độ: Chạy trực tiếp (dotnet run) và chạy qua Test Host (dotnet test).
/// </summary>
public static class Program
{
    public static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=======================================================================");
        Console.WriteLine("   MDMSv2 - AUTOMATED TEST SUITE (PHASE 4: TESTING & VERIFICATION)");
        Console.WriteLine("=======================================================================");

        var testClasses = new[]
        {
            typeof(MeteringCalculationTests),
            typeof(EmailSecurityAndConfigurationTests),
            typeof(StatementPdfGeneratorTests),
            typeof(Phase1TelemetryAndPartitionTests)
        };

        int totalTests = 0;
        int passedTests = 0;
        int failedTests = 0;
        var stopwatch = Stopwatch.StartNew();

        foreach (var testClass in testClasses)
        {
            Console.WriteLine($"\n[TEST FIXTURE]: {testClass.Name}");
            object? instance = null;
            try
            {
                instance = Activator.CreateInstance(testClass);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   [FAIL] Khởi tạo fixture thất bại: {ex.Message}");
                Console.ResetColor();
                failedTests++;
                continue;
            }

            var methods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var testMethodAttr = method.GetCustomAttribute<TestMethodAttribute>();
                if (testMethodAttr == null) continue;

                var dataRows = method.GetCustomAttributes<DataRowAttribute>();
                var dataRowList = new List<DataRowAttribute>(dataRows);

                if (dataRowList.Count > 0)
                {
                    // Parameterized tests
                    int rowIdx = 1;
                    foreach (var row in dataRowList)
                    {
                        totalTests++;
                        try
                        {
                            method.Invoke(instance, row.Data);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"   [PASS] {method.Name} (Kịch bản #{rowIdx})");
                            Console.ResetColor();
                            passedTests++;
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            var inner = ex.InnerException ?? ex;
                            Console.WriteLine($"   [FAIL] {method.Name} (Kịch bản #{rowIdx}): {inner.Message}");
                            Console.ResetColor();
                            failedTests++;
                        }
                        rowIdx++;
                    }
                }
                else
                {
                    // Standard unit test
                    totalTests++;
                    try
                    {
                        method.Invoke(instance, null);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"   [PASS] {method.Name}");
                        Console.ResetColor();
                        passedTests++;
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        var inner = ex.InnerException ?? ex;
                        Console.WriteLine($"   [FAIL] {method.Name}: {inner.Message}");
                        Console.ResetColor();
                        failedTests++;
                    }
                }
            }
        }

        stopwatch.Stop();
        Console.WriteLine("\n=======================================================================");
        Console.WriteLine($" KẾT QUẢ KIỂM THỬ: Tổng {totalTests} | Thành công: {passedTests} | Thất bại: {failedTests}");
        Console.WriteLine($" Thời gian thực thi: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine("=======================================================================");

        if (failedTests > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   -> TRẠNG THÁI: CÓ KIỂM THỬ THẤT BẠI!");
            Console.ResetColor();
            return 1;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("   -> TRẠNG THÁI: TẤT CẢ KIỂM THỬ THÀNH CÔNG 100%!");
        Console.ResetColor();
        return 0;
    }
}

