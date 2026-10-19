import PlusOutlined from '@ant-design/icons/lib/icons/PlusOutlined';
import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { FiDownload, FiShare } from 'react-icons/fi';
import { IoBackspaceOutline, IoSearch } from 'react-icons/io5';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListMeasurementPointsRequest } from '@/core/application/dto/measurement-point/requests/GetListMeasurementPointsRequest';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon';
import PrivateComponent from '@/infrastructure/common/components/private-component/private-component.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useChangeMeasurementPointStatusHook from '@/infrastructure/repository/measurement-point/hooks/useChangeMeasurementPointStatus.hook';
import useGetListMeasurementPointsHook from '@/infrastructure/repository/measurement-point/hooks/useGetListMeasurementPoints.hook';
import { deleteParams } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';
import DialogImportPoint from '@/pages/point-management/components/dialog-import-point';
import TablePointManagement from '@/pages/point-management/components/table-point';

const PointManagement = () => {
  const navigate = useNavigate();
  const [dataForm, setDataForm] = useStateDataForm({
    code: '',
    status: Constants.CustomerStatus.Active,
  });
  const [getListMeasurementPoints] = useGetListMeasurementPointsHook();
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [isShowImport, setIsShowImport] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(10);
  const [data, setData] = useState<any[]>([]);
  const [changeMeasurementPointStatus] = useChangeMeasurementPointStatusHook();
  const [isShowDialog, setIsShowDialog] = useState<any>({
    record: null,
    status: false,
  });
  const [getListMeasurementParamsRequest, setGetListMeasurementParamsRequest] =
    useState<GetListMeasurementPointsRequest>({
      searchByFields: [
        {
          searchFieldName: 'status',
          searchValue: Constants.AccountStatus.Active,
        },
      ],
      sortByFields: [],
    });
  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
    totalCount?: number;
  }>({ hasNext: false, hasPrevious: false });
  const firstPointID = data?.[0]?.id;
  const lastPointID = data?.[data.length - 1]?.id;

  const handleClearSearch = () => {
    setDataForm({
      code: '',
      status: '',
    });
    setGetListMeasurementParamsRequest((prev) => {
      const updatedParams = {
        ...prev,
        searchByFields: [],
      };
      return deleteParams(updatedParams, ['before', 'after']);
    });
  };

  const updateStatus = async (id: number) => {
    await changeMeasurementPointStatus(
      { id },
      () => {
        notifySuccess('', _t('Cập nhật trạng thái thành công'));
        getListMeasurementPointsAsync({ ...getListMeasurementParamsRequest });
        setIsShowDialog({
          record: '',
          status: false,
        });
      },
      () => {},
      () => {}
    );
  };

  const getListMeasurementPointsAsync = async ({
    pageSize = size,
    before,
    after,
    searchByFields = [],
    sortByFields = [],
  }: GetListMeasurementPointsRequest) => {
    const params: GetListMeasurementPointsRequest = {
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
      searchByFields: searchByFields.length > 0 ? searchByFields : [],
      sortByFields: sortByFields.length > 0 ? sortByFields : [],
    };
    await getListMeasurementPoints(
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

  // Sắp xếp thứ tự từ cũ đến mới (Điểm đo tạo cũ nhất hiển thị đầu danh sách)
  const sortedData = [...(data ?? [])].sort((a, b) => {
    const timeA = a.createdTime ? new Date(a.createdTime).getTime() : 0;
    const timeB = b.createdTime ? new Date(b.createdTime).getTime() : 0;
    return timeA - timeB || Number(a.id) - Number(b.id);
  });

  const dataExport = sortedData.map((item, index) => {
    const emails = [item.email, item.email2, item.email3].filter(Boolean).join('; ');
    return {
      index: index + 1,
      code: item.code ?? 'N/A',
      type: item.type ?? 'N/A',
      address: item.address ?? 'N/A',
      description: item.description ?? 'N/A',
      customer: item.customer?.fullName ?? 'N/A',
      emails: emails || '',
      status: item.status ?? 'N/A',
    };
  });

  const debounceCode = useDebounce(dataForm?.code, 300);
  useEffect(() => {
    const searchByFields: any = [];
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

    setGetListMeasurementParamsRequest((prev) => {
      const updatedParams = {
        ...prev,
        searchByFields: searchByFields.length > 0 ? searchByFields : [],
      };
      return deleteParams(updatedParams, ['before', 'after']);
    });
  }, [debounceCode, dataForm?.status]);
  useEffect(() => {
    getListMeasurementPointsAsync({ ...getListMeasurementParamsRequest });
  }, [size, getListMeasurementParamsRequest]);

  return (
    <div>
      <PageTitle title={'Quản lý điểm đo'} />
      <div className='flex justify-between items-center'>
        <h1 className='text-24-28 font-bold'>{_t('Quản lý điểm đo')}</h1>
        <div className='flex gap-1'>
          <PrivateComponent roles={[Constants.Role.SuperAdmin.value, Constants.Role.Admin.value]}>
            <ButtonCommonCustomizable
              title={_t('')}
              classButton='text-button-variant'
              tooltipText='Thêm điểm đo'
              onClick={() => {
                navigate(ROUTE_PATH.ADD_POINT);
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
            disabled={!(data && data.length > 0)}
          />

          <PrivateComponent roles={[Constants.Role.SuperAdmin.value, Constants.Role.Admin.value]}>
            <ButtonCommonCustomizable
              title={''}
              tooltipText={'Nhập từ CSV'}
              onClick={() => {
                setIsShowImport(true);
              }}
              icon={<FiDownload size={20} />}
              classButton='text-button-variant'
            />
          </PrivateComponent>
        </div>
      </div>
      <Card className='mt-5 shadow-custom'>
        <div className='mb-5 flex justify-between items-center'>
          <div className='flex items-center w-2/3 gap-2'>
            <div className='basis-4/5 flex items-center gap-2'>
              <div className='basis-1/2'>
                <InputTextCommon
                  label={null}
                  attribute={'code'}
                  dataAttribute={dataForm?.code}
                  placeholder={_t('Nhập mã điểm đo')}
                  setData={setDataForm}
                  isRequired={false}
                  prefix={<IoSearch />}
                  allowClear={true}
                />
              </div>
              <div className='basis-1/2'>
                <InputSelectCommon
                  label={null}
                  attribute={'status'}
                  dataAttribute={dataForm?.status}
                  placeholder={'Trạng thái'}
                  setData={setDataForm}
                  isRequired={false}
                  allowClear={true}
                  listDataItem={[
                    { label: 'Đang sử dụng', value: Constants.AccountStatus.Active },
                    { label: 'Ngừng sử dụng', value: Constants.AccountStatus.Deactivate },
                  ]}
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
        <div>
          <TablePointManagement
            dataSource={data ?? []}
            setIsShowDialog={setIsShowDialog}
            loading={loading}
          />
        </div>
        <PaginationCommon
          pageSize={size}
          total={paginationData?.totalCount}
          currentCount={data?.length || 0}
          setSize={(newSize) => {
            setSize(newSize);
            setGetListMeasurementParamsRequest((prev) => {
              const updatedParams = { ...prev, pageSize: newSize };
              return deleteParams(updatedParams, ['before', 'after']);
            });
          }}
          hasNext={paginationData?.hasNext}
          hasPrevious={paginationData?.hasPrevious}
          onClickPrevious={() => {
            setGetListMeasurementParamsRequest((prev) => {
              const updatedParams = { ...prev, before: firstPointID };
              return deleteParams(updatedParams, ['after']);
            });
          }}
          onClickNext={() => {
            setGetListMeasurementParamsRequest((prev) => {
              const updatedParams = { ...prev, after: lastPointID };
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
        title='Quản lý điểm đo'
        dataSource={dataExport}
        listHeader={Constants.listHeaderExport.PointManagement}
        fileNameCsv={'diemdo'}
        typeExportAll={'measurement-points'}
        requestBody={getListMeasurementParamsRequest}
      />
      {/* Dialog import file */}
      <DialogImportPoint
        open={isShowImport}
        handleCancel={() => {
          setIsShowImport(false);
        }}
        onSuccess={() => {
          getListMeasurementPointsAsync({ ...getListMeasurementParamsRequest });
        }}
      />
      <DialogCommon
        title={''}
        visible={isShowDialog.status}
        handleCancel={() => {
          setIsShowDialog({
            record: '',
            status: false,
          });
        }}
        handleConfirm={() => {
          updateStatus(isShowDialog.record?.key);
        }}
        isButtonSubmit={true}
        showFooter={true}
      >
        <div className='flex flex-col gap-4 p-2'>
          {isShowDialog.record?.status === Constants.AccountStatus.Active ? (
            <div className='flex flex-col justify-center items-center my-5 gap-2'>
              <p className='text-16-24 font-bold'>{_t('Xác nhận vô hiệu hóa điểm đo?')}</p>
              <p className=''>{_t('Sau khi xác nhận điểm đo sẽ được vô hiệu hóa')}</p>
            </div>
          ) : (
            <div className='flex flex-col justify-center items-center my-5 gap-2'>
              <p className='text-16-24 font-bold'>{_t('Xác nhận kích hoạt điểm đo?')}</p>
              <p className=''>{_t('Sau khi xác nhận điểm đo sẽ được kích hoạt')}</p>
            </div>
          )}
        </div>
      </DialogCommon>
    </div>
  );
};

export default PointManagement;
