import { Card } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useRef, useState } from 'react';
import { FiShare } from 'react-icons/fi';
import { IoBackspaceOutline } from 'react-icons/io5';
import { useRecoilValue } from 'recoil';

import { CountState } from '@/core/application/common/atoms/global/countState';
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
import SocketCommon from '@/infrastructure/common/components/websocket/socket-common.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListMeasurementPointAlertHook from '@/infrastructure/repository/measurement-point-monitoring/hooks/useGetListMeasurementPointAlert.hook';
import { convertDateTime, deleteParams, getISODate } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';
import TableWarning from '@/pages/point-monitoring/point-warning/components/table-warning';

const PointMonitoringWarning = () => {
  const tableRef = useRef(null);
  const [dataForm, setDataForm] = useStateDataForm({});
  const [getListMeasurementPointAlert] = useGetListMeasurementPointAlertHook();
  const [loading, setLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(10);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [data, setData] = useState<any[]>([]);
  const [dataSocket, setDataSocket] = useState(null);
  const countState = useRecoilValue(CountState);

  const debounceEventName = useDebounce(dataForm?.eventName, 300);
  const debounceMeasurementPoint = useDebounce(dataForm?.measurementPointCode, 300);

  const [getListMeasurementParamsRequest, setGetListMeasurementParamsRequest] =
    useState<GetListMeasurementPointMonitoringRequest>({});

  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
  }>({ hasNext: false, hasPrevious: false });

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

  // Effect for handling search parameters
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

  const getListMeasurementPointAlertAsync = async ({
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
      ...(measurementpointCode && { measurementpointCode }),
      ...(startDate && { startDate: await getISODate(startDate) }),
      ...(endDate && { endDate: await getISODate(endDate) }),
      searchByFields: searchByFields.length > 0 ? searchByFields : [],
      sortByFields: sortByFields.length > 0 ? sortByFields : [],
    };

    await getListMeasurementPointAlert(
      params,
      (res: DataResponse) => {
        const { data } = res;
        setData(data?.data || []);
        setPaginationData({
          hasNext: data.hasNext,
          hasPrevious: data.hasPrevious,
        });
      },
      () => {},
      setLoading
    );
  };

  const dataExport = data?.map((item) => ({
    createdTime: convertDateTime(item.createdTime, 'dd-mm-yyyy hh:mm:ss') ?? 'N/A',
    main: `${item.measurementPointCode ?? 'N/A'} - ${item.customerName ?? 'N/A'}`,
    eventName: item.eventName ? item.eventName : 'N/A',
    status: `${item.status.charAt(0)}${item.status.slice(1).toLowerCase()}`,
    value: item.value ? 1 : 0,
  }));
  // Effect for fetching data
  useEffect(() => {
    getListMeasurementPointAlertAsync({
      ...getListMeasurementParamsRequest,
    });
  }, [size, getListMeasurementParamsRequest]);
  useEffect(() => {
    getListMeasurementPointAlertAsync({ ...getListMeasurementParamsRequest });
  }, [dataSocket, countState]);

  return (
    <>
      <PageTitle title={'Giám sát điểm đo'} />
      <SocketCommon
        setDataSocket={setDataSocket}
        endPoint={Constants.socketIoEndpoint.event}
        status={Constants.socketIoStatus.ReceiveAlert}
      >
        <div className='flex justify-between items-center'>
          <BreadcrumbUI items={[{ name: 'Giám sát điểm đo' }, { name: 'Bảng cảnh báo' }]} />
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
        <Card className='mt-5 shadow-custom'>
          <div className='mb-5 flex justify-between items-center'>
            <div className='flex items-center w-2/3 gap-1'>
              <div className='basis-4/5 flex items-center gap-1'>
                <div className='basis-1/3'>
                  <RangeDatePickerSearchCommon
                    data={dataForm?.date}
                    setData={setDataForm}
                    onCallBackFunction={(val) => setDataForm({ date: val })}
                    allowClear
                    placeholder={['dd/mm/yyyy', 'dd/mm/yyyy']}
                  />
                </div>
                <div className='basis-1/3'>
                  <InputTextCommon
                    label={null}
                    attribute={'measurementPointCode'}
                    dataAttribute={dataForm?.measurementPointCode}
                    placeholder={_t('Nguồn')}
                    setData={setDataForm}
                    isRequired={false}
                  />
                </div>
                <div className='basis-1/3'>
                  <InputTextCommon
                    label={null}
                    dataAttribute={dataForm?.eventName}
                    attribute={'eventName'}
                    setData={setDataForm}
                    isRequired={false}
                    placeholder={_t('Tên cảnh báo')}
                  />
                </div>
              </div>
              <div className='flex gap-1'>
                {(dataForm?.date && dataForm?.date.length > 0) ||
                dataForm?.measurementPointCode ||
                dataForm?.eventName ? (
                  <ButtonCommonCustomizable
                    title={_t('')}
                    tooltipText='Xoá tất cả'
                    icon={<IoBackspaceOutline size={25} />}
                    classButton='text-button-variant'
                    onClick={handleClearSearch}
                    variant={'text'}
                  />
                ) : null}
              </div>
            </div>
            <PaginationCommon
              pageSize={size}
              setSize={(newSize) => {
                setSize(newSize);
                setGetListMeasurementParamsRequest((prev) =>
                  deleteParams({ ...prev, pageSize: newSize }, ['before', 'after'])
                );
              }}
              hasNext={paginationData?.hasNext}
              hasPrevious={paginationData?.hasPrevious}
              onClickPrevious={() => {
                setGetListMeasurementParamsRequest((prev) =>
                  deleteParams({ ...prev, before: firstCustomerID }, ['after'])
                );
              }}
              onClickNext={() => {
                setGetListMeasurementParamsRequest((prev) =>
                  deleteParams({ ...prev, after: lastCustomerID }, ['before'])
                );
              }}
            />
          </div>
          <div ref={tableRef}>
            <TableWarning dataSource={data ?? []} loading={loading} />
          </div>
        </Card>
        <DialogExportForm
          open={isShowExport}
          handleCancel={() => setIsShowExport(false)}
          title='Quản lý cảnh báo'
          dataSource={dataExport}
          listHeader={Constants.listHeaderExport.warningParams}
          fileNameCsv={'canhbao'}
          typeExportAll={'measurement-point-monitoring'}
          subTypeExportAll={'measurement-point-alert'}
        />
      </SocketCommon>
    </>
  );
};

export default PointMonitoringWarning;
