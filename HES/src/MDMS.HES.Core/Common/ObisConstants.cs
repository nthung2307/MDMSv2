namespace MDMS.HES.Core.Common;

public static class ObisConstants
{
    // 8 OBIS DateTime strings synthesized from 24 short parts (matches ObisTelemetryCatalog)
    public const string ObisCurrentTime = "0_9_1"; // HH:mm:ss (DT.Current)
    public const string ObisCurrentDate = "0_9_2"; // yyyy-MM-dd (DT.Current)
    public const string ObisResetDate   = "0_9_3"; // yyyy-MM-dd (DT.Reset / ClosingDate)
    public const string ObisResetTime   = "0_9_4"; // HH:mm:ss (DT.Reset / ClosingTime)
    public const string ObisMdcDate     = "0_9_5"; // yyyy-MM-dd (DT.MDC / PmaxCurrentDate)
    public const string ObisMdcTime     = "0_9_6"; // HH:mm:ss (DT.MDC / PmaxCurrentTime)
    public const string ObisMdpDate     = "0_9_7"; // yyyy-MM-dd (DT.MDP / PmaxPrevDate)
    public const string ObisMdpTime     = "0_9_8"; // HH:mm:ss (DT.MDP / PmaxPrevTime)

    // Instantaneous Electrical Measurements (Group A in ObisTelemetryCatalog)
    public const string CurrentPhaseA       = "31_7_0"; // A
    public const string CurrentPhaseB       = "51_7_0"; // A
    public const string CurrentPhaseC       = "71_7_0"; // A
    public const string VoltagePhaseA       = "32_7_0"; // kV / V
    public const string VoltagePhaseB       = "52_7_0"; // kV / V
    public const string VoltagePhaseC       = "72_7_0"; // kV / V
    public const string ActivePowerPhaseA   = "21_7_0"; // kW
    public const string ActivePowerPhaseB   = "41_7_0"; // kW
    public const string ActivePowerPhaseC   = "61_7_0"; // kW
    public const string ActivePowerTotal    = "1_7_0";  // kW (CongSuatTacDungTong3Pha)
    public const string ReactivePowerPhaseA = "23_7_0"; // kVAr
    public const string ReactivePowerPhaseB = "43_7_0"; // kVAr
    public const string ReactivePowerPhaseC = "63_7_0"; // kVAr
    public const string ReactivePowerTotal  = "3_7_0";  // kVAr (CongSuatPhanKhangTong3Pha)
    public const string PowerFactorPhaseA   = "33_7_0";
    public const string PowerFactorPhaseB   = "53_7_0";
    public const string PowerFactorPhaseC   = "73_7_0";
    public const string PowerFactorAverage  = "13_7_0"; // HeSoCongSuatPFTrungBinh3Pha
    public const string Frequency           = "14_7_0"; // Hz (TanSoHeThong)

    // Cumulative Energy Tariffs (Group B)
    public const string EnergyTotalImport    = "1_8_0";
    public const string EnergyNormalTariff   = "1_8_1";
    public const string EnergyPeakTariff     = "1_8_2";
    public const string EnergyLowTariff      = "1_8_3";
    public const string ReactiveTotalImport  = "3_8_0";

    // Billing Reset Energy (Group C)
    public const string BillingTotalImport   = "1_8_0_1";
    public const string BillingNormalTariff  = "1_8_1_1";
    public const string BillingPeakTariff    = "1_8_2_1";
    public const string BillingLowTariff     = "1_8_3_1";
    public const string BillingReactiveTotal = "3_8_0_1";

    // Maximum Demand Pmax (Group D)
    public const string MaxDemandCurrent     = "1_6_0";
    public const string MaxDemandPrevious    = "1_6_0_1";

    // Status / Alarms (Group F)
    public const string StatusInternalError  = "96_5_0";
    public const string AlarmPhase1Loss      = "97_97_1";
    public const string AlarmPhase2Loss      = "97_97_2";
    public const string AlarmPhase3Loss      = "97_97_3";
    public const string AlarmReversePower    = "97_97_4";
    public const string AlarmOverCurrent     = "97_97_5";
    public const string AlarmOverVoltage     = "97_97_6";
    public const string AlarmTerminalOpen    = "97_97_7";
}
