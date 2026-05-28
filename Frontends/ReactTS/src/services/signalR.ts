import * as signalR from '@microsoft/signalr';

class SignalRService {
  private connection: signalR.HubConnection | null = null;

  startConnection(): signalR.HubConnection {
    const token = localStorage.getItem('accessToken');

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/movimientos', {
        accessTokenFactory: () => token || ''
      })
      .withAutomaticReconnect()
      .build();

    this.connection.start().catch((err) => {
      console.error('SignalR connection error:', err);
    });

    return this.connection;
  }

  async joinDashboard(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('JoinDashboardGroup');
    }
  }

  onMovimiento(callback: (data: unknown) => void): void {
    this.connection?.on('NuevoMovimiento', callback);
  }

  stopConnection(): void {
    this.connection?.stop();
  }

  getConnection(): signalR.HubConnection | null {
    return this.connection;
  }
}

export const signalRService = new SignalRService();
