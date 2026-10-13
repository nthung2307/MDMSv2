import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { TbBook2 } from 'react-icons/tb';
import { useLocation } from 'react-router-dom';

import bgEmpty from '@/assets/bg-empty.svg';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GCSCutOffCycleRequest } from '@/core/application/dto/gcs/request/GCSCutOffCycleRequest';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGCSCutOffCycleHook from '@/infrastructure/repository/gcs/hooks/useGCSCutOffCycle.hook';
import { deleteParams } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation.ts';
import DialogNotice from '@/pages/notice-management/components/dialog/dialog-notice';
import SearchDetailGCS from '@/pages/notice-management/components/search/search-detail-gcs';
import TableDetailGcs from '@/pages/notice-management/components/table/table-detail-gcs';

interface GcsParams {
  gcsId: number;
}

const NoticeManagement = () => {
  const location = useLocation();
  const stateData = location.state;
  const [dataForm, setDataForm] = useStateDataForm({});
  const [loading, setLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(10);
  const [GCSName, setGCSName] = useState<string>('');
  const [gcsParams, setGcsParams] = useState<GcsParams>();
  const [data, setData] = useState<any[]>([]);
  const [getGCSCutOffCycle] = useGCSCutOffCycleHook();
  const [isShowDialog, setIsShowDialog] = useState<any>(false);
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
    setGetGCSCutOffCycleRequest({ gcsId: stateData.idGcs });
  };

  useEffect(() => {
    setGetGCSCutOffCycleRequest({
      gcsId: gcsParams?.gcsId ?? 0,
      month: Number(dataForm?.month) || 0,
      year: Number(dataForm?.year) || 0,
    });
  }, [dataForm?.month, dataForm?.year, gcsParams?.gcsId]);

  const handleCancel = () => {
    setIsShowDialog(false);
  };

  const handelSubmit = (params: GcsParams) => {
    setGcsParams(params);
  };

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
      pageSize,
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
    if (location.state) {
      setGcsParams({ gcsId: stateData.idGcs });
      setGCSName(stateData.nameGcs);
    }
  }, [location.state]);

  return (
    <div>
      <PageTitle title={'Quản lý phiếu báo'} />
      <div className='flex justify-between items-center'>
        {gcsParams ? (
          <BreadcrumbUI
            items={[
              { name: 'Quản lý phiếu báo' },
              {
                name: `${GCSName}`,
              },
            ]}
          />
        ) : (
          <h1 className='text-24-28 font-bold'>{_t('Quản lý phiếu báo')}</h1>
        )}
        <div className='flex gap-5'>
          <ButtonCommonCustomizable
            title={''}
            classButton='text-button-variant'
            tooltipText={'Chọn sổ'}
            onClick={() => {
              setIsShowDialog(true);
            }}
            icon={<TbBook2 size={25} />}
          />
        </div>
      </div>

      {gcsParams ? (
        <div>
          <Card className='mt-5 shadow-custom'>
            <div className='mb-5 flex justify-between items-center'>
              <SearchDetailGCS
                dataForm={dataForm}
                setDataForm={setDataForm}
                handleClearSearch={handleClearSearch}
                gcsId={gcsParams?.gcsId}
              />
              <PaginationCommon
                pageSize={size}
                setSize={(newSize) => {
                  setSize(newSize);
                  setGetGCSCutOffCycleRequest((prev) =>
                    deleteParams({ ...prev, pageSize: newSize }, ['before', 'after'])
                  );
                }}
                hasNext={paginationData?.hasNext}
                hasPrevious={paginationData?.hasPrevious}
                onClickPrevious={() => {
                  setGetGCSCutOffCycleRequest((prev) =>
                    deleteParams({ ...prev, before: firstPointID }, ['after'])
                  );
                }}
                onClickNext={() => {
                  setGetGCSCutOffCycleRequest((prev) =>
                    deleteParams({ ...prev, after: lastPointID }, ['before'])
                  );
                }}
              />
            </div>
            <TableDetailGcs
              dataSource={data ?? []}
              setIsShowDialog={setIsShowDialog}
              GCSName={GCSName}
              gcsId={gcsParams.gcsId}
              loading={loading}
            />
          </Card>
        </div>
      ) : (
        <Card className='mt-5'>
          <div className='flex justify-center'>
            <img src={bgEmpty} alt={''} />
          </div>
        </Card>
      )}

      <DialogNotice
        open={isShowDialog}
        handleCancel={handleCancel}
        handelSubmit={handelSubmit}
        setGCSName={setGCSName}
      />
    </div>
  );
};

export default NoticeManagement;
