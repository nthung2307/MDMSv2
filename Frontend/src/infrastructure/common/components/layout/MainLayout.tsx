import { Layout } from 'antd';
import { useEffect, useRef, useState } from 'react';
import { FiColumns, FiExternalLink, FiMaximize2 } from 'react-icons/fi';
import { IoClose } from 'react-icons/io5';
import { useRecoilState } from 'recoil';

import NotificationSound from '@/assets/notification-sound.mp3';
import { AlertState } from '@/core/application/common/atoms/global/alertState';
import { splitViewState } from '@/core/application/common/atoms/global/splitViewState';
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
  const [splitView, setSplitView] = useRecoilState(splitViewState);
  const [isResizing, setIsResizing] = useState(false);
  const isDraggingRef = useRef(false);

  // Kiểm tra nếu trang đang được mở trong iframe (chế độ nhúng / phân tách)
  const isEmbedded =
    typeof window !== 'undefined' &&
    (window.self !== window.top ||
      new URLSearchParams(window.location.search).get('embed') === 'true');

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

  // Kéo thả thay đổi độ rộng cột phân tách (Split resizer)
  const handleMouseDownResize = (e: React.MouseEvent) => {
    e.preventDefault();
    isDraggingRef.current = true;
    setIsResizing(true);
    const onMouseMove = (moveEvent: MouseEvent) => {
      if (!isDraggingRef.current) return;
      const rightWidth = window.innerWidth - moveEvent.clientX;
      let newPercent = Math.round((rightWidth / window.innerWidth) * 100);
      if (newPercent < 25) newPercent = 25;
      if (newPercent > 80) newPercent = 80;
      setSplitView((prev) => ({ ...prev, widthPercent: newPercent }));
    };
    const onMouseUp = () => {
      isDraggingRef.current = false;
      setIsResizing(false);
      window.removeEventListener('mousemove', onMouseMove);
      window.removeEventListener('mouseup', onMouseUp);
    };
    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);
  };

  // Nếu ở chế độ nhúng (embedded trong iframe), chỉ hiển thị Content thuần túy
  if (isEmbedded) {
    return (
      <div className='h-screen w-screen overflow-auto bg-gray-50 p-3'>
        <Content children={children} />
      </div>
    );
  }

  const splitUrl = splitView.url
    ? splitView.url.includes('?')
      ? `${splitView.url}&embed=true`
      : `${splitView.url}?embed=true`
    : '';

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
          {splitView.isOpen && splitView.url ? (
            <Layout className='flex-row overflow-hidden relative'>
              {/* Cột trái: Màn hình chính hiện tại */}
              <div
                className='h-full overflow-auto'
                style={{ width: `${100 - (splitView.widthPercent || 50)}%` }}
              >
                <Content children={children} />
              </div>

              {/* Thanh kéo phân tách (Split Resizer Bar) */}
              <div
                className='w-2 h-full bg-gray-200 hover:bg-blue-400 active:bg-blue-600 cursor-col-resize transition-colors flex items-center justify-center select-none shadow-sm z-20'
                onMouseDown={handleMouseDownResize}
                title='Kéo để thay đổi độ rộng phân tách'
              >
                <div className='w-0.5 h-8 bg-gray-400 rounded-full' />
              </div>

              {/* Cột phải: Màn hình phân tách (Split View Pane) */}
              <div
                className='h-full border-l border-gray-300 bg-white flex flex-col shadow-2xl z-10'
                style={{ width: `${splitView.widthPercent || 50}%` }}
              >
                {/* Header thanh công cụ phân tách */}
                <div className='h-11 px-3 bg-gray-100 border-b border-gray-300 flex items-center justify-between select-none flex-shrink-0'>
                  <div className='flex items-center gap-2 text-gray-800 font-semibold text-13-18 truncate mr-2'>
                    <FiColumns className='text-purple-600 flex-shrink-0' size={16} />
                    <span className='truncate'>{splitView.title || 'Chế độ xem phân tách'}</span>
                  </div>
                  <div className='flex items-center gap-1 flex-shrink-0'>
                    <button
                      type='button'
                      title='Mở trong tab mới'
                      className='p-1.5 text-gray-500 hover:text-blue-600 hover:bg-gray-200 rounded transition-colors'
                      onClick={() => window.open(splitView.url!, '_blank')}
                    >
                      <FiExternalLink size={15} />
                    </button>
                    <button
                      type='button'
                      title={
                        splitView.widthPercent === 50
                          ? 'Mở rộng 70%'
                          : splitView.widthPercent === 70
                          ? 'Toàn màn hình 85%'
                          : 'Chia đôi 50%'
                      }
                      className='p-1.5 text-gray-500 hover:text-blue-600 hover:bg-gray-200 rounded transition-colors'
                      onClick={() =>
                        setSplitView((prev) => ({
                          ...prev,
                          widthPercent:
                            prev.widthPercent === 50 ? 70 : prev.widthPercent === 70 ? 80 : 50,
                        }))
                      }
                    >
                      <FiMaximize2 size={15} />
                    </button>
                    <button
                      type='button'
                      title='Đóng chế độ phân tách'
                      className='p-1.5 text-gray-500 hover:text-red-600 hover:bg-red-50 rounded ml-1 transition-colors'
                      onClick={() => setSplitView((prev) => ({ ...prev, isOpen: false }))}
                    >
                      <IoClose size={18} />
                    </button>
                  </div>
                </div>

                {/* Nội dung trang phân tách qua Iframe */}
                <div className='flex-1 w-full h-full relative overflow-hidden bg-white'>
                  <iframe
                    src={splitUrl}
                    className={`w-full h-full border-0 ${isResizing ? 'pointer-events-none select-none' : ''}`}
                    title='Khung phân tách'
                  />
                </div>
              </div>
            </Layout>
          ) : (
            <Layout className='overflow-auto'>
              <Content children={children} />
            </Layout>
          )}
        </Layout>
      </Layout>
      <audio ref={audioPlayer} src={NotificationSound} />
    </SocketCommon>
  );
};

export default MainLayout;

