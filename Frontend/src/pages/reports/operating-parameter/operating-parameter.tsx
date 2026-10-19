import { Button, Card, DatePicker, Radio, Segmented } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useMemo, useState } from 'react';
import { FiBarChart2, FiShare, FiTable } from 'react-icons/fi';
import { IoBackspaceOutline } from 'react-icons/io5';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import { GetListCustomerRequest } from '@/core/application/dto/customer/request/GetListCustomerRequest.ts';
import { GetListMeasurementPointsRequest } from '@/core/application/dto/measurement-point/requests/GetListMeasurementPointsRequest.ts';
import {
  GetOperatingParameterReportRequest,
  OperatingParameterItem,
} from '@/core/application/dto/reports/requests/GetOperatingParameterReportRequest.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useGetListCustomerForReportHook from '@/infrastructure/repository/customer/hooks/useGetCustomerListForReport.hook.tsx';
import useGetListMeasurementPointsForReportHook from '@/infrastructure/repository/measurement-point/hooks/useGetListMeasurementPointsForReport.hook.tsx';
import useGetOperatingParameterReportHook from '@/infrastructure/repository/reports/hooks/useGetOperatingParameterReport.hook.ts';
import { _t } from '@/infrastructure/utils/translation.ts';
import OperatingParameterChart from '@/pages/reports/operating-parameter/components/operating-parameter-chart.tsx';
import OperatingParameterTable from '@/pages/reports/operating-parameter/components/operating-parameter-table.tsx';

const { RangePicker } = DatePicker;

