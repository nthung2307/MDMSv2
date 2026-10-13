import { Col, Row, Tooltip } from 'antd';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { useRecoilValue } from 'recoil';

import { IconAvatar } from '@/assets/icons';
import { IconFluctuation } from '@/assets/icons';
import { IconShare } from '@/assets/icons';
import { AlertState } from '@/core/application/common/atoms/global/alertState';
import { CountState } from '@/core/application/common/atoms/global/countState';
import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetMeasurementPointMonitoringRequest } from '@/core/application/dto/measurement-point-monitoring/requests/GetMeasurementPointMonitoringRequest';
import CollapseCommon from '@/infrastructure/common/components/collapse/collapse';
// import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import SocketDetail from '@/infrastructure/common/components/websocket/socket-io-detail';
import useGetMeasurementPointMonitoringHook from '@/infrastructure/repository/measurement-point-monitoring/hooks/useGetMeasurementPointMonitoring.hook';
import {
  convertDateTime,
  formatNumberWithCommas,
  renderTextNumber,
} from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';

dayjs.extend(utc);
interface DataUser {
  customerName: string;
  customerCode: string;
  measurementPointCode: string;
  currentDateTime: string;
  matKetNoi: string;
  currentDate: string;
  currentMonth: string;
  currentYear: string;
  currentTime: string;
  currentMinute: string;
  currentSecond: string;
  [key: string]: any;
}

