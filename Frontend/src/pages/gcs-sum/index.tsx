import { Card } from 'antd';
import { useEffect, useRef, useState } from 'react';
import { FiRefreshCw, FiShare } from 'react-icons/fi';
import { LuSave } from 'react-icons/lu';
import { TbBook2 } from 'react-icons/tb';
import { useNavigate } from 'react-router-dom';

import bgEmpty from '@/assets/bg-empty.svg';
import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants';
import { GetListDataGCSRequest } from '@/core/application/dto/IoT/requests/GetDataGcsRequest';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { FinalMeterReadingRequest } from '@/core/application/dto/gcs/request/FinalMeterReadingRequest';
import { UpdateGCSTemporaryRequest } from '@/core/application/dto/gcs/request/UpdateGCSTemporaryRequest';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputNumberDialogCommon from '@/infrastructure/common/components/input/input-number-dialog-common';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifyError, notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetDataGcsHook from '@/infrastructure/repository/IoT/hooks/useGetDataGcs.hook';
import useFinalMeterReadingHook from '@/infrastructure/repository/gcs/hooks/useFinalMeterReding.hook';
import useUpdateGcsTemporaryHook from '@/infrastructure/repository/gcs/hooks/useUpdateGCSTemporary.hook';
import { deleteParams, formatNumberWithCommas } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';
import DialogGCS from '@/pages/gcs-sum/components/dialog-gcs';
import SearchGCS from '@/pages/gcs-sum/components/search-gcs';
import TableGcs from '@/pages/gcs-sum/components/table-gcs';

interface GcsParams {
  gcsId: number;
  periodNumber: number;
  month: number;
  year: number;
  nameGcs: any;
}

