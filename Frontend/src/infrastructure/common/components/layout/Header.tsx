import { Button, Dropdown, Layout, Tooltip } from 'antd';
import type { MenuProps } from 'antd';
import { useEffect, useState } from 'react';
import { FaCircleUser } from 'react-icons/fa6';
import { GoBell } from 'react-icons/go';
import { useLocation, useNavigate } from 'react-router-dom';
import { useRecoilValue } from 'recoil';

import { LogoLandscape } from '@/assets/icons';
import { ROUTE_PATH } from '@/core/application/common/appRouters';
import { AlertState } from '@/core/application/common/atoms/global/alertState';
import { CountState } from '@/core/application/common/atoms/global/countState';
import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListMeasurementPointMonitoringRequest } from '@/core/application/dto/measurement-point-monitoring/requests/GetListMeasurementPointMonitoringRequest';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import SocketCommon from '@/infrastructure/common/components/websocket/socket-common.tsx';
import { setRecoilStateAsync } from '@/infrastructure/common/libs/recoil-outside/recoil.service.tsx';
import useGetCountAlertHook from '@/infrastructure/repository/measurement-point/hooks/useGetCountAlert.hook';
import useGetListMeasurementPointAlertHook from '@/infrastructure/repository/measurement-point-monitoring/hooks/useGetListMeasurementPointAlert.hook';
import LocalStorageService from '@/infrastructure/services/localStorage.service';
import { getAccountRole, getTimeline } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation.ts';

interface Props {
  setStatusNoti: any;
  statusNoti: any;
}

