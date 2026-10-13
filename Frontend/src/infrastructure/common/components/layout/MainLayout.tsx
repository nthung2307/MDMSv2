import { Layout } from 'antd';
import { useEffect, useRef, useState } from 'react';

import NotificationSound from '@/assets/notification-sound.mp3';
import { AlertState } from '@/core/application/common/atoms/global/alertState';
import Constants from '@/core/application/common/constants';
import Content from '@/infrastructure/common/components/layout/Content';
import Header from '@/infrastructure/common/components/layout/Header';
import LeftMenu from '@/infrastructure/common/components/layout/LeftMenu';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import SocketCommon from '@/infrastructure/common/components/websocket/socket-common.tsx';
import { setRecoilStateAsync } from '@/infrastructure/common/libs/recoil-outside/recoil.service.tsx';
import { _t } from '@/infrastructure/utils/translation';
const MainLayout = ({ children }: { children: React.ReactNode }) => {
  const [dataSocket, setDataSocket] = useState(null);
  const audioPlayer = useRef<HTMLAudioElement>(null);
  const [statusNoti, setStatusNoti] = useState(true);
  useEffect(() => {
    if (dataSocket) {
      setRecoilStateAsync(AlertState, dataSocket);
      if (audioPlayer.current) {
        notifySuccess('', _t('Có cảnh báo mới'));
        if (statusNoti) {
          audioPlayer.current.play();
        }
        setDataSocket(null);
      }
    }
  }, [dataSocket]);

  return (
    <SocketCommon
      setDataSocket={setDataSocket}
      endPoint={Constants.socketIoEndpoint.event}
      status={Constants.socketIoStatus.ReceiveAlert}
    >
      <Layout className='fixed top-0 bottom-0 left-0 right-0'>
        <Header setStatusNoti={setStatusNoti} statusNoti={statusNoti} />
        <Layout>
          <LeftMenu />
          <Layout className='overflow-auto'>
            <Content children={children} />
          </Layout>
        </Layout>
      </Layout>
      <audio ref={audioPlayer} src={NotificationSound} />
    </SocketCommon>
  );
};

export default MainLayout;
