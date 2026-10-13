import * as signalR from '@microsoft/signalr';
import { useEffect, useMemo, ReactNode, Dispatch, SetStateAction } from 'react';

import Constants from '@/core/application/common/constants';

interface Props {
  setDataSocket: (data: any) => void | Dispatch<SetStateAction<any>>;
  id: any;
  children: ReactNode;
}

const SocketDetail = ({ children, setDataSocket, id }: Props) => {
  const baseURL = `${import.meta.env.VITE_APP_SOCKET_URL}`;

  // Initialize SignalR connection
  const hubConnection = useMemo(() => {
    return new signalR.HubConnectionBuilder()
      .withUrl(`${baseURL}/${Constants.socketIoEndpoint.detail}`)
      .withAutomaticReconnect([0, 2000, 10000, 30000]) // Retry after 2s, 10s, and 30s if connection drops
      .build();
  }, [id]);

  const hubConnectionDateTime = useMemo(() => {
    return new signalR.HubConnectionBuilder()
      .withUrl(`${baseURL}/${Constants.socketIoEndpoint.dateTime}`)
      .withAutomaticReconnect([0, 2000, 10000, 30000]) // Retry after 2s, 10s, and 30s if connection drops
      .build();
  }, [id]);

  const startConnection = async () => {
    if (
      hubConnection.state === signalR.HubConnectionState.Disconnected &&
      hubConnectionDateTime.state === signalR.HubConnectionState.Disconnected
    ) {
      try {
        await hubConnection.start();
        await hubConnectionDateTime.start();

        // Subscribe to detail updates for the specific item
        await hubConnection.invoke('SubscribeToDetail', id);
        await hubConnectionDateTime.invoke('SubscribeToDetail', id);

        hubConnection.on('ReceiveDetailUpdate', (message) => {
          setDataSocket(message);
        });

        hubConnectionDateTime.on('ReceiveDetailDateTimeUpdate', (message) => {
          setDataSocket(message);
        });
      } catch (err) {
        // console.error('Error connecting to SignalR Hub:', err);
      }
    }
  };

  useEffect(() => {
    startConnection();

    // Cleanup when navigating away: stop the connection
    return () => {
      if (hubConnection.state === signalR.HubConnectionState.Connected) {
        hubConnection.invoke('UnsubscribeFromDetail', id);
        hubConnection
          .stop()
          .then(() => {
            // console.log('SignalR connection for Detail View stopped');
          })
          .catch(() => {
            // console.error('Error stopping SignalR connection:', error);
          });
      }
    };
  }, [hubConnection, id]);

  return <>{children}</>; // Fragment for cleaner JSX
};

export default SocketDetail;
