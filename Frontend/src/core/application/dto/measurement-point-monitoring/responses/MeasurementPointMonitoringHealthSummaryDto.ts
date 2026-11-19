export interface MeasurementPointMonitoringHealthSummaryDto {
  totalPoints: number;
  goodPoints: number;
  modbusLostPoints: number;
  restStalePoints: number;
  badQualityPoints: number;
  restInactivityTimeoutMinutes: number;
  evaluatedAt: string;
}

