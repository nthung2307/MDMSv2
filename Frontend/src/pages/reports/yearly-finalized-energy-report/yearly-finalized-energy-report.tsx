import { Button, Card, DatePicker } from 'antd';
import dayjs from 'dayjs';
import { useEffect, useState } from 'react';
import { FiShare } from 'react-icons/fi';
import { IoBackspaceOutline } from 'react-icons/io5';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListCustomerRequest } from '@/core/application/dto/customer/request/GetListCustomerRequest.ts';
import { GetListMeasurementPointsRequest } from '@/core/application/dto/measurement-point/requests/GetListMeasurementPointsRequest.ts';
import { GetYearlyFinalizedEnergyReportRequest } from '@/core/application/dto/reports/requests/GetYearlyFinalizedEnergyReportRequest.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListCustomerForReportHook from '@/infrastructure/repository/customer/hooks/useGetCustomerListForReport.hook.tsx';
import useGetListMeasurementPointsForReportHook from '@/infrastructure/repository/measurement-point/hooks/useGetListMeasurementPointsForReport.hook.tsx';
import useGetYearlyFinalizedEnergyReportHook from '@/infrastructure/repository/reports/hooks/useGetYearlyFinalizedEnergyReport.hook.ts';
import { _t } from '@/infrastructure/utils/translation';
import TableYearlyReport from '@/pages/reports/yearly-finalized-energy-report/components/table-yearly-report';

