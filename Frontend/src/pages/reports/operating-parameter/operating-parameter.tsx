import { Card } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useMemo, useRef, useState } from 'react';
import { FiShare } from 'react-icons/fi';
import { IoBackspaceOutline } from 'react-icons/io5';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import { GetListCustomerRequest } from '@/core/application/dto/customer/request/GetListCustomerRequest.ts';
import { GetListMeasurementPointsRequest } from '@/core/application/dto/measurement-point/requests/GetListMeasurementPointsRequest.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useGetListCustomerForReportHook from '@/infrastructure/repository/customer/hooks/useGetCustomerListForReport.hook.tsx';
import useGetListMeasurementPointsForReportHook from '@/infrastructure/repository/measurement-point/hooks/useGetListMeasurementPointsForReport.hook.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';
import ChartFilter, { FilterStateValue } from '@/pages/reports/components/chart-filter.tsx';
import ChartSwitcher from '@/pages/reports/electricity-consumption/components/chart-switcher.tsx';
import PMaxContainer from '@/pages/reports/operating-parameter/components/active-power';
import CurrentChart from '@/pages/reports/operating-parameter/components/current-chart';
import QMaxContainer from '@/pages/reports/operating-parameter/components/reactive-power';
import VoltageChart from '@/pages/reports/operating-parameter/components/voltage-chart';

