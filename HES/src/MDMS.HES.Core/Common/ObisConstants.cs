namespace MDMS.HES.Core.Common;

public static class ObisConstants
{
    // 8 OBIS DateTime strings synthesized from 24 short parts
    public const string ObisCurrentTime = "0_9_1"; // HH:mm:ss
    public const string ObisCurrentDate = "0_9_2"; // yyyy-MM-dd
    public const string ObisMdcTime     = "0_9_3"; // HH:mm:ss
    public const string ObisMdcDate     = "0_9_4"; // yyyy-MM-dd
    public const string ObisMdpTime     = "0_9_5"; // HH:mm:ss
    public const string ObisMdpDate     = "0_9_6"; // yyyy-MM-dd
    public const string ObisResetTime   = "0_9_7"; // HH:mm:ss
    public const string ObisResetDate   = "0_9_8"; // yyyy-MM-dd

    // Energy cumulative tariffs (kWh / kvarh)
    public const string EnergyTotalImport   = "1_8_0";
    public const string EnergyNormalTariff  = "1_8_1";
    public const string EnergyLowTariff     = "1_8_2";
    public const string EnergyPeakTariff    = "1_8_3";
    public const string ReactiveTotalImport = "3_8_0";
    public const string ReactiveNormalTariff= "3_8_1";
    public const string ReactiveLowTariff   = "3_8_2";
    public const string ReactivePeakTariff  = "3_8_3";

    // Instantaneous Measurements
    public const string VoltageL1 = "13_7_0"; // or 32_7_0 / 52_7_0 / 72_7_0
    public const string VoltageL2 = "32_7_0";
    public const string VoltageL3 = "52_7_0";
    public const string CurrentL1 = "14_7_0"; // or 31_7_0 / 51_7_0 / 71_7_0
    public const string CurrentL2 = "31_7_0";
    public const string CurrentL3 = "51_7_0";
    public const string ActivePowerTotal = "21_7_0";
    public const string ReactivePowerTotal = "23_7_0";
    public const string PowerFactorTotal = "33_7_0";
    public const string Frequency = "14_0_0";

    // Status / Alarms
    public const string StatusInternalError = "96_5_0";
    public const string AlarmPhase1Loss     = "97_97_1";
    public const string AlarmPhase2Loss     = "97_97_2";
    public const string AlarmPhase3Loss     = "97_97_3";
    public const string AlarmReversePower   = "97_97_4";
    public const string AlarmOverCurrent    = "97_97_5";
    public const string AlarmOverVoltage    = "97_97_6";
    public const string AlarmTerminalOpen   = "97_97_7";
}