const GCSSum = () => {
  const tableRef = useRef(null);
  const navigate = useNavigate();
  const [dataForm, setDataForm] = useStateDataForm({});
  const [loading, setLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(10);
  const [GCSName, setGCSName] = useState<any>();
  const [dataReading, setDataReading] = useState<any>({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<any>(null);
  const [gcsParams, setGcsParams] = useState<GcsParams>();
  const [data, setData] = useState<any[]>();
  const [getDataGcsRequest] = useGetDataGcsHook();
  const [getFinalMeterReadingRequest] = useFinalMeterReadingHook();
  const [updateGcsTemporaryRequest] = useUpdateGcsTemporaryHook();
  const [isSave, setIsSave] = useState(false);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [isShowDialog, setIsShowDialog] = useState<any>(false);
  const [isShowDialogSave, setIsShowDialogSave] = useState<any>(false);
  const [isShowDialogEdit, setIsShowDialogEdit] = useState<any>({
    isOpen: false,
    record: null,
    fieldType: 'current',
  });
  const [getListDataGCSRequest, setGetListDataGCSRequest] = useState<GetListDataGCSRequest>();
  const [refreshState, setRefreshState] = useState<boolean>(true);
  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
  }>({ hasNext: false, hasPrevious: false });
  const firstPointID = data?.[0]?.id;
  const lastPointID = data?.[data.length - 1]?.id;

  const debounceMeasurementPointId = useDebounce(dataForm?.measurementPointId, 300);
  const debounceFullName = useDebounce(dataForm?.fullName, 300);

  const isValidData = () => {
    let allRequestOK = true;
    setValidate({ ...validate });
    Object.values(validate).forEach((it: any) => {
      if (it.isError === true) {
        allRequestOK = false;
      }
    });

    return allRequestOK;
  };
  const handleClearSearch = () => {
    setDataForm({
      measurementPointId: '',
      fullName: '',
      meterReadingSet: '',
      quanlity: '',
    });
    setGetListDataGCSRequest({
      gcsId: gcsParams?.gcsId ?? 0,
      periodNumber: gcsParams?.periodNumber ?? 0,
      month: gcsParams?.month ?? 0,
      year: gcsParams?.year ?? 0,
    });
  };

  const handleCancel = () => {
    setIsShowDialog(false);
  };

  const handelSubmit = (params: GcsParams) => {
    setGcsParams(params);
    setIsSave(false);
  };

  const updateGcsTemporaryAsync = async (record: any) => {
    setSubmittedTime(new Date());
    const hasNewValue =
      dataReading.newValueReading !== undefined &&
      dataReading.newValueReading !== null &&
      dataReading.newValueReading !== '';
    if (isValidData() && hasNewValue) {
      const dataEx = [...(data ?? [])];
      const indexData = data?.findIndex((item) => item.id === record.id);
      if (indexData !== undefined && indexData !== -1) {
        const isEditCurrent = isShowDialogEdit.fieldType === 'current';
        const editedValue = Number(dataReading.newValueReading);
        const nextNewCurrentReading = isEditCurrent
          ? editedValue
          : record.newCurrentReading ?? record.currentReading;
        const nextNewPreviousReading = isEditCurrent
          ? record.newPreviousReading ?? record.previousReading
          : editedValue;
        const consumption =
          record.meterReadingSet === 'Pmax'
            ? null
            : (nextNewCurrentReading - nextNewPreviousReading) * record.multiplierFactor;
        dataEx[indexData] = {
          ...record,
          newCurrentReading: isEditCurrent ? editedValue : record.newCurrentReading,
          newPreviousReading: isEditCurrent ? record.newPreviousReading : editedValue,
          consumption,
        };
      }
      setData(dataEx);
      const requestPayload =
        isShowDialogEdit.fieldType === 'current'
          ? {
              id: record.id,
              newCurrentReading: Number(dataReading.newValueReading),
            }
          : {
              id: record.id,
              newPreviousReading: Number(dataReading.newValueReading),
            };
      const params = new UpdateGCSTemporaryRequest({
        gcsTemporaryRequests: [requestPayload],
        gcsId: gcsParams?.gcsId ?? 0,
        month: gcsParams?.month ?? 0,
        year: gcsParams?.year ?? 0,
        periodNumber: gcsParams?.periodNumber ?? 0,
      });
      await updateGcsTemporaryRequest(
        params,
        () => {
          setIsShowDialogEdit({ isOpen: false, record: null, fieldType: 'current' });
          setDataReading({});
        },
        () => {
          notifyError('', _t('Sửa không thành công'));
        },
        setLoading
      );
      setSubmittedTime(null);
    }
  };

  const finalMeterReadingAsync = async () => {
    setIsShowDialogSave(false);
    const params = new FinalMeterReadingRequest({
      gcsId: gcsParams?.gcsId ?? 0,
      month: gcsParams?.month ?? 0,
      year: gcsParams?.year ?? 0,
      periodNumber: gcsParams?.periodNumber ?? 0,
    });
    await getFinalMeterReadingRequest(
      params,
      (res: any) => {
        const { data } = res;
        notifySuccess('', 'Đã chốt chỉ số thành công');
        setIsSave(true);
        if (gcsParams) {
          navigate(ROUTE_PATH.GCS_CHECK_DETAIL, {
            state: {
              record: {
                id: data.id,
                month: gcsParams?.month,
                year: gcsParams?.year,
                periodNumber: gcsParams?.periodNumber,
              },
              GCSName: GCSName.nameGcs['name'],
              gcs: GCSName,
            },
          });
        }
      },
      () => {},
      setLoading
    );
  };
  const getListDataGcsAsync = async ({
    pageSize = size,
    before,
    after,
    searchByFields = [],
    sortByFields = [],
    gcsId,
    periodNumber,
    month,
    year,
    quanlity,
  }: GetListDataGCSRequest) => {
    const params: GetListDataGCSRequest = {
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
      ...(!quanlity && { quanlity }),
      searchByFields: searchByFields.length > 0 ? searchByFields : [],
      sortByFields: sortByFields.length > 0 ? sortByFields : [],
      gcsId: gcsId ?? 0,
      periodNumber: periodNumber ?? 0,
      month: month ?? 0,
      year: year ?? 0,
      isRefresh: refreshState,
    };
    await getDataGcsRequest(
      params,
      (res: DataResponse) => {
        const { data } = res;
        const result = data?.data;
        handleCancel();
        setData(result);
        setGCSName(gcsParams);
        setPaginationData({
          hasNext: data.hasNext,
          hasPrevious: data.hasPrevious,
        });
      },
      () => {},
      setLoading
    );
  };

  const dataExport = data?.map((item) => {
    return {
      measurementPoint: item.measurementPoint.code ?? 'N/A',
      description: item.measurementPoint.description ?? '',
      customer: item.measurementPoint.customer.fullName ?? 'N/A',
      meterReadingSet: item.meterReadingSet ?? 'N/A',
      previousReading:
        formatNumberWithCommas(item.newPreviousReading ?? item.previousReading) ?? '????',
      isChange: item.newCurrentReading != null || item.newPreviousReading != null,
      currentReading:
        item.newCurrentReading ?? item.currentReading
          ? formatNumberWithCommas(item.newCurrentReading ?? item.currentReading)
          : '????',
      multiplierFactor: item.multiplierFactor ?? '????',
      consumption:
        item.currentReading || item.newCurrentReading
          ? formatNumberWithCommas(item.consumption)
          : 'N/A',
    };
  });
  useEffect(() => {
    if (gcsParams) {
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

      const params = {
        ...getListDataGCSRequest,
        ...gcsParams,
        searchByFields,
        ...(dataForm?.quanlity && { quanlity: false }),
      };

      getListDataGcsAsync(params);
    }
  }, [
    gcsParams,
    debounceMeasurementPointId,
    debounceFullName,
    dataForm?.quanlity,
    size,
    getListDataGCSRequest,
  ]);

  useEffect(() => {
    if (!isShowDialogEdit.isOpen || !isShowDialogEdit.record) {
      return;
    }

    const initValue =
      isShowDialogEdit.fieldType === 'previous'
        ? isShowDialogEdit.record.newPreviousReading ?? isShowDialogEdit.record.previousReading
        : isShowDialogEdit.record.newCurrentReading ?? isShowDialogEdit.record.currentReading;

    setDataReading({ newValueReading: initValue });
  }, [isShowDialogEdit]);

  const periodDate = GCSName ? GCSName.nameGcs.dayPeriod[GCSName?.periodNumber - 1] : '1';
  return (
    <div>
      <PageTitle title={'Ghi chỉ số'} />
      <div className='flex justify-between items-center'>
        {data ? (
          <BreadcrumbUI
            items={[
              { name: 'Chốt tháng mới' },
              {
                name: `${GCSName ? GCSName.nameGcs['name'] : 'Sổ ghi'} Tháng ${GCSName?.month}/${GCSName?.year} kì ${GCSName?.periodNumber} (0:00 [ngày ${periodDate == 999 ? 'cuối tháng' : periodDate}])`,
              },
            ]}
          />
        ) : (
          <h1 className='text-24-28 font-bold'>{_t('Chốt tháng mới')}</h1>
        )}
        <div className='flex gap-2'>
          {data && (
            <ButtonCommonCustomizable
              title={_t('')}
              tooltipText={'Làm mới'}
              icon={<FiRefreshCw size={20} />}
              classButton='text-button-variant'
              onClick={() => {
                setRefreshState(true);
                setGetListDataGCSRequest({
                  ...getListDataGCSRequest,
                  gcsId: gcsParams?.gcsId ?? 0,
                  periodNumber: gcsParams?.periodNumber ?? 0,
                  month: gcsParams?.month ?? 0,
                  year: gcsParams?.year ?? 0,
                });
              }}
              disabled={false}
            />
          )}
          <ButtonCommonCustomizable
            title={''}
            tooltipText={'Chọn sổ'}
            classButton='text-button-variant'
            onClick={() => {
              setIsShowDialog(true);
            }}
            icon={<TbBook2 size={25} />}
          />
          {data && (
            <ButtonCommonCustomizable
              title={''}
              classButton='text-button-variant'
              tooltipText={'Chốt sổ'}
              disabled={isSave}
              onClick={() => {
                setIsShowDialogSave(true);
              }}
              icon={<LuSave size={20} />}
            />
          )}
          {data && (
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
          )}
        </div>
      </div>
      {data ? (
        <div>
          <Card className='mt-5 shadow-custom'>
            <div className='mb-5 flex justify-between'>
              {/*<div>*/}
              {/*  <h2 className={'text-24-28 font-bold'}>*/}
              {/*    {`${GCSName ? GCSName['name'] : 'Sổ ghi'} Tháng ${gcsParams?.month}/${gcsParams?.year} kì ${gcsParams?.periodNumber}`}*/}
              {/*  </h2>*/}
              {/*</div>*/}
              <SearchGCS
                dataForm={dataForm}
                setDataForm={setDataForm}
                handleClearSearch={handleClearSearch}
                setRefreshState={setRefreshState}
              />
              <PaginationCommon
                pageSize={size}
                hasNext={paginationData?.hasNext}
                setSize={(newSize) => {
                  setSize(newSize);
                  setGetListDataGCSRequest((prev) => {
                    const updatedParams = { ...prev, pageSize: newSize };
                    return deleteParams(updatedParams, ['before', 'after']);
                  });
                }}
                hasPrevious={paginationData?.hasPrevious}
                onClickPrevious={() => {
                  setGetListDataGCSRequest((prev) => {
                    const updatedParams = { ...prev, before: firstPointID };
                    return deleteParams(updatedParams, ['after']); // Remove 'after' parameter
                  });
                }}
                onClickNext={() => {
                  setGetListDataGCSRequest((prev) => {
                    const updatedParams = { ...prev, after: lastPointID };
                    return deleteParams(updatedParams, ['before']); // Remove 'before' parameter
                  });
                }}
              />
            </div>
            <div ref={tableRef}>
              <TableGcs
                dataSource={data ?? []}
                setIsShowDialogEdit={setIsShowDialogEdit}
                loading={loading}
              />
            </div>
          </Card>
        </div>
      ) : (
        <Card className='mt-5'>
          <div className='flex justify-center'>
            <img src={bgEmpty} alt={''} />
          </div>
        </Card>
      )}
      {/* Dialog export file */}
      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        title={`${GCSName ? GCSName.nameGcs['name'] : 'Sổ ghi'} Tháng ${GCSName?.month}/${GCSName?.year} kì ${GCSName?.periodNumber} (0:00 [ngày ${periodDate}])`}
        dataSource={dataExport}
        listHeader={Constants.listHeaderExport.GcsSum}
        fileNameCsv={'ghichiso'}
        typeExportAll={'gcs'}
        subTypeExportAll={'gcs-final'}
        requestBody={gcsParams}
      />

      <DialogGCS
        open={isShowDialog}
        handleCancel={handleCancel}
        handelSubmit={handelSubmit}
        loading={loading}
      />
      <DialogCommon
        title={''}
        visible={isShowDialogSave}
        handleCancel={() => {
          setIsShowDialogSave(!isShowDialogSave);
        }}
        handleConfirm={finalMeterReadingAsync}
        isButtonSubmit={true}
        showFooter={true}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-10 gap-2'>
            <p className='text-20-20 font-bold'>{'Xác nhận lưu thông tin sổ GCS'}</p>
            <p className='text-16-16'>{`Sau khi xác nhận sẽ không thể sửa thông tin đã lưu.`}</p>
          </div>
        </div>
      </DialogCommon>
      <DialogCommon
        title={''}
        visible={isShowDialogEdit.isOpen}
        handleCancel={() => {
          setIsShowDialogEdit({ ...isShowDialogEdit, isOpen: false, fieldType: 'current' });
          setValidate({});
          setDataReading({});
        }}
        handleConfirm={() => {
          updateGcsTemporaryAsync(isShowDialogEdit.record);
        }}
        isButtonSubmit={true}
        showFooter={true}
      >
        <div className='flex flex-col gap-4 mb-4'>
          <div className='flex flex-row justify-center items-center gap-2'>
            <p className='text-20-20 font-bold'>{`Sửa ${
              isShowDialogEdit.fieldType === 'previous' ? 'chỉ số cũ' : 'chỉ số mới'
            } ${isShowDialogEdit.record?.meterReadingSet}`}</p>
            <p className='text-16-16 text-red-600'>*</p>
          </div>
          <InputNumberDialogCommon
            label={null}
            isLabel={false}
            dataAttribute={dataReading['newValueReading']}
            attribute={'newValueReading'}
            setData={setDataReading}
            placeholder={_t('')}
            isRequired={true}
            autoComplete='off'
            autoFocus={true}
            validate={validate}
            setValidate={setValidate}
            submittedTime={submittedTime}
            disabled={false}
            isInteger={false}
          />
        </div>
      </DialogCommon>
    </div>
  );
};

export default GCSSum;
