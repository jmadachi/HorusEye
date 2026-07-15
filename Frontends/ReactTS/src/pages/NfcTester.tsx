import { useState, useRef, useCallback } from 'react';
import { Smartphone, Wifi, WifiOff, CheckCircle, XCircle, Send } from 'lucide-react';

type Status = 'idle' | 'listening' | 'reading' | 'sending' | 'success' | 'error';

interface LogEntry {
  id: number;
  time: string;
  tagId: string;
  status: 'success' | 'error';
  message: string;
  tipo: string;
}

const API_BASE = import.meta.env.VITE_API_URL || 'https://horuseye-api.mauricioadachi.dev';

export default function NfcTester() {
  const [status, setStatus] = useState<Status>('idle');
  const [tipoMovimiento, setTipoMovimiento] = useState<'INGRESO' | 'SALIDA'>('INGRESO');
  const [puntoLectura, setPuntoLectura] = useState('PUERTA-01');
  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [lastTag, setLastTag] = useState<string | null>(null);
  const logId = useRef(0);
  const readerRef = useRef<any>(null);

  const addLog = useCallback((tagId: string, status: 'success' | 'error', message: string, tipo: string) => {
    logId.current += 1;
    const now = new Date();
    const time = now.toLocaleTimeString('es-ES');
    setLogs(prev => [{ id: logId.current, time, tagId, status, message, tipo }, ...prev].slice(0, 50));
  }, []);

  const sendToApi = async (uid: string): Promise<{ ok: boolean; message: string }> => {
    const payload = {
      tagId: uid,
      puntoLecturaId: puntoLectura,
      tipoMovimiento: tipoMovimiento,
    };

    const res = await fetch(`${API_BASE}/api/eventos-rfid`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    const data = await res.json();

    if (res.ok && data.success) {
      return { ok: true, message: data.message || 'Movimiento registrado' };
    } else {
      return { ok: false, message: data.message || data.errors?.join(', ') || 'Error desconocido' };
    }
  };

  const startListening = async () => {
    if (!('NDEFReader' in window)) {
      setStatus('error');
      addLog('-', 'error', 'Web NFC no soportado. Usa Chrome en Android.', '-');
      return;
    }

    try {
      const ndef = new (window as any).NDEFReader();
      readerRef.current = ndef;

      ndef.addEventListener('reading', async (event: any) => {
        const uid = event.serialNumber;
        setLastTag(uid);
        setStatus('reading');

        try {
          setStatus('sending');
          const result = await sendToApi(uid);
          setStatus(result.ok ? 'success' : 'error');
          addLog(uid, result.ok ? 'success' : 'error', result.message, tipoMovimiento);
        } catch (err: any) {
          setStatus('error');
          addLog(uid, 'error', err.message || 'Error de conexion', tipoMovimiento);
        }

        setTimeout(() => setStatus('listening'), 2000);
      });

      ndef.addEventListener('readingerror', () => {
        addLog('-', 'error', 'No se pudo leer el tag. Intenta de nuevo.', '-');
      });

      await ndef.scan();
      setStatus('listening');
      addLog('-', 'success', 'Escuchando tags NFC...', '-');
    } catch (err: any) {
      setStatus('error');
      addLog('-', 'error', err.message || 'Error al iniciar NFC', '-');
    }
  };

  const stopListening = () => {
    readerRef.current = null;
    setStatus('idle');
    addLog('-', 'success', 'Lectura detenida', '-');
  };

  const statusConfig = {
    idle: { color: 'bg-gray-500', text: 'Inactivo', icon: WifiOff },
    listening: { color: 'bg-blue-500', text: 'Escuchando...', icon: Wifi },
    reading: { color: 'bg-yellow-500', text: 'Leyendo tag...', icon: Smartphone },
    sending: { color: 'bg-purple-500', text: 'Enviando a API...', icon: Send },
    success: { color: 'bg-green-500', text: 'Enviado OK', icon: CheckCircle },
    error: { color: 'bg-red-500', text: 'Error', icon: XCircle },
  };

  const current = statusConfig[status];
  const StatusIcon = current.icon;

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <h1 className="text-2xl font-bold text-gray-800 dark:text-white">
        NFC Tester
      </h1>
      <p className="text-sm text-gray-500 dark:text-gray-400">
        Acerca una tarjeta NFC al celular para enviar un evento RFID a la API.
      </p>

      {/* Config */}
      <div className="bg-white dark:bg-gray-800 rounded-lg p-4 shadow space-y-4">
        <h2 className="font-semibold text-gray-700 dark:text-gray-200">Configuracion</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-600 dark:text-gray-300 mb-1">
              Tipo de Movimiento
            </label>
            <select
              value={tipoMovimiento}
              onChange={e => setTipoMovimiento(e.target.value as 'INGRESO' | 'SALIDA')}
              className="w-full border rounded-lg px-3 py-2 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
            >
              <option value="INGRESO">INGRESO</option>
              <option value="SALIDA">SALIDA</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-600 dark:text-gray-300 mb-1">
              Punto de Lectura
            </label>
            <input
              type="text"
              value={puntoLectura}
              onChange={e => setPuntoLectura(e.target.value)}
              className="w-full border rounded-lg px-3 py-2 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
            />
          </div>
        </div>
      </div>

      {/* Status + Button */}
      <div className="bg-white dark:bg-gray-800 rounded-lg p-6 shadow text-center space-y-4">
        <div className={`inline-flex items-center gap-2 px-4 py-2 rounded-full text-white text-sm font-medium ${current.color}`}>
          <StatusIcon size={16} />
          {current.text}
        </div>

        {lastTag && (
          <div className="text-sm text-gray-500 dark:text-gray-400">
            Ultimo tag: <code className="font-mono bg-gray-100 dark:bg-gray-700 px-2 py-0.5 rounded">{lastTag}</code>
          </div>
        )}

        {status === 'idle' ? (
          <button
            onClick={startListening}
            className="px-6 py-3 bg-[#1e3a5f] hover:bg-[#2d5a8e] text-white rounded-lg font-medium transition"
          >
            Iniciar Lectura NFC
          </button>
        ) : (
          <button
            onClick={stopListening}
            className="px-6 py-3 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium transition"
          >
            Detener
          </button>
        )}

        <p className="text-xs text-gray-400">
          Endpoint: <code className="font-mono">{API_BASE}/api/eventos-rfid</code>
        </p>
      </div>

      {/* Log */}
      <div className="bg-white dark:bg-gray-800 rounded-lg p-4 shadow">
        <h2 className="font-semibold text-gray-700 dark:text-gray-200 mb-3">Log de Eventos</h2>
        {logs.length === 0 ? (
          <p className="text-sm text-gray-400">Sin eventos aun.</p>
        ) : (
          <div className="space-y-2 max-h-80 overflow-y-auto">
            {logs.map(log => (
              <div
                key={log.id}
                className={`flex items-start gap-3 text-sm p-2 rounded ${
                  log.status === 'success' ? 'bg-green-50 dark:bg-green-900/20' : 'bg-red-50 dark:bg-red-900/20'
                }`}
              >
                <span className="text-gray-400 text-xs whitespace-nowrap">{log.time}</span>
                <span className={`font-mono text-xs ${log.status === 'success' ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'}`}>
                  {log.tagId}
                </span>
                <span className="text-xs px-1.5 py-0.5 rounded bg-gray-200 dark:bg-gray-600 text-gray-600 dark:text-gray-300">
                  {log.tipo}
                </span>
                <span className="text-gray-700 dark:text-gray-300 flex-1">{log.message}</span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
