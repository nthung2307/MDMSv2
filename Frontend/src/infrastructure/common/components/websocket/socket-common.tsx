import * as signalR from '@microsoft/signalr';
import { useEffect, useMemo, ReactNode, Dispatch, SetStateAction } from 'react';

import { CountState } from '@/core/application/common/atoms/global/countState';
import Constants from '@/core/application/common/constants';
import { setRecoilStateAsync } from '@/infrastructure/common/libs/recoil-outside/recoil.service';
import LocalStorageService from '@/infrastructure/services/localStorage.service';
import { getAccountRole } from '@/infrastructure/utils/helpers';

interface Props {
  setDataSocket: (data: any) => void | Dispatch<SetStateAction<any>>;
  endPoint: string;
  status: string;
  children: ReactNode;
}

const SocketCommon = ({ children, setDataSocket, endPoint, status }: Props) => {
  const baseURL = `${import.meta.env.VITE_APP_SOCKET_URL ?? import.meta.env.VITE_APP_API_URL ?? ''}`;
  const role = getAccountRole();

  // Initialize SignalR connection
  const hubConnection = useMemo(() => {
    const localStorageService = new LocalStorageService();
    return new signalR.HubConnectionBuilder()
      .withUrl(`${baseURL}/${endPoint}`, {
        accessTokenFactory: () =>
          localStorageService.readStorage(Constants.API_TOKEN_STORAGE) || '',
      })
      .withAutomaticReconnect([0, 2000, 10000, 30000]) // Retry after 2s, 10s, and 30s if connection drops
      .build();
  }, [baseURL, endPoint]);
  // ReceiveEvent
  // CountAlert
  // ReceiveAlert
  const startConnection = async () => {
    if (hubConnection.state === signalR.HubConnectionState.Disconnected) {
      try {
        await hubConnection.start();
        // Listen for list updates
        hubConnection.off(status);
        hubConnection.on(status, (message) => {
          setDataSocket(message);
        });
        if (
          endPoint === Constants.socketIoEndpoint.event &&
          status !== Constants.socketIoStatus.CountAlert
        ) {
          hubConnection.off(Constants.socketIoStatus.CountAlert);
          hubConnection.on(Constants.socketIoStatus.CountAlert, (message) => {
            const nextCount = Number(message);
            if (!Number.isNaN(nextCount)) {
              setRecoilStateAsync(CountState, nextCount);
            }
          });
        }
        hubConnection.onreconnected(() => {
          hubConnection.off(status);
          hubConnection.on(status, (message) => {
            setDataSocket(message);
          });
          if (
            endPoint === Constants.socketIoEndpoint.event &&
            status !== Constants.socketIoStatus.CountAlert
          ) {
            hubConnection.off(Constants.socketIoStatus.CountAlert);
            hubConnection.on(Constants.socketIoStatus.CountAlert, (message) => {
              const nextCount = Number(message);
              if (!Number.isNaN(nextCount)) {
                setRecoilStateAsync(CountState, nextCount);
              }
            });
          }
        });
      } catch (err) {
        // console.error('Error connecting to SignalR Hub:', err);
      }
    }
  };

  useEffect(() => {
    if (role !== Constants.Role.Customer.value) {
      startConnection();
    }

    // Cleanup when navigating away: stop the connection regardless of Connecting/Connected state
    return () => {
      hubConnection.off(status);
      if (endPoint === Constants.socketIoEndpoint.event) {
        hubConnection.off(Constants.socketIoStatus.CountAlert);
      }
      hubConnection.stop().catch(() => {
        // console.error('Error stopping SignalR connection:', error);
      });
    };
  }, [hubConnection]);

  return <>{children}</>; // Fragment for cleaner JSX
};

export default SocketCommon;