const YearlyFinalizedEnergyReport = () => {
  const navigate = useNavigate();
  const [dataForm, setDataForm] = useStateDataForm({
    customerId: null,
    measurementPointId: null,
    selectedYear: dayjs(),
  });

  const [getYearlyReport] = useGetYearlyFinalizedEnergyReportHook();
  const [getListCustomerRequest] = useGetListCustomerForReportHook();
  const [getListMeasurementPointRequest] = useGetListMeasurementPointsForReportHook();

  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [data, setData] = useState<any[]>([]);
  const [customers, setCustomers] = useState<any[]>([]);
  const [measurementPoints, setMeasurementPoints] = useState<any[]>([]);
  const [searchData, setSearchData] = useState<{ customerCode: string; monitorPoint: string }>({
    customerCode: '',
    monitorPoint: '',
  });

  const debouncedCustomerCodeSearch = useDebounce(searchData?.customerCode, 300);
  const debouncedMeasurementPointSearch = useDebounce(searchData?.monitorPoint, 300);

  const handleClearSearch = () => {
    setDataForm({
      customerId: null,
      measurementPointId: null,
      selectedYear: dayjs(),
    });
    setSearchData({ customerCode: '', monitorPoint: '' });
    setMeasurementPoints([]);
    setData([]);
  };

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
        const customersList = res?.data?.data?.map((it: any) => ({
          value: it?.id,
          label: it?.customerCode,
          fullName: it?.fullName,
        }));
        setCustomers(customersList);
      },
      () => {},
      () => {}
    );
  };

  const getListMeasurementPointAsync = async ({
    pageSize,
    searchByFields,
    customerId,
  }: GetListMeasurementPointsRequest) => {
    const params = { pageSize, searchByFields, customerId };
    await getListMeasurementPointRequest(
      params,
      (res) => {
        const measurementPointsList = res?.data?.data?.map((it: any) => ({
          value: it?.id,
          label: it?.code,
        }));
        setMeasurementPoints(measurementPointsList);
      },
      () => {},
      () => {}
    );
  };

  const onSearchCustomer = (val: string) => {
    setSearchData({ ...searchData, customerCode: val });
  };

  const onSearchMeasurementPoint = (val: string) => {
    setSearchData({ ...searchData, monitorPoint: val });
  };

  const getReportData = async () => {
    if (!dataForm.customerId || !dataForm.measurementPointId) {
      return;
    }

    const params: GetYearlyFinalizedEnergyReportRequest = {
      customerId: Number(dataForm.customerId),
      measurementPointId: Number(dataForm.measurementPointId),
      year: dataForm.selectedYear.year(),
      pageSize: 50,
    };

    setLoading(true);
    await getYearlyReport(
      params,
      (res: DataResponse) => {
        setData(res?.data?.data || []);
        setLoading(false);
      },
      () => {
        setData([]);
        setLoading(false);
      },
      setLoading
    );
  };

  const dataExport = data.map((item, index) => ({
    index: index + 1,
    createdTime: item.createdTime ? dayjs(item.createdTime).format('DD/MM/YYYY HH:mm') : 'N/A',
    chiSoTichLuyDienNangTacDungAPTongKenhGiao:
      item.chiSoTichLuyDienNangTacDungAPTongKenhGiao ?? 'N/A',
    chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao:
      item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao ?? 'N/A',
    chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao:
      item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao ?? 'N/A',
    chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao:
      item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao ?? 'N/A',
    chiSoTichLuyDienNangPhanKhangAQTongKenhGiao:
      item.chiSoTichLuyDienNangPhanKhangAQTongKenhGiao ?? 'N/A',
    chiSoTichLuyDienNangTacDungAPTongKenhNhan:
      item.chiSoTichLuyDienNangTacDungAPTongKenhNhan ?? 'N/A',
    chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan:
      item.chiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan ?? 'N/A',
    chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan:
      item.chiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan ?? 'N/A',
    chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan:
      item.chiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan ?? 'N/A',
    chiSoTichLuyDienNangPhanKhangAQTongKenhNhan:
      item.chiSoTichLuyDienNangPhanKhangAQTongKenhNhan ?? 'N/A',
  }));

  useEffect(() => {
    getListCustomerAsync();
  }, [debouncedCustomerCodeSearch]);

  useEffect(() => {
    if (dataForm?.customerId) {
      setDataForm({ ...dataForm, measurementPointId: null });
      getListMeasurementPointAsync({
        pageSize: 10,
        searchByFields: [
          {
            searchFieldName: 'code',
            searchValue: debouncedMeasurementPointSearch || '',
          },
        ],
        customerId: dataForm?.customerId,
      });
    } else {
      setMeasurementPoints([]);
      setDataForm({ ...dataForm, measurementPointId: null });
    }
  }, [debouncedMeasurementPointSearch, dataForm?.customerId]);

  useEffect(() => {
    if (dataForm.customerId && dataForm.measurementPointId) {
      getReportData();
    }
  }, [dataForm.customerId, dataForm.measurementPointId, dataForm.selectedYear]);

  return (
    <div>
      <PageTitle title={'Chỉ số điện hàng năm'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Báo cáo', link: ROUTE_PATH.REPORTS },
            { name: 'Chỉ số điện', link: ROUTE_PATH.HALF_HOUR_FINALIZED_ENERGY_REPORT },
            { name: 'Hàng năm (0:00 đầu tháng)' },
          ]}
        />
        <div className='flex gap-1'>
          <ButtonCommonCustomizable
            title={_t('')}
            classButton='text-button-variant'
            tooltipText='Xuất file'
            onClick={() => {
              setIsShowExport(true);
            }}
            icon={<FiShare size={20} />}
            disabled={!(data && data.length > 0)}
          />
        </div>
      </div>

      <Card className='mt-5 shadow-custom'>
        <div className='mb-5 flex justify-between items-center'>
          <div className='flex items-center w-2/3 gap-2'>
            <div className='basis-4/5 flex items-center gap-3'>
              <div className='basis-1/2'>
                <InputSelectCommon
                  label={null}
                  attribute={'customerId'}
                  dataAttribute={dataForm?.customerId}
                  placeholder={'Nhập mã khách hàng'}
                  setData={setDataForm}
                  isRequired={false}
                  allowClear={true}
                  listDataItem={customers}
                  showSearch={true}
                  onSearch={onSearchCustomer}
                />
              </div>
              <div className='basis-1/2'>
                <InputSelectCommon
                  label={null}
                  attribute={'measurementPointId'}
                  dataAttribute={dataForm?.measurementPointId}
                  placeholder={'Nhập mã điểm đo'}
                  setData={setDataForm}
                  isRequired={false}
                  allowClear={true}
                  listDataItem={measurementPoints}
                  disabled={!dataForm.customerId}
                  showSearch={true}
                  onSearch={onSearchMeasurementPoint}
                />
              </div>
            </div>
            <div className='flex gap-1'>
              {dataForm?.customerId || dataForm?.measurementPointId ? (
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
        </div>

        {/* Navigation Tabs between 3 reports */}
        <div className='mb-4 flex gap-3 items-center'>
          <Button onClick={() => navigate(ROUTE_PATH.HALF_HOUR_FINALIZED_ENERGY_REPORT)}>
            Chu kỳ 30 phút
          </Button>
          <Button onClick={() => navigate(ROUTE_PATH.DAILY_FINALIZED_ENERGY_REPORT)}>
            Hàng tháng (0:00 hàng ngày)
          </Button>
          <Button type='primary'>
            Hàng năm (0:00 đầu mỗi tháng)
          </Button>
          <DatePicker
            picker='year'
            format='YYYY'
            value={dataForm.selectedYear}
            onChange={(date) => {
              if (date) {
                setDataForm({ selectedYear: date });
              }
            }}
            placeholder='Chọn năm'
            allowClear={false}
          />
        </div>

        <div>
          <TableYearlyReport dataSource={data} loading={loading} />
        </div>
      </Card>

      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        title='Báo cáo chỉ số điện hàng năm (0:00 đầu mỗi tháng)'
        dataSource={dataExport}
        listHeader={Constants.listHeaderExport.DailyFinalizedEnergyReport}
        fileNameCsv={`bao-cao-chi-so-dien-hang-nam-${dataForm.selectedYear.year()}`}
        typeExportAll={'yearly-finalized-energy'}
        requestBody={{
          customerId: dataForm.customerId,
          measurementPointId: dataForm.measurementPointId,
          year: dataForm.selectedYear.year(),
        }}
        onlyExportAll={true}
      />
    </div>
  );
};

export default YearlyFinalizedEnergyReport;