const Header = (props: Props) => {
  const { setStatusNoti, statusNoti } = props;
  const navigate = useNavigate();
  const location = useLocation();
  const alertState = useRecoilValue(AlertState);
  const localStorageService = new LocalStorageService();
  const [getListMeasurementPointAlert] = useGetListMeasurementPointAlertHook();
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);
  const [dataSocket, setDataSocket] = useState(null);
  const [dataAlert, setDataAlert] = useState<any[]>([]);
  const [countAlert, setCountAlert] = useState(0);
  const [getCountAlert] = useGetCountAlertHook();
  const role = getAccountRole();

  const countState = useRecoilValue(CountState);

  const getListMeasurementPointAlertAsync = async () => {
    const params: GetListMeasurementPointMonitoringRequest = {
      pageSize: 10,
      searchByFields: [],
      sortByFields: [],
    };
    await getListMeasurementPointAlert(
      params,
      (res: DataResponse) => {
        const { data } = res;
        const result = data?.data;
        setDataAlert(result || []);
        if (typeof data?.totalCount === 'number') {
          setCountAlert(data.totalCount);
        } else if (Array.isArray(result)) {
          setCountAlert(result.length);
        }
      },
      () => {},
      () => {}
    );
  };

  const handleSeenAll = () => {
    const targetRoute = ROUTE_PATH.POINT_MONITORING_WARNING;

    if (location.pathname === targetRoute) {
      window.location.reload();
    } else {
      navigate(targetRoute);
    }
  };

  const menuAlert = () => {
    return (
      <div className='flex flex-col items-start w-full p-3 bg-white shadow-xl rounded-md'>
        <div className='overflow-auto scrollbar max-h-[40vh] w-96'>
          {dataAlert.length > 0 ? (
            dataAlert.map((item: any, index: number) => (
              <Tooltip title={`${item.measurementPointCode} - ${item.customerName}`}>
                <div
                  key={index}
                  className='bg-[#FBEDED] flex justify-between px-4 py-2 rounded-md mb-2 overflow-hidden'
                >
                  <div className='mr-4'>
                    <h2 className='text-[15px] text-[#00000080] w-60 truncate'>{`${item.measurementPointCode}-${item.customerName}`}</h2>
                    <p className='text-14-18 mt-2'>{item.eventName}</p>
                  </div>
                  <div>
                    <p className='mb-2'>{getTimeline(item.createdTime)}</p>
                  </div>
                </div>
              </Tooltip>
            ))
          ) : (
            <div className='flex h-64 justify-between px-4 py-2 rounded-md mb-2 cursor-pointer'>
              <div className='m-auto '>
                <h2 className='text-18-20 text-black p-3 rounded-md'>Hiện tại chưa có thông báo</h2>
              </div>
            </div>
          )}
        </div>
        <div className='flex justify-between text-[#B22222] w-full mt-3'>
          <button
            type='button'
            onClick={() => {
              setStatusNoti(!statusNoti);
            }}
            className={'font-bold'}
          >
            {statusNoti ? 'Tắt cảnh báo' : 'Bật cảnh báo'}
          </button>
          <button type='button' onClick={handleSeenAll} className={'font-bold'}>
            Xem tất cả
          </button>
        </div>
      </div>
    );
  };

  const handleLogout = async () => {
    localStorageService.removeStorage(Constants.API_TOKEN_STORAGE);
    navigate(ROUTE_PATH.LOGIN);
  };

  const getCountAlertAsync = async () => {
    await getCountAlert(
      (res: DataResponse) => {
        const { data } = res;
        setCountAlert(data);
        setRecoilStateAsync(AlertState, null);
      },
      () => {},
      () => {}
    );
  };

  // TODO: Tạo API để lấy thông tin tài khoản

  const items: MenuProps['items'] = [
    {
      label: 'Thông tin tài khoản',
      key: '1',
      onClick: () => navigate(ROUTE_PATH.USER_PROFILE),
    },
    {
      label: 'Đăng xuất',
      key: '2',
      onClick: () => setIsShowDialog(!isShowDialog),
    },
  ];

  useEffect(() => {
    if (role !== Constants.Role.Customer.value) {
      if (dataSocket) {
        setCountAlert(Number(dataSocket));
        setRecoilStateAsync(CountState, dataSocket);
      }
      getCountAlertAsync();
      getListMeasurementPointAlertAsync();
    }
  }, [dataSocket, alertState]);

  useEffect(() => {
    if (typeof countState === 'number') {
      setCountAlert(countState);
    }
  }, [countState]);

  return (
    <SocketCommon
      setDataSocket={setDataSocket}
      endPoint={Constants.socketIoEndpoint.event}
      status={Constants.socketIoStatus.CountAlert}
    >
      <Layout.Header className='flex justify-between mx-0 px-[30px] border-b-2 border-firebrick shadow-md z-30'>
        <div className='flex items-center'>
          <div className='flex items-end gap-2'>
            <LogoLandscape height={25} />
          </div>
        </div>
        <div className='flex gap-6 items-center'>
          <div className={'flex gap-3 items-center'}>
            {role !== Constants.Role.Customer.value && (
              <Dropdown dropdownRender={menuAlert} placement='bottomRight'>
                <Button
                  type={'text'}
                  icon={
                    <div className='relative'>
                      <GoBell size={30} color={'firebrick'} />
                      <div className='absolute top-[-10%] left-1/2 p-1 rounded-full w-5 h-5 bg-firebrick'>
                        <p className='text-8-10 font-bold text-white'>
                          {countAlert > 99 ? '99+' : countAlert}
                        </p>
                      </div>
                    </div>
                  }
                  shape={'circle'}
                  className={'p-5'}
                />
              </Dropdown>
            )}
            <Dropdown menu={{ items }}>
              <Button
                type={'text'}
                icon={<FaCircleUser size={30} className='flex h-[30px]' color={'firebrick'} />}
                shape={'circle'}
                className={'p-5'}
              />
            </Dropdown>
          </div>
        </div>
        {/* <FullPageLoading isLoading={loading} /> */}
      </Layout.Header>
      <DialogCommon
        title={''}
        visible={isShowDialog}
        handleCancel={() => {
          setIsShowDialog(!isShowDialog);
        }}
        handleConfirm={() => {
          handleLogout();
        }}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-5 gap-3'>
            <p className='text-20-20 font-bold'>{_t('Đăng xuất tài khoản?')}</p>
            <p className=''>
              {_t('Sau khi xác nhận, tài khoản của bạn sẽ được đăng xuất khỏi hệ thống.')}
            </p>
          </div>
        </div>
      </DialogCommon>
    </SocketCommon>
  );
};

export default Header;
