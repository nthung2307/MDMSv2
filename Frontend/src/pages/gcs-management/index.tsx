import { PlusOutlined } from '@ant-design/icons';
import { Card, TableProps } from 'antd';
import { useEffect, useState } from 'react';
import { FaRegTrashCan } from 'react-icons/fa6';
import { FiCheckSquare, FiShare } from 'react-icons/fi';
import { IoBackspaceOutline, IoSearch } from 'react-icons/io5';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { DeleteGCSRequest } from '@/core/application/dto/gcs/request/DeleteGCSRequest';
import { GetListGCSRequest } from '@/core/application/dto/gcs/request/GetListGCSRequest';
import { IGCSListResponseInterface } from '@/core/application/interfaces/response/gcs/gcsResponse.interface';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { ActionDetailLink } from '@/infrastructure/common/components/context-menu/ActionDetailLink';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common.tsx';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common.tsx';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PrivateComponent from '@/infrastructure/common/components/private-component/private-component.tsx';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common.tsx';
import TableCommon from '@/infrastructure/common/components/table/table-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useDeleteGCSHook from '@/infrastructure/repository/gcs/hooks/useDeleteGCS.hook.tsx';
import useGetListGCSHook from '@/infrastructure/repository/gcs/hooks/useGetGCSList.hook';
import { deleteParams } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation.ts';

const GCSManagement = () => {
  const navigate = useNavigate();
  const [getListGCS] = useGetListGCSHook();
  const [deleteGCSRequest] = useDeleteGCSHook();
  const [selectedGcsForDelete, setSelectedGcsForDelete] = useState<{ id: number; name: string } | null>(null);
  const [isShowDeleteDialog, setIsShowDeleteDialog] = useState<boolean>(false);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [data, setData] = useState<IGCSListResponseInterface>();
  const [dataForm, setDataForm] = useStateDataForm({
    code: '',
    status: Constants.CustomerStatus.Active,
  });

  const deleteGCSAsync = async () => {
    if (!selectedGcsForDelete?.id) return;
    const params = new DeleteGCSRequest({ id: selectedGcsForDelete.id });
    await deleteGCSRequest(
      params,
      () => {
        setIsShowDeleteDialog(false);
        setSelectedGcsForDelete(null);
        notifySuccess('', _t('Xóa sổ GCS thành công'));
        getListGCSAsync({ ...getListGCSParamsRequest });
      },
      () => {
        setIsShowDeleteDialog(false);
      },
      setLoading
    );
  };
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
    pageNumber,
    searchByFields = [],
    sortByFields = [],
  }: GetListGCSRequest) => {
    const params = {
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
      ...(pageNumber && { pageNumber }),
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
      return deleteParams(updated, ['before', 'after', 'pageNumber']);
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
      title: 'Thao tác',
      key: 'action',
      align: 'center',
      fixed: 'right',
      width: 100,
      render: (_, record) => {
        const detailUrl = ROUTE_PATH.GCS_DETAILS.replace(
          Constants.ReplaceRouterConfig.id,
          record.id
        );
        const checkUrl = `${ROUTE_PATH.GCS_CHECK}?id=${record.id}&name=${encodeURIComponent(record.name || '')}&code=${encodeURIComponent(record.code || '')}`;

        return (
          <div className='flex items-center justify-center gap-2'>
            <ActionDetailLink
              href={detailUrl}
              title={`Chi tiết sổ: ${record.name || record.code}`}
            />
            <ActionDetailLink
              href={checkUrl}
              title={`Tra soát sổ: ${record.name || record.code}`}
              className='text-emerald-600 hover:text-emerald-800 hover:bg-emerald-50 p-1.5 rounded transition-colors inline-flex items-center justify-center cursor-pointer select-none'
              icon={<FiCheckSquare size={16} className='flex-shrink-0' />}
            />
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
        <h1 className='text-24-28 font-bold'>{_t('Quản lý sổ GCS')}</h1>
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
              return deleteParams(updatedParams, ['before', 'after', 'pageNumber']);
            });
          }}
          hasNext={data?.hasNext || false}
          hasPrevious={data?.hasPrevious || false}
          onClickPrevious={() => {
            setGetListGCSParamsRequest((prev) => {
              const updatedParams = { ...prev, before: firstRecordId };
              return deleteParams(updatedParams, ['after', 'pageNumber']);
            });
          }}
          onClickNext={() => {
            setGetListGCSParamsRequest((prev) => {
              const updatedParams = { ...prev, after: lastRecordId };
              return deleteParams(updatedParams, ['before', 'pageNumber']);
            });
          }}
          onChangePage={(page) => {
            setGetListGCSParamsRequest((prev) => {
              const updatedParams = { ...prev, pageNumber: page };
              return deleteParams(updatedParams, ['before', 'after']);
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
        fileNameCsv={'MDMS_DanhSachSoGCS'}
        typeExportAll={'gcs'}
      />
      <DialogCommon
        title={''}
        visible={isShowDeleteDialog}
        handleCancel={() => {
          setIsShowDeleteDialog(false);
          setSelectedGcsForDelete(null);
        }}
        handleConfirm={deleteGCSAsync}
        isButtonSubmit={true}
        showFooter={true}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-10 gap-2'>
            <p className='text-20-20 font-bold'>{_t('Xác nhận xoá sổ GCS')}</p>
            <p className='text-16-16 text-center'>
              {_t(`Sau khi xác nhận sổ "${selectedGcsForDelete?.name || ''}" sẽ bị xoá khỏi hệ thống.`)}
            </p>
          </div>
        </div>
      </DialogCommon>
    </div>
  );
};

export default GCSManagement;
