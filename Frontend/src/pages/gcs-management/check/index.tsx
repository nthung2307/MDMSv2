import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GCSCutOffCycleRequest } from '@/core/application/dto/gcs/request/GCSCutOffCycleRequest';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGCSCutOffCycleHook from '@/infrastructure/repository/gcs/hooks/useGCSCutOffCycle.hook';
import { deleteParams } from '@/infrastructure/utils/helpers';
import TableCheckGcs from '@/pages/gcs-management/components/table-check-gcs';
import SearchDetailGCS from '@/pages/notice-management/components/search/search-detail-gcs';

interface GcsParams {
  gcsId: number;
}

const GCSCheck = () => {
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  const queryId = searchParams.get('id');
  const queryName = searchParams.get('name');
  const queryCode = searchParams.get('code');

  const record = location.state?.record || {
    id: queryId ? Number(queryId) : 0,
    gcsId: queryId ? Number(queryId) : 0,
    name: queryName || '',
    code: queryCode || '',
  };

  const [dataForm, setDataForm] = useStateDataForm({});
  const [loading, setLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(10);
  const [GCSName, setGCSName] = useState<string>('');
  const [gcsParams, setGcsParams] = useState<GcsParams>();
  const [data, setData] = useState<any[]>([]);
  const [getGCSCutOffCycle] = useGCSCutOffCycleHook();
  const [getGCSCutOffCycleRequest, setGetGCSCutOffCycleRequest] = useState<GCSCutOffCycleRequest>();
  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
  }>({ hasNext: false, hasPrevious: false });
  const firstPointID = data?.[0]?.id;
  const lastPointID = data?.[data.length - 1]?.id;

  const handleClearSearch = () => {
    setDataForm({
      month: '',
      year: '',
    });
    setGetGCSCutOffCycleRequest({ gcsId: record?.gcsId ? record.gcsId : record?.id });
  };

  useEffect(() => {
    setGetGCSCutOffCycleRequest({
      gcsId: gcsParams?.gcsId ?? 0,
      month: Number(dataForm?.month) || 0,
      year: Number(dataForm?.year) || 0,
    });
  }, [dataForm?.month, dataForm?.year, gcsParams?.gcsId]);

  const getGCSCutOffCycleAsync = async ({
    pageSize = size,
    before,
    after,
    searchByFields = [],
    sortByFields = [],
    gcsId,
    month,
    year,
  }: GCSCutOffCycleRequest) => {
    const params: GCSCutOffCycleRequest = {
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
      ...(month && { month }),
      ...(year && { year }),
      searchByFields: searchByFields.length > 0 ? searchByFields : [],
      sortByFields: sortByFields.length > 0 ? sortByFields : [],
      gcsId: gcsId ?? 0,
    };
    await getGCSCutOffCycle(
      params,
      (res: DataResponse) => {
        const { data } = res;
        const result = data?.data;
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

  useEffect(() => {
    if (gcsParams) {
      getGCSCutOffCycleAsync({ ...getGCSCutOffCycleRequest, ...gcsParams });
    }
  }, [size, getGCSCutOffCycleRequest, gcsParams]);

  useEffect(() => {
    const activeGcsId = record?.gcsId ? record.gcsId : record?.id;
    const activeGcsName = record?.name ? record.name : (record?.nameGcs?.name || '');
    if (activeGcsId) {
      setGcsParams({ gcsId: activeGcsId });
    }
    if (activeGcsName) {
      setGCSName(activeGcsName);
    }
  }, [location.state, location.search]);

  return (
    <div>
      <PageTitle title={'Quản lý sổ GCS'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Quản lý sổ ghi chỉ số', link: ROUTE_PATH.GCS_MANAGEMENT },
            {
              name: `${GCSName}`,
            },
          ]}
        />
      </div>
      <div>
        <Card className='mt-5 shadow-custom'>
          <div className='mb-5 flex justify-between'>
            <SearchDetailGCS
              dataForm={dataForm}
              setDataForm={setDataForm}
              handleClearSearch={handleClearSearch}
              gcsId={record?.gcsId ? record.gcsId : record?.id}
            />
            <PaginationCommon
              pageSize={size}
              hasNext={paginationData?.hasNext}
              setSize={(newSize) => {
                setSize(newSize);
                setGetGCSCutOffCycleRequest((prev) => {
                  const updatedParams = { ...prev, pageSize: newSize };
                  return deleteParams(updatedParams, ['before', 'after']);
                });
              }}
              hasPrevious={paginationData?.hasPrevious}
              onClickPrevious={() => {
                setGetGCSCutOffCycleRequest((prev) => {
                  const updatedParams = { ...prev, before: firstPointID };
                  return deleteParams(updatedParams, ['after']); // Remove 'after' parameter
                });
              }}
              onClickNext={() => {
                setGetGCSCutOffCycleRequest((prev) => {
                  const updatedParams = { ...prev, after: lastPointID };
                  return deleteParams(updatedParams, ['before']); // Remove 'before' parameter
                });
              }}
            />
          </div>
          <TableCheckGcs dataSource={data ?? []} GCSName={GCSName} gcs={record} loading={loading} />
        </Card>
      </div>
    </div>
  );
};

export default GCSCheck;
