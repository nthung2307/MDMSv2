import { Card } from 'antd';
import { useEffect, useState, useMemo } from 'react';
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
  const debounceSearch = useDebounce(dataForm.name, 300);
  const debounceSearchCode = useDebounce(dataForm.measurementPointCode, 300);

  const [getListMeasurementParamsRequest, setGetListMeasurementParamsRequest] =
    useState<GetListMeasurementPointMonitoringRequest>();

  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
  }>({ hasNext: false, hasPrevious: false });

  const firstCustomerID = data?.[0]?.measurementPointsId;
  const lastCustomerID = data?.[data.length - 1]?.measurementPointsId;

  const handleClearSearch = () => {
    setDataForm({
      name: '',
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
    customerName,
    measurementPointCode,
  }: GetListMeasurementPointMonitoringRequest) => {
    const params: GetListMeasurementPointMonitoringRequest = {
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
      ...(customerName && { customerName }),
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
        });
      },
      () => {},
      setLoading
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
        ...(debounceSearch && { customerName: debounceSearch }),
        ...(debounceSearchCode && { measurementPointCode: debounceSearchCode }),
      };
      if (!debounceSearch) delete updated.customerName;
      if (!debounceSearchCode) delete updated.measurementPointCode;
      return deleteParams(updated, ['before', 'after']);
    });
  }, [debounceSearch, debounceSearchCode, dataForm.connectionStatus]);

  useEffect(() => {
    getListMeasurementPointMonitoringAsync({
      ...getListMeasurementParamsRequest,
    });
  }, [size, getListMeasurementParamsRequest]);

  useEffect(() => {
    if (!dataSocket?.data || !Array.isArray(dataSocket.data)) return;
    setData((prevData) => {
      if (!prevData || prevData.length === 0) return prevData;
      const updatedMap = new Map(
        dataSocket.data.map((item: any) => [item.measurementPointsId || item.id, item])
      );
      return prevData.map((item: any) => {
        const update = updatedMap.get(item.measurementPointsId || item.id);
        return update ? { ...item, ...update } : item;
      });
    });
  }, [dataSocket]);

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
              <div className='basis-4/5 flex items-center gap-1'>
                <div className='basis-1/2'>
                  <InputTextCommon
                    label={null}
                    attribute={'name'}
                    dataAttribute={dataForm?.name}
                    placeholder={_t('Nhập tên khách hàng')}
                    setData={setDataForm}
                    isRequired={false}
                    prefix={<IoSearch />}
                  />
                </div>
                <div className='basis-1/2'>
                  <InputTextCommon
                    label={null}
                    attribute={'measurementPointCode'}
                    dataAttribute={dataForm?.measurementPointCode}
                    placeholder={'Nhập mã điểm đo'}
                    setData={setDataForm}
                    isRequired={false}
                    allowClear={true}
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
                {(dataForm.name || dataForm.connectionStatus) && (
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
            <PaginationCommon
              pageSize={size}
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
          </div>
          <TableSummary dataSource={data ?? []} loading={loading} />
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
