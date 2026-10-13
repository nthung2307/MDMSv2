import { Card } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useRef, useState } from 'react';
import { FiShare } from 'react-icons/fi';
import { IoBackspaceOutline } from 'react-icons/io5';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import { GetListCustomerRequest } from '@/core/application/dto/customer/request/GetListCustomerRequest.ts';
import { GetListMeasurementPointsRequest } from '@/core/application/dto/measurement-point/requests/GetListMeasurementPointsRequest.ts';
import { GetListFinalizedOutputElectricityRequest } from '@/core/application/dto/reports/requests/GetListFinalizedOutputElectricityRequest.ts';
import { IGetListFinalizedOutputElectricityResponse } from '@/core/application/interfaces/response/reports/report.interface.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputDatePickerCommon from '@/infrastructure/common/components/input/input-date-picker-common.tsx';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useGetListCustomerForReportHook from '@/infrastructure/repository/customer/hooks/useGetCustomerListForReport.hook.tsx';
import useGetListMeasurementPointsForReportHook from '@/infrastructure/repository/measurement-point/hooks/useGetListMeasurementPointsForReport.hook.tsx';
import useGetListFinalizedOutputElectricityHook from '@/infrastructure/repository/reports/hooks/useGetListFinalizedOutputElectricity.hook.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';
import TableOutputMeasurement from '@/pages/reports/total-output-measurement-points/components/table-output-measurement.tsx';

const TotalOutputMeasurementPoints = () => {
  const chartRef = useRef(null);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [getListCustomerRequest] = useGetListCustomerForReportHook();
  const [getListMeasurementPointRequest] = useGetListMeasurementPointsForReportHook();
  const [getListFinalizedOutputElectricityRequest] = useGetListFinalizedOutputElectricityHook();

  // Initialize with the current month and year
  const [dataForm, setDataForm] = useStateDataForm({
    customerCode: null,
    monitorPoint: null,
    datePicker: dayjs(),
  });

  const [showClearSearch, setShowClearSearch] = useState(false);
  const [listMeasurementPoint, setListMeasurementPoint] = useState([]);
  const [listCustomer, setListCustomer] = useState([]);
  const [dataList, setDataList] = useState<IGetListFinalizedOutputElectricityResponse>([]);
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

  const getListFinalizedOutputElectricityAsync = async () => {
    const params = new GetListFinalizedOutputElectricityRequest({
      measurementPointId: dataForm?.monitorPoint ? dataForm?.monitorPoint : null,
      customerId: dataForm?.customerCode ? dataForm?.customerCode : null,
      month: dayjs(dataForm?.datePicker, Constants.DateTime.MonthYearFormat).month() + 1,
      year: dayjs(dataForm?.datePicker, Constants.DateTime.MonthYearFormat).year(),
    });

    await getListFinalizedOutputElectricityRequest(
      params as any,
      (res) => {
        setDataList(res?.data);
      },
      () => {
        setDataList([]);
      },
      () => {}
    );
  };

  const onSearch = (key: any, data: string) => {
    setSearchData({ ...searchData, [key]: data });
  };

  const clearSearch = () => {
    setDataForm({ customerCode: '', monitorPoint: '' });
    setSearchData({ customerCode: '', monitorPoint: '' });
    setListMeasurementPoint([]);
    setDataList([]);
    setShowClearSearch(false);
  };

  const fileNameBuilder = () => {
    return `Bang_tong_san_luong_tat_ca_cac_diem_do_${dataForm?.datePicker}`;
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
    getListFinalizedOutputElectricityAsync();
  }, [dataForm.customerCode, dataForm.monitorPoint, dataForm.datePicker]);

  useEffect(() => {
    setShowClearSearch(!!dataForm?.customerCode || !!dataForm?.monitorPoint);
  }, [dataForm]);
  return (
    <div>
      <PageTitle title={'Báo cáo'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Báo cáo', link: ROUTE_PATH.TOTAL_OUTPUT_MEASUREMENT_POINTS },
            {
              name: `Bảng tổng sản lượng tất cả các điểm đo tháng ${dayjs(
                dataForm?.datePicker
              ).format(Constants.DateTime.MonthYearFormat)}`,
            },
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
            disabled={!(dataList?.length > 0)}
            icon={<FiShare size={20} />}
          />
        </div>
      </div>
      <Card className='mt-5 shadow-custom'>
        <div>
          <div className={'mb-5'}>
            <div className='flex gap-1 items-center'>
              <div className='basis-1/2 flex items-center gap-3'>
                <div className='basis-1/3'>
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
                <div className='basis-1/3'>
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
                  />
                </div>
                <div className='basis-1/3'>
                  <InputDatePickerCommon
                    label={''}
                    attribute={'datePicker'}
                    dataAttribute={dataForm?.datePicker}
                    setData={setDataForm}
                    isRequired={false}
                    placeholder={'Nhập tháng/năm'}
                    picker={'month'}
                    format={Constants.DateTime.MonthYearFormat}
                    allowClear={true}
                    onCallBackFunction={(data: any) => {
                      if (data) {
                        setDataForm({
                          ...dataForm,
                          datePicker: dayjs(data),
                        });
                      }
                    }}
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
          </div>
          <div ref={chartRef}>
            <TableOutputMeasurement dataList={dataList} />
          </div>
        </div>
      </Card>

      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        onlyPDF={true}
        title='Tổng sản lượng các điểm đo'
        dataSource={dataList}
        listHeader={Constants.listHeaderExport.TotalOutputPoint}
        fileNameCsv={fileNameBuilder()}
        typeExportAll={''}
      />
    </div>
  );
};

export default TotalOutputMeasurementPoints;
