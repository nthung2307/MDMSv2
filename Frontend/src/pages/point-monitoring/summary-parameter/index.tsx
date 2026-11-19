import { Card } from 'antd';
import { useEffect, useState, useMemo, useRef } from 'react';
import { FiShare } from 'react-icons/fi';
import { IoBackspaceOutline, IoSearch } from 'react-icons/io5';

import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListMeasurementPointMonitoringRequest } from '@/core/application/dto/measurement-point-monitoring/requests/GetListMeasurementPointMonitoringRequest';
import { MeasurementPointMonitoringHealthSummaryDto } from '@/core/application/dto/measurement-point-monitoring/responses/MeasurementPointMonitoringHealthSummaryDto';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import SocketCommon from '@/infrastructure/common/components/websocket/socket-common.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListMeasurementPointMonitoringHook from '@/infrastructure/repository/measurement-point-monitoring/hooks/useGetListMeasurementPointMonitoring.hook';
import useGetMonitoringHealthSummaryHook from '@/infrastructure/repository/measurement-point-monitoring/hooks/useGetMonitoringHealthSummary.hook';
import { deleteParams } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';
import MonitoringHealthWidget from '@/pages/point-monitoring/summary-parameter/components/monitoring-health-widget';
import TableSummary from '@/pages/point-monitoring/summary-parameter/components/table-point';