const OperatingParameter = () => {
  const [activeTab, setActiveTab] = useState<'table' | 'chart'>('table');
  const [dateMode, setDateMode] = useState<'single' | 'range'>('single');
  const [singleDate, setSingleDate] = useState<dayjs.Dayjs>(dayjs());
  const [rangeDate, setRangeDate] = useState<[dayjs.Dayjs, dayjs.Dayjs]>([
    dayjs().startOf('day'),
    dayjs().endOf('day'),
  ]);

  const [tableData, setTableData] = useState<OperatingParameterItem[]>([]);
  const [tableLoading, setTableLoading] = useState<boolean>(false);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);

  const [getListCustomerRequest] = useGetListCustomerForReportHook();
  const [getListMeasurementPointRequest] = useGetListMeasurementPointsForReportHook();
  const [getOperatingParameterReport] = useGetOperatingParameterReportHook();

  const [dataForm, setDataForm] = useStateDataForm({});
  const [showClearSearch, setShowClearSearch] = useState(false);
  const [listMeasurementPoint, setListMeasurementPoint] = useState<any>([]);
  const [listCustomer, setListCustomer] = useState<any>([]);
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
        const customers = res?.data?.data?.map((it: any) => ({
          value: it?.id,
          label: it?.customerCode,
        }));
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
        const measurementPoints = res?.data?.data?.map((it: any) => ({
          value: it?.id,
          label: it?.code,
        }));
        setListMeasurementPoint(measurementPoints);
      },
      () => {},
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
    setShowClearSearch(false);
    setTableData([]);
  };

  const fileNameBuilder = () => {
    const customer = listCustomer?.find((c: any) => c.value === dataForm?.customerCode)?.label || '';
    const point = listMeasurementPoint?.find((p: any) => p.value === dataForm?.monitorPoint)?.label || '';
    return `Thong_so_van_hanh_${customer}_${point}`;
  };

  const normalizeOperatingHalfHourData = (
    records: OperatingParameterItem[]
  ): OperatingParameterItem[] => {
    if (!records || !records.length) return [];

    const slotMap = new Map<string, { record: OperatingParameterItem; diffMs: number }>();

    for (const item of records) {
      if (!item.createdTime) continue;
      const d = dayjs(item.createdTime);
      const minute = d.minute();

      let rounded: dayjs.Dayjs | null = null;

      if (minute >= 55) {
        rounded = d.add(1, 'hour').minute(0).second(0).millisecond(0);
      } else if (minute <= 5) {
        rounded = d.minute(0).second(0).millisecond(0);
      } else if (minute >= 25 && minute <= 35) {
        rounded = d.minute(30).second(0).millisecond(0);
      }

      if (!rounded) continue;

      const slotKey = rounded.toISOString();
      const diffMs = Math.abs(d.diff(rounded));

      if (!slotMap.has(slotKey) || diffMs < slotMap.get(slotKey)!.diffMs) {
        slotMap.set(slotKey, {
          record: {
            ...item,
            createdTime: rounded.toISOString(),
          },
          diffMs,
        });
      }
    }

    return Array.from(slotMap.values())
      .map((v) => v.record)
      .sort(
        (a, b) =>
          new Date(a.createdTime || 0).getTime() - new Date(b.createdTime || 0).getTime()
      );
  };

  const fetchTableData = async () => {
    if (!dataForm?.customerCode || !dataForm?.monitorPoint) {
      setTableData([]);
      return;
    }

    const startTime =
      dateMode === 'single'
        ? singleDate.startOf('day').toISOString()
        : rangeDate[0].startOf('day').toISOString();
    const endTime =
      dateMode === 'single'
        ? singleDate.endOf('day').toISOString()
        : rangeDate[1].endOf('day').toISOString();

    const params: GetOperatingParameterReportRequest = {
      customerId: Number(dataForm.customerCode),
      measurementPointId: Number(dataForm.monitorPoint),
      startTime,
      endTime,
      pageSize: 500,
    };

    setTableLoading(true);
    await getOperatingParameterReport(
      params,
      (res: any) => {
        const raw = res?.data?.data || [];
        setTableData(normalizeOperatingHalfHourData(raw));
        setTableLoading(false);
      },
      () => {
        setTableData([]);
        setTableLoading(false);
      },
      setTableLoading
    );
  };

  useEffect(() => {
    getListCustomerAsync();
  }, [debouncedCustomerCodeSearch]);

  useEffect(() => {
    if (dataForm?.customerCode) {
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
      setDataForm((prev: any) => ({ ...prev, monitorPoint: '' }));
    }
  }, [debouncedMeasurementPointSearch, dataForm?.customerCode]);

  useEffect(() => {
    setShowClearSearch(!!dataForm?.customerCode || !!dataForm?.monitorPoint);
  }, [dataForm]);

  useEffect(() => {
    if (dataForm?.customerCode && dataForm?.monitorPoint) {
      fetchTableData();
    }
  }, [dataForm?.customerCode, dataForm?.monitorPoint, dateMode, singleDate, rangeDate]);

  const exportRequestBody = useMemo(() => {
    const startTime =
      dateMode === 'single'
        ? singleDate.startOf('day').toISOString()
        : rangeDate[0].startOf('day').toISOString();
    const endTime =
      dateMode === 'single'
        ? singleDate.endOf('day').toISOString()
        : rangeDate[1].endOf('day').toISOString();

    return {
      customerId: Number(dataForm?.customerCode),
      measurementPointId: Number(dataForm?.monitorPoint),
      startTime,
      endTime,
      pageSize: 1000,
    };
  }, [dataForm?.customerCode, dataForm?.monitorPoint, dateMode, singleDate, rangeDate]);

  const currentVoltageUnit = useMemo(() => {
    const item = tableData.find((d) => d.dienApPhaAUnit);
    return item?.dienApPhaAUnit || 'kV';
  }, [tableData]);

  return (
    <div>
      <PageTitle title={'Thông số vận hành'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Báo cáo', link: ROUTE_PATH.REPORTS },
            { name: 'Thông số vận hành' },
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
            disabled={!(dataForm.customerCode && dataForm.monitorPoint)}
            icon={<FiShare size={20} />}
          />
        </div>
      </div>

      <Card className='mt-5 shadow-custom'>
        <div>
          {/* Customer & Measurement Point Selection */}
          <div className='flex gap-3 items-center mb-5'>
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
                  onClick={clearSearch}
                  variant={'text'}
                />
              </div>
            )}
          </div>

          {/* Mode Switcher: Table vs Chart & Unified Time Controls */}
          <div className='flex flex-wrap justify-between items-center gap-4 mb-5 pb-3 border-b'>
            <Segmented
              options={[
                {
                  label: (
                    <div className='flex items-center gap-2 px-3 py-1 font-medium'>
                      <FiTable />
                      <span>Bảng dữ liệu</span>
                    </div>
                  ),
                  value: 'table',
                },
                {
                  label: (
                    <div className='flex items-center gap-2 px-3 py-1 font-medium'>
                      <FiBarChart2 />
                      <span>Biểu đồ</span>
                    </div>
                  ),
                  value: 'chart',
                },
              ]}
              value={activeTab}
              onChange={(val) => setActiveTab(val as 'table' | 'chart')}
            />

            <div className='flex items-center gap-3'>
              <Radio.Group
                value={dateMode}
                onChange={(e) => setDateMode(e.target.value)}
                buttonStyle='solid'
              >
                <Radio.Button value='single'>Hàng ngày</Radio.Button>
                <Radio.Button value='range'>Khoảng thời gian</Radio.Button>
              </Radio.Group>

              {dateMode === 'single' ? (
                <DatePicker
                  value={singleDate}
                  onChange={(d) => d && setSingleDate(d)}
                  format='DD/MM/YYYY'
                  allowClear={false}
                />
              ) : (
                <RangePicker
                  value={rangeDate}
                  onChange={(dates) => {
                    if (dates && dates[0] && dates[1]) {
                      setRangeDate([dates[0], dates[1]]);
                    }
                  }}
                  format='DD/MM/YYYY'
                  allowClear={false}
                />
              )}

              <Button type='primary' onClick={fetchTableData} loading={tableLoading}>
                Truy vấn
              </Button>
            </div>
          </div>

          {/* Tab 1: Table Content */}
          {activeTab === 'table' && (
            <div>
              <OperatingParameterTable
                data={tableData}
                loading={tableLoading}
                voltageUnit={currentVoltageUnit}
              />
            </div>
          )}

          {/* Tab 2: Chart Content bound directly to tableData */}
          {activeTab === 'chart' && (
            <div>
              <OperatingParameterChart
                data={tableData}
                loading={tableLoading}
                voltageUnit={currentVoltageUnit}
              />
            </div>
          )}
        </div>
      </Card>

      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        title='Báo cáo thông số vận hành'
        onlyPDF={false}
        chartRef={null}
        listHeader={[]}
        fileNameCsv={fileNameBuilder()}
        typeExportAll={'report'}
        subTypeExportAll={'operating-parameter'}
        requestBody={exportRequestBody}
        onlyExportAll={true}
      />
    </div>
  );
};

export default OperatingParameter;
