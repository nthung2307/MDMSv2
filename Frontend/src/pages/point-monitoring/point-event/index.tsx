import { Card } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useRef, useState } from 'react';
import { FiShare } from 'react-icons/fi';
import { IoBackspaceOutline } from 'react-icons/io5';

import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListMeasurementPointMonitoringRequest } from '@/core/application/dto/measurement-point-monitoring/requests/GetListMeasurementPointMonitoringRequest';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import RangeDatePickerSearchCommon from '@/infrastructure/common/components/input/range-date-picker-search-common';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import SocketCommon from '@/infrastructure/common/components/websocket/socket-common';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListMeasurementPointEventHook from '@/infrastructure/repository/measurement-point-monitoring/hooks/useGetListMeasurementPointEvent.hook';
import { convertDateTime, deleteParams, getISODate } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';
import TableEvent from '@/pages/point-monitoring/point-event/components/table-event';

const PointMonitoringEvent = () => {
  const tableRef = useRef(null);
  const [dataForm, setDataForm] = useStateDataForm({});
  const [getListMeasurementPointEvent] = useGetListMeasurementPointEventHook();
  const [loading, setLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(10);
  const [data, setData] = useState<any[]>([]);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [dataSocket, setDataSocket] = useState(null);
  const [getListMeasurementParamsRequest, setGetListMeasurementParamsRequest] =
    useState<GetListMeasurementPointMonitoringRequest>();

  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
    totalCount?: number;
  }>({
    hasNext: false,
    hasPrevious: false,
  });

  const currentParamsRef = useRef<GetListMeasurementPointMonitoringRequest | undefined>(
    getListMeasurementParamsRequest
  );
  currentParamsRef.current = getListMeasurementParamsRequest;

  const sizeRef = useRef(size);
  sizeRef.current = size;

  const firstCustomerID = data?.[0]?.id;
  const lastCustomerID = data?.[data.length - 1]?.id;

  const updateParamsValueFromListCustomerParams = (paramsToUpdate: any) => {
    setGetListMeasurementParamsRequest((prev) => {
      const updated = {
        ...prev,
        ...paramsToUpdate,
      };
      return deleteParams(updated, ['before', 'after']);
    });
  };

  const handleClearSearch = () => {
    setDataForm({
      eventName: '',
      measurementPointCode: '',
      date: [],
    });
    updateParamsValueFromListCustomerParams({
      searchByFields: [],
      measurementpointCode: '',
      startDate: '',
      endDate: '',
    });
  };

  const debounceEventName = useDebounce(dataForm?.eventName, 300);
  const debounceMeasurementPoint = useDebounce(dataForm?.measurementPointCode, 300);

  const getListMeasurementPointEventAsync = async ({
    pageSize = size,
    before,
    after,
    searchByFields = [],
    sortByFields = [],
    measurementpointCode,
    startDate,
    endDate,
  }: GetListMeasurementPointMonitoringRequest) => {
    const params: GetListMeasurementPointMonitoringRequest = {
      pageSize,
      ...(before && { before }),
      ...(after && { after }),
      ...(measurementpointCode && {
        measurementpointCode,
        measurementPointCode: measurementpointCode,
      }),
      ...(startDate && { startDate: await getISODate(startDate) }),
      ...(endDate && { endDate: await getISODate(endDate) }),
      searchByFields: searchByFields.length > 0 ? searchByFields : [],
      sortByFields: sortByFields.length > 0 ? sortByFields : [],
    };
    await getListMeasurementPointEvent(
      { ...params },
      (res: DataResponse) => {
        const { data } = res;
        const result = data?.data;
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
    const currentSize = sizeRef.current || 10;
    const measurementpointCode =
      currentParams.measurementpointCode || currentParams.measurementPointCode;
    const params: GetListMeasurementPointMonitoringRequest = {
      pageSize: currentParams.pageSize || currentSize,
      ...(currentParams.before && { before: currentParams.before }),
      ...(currentParams.after && { after: currentParams.after }),
      ...(measurementpointCode && {
        measurementpointCode,
        measurementPointCode,
      }),
      ...(currentParams.startDate && { startDate: await getISODate(currentParams.startDate) }),
      ...(currentParams.endDate && { endDate: await getISODate(currentParams.endDate) }),
      searchByFields: currentParams.searchByFields?.length ? currentParams.searchByFields : [],
      sortByFields: currentParams.sortByFields?.length ? currentParams.sortByFields : [],
    };

    await getListMeasurementPointEvent(
      { ...params },
      (res: DataResponse) => {
        const { data: resData } = res;
        const result = resData?.data || [];
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

  const dataExport = data?.map((item, idx) => ({
    index: idx + 1,
    createdTime: convertDateTime(item.createdTime, 'dd-mm-yyyy hh:mm:ss') ?? 'N/A',
    measurementPointCode: item.measurementPointCode ?? 'N/A',
    customerName: item.customerName ?? 'N/A',
    eventName: item.eventName ? item.eventName : 'N/A',
    status: `${item.status.charAt(0)}${item.status.slice(1).toLowerCase()}`,
    value: item.value === true || item.value === 'True' || item.value === 1 || item.value === '1' ? 1 : 0,
  }));
  useEffect(() => {
    const searchByFields: any = [];
    let measurementpointCode = '';
    let startDate = '';
    let endDate = '';

    const dateArr = dataForm.date;
    if (dateArr) {
      startDate = dateArr[0]
        ? dayjs(dateArr[0]).startOf('day').format('YYYY-MM-DDTHH:mm:ss[Z]')
        : '';
      endDate = dateArr[1] ? dayjs(dateArr[1]).endOf('day').format('YYYY-MM-DDTHH:mm:ss[Z]') : '';
    }

    if (debounceMeasurementPoint) {
      measurementpointCode = debounceMeasurementPoint;
    }

    if (debounceEventName) {
      searchByFields.push({ searchFieldName: 'eventName', searchValue: debounceEventName });
    }

    updateParamsValueFromListCustomerParams({
      searchByFields,
      measurementpointCode,
      startDate,
      endDate,
    });
  }, [debounceMeasurementPoint, debounceEventName, dataForm.date]);

  useEffect(() => {
    getListMeasurementPointEventAsync({
      ...getListMeasurementParamsRequest,
    });
  }, [size, getListMeasurementParamsRequest]);

  useEffect(() => {
    if (dataSocket) {
      refreshCurrentPageAsync();
      setDataSocket(null);
    }
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
        endPoint={Constants.socketIoEndpoint.event}
        status={Constants.socketIoStatus.ReceiveEvent}
      >
        <div>
          <div className='flex justify-between items-center'>
            <BreadcrumbUI items={[{ name: 'Giám sát điểm đo' }, { name: 'Bảng sự kiện' }]} />
            <div className='flex gap-1'>
              {/* <ButtonCommonCustomizable
                title={_t('')}
                icon={<FiRefreshCw size={20} />}
                tooltipText={'Làm mới'}
                onClick={() => {
                  getListMeasurementPointEventAsync({
                    ...getListMeasurementParamsRequest,
                    pageSize: size,
                  });
                  setDataSocket(null);
                }}
                disabled={!dataSocket}
                classButton='text-button-variant'
              /> */}

              <ButtonCommonCustomizable
                title={''}
                tooltipText={'Xuất file'}
                classButton='text-button-variant'
                onClick={() => {
                  setIsShowExport(true);
                }}
                icon={<FiShare size={20} />}
                disabled={!(data && data.length > 0)}
              />
            </div>
          </div>
          <Card className='mt-5 shadow-custom '>
            <div className='mb-5 flex justify-between items-center'>
              <div className='flex items-center w-2/3 gap-1'>
                <div className='basis-4/5 flex items-center gap-1'>
                  <div className='basis-1/3'>
                    <RangeDatePickerSearchCommon
                      data={dataForm?.date}
                      setData={setDataForm}
                      onCallBackFunction={(val) => {
                        setDataForm({ date: val });
                      }}
                      allowClear
                      placeholder={['dd/mm/yyyy', 'dd/mm/yyyy']}
                    />
                  </div>
                  <div className='basis-1/3'>
                    <InputTextCommon
                      label={null}
                      isLabel={false}
                      attribute={'measurementPointCode'}
                      dataAttribute={dataForm?.measurementPointCode}
                      placeholder={_t('Mã điểm đo')}
                      setData={setDataForm}
                      isRequired={false}
                      allowClear={true}
                    />
                  </div>
                  <div className='basis-1/3'>
                    <InputTextCommon
                      label={null}
                      isLabel={false}
                      dataAttribute={dataForm?.eventName}
                      attribute={'eventName'}
                      setData={setDataForm}
                      isRequired={false}
                      placeholder={_t('Tên sự kiện')}
                      allowClear={true}
                    />
                  </div>
                </div>
                <div className='flex gap-1'>
                  {(dataForm?.date?.length > 0 ||
                    dataForm?.measurementPointCode ||
                    dataForm?.eventName) && (
                    <ButtonCommonCustomizable
                      title={_t('')}
                      tooltipText='Xoá tất cả'
                      icon={<IoBackspaceOutline size={25} />}
                      classButton='text-button-variant'
                      onClick={handleClearSearch}
                      variant={'text'}
                    />
                  )}
                </div>
              </div>
            </div>
            <div ref={tableRef}>
              <TableEvent dataSource={data ?? []} loading={loading} />
            </div>
            <PaginationCommon
              pageSize={size}
              total={paginationData?.totalCount}
              currentCount={data?.length || 0}
              hasNext={paginationData?.hasNext}
              setSize={(newSize) => {
                setSize(newSize);
                setGetListMeasurementParamsRequest((prev) => {
                  const updatedParams = { ...prev, pageSize: newSize };
                  return deleteParams(updatedParams, ['before', 'after']);
                });
              }}
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
          <DialogExportForm
            open={isShowExport}
            handleCancel={() => {
              setIsShowExport(false);
            }}
            title='Quản lý sự kiện'
            dataSource={dataExport}
            listHeader={Constants.listHeaderExport.eventParams}
            fileNameCsv={'DanhSachSukien'}
            typeExportAll={'measurement-point-monitoring'}
            subTypeExportAll={'measurement-point-event'}
          />
        </div>
      </SocketCommon>
    </>
  );
};

export default PointMonitoringEvent;
