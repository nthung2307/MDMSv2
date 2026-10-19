import { Card } from 'antd';
import { useEffect, useState, useMemo, useRef } from 'react';
import { FiShare } from 'react-icons/fi';
import { IoBackspaceOutline, IoSearch } from 'react-icons/io5';

import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListMeasurementPointMonitoringRequest } from '@/core/application/dto/measurement-point-monitoring/requests/GetListMeasurementPointMonitoringRequest';
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
import { deleteParams } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';
import TableSummary from '@/pages/point-monitoring/summary-parameter/components/table-point';

const PointMonitoring = () => {
  const [dataForm, setDataForm] = useStateDataForm({});
  const [getListMeasurementPointMonitoring] = useGetListMeasurementPointMonitoringHook();
  const [loading, setLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(10);
  const [data, setData] = useState<any[]>();
  const [dataSocket, setDataSocket] = useState(null);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const debounceSearchCode = useDebounce(dataForm.measurementPointCode, 300);

  const [getListMeasurementParamsRequest, setGetListMeasurementParamsRequest] =
    useState<GetListMeasurementPointMonitoringRequest>();

  const currentParamsRef = useRef<GetListMeasurementPointMonitoringRequest | undefined>(
    getListMeasurementParamsRequest
  );
  currentParamsRef.current = getListMeasurementParamsRequest;

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

  const handleClearSearch = () => {
    setDataForm({
      measurementPointCode: '',
      connectionStatus: null,
    });
    setGetListMeasurementParamsRequest({});
  };

  const getListMeasurementPointMonitoringAsync = async ({
    pageSize = size,
    before,
    after,
    searchByFields = [],
    sortByFields = [],
    measurementPointCode,
  }: GetListMeasurementPointMonitoringRequest) => {
    const params: GetListMeasurementPointMonitoringRequest = {
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
      ...(measurementPointCode && { measurementPointCode }),
      searchByFields: searchByFields.length > 0 ? searchByFields : [],
      sortByFields: sortByFields.length > 0 ? sortByFields : [],
    };

    await getListMeasurementPointMonitoring(
      params,
      (res: DataResponse) => {
        const { data } = res;
        let result = data?.data;

        // Apply filtering based on connectionStatus
        if (dataForm.connectionStatus === 'normal') {
          result = result.filter((item: any) => item.matKetNoi === false);
        } else if (dataForm.connectionStatus === 'lost') {
          result = result.filter((item: any) => item.matKetNoi === true || item.matKetNoi === null);
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
    const params: GetListMeasurementPointMonitoringRequest = {
      pageSize: currentParams.pageSize || currentSize,
      ...(currentParams.before && { before: currentParams.before }),
      ...(currentParams.after && { after: currentParams.after }),
      ...(currentParams.measurementPointCode && { measurementPointCode: currentParams.measurementPointCode }),
      searchByFields: currentParams.searchByFields?.length ? currentParams.searchByFields : [],
      sortByFields: currentParams.sortByFields?.length ? currentParams.sortByFields : [],
    };

    await getListMeasurementPointMonitoring(
      params,
      (res: DataResponse) => {
        const { data: resData } = res;
        let result = resData?.data || [];

        // Apply filtering based on connectionStatus
        if (currentDataForm.connectionStatus === 'normal') {
          result = result.filter((item: any) => item.matKetNoi === false);
        } else if (currentDataForm.connectionStatus === 'lost') {
          result = result.filter((item: any) => item.matKetNoi === true || item.matKetNoi === null);
        }

        setData(result);
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

  const dataExport = data?.map((item) => {
    return {
      measurementPointCode: item.measurementPointCode,
      measurementPointDescription: item.measurementPointDescription,
      customerCode: item.customerCode,
      customerName: item.customerName,
      matKetNoi: item.matKetNoi ? 'Mất kết nối' : 'Bình thường',
      UParams: item.dienApPhaA ? `${item.dienApPhaA} ${item.dienApPhaAUnit}` : '????',
      IParams: item.dongDienPhaA ? `${item.dongDienPhaA} ${item.dongDienPhaAUnit}` : '????',
      PParams: item.congSuatTacDungTong3Pha
        ? `${item.congSuatTacDungTong3Pha} ${item.congSuatTacDungTong3PhaUnit}`
        : '????',
      QParams: item.congSuatPhanKhangTong3Pha
        ? `${item.congSuatPhanKhangTong3Pha} ${item.congSuatPhanKhangTong3PhaUnit}`
        : '????',
      PFParams: item.heSoCongSuatPFTrungBinh3Pha ? `${item.heSoCongSuatPFTrungBinh3Pha}` : '????',
      FParams: item.tanSoHeThong ? `${item.tanSoHeThong} ${item.tanSoHeThongUnit}` : '????',
    };
  });

  useEffect(() => {
    setGetListMeasurementParamsRequest((prev) => {
      const updated: any = {
        ...prev,
        ...(debounceSearchCode && { measurementPointCode: debounceSearchCode }),
      };
      if (!debounceSearchCode) delete updated.measurementPointCode;
      return deleteParams(updated, ['before', 'after']);
    });
  }, [debounceSearchCode, dataForm.connectionStatus]);

  useEffect(() => {
    getListMeasurementPointMonitoringAsync({
      ...getListMeasurementParamsRequest,
    });
  }, [size, getListMeasurementParamsRequest]);

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

    // 2. Tự động refresh dữ liệu từ REST API cho đúng vị trí trang và bộ lọc hiện tại (không nhảy về trang đầu)
    refreshCurrentPageAsync();
  }, [dataSocket]);

  useEffect(() => {
    // Tự động kiểm tra và refresh dữ liệu định kỳ (mỗi 15 giây) để đảm bảo dữ liệu luôn mới nhất
    const interval = setInterval(() => {
      refreshCurrentPageAsync();
    }, 15000);
    return () => clearInterval(interval);
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
        <Card className='mt-5 shadow-custom '>
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
                {(dataForm.measurementPointCode || dataForm.connectionStatus) && (
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
                return deleteParams(updatedParams, ['before', 'after']);
              });
            }}
            hasNext={paginationData?.hasNext}
            hasPrevious={paginationData?.hasPrevious}
            onClickPrevious={() => {
              setGetListMeasurementParamsRequest((prev) => {
                const updatedParams = { ...prev, before: firstCustomerID };
                return deleteParams(updatedParams, ['after']);
              });
            }}
            onClickNext={() => {
              setGetListMeasurementParamsRequest((prev) => {
                const updatedParams = { ...prev, after: lastCustomerID };
                return deleteParams(updatedParams, ['before']);
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
          fileNameCsv={'thongsotonghop'}
          typeExportAll={'measurement-point-monitoring'}
        />
      </SocketCommon>
    </>
  );
};

export default PointMonitoring;
