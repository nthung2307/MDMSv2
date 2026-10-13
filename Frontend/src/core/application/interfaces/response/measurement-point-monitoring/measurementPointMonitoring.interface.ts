// Main Interface for MeasurementPointMonitoringResponse
export interface IMeasurementPointMonitoringResponseInterface {
  hasNext: boolean;
  hasPrevious: boolean;
  data: IMeasurementPointMonitoringData[];
}

// Main Interface for MeasurementPointMonitoringOffsetResponse
export interface IMeasurementPointMonitoringOffsetResponseInterface {
  currentPage: number;
  pageSize: number;
  total: number;
  data: IMeasurementPointMonitoringOffsetData[];
}

// Data Interface for MeasurementPointMonitoringResponse
export interface IMeasurementPointMonitoringData {
  id: number;
  measurementPointsId: number;
  measurementPoints: IMeasurementPoint;
  phaseCurrentValuesIa: number;
  phaseCurrentValuesIb: number;
  phaseCurrentValuesIc: number;
  phaseVoltageValuesVa: number;
  phaseVoltageValuesVb: number;
  phaseVoltageValuesVc: number;
  activePowerPerPhasePa: number;
  activePowerPerPhasePb: number;
  activePowerPerPhasePc: number;
  reactivePowerPerPhaseQa: number;
  reactivePowerPerPhaseQb: number;
  reactivePowerPerPhaseQc: number;
  totalActivePowerPt: number;
  totalReactivePowerQt: number;
  averageCosphi: number;
  frequencyValue: number;
  currentDeviceTime: string;
  connectStatus: string;
  warningStatus: string;
}

// Data Interface for MeasurementPointMonitoringOffsetResponse
export interface IMeasurementPointMonitoringOffsetData {
  id: number;
  measurementPointsId: number;
  measurementPoints: IMeasurementPoint;
  phaseCurrentValuesIa: number;
  phaseCurrentValuesIb: number;
  phaseCurrentValuesIc: number;
  phaseVoltageValuesVa: number;
  phaseVoltageValuesVb: number;
  phaseVoltageValuesVc: number;
  activePowerPerPhasePa: number;
  activePowerPerPhasePb: number;
  activePowerPerPhasePc: number;
  reactivePowerPerPhaseQa: number;
  reactivePowerPerPhaseQb: number;
  reactivePowerPerPhaseQc: number;
  totalActivePowerPt: number;
  totalReactivePowerQt: number;
  averageCosphi: number;
  frequencyValue: number;
  currentDeviceTime: string;
  connectStatus: string;
  warningStatus: string;
}

// Measurement Point Interface
export interface IMeasurementPoint {
  id: number;
  code: string;
  type: string;
  address: string;
  description: string;
  userId: number;
  user: IUser;
  email: string;
  gcsId: number;
  gcs: IGcs;
  createdTime: string;
  createdBy: number;
  createdName: string;
  modifiedTime: string;
  modifiedBy: number;
  modifiedName: string;
  deleted: boolean;
  deletedTime: string;
  deletedBy: number;
  deletedName: string;
}

// User Interface
export interface IUser {
  id: number;
  userName: string;
  password: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  address: string;
  customerCode: string;
  status: string;
  countLocked: number;
  timeLocked: string;
  isLocked: boolean;
  roleName: string;
  createdTime: string;
  createdBy: number;
  createdName: string;
  modifiedTime: string;
  modifiedBy: number;
  modifiedName: string;
  deleted: boolean;
  deletedTime: string;
  deletedBy: number;
  deletedName: string;
  userTokens: IUserToken[];
  measurePoints: string[];
}

// User Token Interface
export interface IUserToken {
  id: number;
  userId: number;
  user: string;
  accessToken: string;
  refreshToken: string;
  type: string;
  expiredTime: string;
}

// GCS Interface
export interface IGcs {
  id: number;
  code: string;
  name: string;
  periodNumber: number;
  periodDate: number;
  createdTime: string;
  createdBy: number;
  createdName: string;
  modifiedTime: string;
  modifiedBy: number;
  modifiedName: string;
  deleted: boolean;
  deletedTime: string;
  deletedBy: number;
  deletedName: string;
  measurementPoints: string[];
}
