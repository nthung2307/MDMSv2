import { Card } from 'antd';
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-expect-error
import html2pdf from 'html2pdf.js';
import { useEffect, useMemo, useRef, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { IoMailOutline } from 'react-icons/io5';
import { IoPrintOutline } from 'react-icons/io5';
import { IoPaperPlaneOutline } from 'react-icons/io5';
import { useLocation } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListNoticeRequest } from '@/core/application/dto/notice/request/GetListNoticeRequest';
import { SendAllMailNoticeRequest } from '@/core/application/dto/notice/request/SendAllMailRequest';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListNoticeHook from '@/infrastructure/repository/notice/hooks/useCreateGCS.hook';
import useGetStatusSendMailHook from '@/infrastructure/repository/notice/hooks/useGetStatusSendMail.hook';
import useSendAllMailNoticeHook from '@/infrastructure/repository/notice/hooks/useSendAllMailNotice.hook';
import useSendMailNoticeHook from '@/infrastructure/repository/notice/hooks/useSendMailNotice.hook';
import { deleteParams } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';
import DialogFormPDF from '@/pages/notice-management/components/dialog/dialog-form-pdf';
import TemplateNotice from '@/pages/notice-management/components/form-pdf';
import SearchNotice from '@/pages/notice-management/components/search/search-notice';
import TableDetailNotice from '@/pages/notice-management/components/table/table-notice';

