import { PlusOutlined } from '@ant-design/icons';
import { Card, Dropdown, MenuProps, TableProps } from 'antd';
import { useEffect, useState } from 'react';
import { BsThreeDots } from 'react-icons/bs';
import { FiShare } from 'react-icons/fi';
import { IoBackspaceOutline, IoSearch } from 'react-icons/io5';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListGCSRequest } from '@/core/application/dto/gcs/request/GetListGCSRequest';
import { IGCSListResponseInterface } from '@/core/application/interfaces/response/gcs/gcsResponse.interface';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common.tsx';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common.tsx';
import PrivateComponent from '@/infrastructure/common/components/private-component/private-component.tsx';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common.tsx';
import TableCommon from '@/infrastructure/common/components/table/table-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListGCSHook from '@/infrastructure/repository/gcs/hooks/useGetGCSList.hook';
import { deleteParams } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation.ts';

const GCSManagement = () => {
  const navigate = useNavigate();
  const [getListGCS] = useGetListGCSHook();
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [data, setData] = useState<IGCSListResponseInterface>();
  const [dataForm, setDataForm] = useStateDataForm({
    code: '',
    status: Constants.CustomerStatus.Active,
  });
  const [getListGCSParamsRequest, setGetListGCSParamsRequest] = useState<GetListGCSRequest>({
    searchByFields: [
      {
        searchFieldName: 'status',
        searchValue: Constants.CustomerStatus.Active,
      },
    ],
    sortByFields: [],
  });

  const [size, setSize] = useState<number>(10);

  const firstRecordId = data?.data?.[0]?.id;
  const lastRecordId = data?.data?.[data?.data?.length - 1]?.id;

  const debounceCode = useDebounce(dataForm?.code, 300);

  const getListGCSAsync = async ({
    before,
    pageSize = size,
    after,
    searchByFields = [],
    sortByFields = [],
  }: GetListGCSRequest) => {
    const params = {
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
      ...(searchByFields.length > 0 && { searchByFields }),
      ...(sortByFields.length > 0 && { sortByFields }),
    };
    await getListGCS(
      params,
      (res: DataResponse) => {
        const { data } = res;
        setData(data);
      },
      () => {},
      setLoading
    );
  };

  const handleClearSearch = () => {
    setDataForm({
      code: '',
      status: '',
    });
    setGetListGCSParamsRequest({
      searchByFields: [],
      sortByFields: [],
    });
  };

  useEffect(() => {
    const searchByFields: any = [];
    if (debounceCode) {
      searchByFields.push({ searchFieldName: 'code', searchValue: debounceCode });
    }
    if (dataForm?.status) {
      searchByFields.push({ searchFieldName: 'status', searchValue: dataForm.status });
    }

    setGetListGCSParamsRequest((prev) => {
      const updated = {
        ...prev,
        searchByFields: searchByFields.length > 0 ? searchByFields : [],
      };
      return deleteParams(updated, ['before', 'after']);
    });
  }, [debounceCode, dataForm?.status]);

  const columns: TableProps['columns'] = [
    {
      title: 'Mã sổ',
      dataIndex: 'code',
      align: 'left',
    },
    {
      title: 'Tên sổ',
      dataIndex: 'name',
      align: 'left',
    },
    {
      title: 'Số kì',
      dataIndex: 'periodNumber',
      align: 'left',
      width: '5%',
    },
    {
      title: 'Mô tả',
      dataIndex: 'description',
      align: 'left',
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      align: 'left',
      width: '10%',
      render: (_, record) => {
        const isActive = record.status === Constants.CustomerStatus.Active;
        const backgroundColor = isActive ? '#41A026' : '#B22222';
        const statusText = isActive ? 'Đang mở' : 'Đã đóng';
        return (
          <StatusTagCommon status={statusText} bgColor={backgroundColor} textColor={'#FFFFFF'} />
        );
      },
    },
    {
      title: <div className={'text-center'}></div>,
      key: 'action',
      align: 'left',
      fixed: 'right',
      width: 50,
      render: (_, record) => {
        const items: MenuProps['items'] = [
          {
            key: '1',
            label: 'Chi tiết',
            onClick: () => {
              const url = ROUTE_PATH.GCS_DETAILS.replace(
                Constants.ReplaceRouterConfig.id,
                record.id
              );
              window.open(url, '_blank');
            },
          },
          {
            key: '3',
            label: 'Tra soát',
            onClick: () => {
              const url = `${ROUTE_PATH.GCS_CHECK}?id=${record.id}&name=${encodeURIComponent(record.name || '')}&code=${encodeURIComponent(record.code || '')}`;
              window.open(url, '_blank');
            },
          },
        ];

        return (
          <div className={'flex justify-center cursor-pointer'}>
            <Dropdown menu={{ items }}>
              <BsThreeDots />
            </Dropdown>
          </div>
        );
      },
    },
  ];
  const tableDataSource = data?.data?.map((item, index) => {
    return {
      key: item.id,
      id: item.id,
      index: index + 1,
      code: item.code,
      name: item.name,
      periodNumber: item.periodNumber,
      description: item.description,
      status: item.status,
      changeStatus: item.status,
      measurementPoints: item.measurementPoints,
    };
  });

  useEffect(() => {
    getListGCSAsync({
      ...getListGCSParamsRequest,
    });
  }, [size, getListGCSParamsRequest]);

  return (
    <div>
      <PageTitle title={'Quản lý sổ GCS'} />
      <div className='flex justify-between items-center'>
        <h1 className='text-24-28 font-bold'>{_t('Quản lý sổ ghi chỉ số')}</h1>
        <div className='flex gap-1'>
          <PrivateComponent roles={[Constants.Role.SuperAdmin.value, Constants.Role.Admin.value]}>
            <ButtonCommonCustomizable
              title={_t('')}
              tooltipText='Thêm sổ'
              classButton='text-button-variant'
              onClick={() => {
                navigate(ROUTE_PATH.ADD_GCS);
              }}
              icon={<PlusOutlined size={20} />}
            />
          </PrivateComponent>
          <ButtonCommonCustomizable
            title={_t('')}
            classButton='text-button-variant'
            tooltipText='Xuất file'
            onClick={() => {
              setIsShowExport(true);
            }}
            icon={<FiShare size={20} />}
            disabled={!(tableDataSource && tableDataSource.length > 0)}
          />
        </div>
      </div>
      <Card className='mt-5 shadow-custom'>
        <div className={'mb-5 flex justify-between items-center'}>
          <div className='flex items-center w-1/2 gap-2'>
            <div className='basis-4/5 flex items-center gap-2'>
              <div className='basis-1/2'>
                <InputTextCommon
                  label={null}
                  attribute={'code'}
                  dataAttribute={dataForm?.code}
                  placeholder={_t('Nhập mã sổ')}
                  setData={setDataForm}
                  isRequired={false}
                  prefix={<IoSearch />}
                />
              </div>
              <div className='basis-1/2'>
                <InputSelectCommon
                  label={''}
                  attribute={'status'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.status}
                  isRequired={false}
                  placeholder={'Trạng thái'}
                  listDataItem={[
                    { label: 'Đang mở', value: Constants.AccountStatus.Active },
                    { label: 'Đã đóng', value: Constants.AccountStatus.Deactivate },
                  ]}
                  allowClear={true}
                />
              </div>
            </div>
            <div className='flex gap-1'>
              {dataForm?.code || dataForm?.status ? (
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
        <TableCommon
          columns={columns}
          dataSource={tableDataSource}
          pagination={false}
          loading={loading}
          scroll={{ x: 'max-content', y: window.innerHeight * 0.66 }}
        />
        <PaginationCommon
          pageSize={size}
          total={data?.totalCount}
          currentCount={tableDataSource?.length || 0}
          setSize={(newSize) => {
            setSize(newSize);
            setGetListGCSParamsRequest((prev) => {
              const updatedParams = { ...prev, pageSize: newSize };
              return deleteParams(updatedParams, ['before', 'after']);
            });
          }}
          hasNext={data?.hasNext || false}
          hasPrevious={data?.hasPrevious || false}
          onClickPrevious={() => {
            setGetListGCSParamsRequest((prev) => {
              const updatedParams = { ...prev, before: firstRecordId };
              return deleteParams(updatedParams, ['after']);
            });
          }}
          onClickNext={() => {
            setGetListGCSParamsRequest((prev) => {
              const updatedParams = { ...prev, after: lastRecordId };
              return deleteParams(updatedParams, ['before']);
            });
          }}
        />
      </Card>
      {/* Dialog export file */}
      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        title='Quản lý sổ GCS'
        dataSource={tableDataSource}
        listHeader={Constants.listHeaderExport.GcsManagement}
        fileNameCsv={'so-gcs'}
        typeExportAll={'gcs'}
      />
    </div>
  );
};

export default GCSManagement;