const PointMonitoringDetail = () => {
  const { id } = useParams();
  const [getMeasurementPointMonitoring] = useGetMeasurementPointMonitoringHook();
  // const [loading, setLoading] = useState<boolean>(false);
  const [dataUser, setDataUser] = useState<DataUser>({
    customerName: '',
    customerCode: '',
    measurementPointCode: '',
    currentDateTime: '',
    matKetNoi: '',
    currentDate: '',
    currentMonth: '',
    currentYear: '',
    currentTime: '',
    currentMinute: '',
    currentSecond: '',
  });
  const [dataSocket, setDataSocket] = useState<any>(null);
  const alertState = useRecoilValue(AlertState);
  const countState = useRecoilValue(CountState);
  // const isFirstLoad = useRef(true);

  const cardInfoUser = [
    { label: 'Tên khách hàng', attribute: 'customerName', Icon: IconAvatar },
    { label: 'Mã khách hàng', attribute: 'customerCode', Icon: IconAvatar },
    { label: 'Mã điểm đo', attribute: 'measurementPointCode', Icon: IconFluctuation },
    { label: 'Đồng hồ thiết bị', attribute: 'currentDateTime', Icon: IconFluctuation },
    { label: 'Trạng thái kết nối', attribute: 'matKetNoi', Icon: IconShare },
  ];

  // const getMeasurementPointMonitoringAsync = async () => {
  //   const params: GetMeasurementPointMonitoringRequest = {
  //     tagName: id ?? '',
  //   };

  //   await getMeasurementPointMonitoring(
  //     params,
  //     (res: DataResponse) => {
  //       const { data } = res;
  //       setDataUser(data);
  //     },
  //     () => {},
  //     setLoading
  //   );
  // };

  const getMeasurementPointMonitoringNoLoadingAsync = async () => {
    const params: GetMeasurementPointMonitoringRequest = {
      tagName: id ?? '',
    };

    await getMeasurementPointMonitoring(
      params,
      (res: DataResponse) => {
        const { data } = res;
        setDataUser(data);
      },
      () => {},
      () => {}
    );
  };

  const getCurrentDateTime = (
    date: string,
    month: string,
    year: string,
    hours: string,
    minutes: string,
    seconds: string,
    ICT: boolean = true
  ) => {
    if (!date || !month || !year) return 'N/A';
    return `${renderTextNumber(date)}/${renderTextNumber(month)}/${year ?? '0000'} ${renderTextNumber(ICT ? Number(hours) + 7 : Number(hours))}:${renderTextNumber(minutes)}:${renderTextNumber(seconds)}`;
  };

  const getBackgroundColor = (index: number) => {
    const baseKey = `canhBao${index === 9 ? '10' : `0${index + 1}`}`;
    const warningKey = baseKey;
    const quantityKey = `${baseKey}Quanlity`;

    if (!dataUser) return 'quanlity';

    if (dataUser[quantityKey] === null) return 'normal';

    return dataUser[quantityKey] ? (dataUser[warningKey] ? 'warning' : 'normal') : 'quanlity';
  };
  useEffect(() => {
    // if (isFirstLoad.current) {
    //   getMeasurementPointMonitoringAsync();
    // } else {
    // }
    getMeasurementPointMonitoringNoLoadingAsync();

    // if (isFirstLoad.current) {
    //   isFirstLoad.current = false;
    // }
  }, [alertState, countState]);

  useEffect(() => {
    if (dataSocket) {
      setDataUser({ ...dataUser, ...dataSocket });
    }
  }, [dataSocket]);
  const electricParam = Array.from({ length: 3 }, (_, i) =>
    Constants.DetailElectricParams.slice(i * 2, (i + 1) * 2)
  );

  const operatingParam = Array.from({ length: 2 }, (_, i) =>
    Constants.DetailOperatingParams.slice(i * 3, (i + 1) * 3)
  );

  return (
    <>
      <PageTitle title={'Giám sát điểm đo'} />
      <SocketDetail setDataSocket={setDataSocket} id={id}>
        <div>
          <h1 className='text-24-28 font-bold'>{_t('Thông số chi tiết')}</h1>
          <div className='w-full'>
            <Row gutter={[20, 16]} className='mt-4'>
              {cardInfoUser.map((info, index) => (
                <Col
                  key={index}
                  xs={{ flex: '20%' }}
                  sm={{ flex: '20%' }}
                  md={{ flex: '20%' }}
                  lg={{ flex: '20%' }}
                  xl={{ flex: '20%' }}
                >
                  <div className='flex bg-white shadow-custom p-3 rounded-lg items-center custom-card'>
                    <div className='flex-1 flex flex-col justify-around'>
                      <h2 className='text-14-20 font-semibold text-gray-400 opacity-85'>
                        {info.label}
                      </h2>
                      {index == cardInfoUser.length - 1 ? (
                        <div>
                          {dataUser['matKetNoi'] != null ? (
                            <p
                              className={`text-12-16 font-bold ${dataUser[info.attribute] ? 'disconnect-msg' : 'connect-msg'}`}
                            >
                              {dataUser[info.attribute] ? 'Mất kết nối' : 'Bình thường'}
                              {!dataUser['matKetNoiQuanlity'] && (
                                <span className='ml-1 disconnect-msg'>!</span>
                              )}
                            </p>
                          ) : (
                            <p className={`text-12-16 font-bold disconnect-msg`}>????</p>
                          )}
                        </div>
                      ) : (
                        <p className='text-12-16 font-bold'>
                          {info.attribute == 'currentDateTime' ? (
                            dataUser.currentDate ? (
                              getCurrentDateTime(
                                dataUser.currentDate,
                                dataUser.currentMonth,
                                dataUser.currentYear,
                                dataUser.currentTime,
                                dataUser.currentMinute,
                                dataUser.currentSecond,
                                false
                              )
                            ) : (
                              <span className='disconnect-msg'>????</span>
                            )
                          ) : (
                            dataUser[info.attribute]
                          )}
                        </p>
                      )}
                    </div>
                    <info.Icon />
                  </div>
                </Col>
              ))}
            </Row>
          </div>
          <div className='mt-5'>
            <div>
              <Row className='mb-5'>
                <CollapseCommon
                  className='bg-white shadow-custom rounded-lg w-full custom-card'
                  header={
                    <div
                      className={
                        'flex flex-row gap-2 items-center border-b-2 pb-2 border-[#c75454]'
                      }
                    >
                      <p className='font-bold text-20-24 mt-2'>
                        Trạng thái hiện tại của các cảnh báo
                      </p>
                    </div>
                  }
                >
                  <div className='flex flex-col py-2 w-full'>
                    <Row className='justify-between w-[90%] ml-4' gutter={[40, 40]}>
                      <Col span='12'>
                        <div>
                          {Array.from({ length: 5 }).map((_, index) => {
                            const canhbao = dataUser[`canhBao0${index + 1}CustomName`];
                            const canhbaoTimestamp = dataUser[`canhBao0${index + 1}CreatedTime`];
                            const bgStatus = getBackgroundColor(index);
                            const borderColor =
                              bgStatus == 'warning' ? 'border-[#ebbdbd]' : 'border-[#b5b6b5a8]';

                            const bgColor =
                              bgStatus == 'warning'
                                ? 'bg-[#fbeded]'
                                : bgStatus == 'quanlity'
                                  ? '#eeeeee'
                                  : '';
                            return (
                              <Tooltip
                                title={
                                  <div>
                                    <p>Lần cập nhật cuối</p>
                                    <p>
                                      {convertDateTime(canhbaoTimestamp, 'dd/mm/yyyy hh:mi:ss')}
                                    </p>
                                  </div>
                                }
                              >
                                <div
                                  className={`flex gap-2 font-bold mb-3 items-center justify-start p-1 border-[2px] ${borderColor} ${bgColor} rounded-2xl`}
                                >
                                  <div className={`size-4 rounded-[50%] status-${bgStatus}`}></div>
                                  <p className='text-14-14'>
                                    {canhbao !== null && canhbao !== '' ? canhbao : 'Dự phòng'}
                                  </p>
                                </div>
                              </Tooltip>
                            );
                          })}
                        </div>
                      </Col>
                      <Col span='12'>
                        <div>
                          {Array.from({ length: 5 }).map((_, index) => {
                            const canhbao2 =
                              dataUser[`canhBao${index == 4 ? `10` : `0${index + 6}`}CustomName`];
                            const canhbao2Timestamp =
                              dataUser[`canhBao${index == 4 ? `10` : `0${index + 6}`}CreatedTime`];
                            const borderColor =
                              getBackgroundColor(index + 5) == 'warning'
                                ? 'border-[#ebbdbd]'
                                : 'border-[#b5b6b5a8]';

                            const bgColor =
                              getBackgroundColor(index + 5) == 'warning'
                                ? 'bg-[#fbeded]'
                                : getBackgroundColor(index + 5) == 'quanlity'
                                  ? '#eeeeee'
                                  : '';
                            return (
                              <Tooltip
                                title={
                                  <div>
                                    <p>Lần cập nhật cuối</p>
                                    <p>
                                      {convertDateTime(canhbao2Timestamp, 'dd/mm/yyyy hh:mi:ss')}
                                    </p>
                                  </div>
                                }
                              >
                                <div
                                  className={`flex gap-2 font-bold mb-3 items-center justify-start p-1 border-[2px] ${borderColor} ${bgColor} rounded-2xl`}
                                >
                                  <div
                                    className={`size-4 rounded-[50%] status-${getBackgroundColor(index + 5)}`}
                                  ></div>
                                  <p className='text-14-14'>
                                    {canhbao2 !== null && canhbao2 !== '' ? canhbao2 : 'Dự phòng'}
                                  </p>
                                </div>
                              </Tooltip>
                            );
                          })}
                        </div>
                      </Col>
                    </Row>
                  </div>
                </CollapseCommon>
              </Row>
              <Row className='mb-5'>
                <CollapseCommon
                  className='bg-white shadow-custom rounded-lg w-full custom-card'
                  header={
                    <div
                      className={
                        'flex flex-row gap-2 items-center border-b-2 pb-2 border-[#c75454]'
                      }
                    >
                      <p className='font-bold text-20-24 mt-2'>Thông số điện năng</p>
                    </div>
                  }
                >
                  <div className='py-2'>
                    <div className='flex flex-col gap-4 px-4'>
                      {electricParam.map((content, rowIndex) => (
                        <Row
                          gutter={[24, 12]}
                          className='rounded-lg border-[2px] border-[#b5b6b5a8] py-6'
                        >
                          <Col span={24} className='!m-0 !p-0'>
                            <div className='text-center font-extrabold text-16-24'>
                              <p>
                                {rowIndex == 0
                                  ? 'Chỉ số tích lũy'
                                  : rowIndex == 1
                                    ? 'Chỉ số chốt'
                                    : 'Công suất cực đại'}
                              </p>
                            </div>
                            {rowIndex == 1 && (
                              <Col span={24}>
                                <div className='w-2/5 flex gap-4 font-bold  justify-center'>
                                  <div className=''>
                                    <p>Thời điểm chốt</p>
                                  </div>
                                  <div className='text-14-14 flex gap-2 justify-center items-center'>
                                    <p className='border-[1px] border-[#b5b6b5a8] p-1 w-52 text-right rounded-md'>
                                      {getCurrentDateTime(
                                        dataUser.latestClosingDate,
                                        dataUser.latestClosingMonth,
                                        dataUser.latestClosingYear,
                                        dataUser.latestClosingTime,
                                        dataUser.latestClosingMinute,
                                        dataUser.latestClosingSecond,
                                        false
                                      )}
                                    </p>
                                  </div>
                                </div>
                              </Col>
                            )}
                          </Col>
                          {content.map((cell, index) => (
                            <Col key={index} span='12'>
                              {cell.label_1 && (
                                <div className='text-center mb-4 font-extrabold text-16-24'>
                                  <p>{cell.label_1}</p>
                                </div>
                              )}
                              {cell.value.map((item: any, i: number) => {
                                return (
                                  <Tooltip
                                    title={
                                      <div>
                                        <p>Lần cập nhật cuối</p>
                                        <p>
                                          {item.timestampComputed ? (
                                            item.timestampComputed(dataUser)
                                          ) : (
                                            <>
                                              {convertDateTime(
                                                dataUser[item.timestamp],
                                                'dd/mm/yyyy hh:mi:ss'
                                              )}
                                            </>
                                          )}
                                        </p>
                                      </div>
                                    }
                                  >
                                    <div
                                      key={i}
                                      className='flex gap-2 font-bold mb-1 w-3/5 items-center justify-between py-1 m-auto'
                                    >
                                      <div className=''>
                                        <p>{item.symbol}</p>
                                      </div>
                                      <div className='text-14-14 flex gap-2 justify-center items-center'>
                                        <p className='border-[1px] border-[#b5b6b5a8] p-1 w-40 text-right rounded-md'>
                                          {item.attributeComputed ? (
                                            item.attributeComputed(dataUser)
                                          ) : (
                                            <>
                                              {formatNumberWithCommas(
                                                dataUser[item.attribute] ?? '????'
                                              )}{' '}
                                            </>
                                          )}
                                        </p>
                                        <span className='w-10'>
                                          {dataUser[item.attribute] ? dataUser[item.unit] : ''}
                                          {dataUser[item.quanlity] == null ||
                                          dataUser[item.quanlity] ? (
                                            ''
                                          ) : (
                                            <span className='text-[#FF0000] font-bold'>{' !'}</span>
                                          )}
                                        </span>
                                      </div>
                                    </div>
                                  </Tooltip>
                                );
                              })}
                            </Col>
                          ))}
                        </Row>
                      ))}
                    </div>
                  </div>
                </CollapseCommon>
              </Row>
              <Row>
                <CollapseCommon
                  className='bg-white shadow-custom rounded-lg w-full custom-card'
                  header={
                    <div
                      className={
                        'flex flex-row gap-2 items-center border-b-2 pb-2 border-[#c75454]'
                      }
                    >
                      <p className='font-bold text-20-24 mt-2'>Thông số vận hành</p>
                    </div>
                  }
                >
                  <div className='py-2'>
                    <div className='flex flex-col gap-4 px-4'>
                      {operatingParam.map((content) => (
                        <Row
                          gutter={[24, 24]}
                          className='rounded-lg border-[2px] border-[#b5b6b5a8] py-6'
                        >
                          {content.map((cell, index) => (
                            <Col key={index} span='8'>
                              <div className='text-center mb-4 font-extrabold text-16-24'>
                                <p>{cell.label_1}</p>
                              </div>
                              {cell.value.map((item: any, i: number) => (
                                <Tooltip
                                  title={
                                    <div>
                                      <p>Lần cập nhật cuối</p>
                                      <p>
                                        {convertDateTime(
                                          dataUser[item.timestamp],
                                          'dd/mm/yyyy hh:mi:ss'
                                        )}
                                      </p>
                                    </div>
                                  }
                                >
                                  <div
                                    key={i}
                                    className='flex gap-2 font-bold mb-1 w-1/2 items-center justify-between py-1 m-auto'
                                  >
                                    <div className=''>
                                      <p className='w-12'>{item.symbol}</p>
                                    </div>
                                    <div className='text-14-14 flex gap-2 justify-center items-center'>
                                      <p className='border-[1px] border-[#b5b6b5a8] p-1 w-40 text-right rounded-md'>
                                        {formatNumberWithCommas(dataUser[item.attribute] ?? '????')}{' '}
                                      </p>
                                      <span className='w-10'>
                                        {dataUser[item.attribute] ? dataUser[item.unit] : ''}
                                        {dataUser[item.quanlity] == null ||
                                        dataUser[item.quanlity] ? (
                                          ''
                                        ) : (
                                          <span className='text-[#FF0000] font-bold'>{' !'}</span>
                                        )}
                                      </span>
                                    </div>
                                  </div>
                                </Tooltip>
                              ))}
                            </Col>
                          ))}
                        </Row>
                      ))}
                    </div>
                  </div>
                </CollapseCommon>
              </Row>
            </div>
          </div>
        </div>
        {/* <FullPageLoading isLoading={loading} /> */}
      </SocketDetail>
    </>
  );
};

export default PointMonitoringDetail;
