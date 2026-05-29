import { useEffect, useState, useRef, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

interface UseSignalROptions {
  onMovimiento?: (data: unknown) => void;
}

export function useSignalR({ onMovimiento }: UseSignalROptions = {}) {
  const [connected, setConnected] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    const token = localStorage.getItem('accessToken');
    if (!token) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/movimientos', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    connection.onreconnecting(() => setConnected(false));
    connection.onreconnected(() => setConnected(true));
    connection.onclose(() => setConnected(false));

    if (onMovimiento) {
      connection.on('NuevoMovimiento', onMovimiento);
    }

    connection.start()
      .then(async () => {
        await connection.invoke('JoinDashboardGroup');
        setConnected(true);
      })
      .catch(() => setConnected(false));

    connectionRef.current = connection;

    return () => {
      connection.stop();
      connectionRef.current = null;
    };
  }, []);

  const reiniciar = useCallback(async () => {
    if (connectionRef.current) {
      await connectionRef.current.stop();
      await connectionRef.current.start();
      await connectionRef.current.invoke('JoinDashboardGroup');
      setConnected(true);
    }
  }, []);

  return { connected, reiniciar };
}
