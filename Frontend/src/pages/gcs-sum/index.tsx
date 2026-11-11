import { Card, Checkbox } from 'antd';
import { useEffect, useRef, useState } from 'react';
import { FiDownload, FiRefreshCw, FiShare } from 'react-icons/fi';
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
import useExportGcsTemporaryExcelHook from '@/infrastructure/repository/gcs/hooks/useExportGcsTemporaryExcel.hook';
import useFinalMeterReadingHook from '@/infrastructure/repository/gcs/hooks/useFinalMeterReding.hook';
import useUpdateGcsTemporaryHook from '@/infrastructure/repository/gcs/hooks/useUpdateGCSTemporary.hook';
import { deleteParams, formatNumberWithCommas } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';
import DialogGCS from '@/pages/gcs-sum/components/dialog-gcs';
import DialogImportExcel from '@/pages/gcs-sum/components/dialog-import-excel';
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
  const [size, setSize] = useState<number>(24);
  const [GCSName, setGCSName] = useState<any>();
  const [dataReading, setDataReading] = useState<any>({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<any>(null);
  const [gcsParams, setGcsParams] = useState<GcsParams>();
  const [data, setData] = useState<any[]>();
  const [getDataGcsRequest] = useGetDataGcsHook();
  const [exportExcel] = useExportGcsTemporaryExcelHook();
  const [getFinalMeterReadingRequest] = useFinalMeterReadingHook();
  const [updateGcsTemporaryRequest] = useUpdateGcsTemporaryHook();
  const [isSave, setIsSave] = useState(false);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [isShowImportExcel, setIsShowImportExcel] = useState<boolean>(false);
  const [isShowDialog, setIsShowDialog] = useState<any>(false);
  const [isShowDialogSave, setIsShowDialogSave] = useState<any>(false);
  const [isShowDialogEdit, setIsShowDialogEdit] = useState<any>({
    isOpen: false,
    record: null,
    fieldType: 'current',
  });
  const [isManualChange, setIsManualChange] = useState<boolean>(false);

  useEffect(() => {
    if (isShowDialogEdit.isOpen && isShowDialogEdit.record) {
      setIsManualChange(isShowDialogEdit.record.isChange ?? false);
    }
  }, [isShowDialogEdit.isOpen, isShowDialogEdit.record]);
  const [getListDataGCSRequest, setGetListDataGCSRequest] = useState<GetListDataGCSRequest>();
  const [refreshState, setRefreshState] = useState<boolean>(false);
  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
    totalCount?: number;
    totalMeasurementPoints?: number;
    totalSuspiciousCount?: number;
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
      statusFilter: '',
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
        const isPmax = record.meterReadingSet === 'Pmax';
        const rawNum = Number(dataReading.newValueReading);
        const editedValue = isPmax ? Number(rawNum.toFixed(2)) : rawNum;
        const nextNewCurrentReading = isEditCurrent
          ? editedValue
          : record.newCurrentReading ?? record.currentReading;
        const nextNewPreviousReading = isEditCurrent
          ? record.newPreviousReading ?? record.previousReading
          : editedValue;
        const consumption =
          isPmax
            ? null
            : (nextNewCurrentReading - nextNewPreviousReading) * record.multiplierFactor;
        dataEx[indexData] = {
          ...record,
          newCurrentReading: isEditCurrent ? editedValue : record.newCurrentReading,
          newPreviousReading: isEditCurrent ? record.newPreviousReading : editedValue,
          consumption,
          isChange: isManualChange,
          isSuspicious: false,
          suspiciousReason: null,
        };

        // Tính toán lại hệ số Cosphi (PF) trung bình của điểm đo dựa trên sản lượng AP & AQ
        const pointId = record.measurementPointId ?? record.measurementPoint?.id;
        if (pointId !== undefined) {
          const pointRows = dataEx.filter(
            (r) => (r.measurementPointId ?? r.measurementPoint?.id) === pointId
          );
          const sgRow = pointRows.find((r) => r.meterReadingSet === 'SG');
          const vcRow = pointRows.find((r) => r.meterReadingSet === 'VC');
          const btRow = pointRows.find((r) => r.meterReadingSet === 'BT');
          const cdRow = pointRows.find((r) => r.meterReadingSet === 'CD');
          const tdRow = pointRows.find((r) => r.meterReadingSet === 'TD');

          let apVal = sgRow?.consumption ?? null;
          if (
            (apVal === null || apVal <= 0) &&
            (btRow?.consumption != null || cdRow?.consumption != null || tdRow?.consumption != null)
          ) {
            const sumTariff =
              (btRow?.consumption ?? 0) + (cdRow?.consumption ?? 0) + (tdRow?.consumption ?? 0);
            if (sumTariff > 0 || apVal === null) apVal = sumTariff;
          }
          const aqVal = vcRow?.consumption ?? 0;
          let avgCosPhi: number | null = null;
          if (apVal !== null && apVal >= 0) {
            const q = Math.max(0, aqVal);
            const s = Math.sqrt(apVal * apVal + q * q);
            avgCosPhi = s > 0 ? Number((apVal / s).toFixed(2)) : 0;
          }
          dataEx.forEach((r, idx) => {
            if ((r.measurementPointId ?? r.measurementPoint?.id) === pointId) {
              dataEx[idx] = { ...dataEx[idx], cosPhi: avgCosPhi };
            }
          });
        }
      }
      setData(dataEx);
      const requestPayload =
        isShowDialogEdit.fieldType === 'current'
          ? {
              id: record.id,
              newCurrentReading: editedValue,
              isChange: isManualChange,
            }
          : {
              id: record.id,
              newPreviousReading: editedValue,
              isChange: isManualChange,
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
    pageNumber,
    searchByFields = [],
    sortByFields = [],
    gcsId,
    periodNumber,
    month,
    year,
    quanlity,
    isSuspicious,
  }: GetListDataGCSRequest) => {
    const params: GetListDataGCSRequest = {
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
      ...(pageNumber && { pageNumber }),
      ...(quanlity !== undefined && { quanlity }),
      ...(isSuspicious !== undefined && { isSuspicious }),
      searchByFields: searchByFields.length > 0 ? searchByFields : [],
      sortByFields: sortByFields.length > 0 ? sortByFields : [],
      gcsId: gcsId ?? 0,
      periodNumber: periodNumber ?? 0,
      month: month ?? 0,
      year: year ?? 0,
      isRefresh: refreshState,
    };
    if (refreshState) {
      setRefreshState(false);
    }
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
          totalCount: data.totalCount,
          totalMeasurementPoints: data.totalMeasurementPoints,
          totalSuspiciousCount: data.totalSuspiciousCount,
        });
      },
      () => {},
      setLoading
    );
  };

  const handleExportExcel = () => {
    if (!gcsParams) return;
    exportExcel(
      {
        gcsId: gcsParams.gcsId,
        month: gcsParams.month,
        year: gcsParams.year,
        periodNumber: gcsParams.periodNumber,
        onlySuspicious: false,
      },
      () => {
        notifySuccess('Tải file Excel thành công!');
      },
      (err) => {
        notifyError(err, 'Lỗi khi tải file Excel!');
      },
      setLoading
    );
  };

  const dataExport = data?.map((item) => {
    const isPmax = item.meterReadingSet === 'Pmax';
    const prevVal = item.newPreviousReading ?? item.previousReading;
    const currVal = item.newCurrentReading ?? item.currentReading;
    return {
      measurementPoint: item.measurementPoint.code ?? 'N/A',
      description: item.measurementPoint.description ?? '',
      customer: item.measurementPoint.customer.fullName ?? 'N/A',
      meterReadingSet: item.meterReadingSet ?? 'N/A',
      previousReading:
        prevVal != null
          ? formatNumberWithCommas(prevVal, isPmax ? 2 : undefined)
          : '????',
      isChange: item.newCurrentReading != null || item.newPreviousReading != null,
      currentReading:
        currVal != null
          ? formatNumberWithCommas(currVal, isPmax ? 2 : undefined)
          : '????',
      multiplierFactor: isPmax ? '' : (item.multiplierFactor ?? '????'),
      consumption:
        isPmax
          ? 'N/A'
          : item.consumption != null
          ? formatNumberWithCommas(item.consumption)
          : (currVal != null ? formatNumberWithCommas(item.consumption) : 'N/A'),
    };
  });
  const prevFilterKeyRef = useRef<string>('');
  useEffect(() => {
    if (gcsParams) {
      const filterKey = JSON.stringify([
        gcsParams.gcsId,
        gcsParams.periodNumber,
        gcsParams.month,
        gcsParams.year,
        debounceMeasurementPointId,
        debounceFullName,
        dataForm?.statusFilter,
      ]);
      if (prevFilterKeyRef.current !== filterKey) {
        prevFilterKeyRef.current = filterKey;
        if (
          getListDataGCSRequest?.before ||
          getListDataGCSRequest?.after ||
          getListDataGCSRequest?.pageNumber
        ) {
          setGetListDataGCSRequest((prev) =>
            deleteParams({ ...prev }, ['before', 'after', 'pageNumber'])
          );
          return;
        }
      }

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

      const params: any = {
        ...getListDataGCSRequest,
        ...gcsParams,
        searchByFields,
      };

      if (dataForm?.statusFilter === 'suspicious') {
        params.isSuspicious = true;
      } else if (dataForm?.statusFilter === 'valid') {
        params.isSuspicious = false;
      }

      getListDataGcsAsync(params);
    }
  }, [
    gcsParams,
    debounceMeasurementPointId,
    debounceFullName,
    dataForm?.statusFilter,
    size,
    getListDataGCSRequest,
  ]);

  const suspiciousCount = paginationData?.totalSuspiciousCount ?? 0;

  useEffect(() => {
    if (!isShowDialogEdit.isOpen || !isShowDialogEdit.record) {
      return;
    }

    const val =
      isShowDialogEdit.fieldType === 'previous'
        ? isShowDialogEdit.record.newPreviousReading ?? isShowDialogEdit.record.previousReading
        : isShowDialogEdit.record.newCurrentReading ?? isShowDialogEdit.record.currentReading;
    const isPmax = isShowDialogEdit.record.meterReadingSet === 'Pmax';

    setDataReading({
      newValueReading:
        val != null
          ? isPmax
            ? Number(val).toFixed(2)
            : val
          : '',
    });
  }, [isShowDialogEdit]);

  const periodDate = GCSName ? GCSName.nameGcs.dayPeriod[GCSName?.periodNumber - 1] : '1';
  return (
    <div>
      <PageTitle title={'Ghi chỉ số'} />
      <div className='flex justify-between items-center'>
        {data ? (
          <BreadcrumbUI
            items={[
              { name: 'Ghi chỉ số' },
              {
                name: `${GCSName ? GCSName.nameGcs['name'] : 'Sổ ghi'} - Kỳ ${GCSName?.periodNumber ?? 1} Tháng ${GCSName?.month}/${GCSName?.year}`,
              },
            ]}
          />
        ) : (
          <h1 className='text-24-28 font-bold'>{_t('Ghi chỉ số')}</h1>
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
              tooltipText={
                suspiciousCount > 0
                  ? `Còn ${suspiciousCount} số liệu nghi ngờ chưa hiệu chỉnh (Không thể chốt sổ)`
                  : 'Chốt sổ'
              }
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
                handleExportExcel();
              }}
              icon={<FiShare size={20} />}
              disabled={!(data && data.length > 0)}
            />
          )}
          {data && (
            <ButtonCommonCustomizable
              title={_t('')}
              classButton='text-button-variant'
              tooltipText='Nhập file'
              onClick={() => {
                setIsShowImportExcel(true);
              }}
              icon={<FiDownload size={20} />}
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
            </div>

            <div ref={tableRef}>
              <TableGcs
                dataSource={data ?? []}
                setIsShowDialogEdit={setIsShowDialogEdit}
                loading={loading}
              />
            </div>
            <PaginationCommon
              pageSize={size}
              pageSizeOptions={['24', '48', '96', '192']}
              total={paginationData?.totalCount}
              currentCount={data?.length || 0}
              hasNext={paginationData?.hasNext}
              setSize={(newSize) => {
                setSize(newSize);
                setGetListDataGCSRequest((prev) => {
                  const updatedParams = { ...prev, pageSize: newSize };
                  return deleteParams(updatedParams, ['before', 'after', 'pageNumber']);
                });
              }}
              hasPrevious={paginationData?.hasPrevious}
              onClickPrevious={() => {
                setGetListDataGCSRequest((prev) => {
                  const updatedParams = { ...prev, before: firstPointID };
                  return deleteParams(updatedParams, ['after', 'pageNumber']);
                });
              }}
              onClickNext={() => {
                setGetListDataGCSRequest((prev) => {
                  const updatedParams = { ...prev, after: lastPointID };
                  return deleteParams(updatedParams, ['before', 'pageNumber']);
                });
              }}
              onChangePage={(page) => {
                setGetListDataGCSRequest((prev) => {
                  const updatedParams = { ...prev, pageNumber: page };
                  return deleteParams(updatedParams, ['before', 'after']);
                });
              }}
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
      {/* Dialog export file */}
      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        title={`${GCSName ? GCSName.nameGcs['name'] : 'Sổ ghi'} Tháng ${GCSName?.month}/${GCSName?.year} kì ${GCSName?.periodNumber} (0:00 [ngày ${periodDate}])`}
        dataSource={dataExport}
        listHeader={Constants.listHeaderExport.GcsSum}
        fileNameCsv={`MDMS_SoGCS_${(GCSName ? GCSName.nameGcs['name'] : 'SoGCS').replace(/[\\/:*?"<>|]/g, '_')}_Ky${GCSName?.periodNumber}_T${String(GCSName?.month).padStart(2, '0')}_${GCSName?.year}`}
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
          setIsShowDialogSave(false);
        }}
        handleConfirm={finalMeterReadingAsync}
        isButtonSubmit={suspiciousCount === 0}
        disableSubmitButton={suspiciousCount > 0}
        showFooter={true}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-6 gap-2'>
            <p className='text-20-20 font-bold'>{'Xác nhận lưu thông tin sổ GCS'}</p>
            {suspiciousCount === 0 ? (
              <p className='text-16-16'>{`Sau khi xác nhận sẽ không thể sửa thông tin đã lưu.`}</p>
            ) : (
              <div className='mt-3 p-4 bg-red-50 border border-red-300 rounded-lg text-red-900 text-sm text-center font-medium space-y-1'>
                <div className='text-base font-bold text-red-700 flex items-center justify-center gap-1'>
                  ⛔ Không thể xác nhận chốt sổ!
                </div>
                <div>
                  Hiện còn <strong>{suspiciousCount}</strong> bản ghi có số liệu nghi ngờ hoặc chưa cập nhật chỉ số mới.
                </div>
                <div className='text-xs text-red-600 mt-1'>
                  Người dùng chỉ được xác nhận chốt sổ nếu không còn số liệu nghi ngờ (sau khi đã nhập liệu thủ công hoặc Import file Excel có chuyển màu sang <strong>Normal</strong>).
                </div>
              </div>
            )}
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
          <div className='flex items-center gap-2 mt-2 pt-1'>
            <Checkbox
              checked={isManualChange}
              onChange={(e) => setIsManualChange(e.target.checked)}
            >
              <span className='text-sm text-gray-700 font-medium'>
                Đánh dấu đã hiệu chỉnh thủ công (ký hiệu <b>M</b>)
              </span>
            </Checkbox>
          </div>
          <div className='text-xs text-gray-400 italic'>
            Bỏ chọn nếu đây là số liệu chốt chuẩn từ công tơ hoặc không muốn gắn ký hiệu 'M' trên báo cáo.
          </div>
        </div>
      </DialogCommon>

      {gcsParams && (
        <DialogImportExcel
          open={isShowImportExcel}
          handleCancel={() => setIsShowImportExcel(false)}
          onSuccess={() => {
            setIsShowImportExcel(false);
            setRefreshState(false);
            if (dataForm?.statusFilter === 'suspicious') {
              setDataForm({
                ...dataForm,
                statusFilter: '',
              });
            }
            setGetListDataGCSRequest((prev) => {
              const updated = {
                ...prev,
                gcsId: gcsParams?.gcsId ?? 0,
                periodNumber: gcsParams?.periodNumber ?? 0,
                month: gcsParams?.month ?? 0,
                year: gcsParams?.year ?? 0,
              };
              return deleteParams(updated, ['before', 'after', 'pageNumber']);
            });
          }}
          gcsParams={gcsParams}
          onDownloadTemplate={handleExportExcel}
        />
      )}
    </div>
  );
};

export default GCSSum;