const NoticeDetail = () => {
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  const queryGcsId = searchParams.get('gcsId');
  const cycleId = searchParams.get('cycleId');

  const noticeState = useMemo(() => {
    if (location.state && location.state.record) {
      return location.state;
    }
    const storageKey =
      queryGcsId && cycleId ? `notice_detail_${queryGcsId}_${cycleId}` : 'notice_detail_state';
    try {
      const raw =
        sessionStorage.getItem(storageKey) ||
        localStorage.getItem(storageKey) ||
        sessionStorage.getItem('notice_detail_state') ||
        localStorage.getItem('notice_detail_state');
      if (raw) {
        return JSON.parse(raw);
      }
    } catch {
      // Ignore parse error
    }
    return {};
  }, [location.state, queryGcsId, cycleId]);

  const { record, GCSName, gcsId, breadcumbDate } = noticeState;
  const [dataForm, setDataForm] = useStateDataForm({});
  const [isShowDialogPDF, setIsShowDialogPDF] = useState<any>({ status: false, record: null });
  const [loading, setLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(10);
  const [sendData, setSendData] = useState<any>({
    isAll: false,
    listSend: [],
  });
  const [data, setData] = useState<any[]>([]);
  const [getListNotice] = useGetListNoticeHook();
  const [getStatusSendMail] = useGetStatusSendMailHook();
  const [sendMailNotice] = useSendMailNoticeHook();
  const [sendAllMailNotice] = useSendAllMailNoticeHook();
  const [isShowDialog, setIsShowDialog] = useState<any>(false);
  const [mailStatus, setMailStatus] = useState<'confirm' | 'sending' | 'success' | null>(null);
  const isSendingMailRef = useRef<boolean>(false);
  const [getListNoticeRequest, setGetListNoticeRequest] = useState<GetListNoticeRequest>();
  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
  }>({ hasNext: false, hasPrevious: false });
  const firstPointID = data?.[0]?.id;
  const lastPointID = data?.[data.length - 1]?.id;

  const debounceCustomerCode = useDebounce(dataForm?.customerCode, 300);
  const debounceMeasurementPoint = useDebounce(dataForm?.measurementPoint, 300);

  const handleClearSearch = () => {
    setDataForm({
      customerCode: '',
      measurementPoint: '',
      status: undefined,
    });
    setGetListNoticeRequest({ gcsId: gcsId });
  };

  useEffect(() => {
    const searchByFields: any = [];
    if (debounceCustomerCode) {
      searchByFields.push({
        searchFieldName: 'measurementPoint.customer.customerCode',
        searchValue: debounceCustomerCode,
      });
    }
    if (debounceMeasurementPoint) {
      searchByFields.push({
        searchFieldName: 'measurementPoint.code',
        searchValue: debounceMeasurementPoint,
      });
    }
    if (dataForm?.status) {
      searchByFields.push({
        searchFieldName: 'status',
        searchValue: dataForm.status,
      });
    }

    setGetListNoticeRequest((prev) => {
      const updatedParams = {
        ...prev,
        searchByFields,
        gcsId: prev?.gcsId ?? gcsId,
      };
      return deleteParams(updatedParams, ['before', 'after']);
    });
  }, [debounceCustomerCode, debounceMeasurementPoint, dataForm?.status]);

  // Print Notice
  const printListTemplate = () => {
    const tempDiv = document.createElement('div');
    const root = createRoot(tempDiv);
    root.render(
      <div>
        {sendData.listSend.map((record: any, index: number) => (
          <div key={index} style={{ pageBreakAfter: 'always' }}>
            <TemplateNotice record={record} />
          </div>
        ))}
      </div>
    );
    let opt = {
      margin: 0,
      filename: 'phiếu báo.pdf',
      image: { type: 'jpeg', quality: 0.98 },
      html2canvas: { scale: 2 },
      jsPDF: { unit: 'in', format: 'a5', orientation: 'landscape' },
    };

    html2pdf()
      .set(opt)
      .from(tempDiv)
      .toPdf()
      .get('pdf')
      .then((pdf: any) => {
        const pdfBlobUrl = pdf.output('bloburl');
        const printWindow = window.open(pdfBlobUrl, '_blank');
        if (printWindow) {
          printWindow.onload = () => {
            setTimeout(() => {
              printWindow.print();
            }, 500);
          };
        }
      });

    setTimeout(() => {
      root.unmount();
    }, 0);
  };

  const getListNoticeAsync = async ({
    pageSize = size,
    before,
    after,
    searchByFields = [],
    sortByFields = [],
    gcsId,
    month,
    year,
    periodNumber,
  }: GetListNoticeRequest) => {
    const params: GetListNoticeRequest = {
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
      ...(month && { month }),
      ...(year && { year }),
      searchByFields: searchByFields.length > 0 ? searchByFields : [],
      sortByFields: sortByFields.length > 0 ? sortByFields : [],
      gcsId: gcsId,
      month: month,
      year: year,
      periodNumber: periodNumber,
    };
    await getListNotice(
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

  const sendMailNoticeAsync = async () => {
    setMailStatus('sending');
    isSendingMailRef.current = true;
    const params = new SendAllMailNoticeRequest({
      gcsId: Number(gcsId),
      month: record.month,
      year: record.year,
      periodNumber: record.periodNumber,
    });

    const onSuccess = () => {
      setIsShowDialog(false);
      setMailStatus(null);
      setSendData({ isAll: false, listSend: [] });
      notifySuccess('', _t('Đã gửi yêu cầu thành công, hệ thống đang xử lý...'));

      const refreshParams = {
        ...getListNoticeRequest,
        gcsId: Number(gcsId),
        month: record.month,
        year: record.year,
        periodNumber: record.periodNumber,
      };

      // 1. Tải lại dữ liệu ngay lập tức
      void getListNoticeAsync(refreshParams);

      // 2. Tự động kiểm tra và cập nhật định kỳ mỗi 2 giây (trong 12 giây) để cập nhật trạng thái trực tiếp
      let pollCount = 0;
      const pollTimer = setInterval(() => {
        pollCount++;
        void getListNoticeAsync(refreshParams);
        if (pollCount >= 6) {
          clearInterval(pollTimer);
        }
      }, 2000);
    };

    if (sendData.isAll) {
      await sendAllMailNotice(
        params,
        () => {
          onSuccess();
        },
        () => {},
        () => {}
      );
    } else {
      await sendMailNotice(
        {
          ids: sendData.listSend.map((item: any) => {
            return item.id;
          }),
          gcsId: Number(gcsId),
          month: record.month,
          year: record.year,
          periodNumber: record.periodNumber,
        },
        () => {
          onSuccess();
        },
        () => {},
        () => {}
      );
    }
  };

  const getStatusSendMailFirstAsync = async () => {
    if (!record || !gcsId) return;
    const params: any = {
      GcsId: Number(gcsId) ?? 0,
      month: record.month,
      year: record.year,
      periodNumber: record.periodNumber,
    };
    await getStatusSendMail(
      params,
      (res: DataResponse) => {
        if (res.data) {
          isSendingMailRef.current = true;
          notifySuccess('', 'Đang gửi mail, vui lòng chờ vài phút');
        } else {
          isSendingMailRef.current = false;
          setMailStatus(null);
        }
      },
      () => {},
      () => {}
    );
  };

  const getStatusSendMailAsync = async () => {
    if (!record || !gcsId) return;
    if (!isSendingMailRef.current) return;
    const params: any = {
      GcsId: Number(gcsId) ?? 0,
      month: record.month,
      year: record.year,
      periodNumber: record.periodNumber,
    };
    await getStatusSendMail(
      params,
      (res: DataResponse) => {
        if (!res.data) {
          isSendingMailRef.current = false;
          setMailStatus(null);
          getListNoticeAsync({
            ...getListNoticeRequest,
            gcsId: Number(gcsId),
            month: record.month,
            year: record.year,
            periodNumber: record.periodNumber,
          });
        }
      },
      () => {},
      () => {}
    );
  };

  useEffect(() => {
    if (!record || !gcsId) return;
    void getListNoticeAsync({
      ...getListNoticeRequest,
      gcsId: Number(gcsId),
      month: record.month,
      year: record.year,
      periodNumber: record.periodNumber,
    });
  }, [size, getListNoticeRequest, record?.id, gcsId]);

  useEffect(() => {
    if (!record || !gcsId) return;
    getStatusSendMailFirstAsync();
    const intervalId = setInterval(getStatusSendMailAsync, 30000);

    return () => clearInterval(intervalId);
  }, [record?.id, gcsId]);
  const isValidateData = !(sendData.listSend.length > 0) && !sendData.isAll;

  const renderMailDialogContent = () => {
    switch (mailStatus) {
      case 'sending':
        return (
          <div className='flex flex-col justify-center items-center my-10 gap-2 sending-container'>
            <div className='sending-animation'>
              <div className='plane-wrapper'>
                <IoPaperPlaneOutline size={48} className='text-primary' />
              </div>
              <div className='particles'>
                {[...Array(12)].map((_, i) => (
                  <div key={i} className={`particle particle-${i}`} />
                ))}
              </div>
            </div>
            <p className='text-20-20 font-bold sending-text'>{'Đang gửi email...'}</p>
          </div>
        );
      default:
        return (
          <div className='flex flex-col justify-center items-center my-10 gap-2'>
            <div className='mail-icon-wrapper'>
              <IoMailOutline size={48} className='text-primary' />
            </div>
            <p className='text-20-20 font-bold mail-title'>{'Xác nhận gửi email kèm phiếu báo'}</p>
          </div>
        );
    }
  };

  const handleConfirm = () => {
    if (mailStatus === 'confirm') {
      void sendMailNoticeAsync();
    }
  };

  return (
    <div>
      <PageTitle title={'Quản lý phiếu báo'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Quản lý phiếu báo' },
            {
              name: `${GCSName || ''}`,
              link: ROUTE_PATH.NOTICE_MANAGEMENT,
              state: {
                idGcs: gcsId,
                nameGcs: GCSName,
              },
            },
            {
              name: `${breadcumbDate || ''}`,
            },
          ]}
        />
        <div className='flex gap-5'>
          <ButtonCommonCustomizable
            title={''}
            classButton='text-button-variant'
            tooltipText={'Gửi mail'}
            onClick={() => {
              setIsShowDialog(true);
              setMailStatus('confirm');
            }}
            disabled={isValidateData || mailStatus == 'sending'}
            icon={<IoMailOutline size={20} />}
          />
          <ButtonCommonCustomizable
            title={''}
            classButton='text-button-variant'
            tooltipText={'In phiếu báo'}
            onClick={printListTemplate}
            disabled={isValidateData}
            icon={<IoPrintOutline size={20} />}
          />
        </div>
      </div>
      <div>
        <Card className='mt-5 shadow-custom'>
          <div className='mb-5 flex justify-between'>
            <SearchNotice
              dataForm={dataForm}
              setDataForm={setDataForm}
              handleClearSearch={handleClearSearch}
            />
            <PaginationCommon
              pageSize={size}
              hasNext={paginationData?.hasNext}
              setSize={(newSize) => {
                setSize(newSize);
                setGetListNoticeRequest((prev) => {
                  const updatedParams = { ...prev, pageSize: newSize };
                  return deleteParams(updatedParams, ['before', 'after']);
                });
              }}
              hasPrevious={paginationData?.hasPrevious}
              onClickPrevious={() => {
                setGetListNoticeRequest((prev) => {
                  const updatedParams = { ...prev, before: firstPointID };
                  return deleteParams(updatedParams, ['after']);
                });
              }}
              onClickNext={() => {
                setGetListNoticeRequest((prev) => {
                  const updatedParams = { ...prev, after: lastPointID };
                  return deleteParams(updatedParams, ['before']);
                });
              }}
            />
          </div>
          <TableDetailNotice
            sendData={sendData}
            setSendData={setSendData}
            dataSource={data}
            setIsShowDialogPDF={setIsShowDialogPDF}
            loading={loading}
          />
        </Card>
      </div>

      <DialogFormPDF
        open={isShowDialogPDF}
        handleCancel={() => setIsShowDialogPDF({ status: false, record: null })}
      />

      <DialogCommon
        title={''}
        visible={isShowDialog}
        handleCancel={() => {
          setIsShowDialog(false);
        }}
        handleConfirm={handleConfirm}
        isButtonSubmit={mailStatus === 'confirm'}
        showFooter={mailStatus === 'confirm'}
      >
        <div className='mail-dialog-content'>{renderMailDialogContent()}</div>
      </DialogCommon>

      <style>
        {`
          @keyframes slideInDown {
            from {
              transform: translateY(-50px);
              opacity: 0;
            }
            to {
              transform: translateY(0);
              opacity: 1;
            }
          }

          @keyframes bounce {
            0%, 100% {
              transform: translateY(0);
            }
            50% {
              transform: translateY(-10px);
            }
          }

          @keyframes flyPlane {
            0% {
              transform: translate(-50px, 0) rotate(0deg);
            }
            25% {
              transform: translate(-25px, -20px) rotate(15deg);
            }
            50% {
              transform: translate(0, 0) rotate(0deg);
            }
            75% {
              transform: translate(25px, -20px) rotate(-15deg);
            }
            100% {
              transform: translate(50px, 0) rotate(0deg);
            }
          }

          @keyframes scaleIn {
            from {
              transform: scale(0);
              opacity: 0;
            }
            to {
              transform: scale(1);
              opacity: 1;
            }
          }

          @keyframes particleFly {
            0% {
              transform: translate(0, 0) scale(1);
              opacity: 0.8;
            }
            100% {
              transform: translate(var(--tx), var(--ty)) scale(0);
              opacity: 0;
            }
          }

          .sending-container {
            position: relative;
            width: 100%;
            height: 150px;
            overflow: hidden;
          }

          .sending-animation {
            position: relative;
            width: 100%;
            height: 100%;
            display: flex;
            justify-content: center;
            align-items: center;
          }

          .plane-wrapper {
            position: relative;
            animation: flyPlane 3s ease-in-out infinite;
            z-index: 2;
          }

          .particles {
            position: absolute;
            width: 100%;
            height: 100%;
            pointer-events: none;
          }

          .particle {
            position: absolute;
            width: 6px;
            height: 6px;
            border-radius: 50%;
            top: 50%;
            left: 50%;
          }

          ${[...Array(12)]
            .map(
              (_, i) => `
            .particle-${i} {
              --tx: ${Math.random() * 100 - 50}px;
              --ty: ${Math.random() * 100 - 50}px;
              background: ${['#1890ff', '#52c41a', '#722ed1'][i % 3]};
              animation: particleFly 2s ease-out infinite;
              animation-delay: ${i * 0.1}s;
            }
          `
            )
            .join('\n')}

          .sending-text {
            animation: bounce 2s ease-in-out infinite;
          }

          .success-icon-wrapper {
            animation: scaleIn 0.5s ease forwards;
          }

          .mail-title, .success-title {
            opacity: 0;
            animation: slideInDown 0.5s ease forwards;
            animation-delay: 0.2s;
          }

          .mail-icon-wrapper {
            animation: bounce 2s infinite;
          }
        `}
      </style>
    </div>
  );
};

export default NoticeDetail;
