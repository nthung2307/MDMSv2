import axios, { CancelTokenSource } from 'axios';
import { Dispatch, SetStateAction, useCallback, useEffect, useRef } from 'react';

import { Endpoint } from '@/core/application/common/endPoint.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { useApiRequestHook } from '@/infrastructure/common/libs/hooks/useApiRequest.hook.tsx';
import { ConfigManagementService } from '@/infrastructure/repository/config/services/configManagement.service';

export interface MeterProfileTagMappingItem {
  obisCode: string;
  parameterCode: string;
  unit?: string | null;
  customName?: string | null;
  legacyAliases?: string | null;
  dataCategory: string;
  displayOrder: number;
}

export interface MeterProfileItem {
  id: number;
  code: string;
  name: string;
  description?: string | null;
  voltageLevel: string;
  defaultVoltageUnit: string;
  defaultPowerUnit: string;
  isDefault: boolean;
  isSystem: boolean;
  tagMappings: MeterProfileTagMappingItem[];
}

export interface PointApiKeyInfo {
  measurementPointId: number;
  pointCode: string;
  meterSerial?: string | null;
  hasDedicatedKey: boolean;
  keyPrefix?: string | null;
  apiKey?: string | null;
  authTier: string;
}

export interface UnmatchedSerialItem {
  meterSerial: string;
  remoteIp: string;
  firstSeenAt: string;
  lastSeenAt: string;
  hitCount: number;
}

const useIoTConfigHook = () => {
  const [requestCommon] = useApiRequestHook();
  const activeSourcesRef = useRef<Set<CancelTokenSource>>(new Set());

  useEffect(() => {
    return () => {
      activeSourcesRef.current.forEach((source) => {
        try {
          source.cancel();
        } catch {
          // ignore
        }
      });
      activeSourcesRef.current.clear();
    };
  }, []);

  const createCancelToken = useCallback(() => {
    const source = axios.CancelToken.source();
    activeSourcesRef.current.add(source);
    setTimeout(() => {
      activeSourcesRef.current.delete(source);
    }, 30000);
    return source.token;
  }, []);

  async function getIoTConfig(
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      createCancelToken(),
      new ConfigManagementService().getIoTConfigAsync,
      Endpoint.Config.GetIoTConfig,
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  async function updateIoTConfig(
    params: any,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      createCancelToken(),
      new ConfigManagementService().updateIoTConfigAsync,
      Endpoint.Config.UpdateIoTConfig,
      params,
      onSuccess,
      onError,
      setLoading
    );
  }

  async function generateIoTKey(
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      createCancelToken(),
      new ConfigManagementService().generateIoTKeyAsync,
      Endpoint.Config.GenerateIoTKey,
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  async function getMeterProfiles(
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      createCancelToken(),
      new ConfigManagementService().getMeterProfilesAsync,
      Endpoint.Config.GetMeterProfiles,
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  async function getUnmatchedSerials(
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      createCancelToken(),
      new ConfigManagementService().getUnmatchedSerialsAsync,
      Endpoint.Config.GetUnmatchedSerials,
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  async function getPointApiKey(
    pointId: number,
    reveal: boolean,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      createCancelToken(),
      new ConfigManagementService().getPointApiKeyAsync,
      Endpoint.Config.GetPointApiKey.replace(':id', String(pointId)),
      reveal ? { reveal: true } : {},
      onSuccess,
      onError,
      setLoading
    );
  }

  async function generatePointApiKey(
    pointId: number,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      createCancelToken(),
      new ConfigManagementService().generatePointApiKeyAsync,
      Endpoint.Config.GeneratePointApiKey.replace(':id', String(pointId)),
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  async function revokePointApiKey(
    pointId: number,
    onSuccess: (res: DataResponse) => void,
    onError: () => void,
    setLoading?: Dispatch<SetStateAction<boolean>>
  ) {
    await requestCommon(
      createCancelToken(),
      new ConfigManagementService().revokePointApiKeyAsync,
      Endpoint.Config.RevokePointApiKey.replace(':id', String(pointId)),
      {},
      onSuccess,
      onError,
      setLoading
    );
  }

  return {
    getIoTConfig,
    updateIoTConfig,
    generateIoTKey,
    getMeterProfiles,
    getUnmatchedSerials,
    getPointApiKey,
    generatePointApiKey,
    revokePointApiKey,
  };
};

export default useIoTConfigHook;