const OperatingParameter = () => {
  const chartRef = useRef(null);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [getListCustomerRequest] = useGetListCustomerForReportHook();
  const [getListMeasurementPointRequest] = useGetListMeasurementPointsForReportHook();
  const [dataForm, setDataForm] = useStateDataForm({});
  const [showClearSearch, setShowClearSearch] = useState(false);
  const [listMeasurementPoint, setListMeasurementPoint] = useState<any>([]);
  const [listCustomer, setListCustomer] = useState<any>([]);
  const [filterData, setFilterData] = useState<FilterStateValue>({
    chartType: Constants.Reports.Type.OperatingParameter.DienApTrungBinh3Pha.value,
    radioValue: Constants.Reports.Type.OperatingParameter.DienApTrungBinh3Pha.value,
    date: dayjs().toISOString(),
    previousPeriod: false,
  });
  const [chartTimeSelector, setChartTimeSelector] = useState<any>([]);
  const [searchData, setSearchData] = useState<{ customerCode: string; monitorPoint: string }>({
    customerCode: '',
    monitorPoint: '',
  });
  const debouncedCustomerCodeSearch = useDebounce(searchData?.customerCode, 300);
  const debouncedMeasurementPointSearch = useDebounce(searchData?.monitorPoint, 300);
  const getListCustomerAsync = async () => {
    const params = new GetListCustomerRequest({
      pageSize: 10,
      searchByFields: [
        {
          searchFieldName: 'customerCode',
          searchValue: debouncedCustomerCodeSearch || '',
        },
        {
          searchFieldName: 'status',
          searchValue: 'ACTIVE',
        },
      ],
    });
    await getListCustomerRequest(
      params as any,
      (res) => {
        const customers = res?.data?.data?.map((it: any) => {
          return {
            value: it?.id,
            label: it?.customerCode,
          };
        });
        setListCustomer(customers);
      },
      () => {},
      () => {}
    );
  };

  const getListMeasurementPointAsync = async ({
    pageSize,
    searchByFields,
    sortByFields,
    before,
    after,
    customerId,
  }: GetListMeasurementPointsRequest) => {
    const params = { pageSize, searchByFields, sortByFields, before, after, customerId };
    await getListMeasurementPointRequest(
      params,
      (res) => {
        const measurementPoints = res?.data?.data?.map((it: any) => {
          return {
            value: it?.id,
            label: it?.code,
          };
        });
        setListMeasurementPoint(measurementPoints);
      },
      () => {},
      () => {}
    );
  };

  const onSearch = (key: any, data: string) => {
    setSearchData({ ...searchData, [key]: data });
  };

  const loadChartTypeArray = useMemo(() => {
    return Object.values(Constants.Reports.Type.OperatingParameter).map((type) => ({
      label: type.label,
      value: type.value,
    }));
  }, []);

  const chartsList = useMemo(() => {
    return [
      {
        type: Constants.Reports.Type.OperatingParameter.DienApTrungBinh3Pha.value,
        chartComponent: VoltageChart,
      },
      {
        type: Constants.Reports.Type.OperatingParameter.DongDienTrungBinh3Pha.value,
        chartComponent: CurrentChart,
      },
      {
        type: Constants.Reports.Type.OperatingParameter.P.value,
        chartComponent: PMaxContainer,
      },
      {
        type: Constants.Reports.Type.OperatingParameter.Q.value,
        chartComponent: QMaxContainer,
      },
    ];
  }, []);

  const clearSearch = () => {
    setDataForm({ customerCode: '', monitorPoint: '' });
    setSearchData({ customerCode: '', monitorPoint: '' });
    setListMeasurementPoint([]);
    setShowClearSearch(false);
  };

  const fileNameBuilder = () => {
    return `${filterData?.chartType}_${listCustomer?.find((customer: any) => customer.value === dataForm?.customerCode)?.label}_${listMeasurementPoint?.find((measurementPoint: any) => measurementPoint.value === dataForm?.monitorPoint)?.label}`;
  };

  useEffect(() => {
    getListCustomerAsync();
  }, [debouncedCustomerCodeSearch]);

  useEffect(() => {
    if (dataForm?.customerCode) {
      setDataForm({ ...dataForm, monitorPoint: '' });
      getListMeasurementPointAsync({
        pageSize: 10,
        searchByFields: [
          {
            searchFieldName: 'code',
            searchValue: debouncedMeasurementPointSearch || '',
          },
        ],
        customerId: dataForm?.customerCode,
      });
    } else {
      setListMeasurementPoint([]);
      setDataForm({ ...dataForm, monitorPoint: '' });
    }
  }, [debouncedMeasurementPointSearch, dataForm?.customerCode]);

  useEffect(() => {
    // Update clear button visibility based on dataForm's current state
    setShowClearSearch(!!dataForm?.customerCode || !!dataForm?.monitorPoint);
  }, [dataForm]);

  useEffect(() => {
    if (!filterData?.chartType) {
      setChartTimeSelector(Constants.Reports.TimeSelector.OperatingParameter[0].value);
    } else {
      const chartType = filterData?.chartType;
      switch (chartType) {
        case Constants.Reports.Type.OperatingParameter.DienApTrungBinh3Pha.value:
          return setChartTimeSelector(Constants.Reports.TimeSelector.OperatingParameter);
        case Constants.Reports.Type.OperatingParameter.DongDienTrungBinh3Pha.value:
          return setChartTimeSelector(Constants.Reports.TimeSelector.OperatingParameter);
        case Constants.Reports.Type.OperatingParameter.P.value:
          return setChartTimeSelector(Constants.Reports.TimeSelector.OperatingParameter);
        case Constants.Reports.Type.OperatingParameter.Q.value:
          return setChartTimeSelector(Constants.Reports.TimeSelector.OperatingParameter);
        default:
          return setChartTimeSelector([]);
      }
    }
  }, [filterData?.chartType]);

  return (
    <div>
      <PageTitle title={'Báo cáo'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Báo cáo', link: ROUTE_PATH.REPORTS },
            { name: 'Biểu đồ thông số vận hành' },
          ]}
        />
        <div className='flex gap-5'>
          <ButtonCommonCustomizable
            title={_t('')}
            classButton='text-button-variant'
            tooltipText='Xuất file'
            onClick={() => {
              setIsShowExport(true);
            }}
            disabled={!(filterData && dataForm.customerCode && dataForm.monitorPoint)}
            icon={<FiShare size={20} />}
          />
        </div>
      </div>
      <Card className='mt-5 shadow-custom'>
        <div>
          <div className={'mb-5'}>
            <div className='flex gap-3 items-center'>
              <div className='basis-1/2 flex items-center gap-3'>
                <div className='basis-1/2'>
                  <InputSelectCommon
                    isLabel={false}
                    label={''}
                    dataAttribute={dataForm?.customerCode}
                    attribute={'customerCode'}
                    setData={setDataForm}
                    isRequired={false}
                    placeholder={'Nhập mã khách hàng'}
                    listDataItem={listCustomer}
                    onSearch={(value) => {
                      onSearch('customerCode', value);
                    }}
                    showSearch={true}
                    allowClear={true}
                  />
                </div>
                <div className='basis-1/2'>
                  <InputSelectCommon
                    isLabel={false}
                    label={''}
                    attribute={'monitorPoint'}
                    dataAttribute={dataForm?.monitorPoint}
                    setData={setDataForm}
                    isRequired={false}
                    placeholder={'Nhập mã điểm đo'}
                    onSearch={(value) => {
                      onSearch('monitorPoint', value);
                    }}
                    listDataItem={listMeasurementPoint}
                    showSearch={true}
                    allowClear={true}
                    disabled={!dataForm?.customerCode}
                  />
                </div>
              </div>
              {showClearSearch && (
                <div>
                  <ButtonCommonCustomizable
                    title={_t('')}
                    classButton='text-button-variant'
                    tooltipText='Xoá tất cả'
                    icon={<IoBackspaceOutline size={25} />}
                    onClick={() => {
                      clearSearch();
                    }}
                    variant={'text'}
                  />
                </div>
              )}
            </div>
            <div className={'mt-5'}>
              <ChartFilter
                radioOptions={chartTimeSelector}
                chartTypeOptions={loadChartTypeArray}
                setFilterData={setFilterData}
                isPreviousPeriod={false}
              />
            </div>
          </div>
          <div ref={chartRef}>
            {filterData?.chartType && (
              <ChartSwitcher
                chartType={filterData?.chartType}
                chartList={chartsList}
                filterData={filterData}
                dataForm={dataForm}
              />
            )}
          </div>
        </div>
      </Card>
      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        title='Thông số vận hành'
        onlyPDF={true}
        chartRef={chartRef.current}
        listHeader={[]}
        fileNameCsv={fileNameBuilder()}
        typeExportAll={''}
      />
    </div>
  );
};

export default OperatingParameter;
