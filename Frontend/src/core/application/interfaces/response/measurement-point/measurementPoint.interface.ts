export interface IMeasurementPointListResponseInterface {
  hasNext: boolean;
  hasPrevious: boolean;
  data: MeasurementPoint[];
}

export interface IMeasurementPointDetailResponseInterface extends MeasurementPoint {}

interface UserToken {
  id: number;
  userId: number;
  user: string;
  accessToken: string;
  refreshToken: string;
  type: string;
  expiredTime: string;
}

interface MeasurePoint {
  id: number;
  code: string;
  type: string;
  address: string;
  description: string;
  userId: number;
  user: string;
  email: string;
  gcsId: number;
  gcs: string;
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

interface User {
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
  userTokens: UserToken[];
  measurePoints: MeasurePoint[];
}

interface Gcs {
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
  measurementPoints: MeasurePoint[];
}

interface MeasurementPoint {
  id: number;
  code: string;
  type: string;
  address: string;
  description: string;
  userId: number;
  user: User;
  email: string;
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
  gcsId: number;
  gcs: Gcs;
}
