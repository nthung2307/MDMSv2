import { Card } from 'antd';
import { useEffect, useRef, useState } from 'react';
import { FiShare } from 'react-icons/fi';
import { useLocation, useSearchParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GCSMonthlyDetailRequest } from '@/core/application/dto/gcs/request/GcsMonthlyDetailRequest';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGcsMonthlyDetailHook from '@/infrastructure/repository/gcs/hooks/useGcsMonthlyDetail.hook';
import { deleteParams, formatNumberWithCommas } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';
import SearchCheckDetail from '@/pages/gcs-management/components/search-check-detail';
import TableCheckDetail from '@/pages/gcs-management/components/table-check-detail';

const GCSCheckDetail = () => {
  // const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const navState = (location.state as any) || {};

  const record = navState.record || {
    id: searchParams.get('cycleId') || 0,
    month: searchParams.get('month') || '',
    year: searchParams.get('year') || '',
    periodNumber: searchParams.get('period') || '',
  };
  const GCSName = navState.GCSName || searchParams.get('gcsName') || '';
  const gcs = navState.gcs || { id: searchParams.get('gcsId') || '' };

  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [dataForm, setDataForm] = useStateDataForm({});
  const [loading, setLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(24);
  const tableRef = useRef(null);
  const [data, setData] = useState<any[]>([]);
  const [gcsMonthlyDetailRequest] = useGcsMonthlyDetailHook();

  const [getGCSMonthlyDetailRequest, setGCSMonthlyDetailRequest] =
    useState<GCSMonthlyDetailRequest>({ gcsCutOffCycleId: record.id });
  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
    totalCount?: number;
  }>({ hasNext: false, hasPrevious: false });
  const firstPointID = data?.[0]?.id;
  const lastPointID = data?.[data.length - 1]?.id;

  const debounceMeasurementPointId = useDebounce(dataForm?.measurementPointId, 300);
  const debounceFullName = useDebounce(dataForm?.fullName, 300);

  const handleClearSearch = () => {
    setDataForm({
      measurementPointId: '',
      fullName: '',
    });
    setGCSMonthlyDetailRequest({ gcsCutOffCycleId: record.id });
  };
  useEffect(() => {
    const searchByFields: any = [];

    if (debounceMeasurementPointId) {
      searchByFields.push({
        searchFieldName: 'measurementPoint.code',
        searchValue: debounceMeasurementPointId,
      });
    }
    if (debounceFullName) {
      searchByFields.push({
        searchFieldName: 'measurementPoint.customer.fullName',
        searchValue: debounceFullName,
      });
    }

    setGCSMonthlyDetailRequest({
      searchByFields,
      gcsCutOffCycleId: record.id,
    });
  }, [debounceMeasurementPointId, debounceFullName, record.id]);

  const gcsMonthlyDetailAsync = async ({
    pageSize = size,
    before,
    after,
    searchByFields = [],
    sortByFields = [],
    gcsCutOffCycleId = Number(record.id),
    isChange,
  }: GCSMonthlyDetailRequest) => {
    const params: GCSMonthlyDetailRequest = {
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
      searchByFields: searchByFields.length > 0 ? searchByFields : [],
      sortByFields: sortByFields.length > 0 ? sortByFields : [],
      gcsCutOffCycleId: gcsCutOffCycleId ?? 0,
      isChange: isChange,
    };
    await gcsMonthlyDetailRequest(
      params,
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

  const dataExport = data?.map((item) => {
    return {
      measurementPoint: item.measurementPoint.code ?? 'N/A',
      description: item.measurementPoint.description ?? 'N/A',
      customer: item.measurementPoint.customer.fullName ?? 'N/A',
      meterReadingSet: item.meterReadingSet ?? 'N/A',
      previousReading:
        item.previousReading !== null && item.previousReading !== undefined
          ? (item.meterReadingSet === 'Pmax'
              ? Number(item.previousReading).toFixed(2)
              : Math.round(Number(item.previousReading)))
          : '',
      currentReading:
        item.currentReading !== null && item.currentReading !== undefined
          ? (item.meterReadingSet === 'Pmax'
              ? Number(item.currentReading).toFixed(2)
              : Math.round(Number(item.currentReading)))
          : '',
      multiplierFactor: item.multiplierFactor !== null && item.multiplierFactor !== undefined ? Math.round(Number(item.multiplierFactor)) : '',
      consumption: item.consumption !== null && item.consumption !== undefined ? Math.round(Number(item.consumption)) : (item.meterReadingSet === 'Pmax' ? 'N/A' : ''),
      isChange: item.isChange,
    };
  });

  useEffect(() => {
    gcsMonthlyDetailAsync({ ...getGCSMonthlyDetailRequest });
  }, [size, getGCSMonthlyDetailRequest]);
  return (
    <div>
      <PageTitle title={'Quản lý sổ GCS'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Quản lý sổ GCS', link: ROUTE_PATH.GCS_MANAGEMENT },
            { name: `${GCSName}`, link: ROUTE_PATH.GCS_CHECK, state: { record: gcs } },
            {
              name: `Tháng ${record.month}/${record.year} kì ${record.periodNumber}`,
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
            icon={<FiShare size={20} />}
            disabled={!(data && data.length > 0)}
          />
        </div>
      </div>
      <div>
        <Card className='mt-5 shadow-custom'>
          <div className='mb-5 flex justify-between'>
            <SearchCheckDetail
              dataForm={dataForm}
              setDataForm={setDataForm}
              handleClearSearch={handleClearSearch}
              handleSearch={() => {}}
            />
          </div>
          <div ref={tableRef}>
            <TableCheckDetail dataSource={data ?? []} loading={loading} />
          </div>
          <PaginationCommon
            pageSize={size}
            pageSizeOptions={['24', '48', '96', '192']}
            total={paginationData?.totalCount}
            currentCount={data?.length || 0}
            hasNext={paginationData?.hasNext}
            setSize={(newSize) => {
              setSize(newSize);
              setGCSMonthlyDetailRequest((prev) => {
                const updatedParams = { ...prev, pageSize: newSize };
                return deleteParams(updatedParams, ['before', 'after']);
              });
            }}
            hasPrevious={paginationData?.hasPrevious}
            onClickPrevious={() => {
              setGCSMonthlyDetailRequest((prev) => {
                const updatedParams = { ...prev, before: firstPointID };
                return deleteParams(updatedParams, ['after']); // Remove 'after' parameter
              });
            }}
            onClickNext={() => {
              setGCSMonthlyDetailRequest((prev) => {
                const updatedParams = { ...prev, after: lastPointID };
                return deleteParams(updatedParams, ['before']); // Remove 'before' parameter
              });
            }}
          />
        </Card>
      </div>
      {/* Dialog export file */}
      {(() => {
        const safeGcsName = (GCSName || 'SoGCS').replace(/[\\/:*?"<>|]/g, '_').trim();
        const periodStr = record?.periodNumber
          ? `Ky${record.periodNumber}_T${String(record.month).padStart(2, '0')}_${record.year}`
          : '';
        const exportFileName = `MDMS_TraSoat_${safeGcsName}${periodStr ? `_${periodStr}` : ''}`;

        return (
          <DialogExportForm
            open={isShowExport}
            handleCancel={() => {
              setIsShowExport(false);
            }}
            title={`${GCSName}: Tháng ${record.month}/${record.year} kì ${record.periodNumber}`}
            dataSource={dataExport}
            listHeader={Constants.listHeaderExport.GcsCheckDetail}
            fileNameCsv={exportFileName}
            typeExportAll={'gcs-monthly-detail'}
            paramsId={{ gcsCutOffId: Number(record.id) }}
          />
        );
      })()}
    </div>
  );
};

export default GCSCheckDetail;
