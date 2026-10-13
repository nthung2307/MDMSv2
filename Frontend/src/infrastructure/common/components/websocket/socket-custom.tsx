import * as signalR from '@microsoft/signalr';
import { useEffect, useMemo, ReactNode, Dispatch, SetStateAction } from 'react';

import Constants from '@/core/application/common/constants';
import { getAccountRole } from '@/infrastructure/utils/helpers';

interface Props {
  setDataAlert: (data: any) => void | Dispatch<SetStateAction<any>>;
  setDataEvent: (data: any) => void | Dispatch<SetStateAction<any>>;
  children: ReactNode;
}

const SocketCustom = ({ children, setDataAlert, setDataEvent }: Props) => {
  const baseURL = `${import.meta.env.VITE_APP_SOCKET_URL}`;
  const role = getAccountRole();

  // Initialize SignalR connection
  const hubConnection = useMemo(() => {
    return new signalR.HubConnectionBuilder()
      .withUrl(`${baseURL}/notificationeventhub`)
      .withAutomaticReconnect([0, 2000, 10000, 30000]) // Retry after 2s, 10s, and 30s if connection drops
      .build();
  }, [baseURL]);

  // ReceiveEvent
  // CountAlert
  // ReceiveAlert
  const startConnection = async () => {
    if (hubConnection.state === signalR.HubConnectionState.Disconnected) {
      try {
        await hubConnection.start();
        // Listen for list updates
        hubConnection.on('ReceiveAlert', (message) => {
          setDataAlert(message);
        });

        hubConnection.on('ReceiveEvent', (message) => {
          setDataEvent(message);
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

    // Cleanup when navigating away: stop the connection
    return () => {
      if (hubConnection.state === signalR.HubConnectionState.Connected) {
        hubConnection.stop().catch(() => {
          // console.error('Error stopping SignalR connection:', error);
        });
      }
    };
  }, [hubConnection]);

  return <>{children}</>; // Fragment for cleaner JSX
};

export default SocketCustom;
