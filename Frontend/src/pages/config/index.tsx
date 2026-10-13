import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { IoBackspaceOutline, IoSearch } from 'react-icons/io5';

import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListMeasurementPointsRequest } from '@/core/application/dto/measurement-point/requests/GetListMeasurementPointsRequest';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListMeasurementPointsHook from '@/infrastructure/repository/measurement-point/hooks/useGetListMeasurementPoints.hook';
import { deleteParams } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';
import TableConfigManagement from '@/pages/config/components/table-config';

const Configuration = () => {
  const [dataForm, setDataForm] = useStateDataForm({});
  const [getListMeasurementPoints] = useGetListMeasurementPointsHook();
  const [loading, setLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(10);
  const [data, setData] = useState<any[]>([]);
  const [getListMeasurementParamsRequest, setGetListMeasurementParamsRequest] =
    useState<GetListMeasurementPointsRequest>({});
  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
  }>({ hasNext: false, hasPrevious: false });

  const firstCustomerID = data?.[0]?.id;
  const lastCustomerID = data?.[data.length - 1]?.id;

  const debounceFullName = useDebounce(dataForm?.fullName, 300);
  const debounceCode = useDebounce(dataForm?.code, 300);

  const handleClearSearch = () => {
    setDataForm({
      fullName: '',
      status: '',
      code: '',
    });
    setGetListMeasurementParamsRequest({});
  };

  useEffect(() => {
    const searchByFields: any = [];
    if (debounceFullName) {
      searchByFields.push({
        searchFieldName: 'customer.fullName',
        searchValue: debounceFullName,
      });
    }

    if (debounceCode) {
      searchByFields.push({
        searchFieldName: 'code',
        searchValue: debounceCode,
      });
    }

    if (dataForm?.status) {
      searchByFields.push({
        searchFieldName: 'status',
        searchValue: dataForm.status,
      });
    }

    setGetListMeasurementParamsRequest({
      searchByFields: searchByFields.length > 0 ? searchByFields : [],
    });
  }, [debounceFullName, debounceCode, dataForm?.status]);

  const getListMeasurementPointsAsync = async ({
    pageSize = size,
    before,
    after,
    searchByFields = [],
    sortByFields = [],
  }: GetListMeasurementPointsRequest) => {
    const params: GetListMeasurementPointsRequest = {
      pageSize,
      ...(before && { before }),
      ...(after && { after }),
      searchByFields: searchByFields.length > 0 ? searchByFields : [],
      sortByFields: sortByFields.length > 0 ? sortByFields : [],
    };

    await getListMeasurementPoints(
      params,
      (res: DataResponse) => {
        const { data } = res;
        setData(data?.data || []);
        setPaginationData({
          hasNext: data?.hasNext || false,
          hasPrevious: data?.hasPrevious || false,
        });
      },
      () => {},
      setLoading
    );
  };

  useEffect(() => {
    getListMeasurementPointsAsync({ ...getListMeasurementParamsRequest });
  }, [size, getListMeasurementParamsRequest]);

  return (
    <div>
      <PageTitle title={'Cài đặt hệ thống'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI items={[{ name: 'Cài đặt hệ thống' }, { name: 'Dữ liệu điểm đo' }]} />
      </div>
      <Card className='mt-5 shadow-custom'>
        <div className='mb-5 flex justify-between items-center'>
          <div className='flex items-center grow gap-1'>
            <div className='basis-3/5 flex items-center gap-3'>
              <div className='basis-1/2'>
                <InputTextCommon
                  label={null}
                  attribute={'fullName'}
                  dataAttribute={dataForm?.fullName}
                  placeholder={_t('Nhập tên khách hàng')}
                  setData={setDataForm}
                  isRequired={false}
                  prefix={<IoSearch />}
                  allowClear={true}
                />
              </div>
              <div className='basis-1/2'>
                <InputTextCommon
                  label={null}
                  attribute={'code'}
                  dataAttribute={dataForm?.code}
                  placeholder={_t('Mã điểm đo')}
                  setData={setDataForm}
                  isRequired={false}
                  allowClear={true}
                />
              </div>
              <div className='basis-1/2'>
                <InputSelectCommon
                  label={null}
                  attribute={'status'}
                  dataAttribute={dataForm?.status}
                  setData={setDataForm}
                  isRequired={false}
                  listDataItem={[
                    { label: 'Đang sử dụng', value: Constants.AccountStatus.Active },
                    { label: 'Ngừng sử dụng', value: Constants.AccountStatus.Deactivate },
                  ]}
                  placeholder={_t('Trạng thái')}
                  allowClear={true}
                />
              </div>
            </div>
            <div className='flex gap-1 items-center'>
              {dataForm?.fullName || dataForm?.status ? (
                <ButtonCommonCustomizable
                  title={_t('')}
                  classButton='text-button-variant'
                  tooltipText='Xoá tất cả'
                  icon={<IoBackspaceOutline size={25} />}
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
        <TableConfigManagement dataSource={data ?? []} loading={loading} />
      </Card>
    </div>
  );
};

export default Configuration;