const PointMonitoring = () => {
  const [dataForm, setDataForm] = useStateDataForm({});
  const [getListMeasurementPointMonitoring] = useGetListMeasurementPointMonitoringHook();
  const [getMonitoringHealthSummary] = useGetMonitoringHealthSummaryHook();
  const [loading, setLoading] = useState<boolean>(false);
  const [healthLoading, setHealthLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(10);
  const [data, setData] = useState<any[]>();
  const [dataSocket, setDataSocket] = useState(null);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [activeFilter, setActiveFilter] = useState<string>('all');
  const [restTimeoutMinutes, setRestTimeoutMinutes] = useState<number>(5);
  const [healthSummary, setHealthSummary] =
    useState<MeasurementPointMonitoringHealthSummaryDto | null>(null);

  const debounceSearchCode = useDebounce(dataForm.measurementPointCode, 300);

  const [getListMeasurementParamsRequest, setGetListMeasurementParamsRequest] =
    useState<GetListMeasurementPointMonitoringRequest>({
      filterMode: 'all',
      restTimeoutMinutes: 5,
    });

  const currentParamsRef = useRef<GetListMeasurementPointMonitoringRequest | undefined>(
    getListMeasurementParamsRequest
  );
  currentParamsRef.current = getListMeasurementParamsRequest;

  const activeFilterRef = useRef(activeFilter);
  activeFilterRef.current = activeFilter;

  const restTimeoutMinutesRef = useRef(restTimeoutMinutes);
  restTimeoutMinutesRef.current = restTimeoutMinutes;

  const dataFormRef = useRef(dataForm);
  dataFormRef.current = dataForm;

  const sizeRef = useRef(size);
  sizeRef.current = size;

  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
    totalCount?: number;
  }>({ hasNext: false, hasPrevious: false });

  const firstCustomerID = data?.[0]?.measurementPointsId;
  const lastCustomerID = data?.[data.length - 1]?.measurementPointsId;
  const socketRefreshTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  const fetchHealthSummaryAsync = async (isBackground = false) => {
    await getMonitoringHealthSummary(
      { timeoutMinutes: restTimeoutMinutesRef.current },
      (res: DataResponse) => {
        if (res && res.data) {
          setHealthSummary(res.data);
        }
      },
      () => {},
      isBackground ? undefined : setHealthLoading
    );
  };

  const handleClearSearch = () => {
    setDataForm({
      measurementPointCode: '',
      connectionStatus: null,
    });
    setActiveFilter('all');
    setGetListMeasurementParamsRequest({
      filterMode: 'all',
      restTimeoutMinutes,
    });
  };

  const handleFilterChange = (mode: string) => {
    setActiveFilter(mode);
    setGetListMeasurementParamsRequest((prev) => {
      const updated = {
        ...prev,
        filterMode: mode,
        restTimeoutMinutes,
      };
      return deleteParams(updated, ['before', 'after', 'pageNumber']);
    });
  };

  const handleTimeoutChange = (timeout: number) => {
    setRestTimeoutMinutes(timeout);
    setGetListMeasurementParamsRequest((prev) => {
      const updated = {
        ...prev,
        restTimeoutMinutes: timeout,
      };
      return deleteParams(updated, ['before', 'after', 'pageNumber']);
    });
  };

  const getListMeasurementPointMonitoringAsync = async ({
    pageSize = size,
    before,
    after,
    pageNumber,
    searchByFields = [],
    sortByFields = [],
    measurementPointCode,
    filterMode = activeFilter,
  }: GetListMeasurementPointMonitoringRequest) => {
    const params: GetListMeasurementPointMonitoringRequest = {
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
      ...(pageNumber && { pageNumber }),
      ...(measurementPointCode && { measurementPointCode }),
      filterMode: filterMode || activeFilter,
      restTimeoutMinutes: restTimeoutMinutes,
      searchByFields: searchByFields.length > 0 ? searchByFields : [],
      sortByFields: sortByFields.length > 0 ? sortByFields : [],
    };

    await getListMeasurementPointMonitoring(
      params,
      (res: DataResponse) => {
        const { data } = res;
        let result = data?.data;

        // Backward compatibility: If dropdown connectionStatus is chosen and not 'all'
        if (dataForm.connectionStatus === 'normal') {
          result = result.filter((item: any) => item.matKetNoi === false);
        } else if (dataForm.connectionStatus === 'lost') {
          result = result.filter((item: any) => item.matKetNoi === true);
        }

        setData(result);
        setPaginationData({
          hasNext: data.hasNext,
          hasPrevious: data.hasPrevious,
          totalCount: data.totalCount,
        });
      },
      () => {},
      setLoading
    );
  };

  const refreshCurrentPageAsync = async () => {
    const currentParams = currentParamsRef.current || {};
    const currentDataForm = dataFormRef.current || {};
    const currentSize = sizeRef.current || 10;
    const currentFilter = activeFilterRef.current || 'all';
    const currentTimeout = restTimeoutMinutesRef.current || 5;

    const params: GetListMeasurementPointMonitoringRequest = {
      pageSize: currentParams.pageSize || currentSize,
      ...(currentParams.before && { before: currentParams.before }),
      ...(currentParams.after && { after: currentParams.after }),
      ...(currentParams.pageNumber && { pageNumber: currentParams.pageNumber }),
      ...(currentParams.measurementPointCode && {
        measurementPointCode: currentParams.measurementPointCode,
      }),
      filterMode: currentFilter,
      restTimeoutMinutes: currentTimeout,
      searchByFields: currentParams.searchByFields?.length ? currentParams.searchByFields : [],
      sortByFields: currentParams.sortByFields?.length ? currentParams.sortByFields : [],
    };

    await getListMeasurementPointMonitoring(
      params,
      (res: DataResponse) => {
        const { data: resData } = res;
        let result = resData?.data || [];

        if (currentDataForm.connectionStatus === 'normal') {
          result = result.filter((item: any) => item.matKetNoi === false);
        } else if (currentDataForm.connectionStatus === 'lost') {
          result = result.filter((item: any) => item.matKetNoi === true);
        }

        // Merge without breaking reference if data hasn't changed to prevent flickering
        setData((prevData) => {
          if (!prevData || prevData.length === 0) return result;
          if (prevData.length !== result.length) return result;
          const isSame = prevData.every(
            (p, idx) =>
              p.measurementPointCode === result[idx]?.measurementPointCode &&
              p.matKetNoi === result[idx]?.matKetNoi &&
              p.restStatus === result[idx]?.restStatus &&
              p.dienApPhaA === result[idx]?.dienApPhaA &&
              p.dongDienPhaA === result[idx]?.dongDienPhaA &&
              p.congSuatTacDungTong3Pha === result[idx]?.congSuatTacDungTong3Pha &&
              p.congSuatPhanKhangTong3Pha === result[idx]?.congSuatPhanKhangTong3Pha
          );
          return isSame ? prevData : result;
        });
        setPaginationData({
          hasNext: resData?.hasNext || false,
          hasPrevious: resData?.hasPrevious || false,
          totalCount: resData?.totalCount,
        });
      },
      () => {},
      () => {} // Silent refresh without popping table loading overlay
    );
  };

  const connectionStatusArray = useMemo(() => {
    return Object.values(Constants.MonitoringPoints.Connection.Status).map((status) => ({
      label: status.label,
      value: status.value,
    }));
  }, []);

  const dataExport = data?.map((item, index) => {
    let connectionStatus = '-';
    if (item.matKetNoi !== null && item.matKetNoi !== undefined) {
      connectionStatus = item.matKetNoi ? 'Mất kết nối' : 'Bình thường';
    }

    let restStatusText = 'Chưa nhận';
    if (item.restStatus === 'ONLINE') restStatusText = 'Online';
    else if (item.restStatus === 'STALE_STOPPED') restStatusText = 'Ngừng nhận';

    return {
      index: index + 1,
      measurementPointCode: item.measurementPointCode,
      meterSerial: item.meterSerial || '-',
      customerName: item.customerName,
      matKetNoi: connectionStatus,
      restStatus: restStatusText,
      UParams: item.dienApPhaA ? `${item.dienApPhaA} ${item.dienApPhaAUnit || ''}`.trim() : '-',
      IParams: item.dongDienPhaA ? `${item.dongDienPhaA} ${item.dongDienPhaAUnit || ''}`.trim() : '-',
      PParams: item.congSuatTacDungTong3Pha
        ? `${item.congSuatTacDungTong3Pha} ${item.congSuatTacDungTong3PhaUnit || ''}`.trim()
        : '-',
      QParams: item.congSuatPhanKhangTong3Pha
        ? `${item.congSuatPhanKhangTong3Pha} ${item.congSuatPhanKhangTong3PhaUnit || ''}`.trim()
        : '-',
      PFParams: item.heSoCongSuatPFTrungBinh3Pha ? `${item.heSoCongSuatPFTrungBinh3Pha}` : '-',
      FParams: item.tanSoHeThong ? `${item.tanSoHeThong} ${item.tanSoHeThongUnit || ''}`.trim() : '-',
    };
  });

  useEffect(() => {
    setGetListMeasurementParamsRequest((prev) => {
      const updated: any = {
        ...prev,
        filterMode: activeFilter,
        restTimeoutMinutes,
        ...(debounceSearchCode && { measurementPointCode: debounceSearchCode }),
      };
      if (!debounceSearchCode) delete updated.measurementPointCode;
      return deleteParams(updated, ['before', 'after', 'pageNumber']);
    });
  }, [debounceSearchCode, dataForm.connectionStatus, activeFilter, restTimeoutMinutes]);

  useEffect(() => {
    getListMeasurementPointMonitoringAsync({
      ...getListMeasurementParamsRequest,
    });
  }, [size, getListMeasurementParamsRequest]);

  useEffect(() => {
    fetchHealthSummaryAsync();
  }, [restTimeoutMinutes]);

  useEffect(() => {
    if (!dataSocket) return;

    // 1. Cập nhật tức thời (in-memory) cho các điểm đo trùng khớp đang hiển thị trên bảng
    if (dataSocket.data && Array.isArray(dataSocket.data)) {
      setData((prevData) => {
        if (!prevData || prevData.length === 0) return prevData;
        const updatedMap = new Map(
          dataSocket.data.map((item: any) => [String(item.measurementPointsId || item.id), item])
        );
        let hasChanges = false;
        const nextData = prevData.map((item: any) => {
          const key = String(item.measurementPointsId || item.id);
          const update = updatedMap.get(key);
          if (update) {
            hasChanges = true;
            return { ...item, ...update };
          }
          return item;
        });
        return hasChanges ? nextData : prevData;
      });
    }

    // 2. Debounce HTTP refresh: Tránh gọi dồn dập nhiều API khi socket đẩy dữ liệu liên tục gây giật lag/flickering
    if (socketRefreshTimeoutRef.current) {
      clearTimeout(socketRefreshTimeoutRef.current);
    }
    socketRefreshTimeoutRef.current = setTimeout(() => {
      refreshCurrentPageAsync();
      fetchHealthSummaryAsync(true);
    }, 2500);
  }, [dataSocket]);

  useEffect(() => {
    // Tự động kiểm tra và refresh dữ liệu định kỳ (mỗi 15 giây) một cách êm ái (silent) không flicker
    const interval = setInterval(() => {
      refreshCurrentPageAsync();
      fetchHealthSummaryAsync(true);
    }, 15000);
    return () => {
      clearInterval(interval);
      if (socketRefreshTimeoutRef.current) {
        clearTimeout(socketRefreshTimeoutRef.current);
      }
    };
  }, []);

  return (
    <>
      <PageTitle title={'Giám sát điểm đo'} />
      <SocketCommon
        setDataSocket={setDataSocket}
        endPoint={Constants.socketIoEndpoint.summary}
        status={Constants.socketIoStatus.ReceiveUpdate}
      >
        <div className='flex justify-between items-center'>
          <BreadcrumbUI items={[{ name: 'Giám sát điểm đo' }, { name: 'Thông số tổng hợp' }]} />
          <div className='flex gap-1'>
            <ButtonCommonCustomizable
              title={''}
              tooltipText={'Xuất file'}
              onClick={() => {
                setIsShowExport(true);
              }}
              icon={<FiShare size={20} />}
              disabled={!(data && data.length > 0)}
              classButton='text-button-variant'
            />
          </div>
        </div>

        {/* Phase 3: Interactive Health Summary Widget */}
        <div className='mt-4'>
          <MonitoringHealthWidget
            summary={healthSummary}
            loading={healthLoading}
            activeFilter={activeFilter}
            onFilterChange={handleFilterChange}
            timeoutMinutes={restTimeoutMinutes}
            onTimeoutChange={handleTimeoutChange}
            onRefresh={() => fetchHealthSummaryAsync(false)}
          />
        </div>

        <Card className='mt-2 shadow-custom '>
          <div className='mb-5 flex justify-between items-center'>
            <div className='flex w-2/3 items-center gap-2'>
              <div className='basis-3/5 flex items-center gap-1'>
                <div className='basis-1/2'>
                  <InputTextCommon
                    label={null}
                    attribute={'measurementPointCode'}
                    dataAttribute={dataForm?.measurementPointCode}
                    placeholder={'Nhập mã điểm đo'}
                    setData={setDataForm}
                    isRequired={false}
                    allowClear={true}
                    prefix={<IoSearch />}
                  />
                </div>
                <div className='basis-1/2'>
                  <InputSelectCommon
                    label={null}
                    attribute={'connectionStatus'}
                    dataAttribute={dataForm?.connectionStatus}
                    placeholder={'Trạng thái kết nối'}
                    setData={setDataForm}
                    isRequired={false}
                    listDataItem={connectionStatusArray}
                    allowClear={true}
                  />
                </div>
              </div>
              <div className='flex gap-1'>
                {(dataForm.measurementPointCode ||
                  dataForm.connectionStatus ||
                  activeFilter !== 'all') && (
                  <ButtonCommonCustomizable
                    title={_t('')}
                    tooltipText='Xoá tất cả'
                    icon={<IoBackspaceOutline size={20} />}
                    onClick={handleClearSearch}
                    variant={'text'}
                    classButton='text-button-variant'
                  />
                )}
              </div>
            </div>
          </div>
          <TableSummary dataSource={data ?? []} loading={loading} />
          <PaginationCommon
            pageSize={size}
            total={paginationData?.totalCount}
            currentCount={data?.length || 0}
            setSize={(newSize) => {
              setSize(newSize);
              setGetListMeasurementParamsRequest((prev) => {
                const updatedParams = { ...prev, pageSize: newSize };
                return deleteParams(updatedParams, ['before', 'after', 'pageNumber']);
              });
            }}
            hasNext={paginationData?.hasNext}
            hasPrevious={paginationData?.hasPrevious}
            onClickPrevious={() => {
              setGetListMeasurementParamsRequest((prev) => {
                const updatedParams = { ...prev, before: firstCustomerID };
                return deleteParams(updatedParams, ['after', 'pageNumber']);
              });
            }}
            onClickNext={() => {
              setGetListMeasurementParamsRequest((prev) => {
                const updatedParams = { ...prev, after: lastCustomerID };
                return deleteParams(updatedParams, ['before', 'pageNumber']);
              });
            }}
            onChangePage={(page) => {
              setGetListMeasurementParamsRequest((prev) => {
                const updatedParams = { ...prev, pageNumber: page };
                return deleteParams(updatedParams, ['before', 'after']);
              });
            }}
          />
        </Card>
        {/* Dialog export file */}
        <DialogExportForm
          title={'Quản lý thông số tổng hợp'}
          open={isShowExport}
          handleCancel={() => {
            setIsShowExport(false);
          }}
          dataSource={dataExport}
          listHeader={Constants.listHeaderExport.summaryParams}
          fileNameCsv={'MDMS_ThongSoTongHop'}
          typeExportAll={'measurement-point-monitoring'}
        />
      </SocketCommon>
    </>
  );
};

export default PointMonitoring;
