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

  const hubConnectionSummary = useMemo(() => {
    return new signalR.HubConnectionBuilder()
      .withUrl(`${baseURL}/${Constants.socketIoEndpoint.summary}`)
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build();
  }, [baseURL]);

  const startConnection = async () => {
    // 1. Detail Hub
    if (hubConnection.state === signalR.HubConnectionState.Disconnected) {
      try {
        await hubConnection.start();
        if (id) {
          await hubConnection.invoke('SubscribeToDetail', id);
        }
        hubConnection.off('ReceiveDetailUpdate');
        hubConnection.on('ReceiveDetailUpdate', (message) => {
          setDataSocket(message);
        });
      } catch (err) {
        // console.error('Error connecting to Detail Hub:', err);
      }
    }

    // 2. DateTime Hub
    if (hubConnectionDateTime.state === signalR.HubConnectionState.Disconnected) {
      try {
        await hubConnectionDateTime.start();
        if (id) {
          await hubConnectionDateTime.invoke('SubscribeToDetail', id);
        }
        hubConnectionDateTime.off('ReceiveDetailDateTimeUpdate');
        hubConnectionDateTime.on('ReceiveDetailDateTimeUpdate', (message) => {
          setDataSocket(message);
        });
      } catch (err) {
        // console.error('Error connecting to DateTime Hub:', err);
      }
    }

    // 3. Summary Hub (global updates from REST API)
    if (hubConnectionSummary.state === signalR.HubConnectionState.Disconnected) {
      try {
        await hubConnectionSummary.start();
        hubConnectionSummary.off(Constants.socketIoStatus.ReceiveUpdate);
        hubConnectionSummary.on(Constants.socketIoStatus.ReceiveUpdate, (message) => {
          setDataSocket(message);
        });
      } catch (err) {
        // console.error('Error connecting to Summary Hub:', err);
      }
    }
  };

  useEffect(() => {
    hubConnection.onreconnected(async () => {
      if (id && hubConnection.state === signalR.HubConnectionState.Connected) {
        try {
          await hubConnection.invoke('SubscribeToDetail', id);
        } catch (e) {}
      }
    });

    hubConnectionDateTime.onreconnected(async () => {
      if (id && hubConnectionDateTime.state === signalR.HubConnectionState.Connected) {
        try {
          await hubConnectionDateTime.invoke('SubscribeToDetail', id);
        } catch (e) {}
      }
    });

    startConnection();

    // Cleanup when navigating away: stop connections
    return () => {
      if (hubConnection.state === signalR.HubConnectionState.Connected) {
        if (id) {
          hubConnection.invoke('UnsubscribeFromDetail', id).catch(() => {});
        }
        hubConnection.stop().catch(() => {});
      }
      if (hubConnectionDateTime.state === signalR.HubConnectionState.Connected) {
        if (id) {
          hubConnectionDateTime.invoke('UnsubscribeFromDetail', id).catch(() => {});
        }
        hubConnectionDateTime.stop().catch(() => {});
      }
      if (hubConnectionSummary.state === signalR.HubConnectionState.Connected) {
        hubConnectionSummary.stop().catch(() => {});
      }
    };
  }, [hubConnection, hubConnectionDateTime, hubConnectionSummary, id]);

  return <>{children}</>; // Fragment for cleaner JSX
};

export default SocketDetail;
