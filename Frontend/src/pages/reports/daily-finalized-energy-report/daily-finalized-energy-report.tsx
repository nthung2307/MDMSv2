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
import { GetListDailyDailyFinalizedEnergyReportRequest } from '@/core/application/dto/reports/requests/GetDailyFinalizedEnergyReportRequest.ts';
import { IDailyFinalizedEnergyReportListResponse } from '@/core/application/interfaces/response/reports/report.interface.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListCustomerForReportHook from '@/infrastructure/repository/customer/hooks/useGetCustomerListForReport.hook.tsx';
import useGetListMeasurementPointsForReportHook from '@/infrastructure/repository/measurement-point/hooks/useGetListMeasurementPointsForReport.hook.tsx';
import useGetDailyFinalizedEnergyReportHook from '@/infrastructure/repository/reports/hooks/useGetDailyFinalizedEnergyReport.hook';
import { deleteParams } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation';
import TableDailyReport from '@/pages/reports/daily-finalized-energy-report/components/table-daily-report';

const DailyFinalizedEnergyReport = () => {
  const navigate = useNavigate();
  const [dataForm, setDataForm] = useStateDataForm({
    customerId: null,
    measurementPointId: null,
    selectedDate: dayjs(),
  });
  const [getDailyReport] = useGetDailyFinalizedEnergyReportHook();
  const [getListCustomerRequest] = useGetListCustomerForReportHook();
  const [getListMeasurementPointRequest] = useGetListMeasurementPointsForReportHook();
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(10);
  const [data, setData] = useState<IDailyFinalizedEnergyReportListResponse>();
  const [customers, setCustomers] = useState<any[]>([]);
  const [measurementPoints, setMeasurementPoints] = useState<any[]>([]);
  const [searchData, setSearchData] = useState<{ customerCode: string; monitorPoint: string }>({
    customerCode: '',
    monitorPoint: '',
  });
  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
  }>({ hasNext: false, hasPrevious: false });
  const [requestParams, setRequestParams] = useState<
    Partial<GetListDailyDailyFinalizedEnergyReportRequest>
  >({
    pageSize: size,
  });

  const debouncedCustomerCodeSearch = useDebounce(searchData?.customerCode, 300);
  const debouncedMeasurementPointSearch = useDebounce(searchData?.monitorPoint, 300);

  const handleClearSearch = () => {
    setDataForm({
      customerId: null,
      measurementPointId: null,
      selectedDate: dayjs(),
    });
    setSearchData({ customerCode: '', monitorPoint: '' });
    setMeasurementPoints([]);
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
        const customersList = res?.data?.data?.map((it: any) => {
          return {
            value: it?.id,
            label: it?.customerCode,
            fullName: it?.fullName,
          };
        });
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
        const measurementPointsList = res?.data?.data?.map((it: any) => {
          return {
            value: it?.id,
            label: it?.code,
          };
        });
        setMeasurementPoints(measurementPointsList);
      },
      () => {},
      () => {}
    );
  };

  const onSearchCustomer = (data: string) => {
    setSearchData({ ...searchData, customerCode: data });
  };

  const onSearchMeasurementPoint = (data: string) => {
    setSearchData({ ...searchData, monitorPoint: data });
  };

  const getReportData = async ({
    pageSize = size,
    before,
    after,
  }: Partial<GetListDailyDailyFinalizedEnergyReportRequest> = {}) => {
    if (!dataForm.customerId || !dataForm.measurementPointId) {
      return;
    }

    const params: GetListDailyDailyFinalizedEnergyReportRequest = {
      customerId: dataForm.customerId,
      measurementPointId: dataForm.measurementPointId,
      month: dataForm.selectedDate.month() + 1,
      year: dataForm.selectedDate.year(),
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
    };

    await getDailyReport(
      params,
      (res: DataResponse) => {
        setData(res?.data);
        setPaginationData({
          hasNext: res?.data?.hasNext || false,
          hasPrevious: res?.data?.hasPrevious || false,
        });
      },
      () => {},
      setLoading
    );
  };

  const dataExport = data?.data?.map((item, index) => {
    return {
      index: index + 1,
      createdTime: item.createdTime ? dayjs(item.createdTime).format('DD/MM/YYYY') : 'N/A',
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
    };
  });

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
      setRequestParams((prev) => deleteParams(prev, ['before', 'after']));
    }
  }, [dataForm.customerId, dataForm.measurementPointId, dataForm.selectedDate]);

  useEffect(() => {
    if (dataForm.customerId && dataForm.measurementPointId) {
      getReportData({ ...requestParams });
    }
  }, [size, requestParams]);

  return (
    <div>
      <PageTitle title={'Chỉ số điện'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[{ name: 'Báo cáo', link: ROUTE_PATH.REPORTS }, { name: 'Chỉ số điện' }]}
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
            disabled={!(data?.data && data.data.length > 0)}
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
          <PaginationCommon
            pageSize={size}
            setSize={(newSize) => {
              setSize(newSize);
              setRequestParams((prev) => {
                const updatedParams = { ...prev, pageSize: newSize };
                return deleteParams(updatedParams, ['before', 'after']);
              });
            }}
            hasNext={paginationData?.hasNext}
            hasPrevious={paginationData?.hasPrevious}
            onClickPrevious={() => {
              const beforeId = data?.data?.[0]?.id;
              setRequestParams((prev) => {
                const updatedParams = { ...prev, before: beforeId };
                return deleteParams(updatedParams, ['after']);
              });
            }}
            onClickNext={() => {
              const afterId = data?.data?.[data.data.length - 1]?.id;
              setRequestParams((prev) => {
                const updatedParams = { ...prev, after: afterId };
                return deleteParams(updatedParams, ['before']);
              });
            }}
          />
        </div>
        <div className='mb-4 flex gap-3 items-center'>
          <Button onClick={() => navigate(ROUTE_PATH.HALF_HOUR_FINALIZED_ENERGY_REPORT)}>
            Chu kỳ 30 phút
          </Button>
          <Button type='primary'>Hàng tháng (0:00 hàng ngày)</Button>
          <Button onClick={() => navigate(ROUTE_PATH.YEARLY_FINALIZED_ENERGY_REPORT)}>
            Hàng năm (0:00 đầu mỗi tháng)
          </Button>
          <DatePicker
            picker='month'
            format='MM/YYYY'
            value={dataForm.selectedDate}
            onChange={(date) => {
              if (date) {
                setDataForm({ selectedDate: date });
              }
            }}
            placeholder='Chọn tháng'
          />
        </div>
        <div>
          <TableDailyReport dataSource={data?.data ?? []} loading={loading} />
        </div>
      </Card>
      {/* Dialog export file */}
      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        title='Báo cáo số liệu định kỳ ngày'
        dataSource={dataExport}
        listHeader={Constants.listHeaderExport.DailyFinalizedEnergyReport}
        fileNameCsv={'bao-cao-hang-ngay'}
        typeExportAll={'daily-finalized-energy'}
        requestBody={{
          customerId: dataForm.customerId,
          measurementPointId: dataForm.measurementPointId,
          month: dataForm.selectedDate.month() + 1,
          year: dataForm.selectedDate.year(),
        }}
        onlyExportAll={true}
      />
    </div>
  );
};

export default DailyFinalizedEnergyReport;
