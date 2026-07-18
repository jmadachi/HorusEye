import { useState, useRef, useCallback, useEffect } from 'react';
import { Smartphone, Wifi, WifiOff, CheckCircle, XCircle, Send, Keyboard } from 'lucide-react';
import api from '../services/api';
import type { DispositivoRfid, ApiResponse } from '../types';

type Status = 'idle' | 'listening' | 'reading' | 'sending' | 'success' | 'error';

interface LogEntry {
  id: number;
  time: string;
  tagId: string;
  status: 'success' | 'error';
  message: string;
  dispositivo: string;
}

const API_BASE = import.meta.env.VITE_API_URL || 'https://horuseye-api.mauricioadachi.dev';

export default function NfcTester() {
  const [status, setStatus] = useState<Status>('idle');
  const [dispositivos, setDispositivos] = useState<DispositivoRfid[]>([]);
  const [dispositivoSeleccionado, setDispositivoSeleccionado] = useState('');
  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [lastTag, setLastTag] = useState<string | null>(null);
  const [manualEpc, setManualEpc] = useState('');
  const logId = useRef(0);
  const readerRef = useRef<any>(null);

  useEffect(() => {
    const loadDispositivos = async () => {
      try {
        const { data } = await api.get('/api/dispositivos?pageSize=200') as { data: ApiResponse<{ items: DispositivoRfid[] }> };
        if (data.success) {
          const handhelds = data.data.items.filter(d =>
            (d.tipoDispositivo === 'HANDHELD' || d.tipoDispositivo === 'FIXED') && d.activo
          );
          setDispositivos(handhelds);
          if (handhelds.length > 0) setDispositivoSeleccionado(handhelds[0].id);
        }
      } catch { /* silently fail */ }
    };
    loadDispositivos();
  }, []);

  const addLog = useCallback((tagId: string, status: 'success' | 'error', message: string, dispositivo: string) => {
    logId.current += 1;
    const now = new Date();
    const time = now.toLocaleTimeString('es-ES');
    setLogs(prev => [{ id: logId.current, time, tagId, status, message, dispositivo }, ...prev].slice(0, 50));
  }, []);

  const sendToApi = async (uid: string): Promise<{ ok: boolean; message: string }> => {
    const dispositivo = dispositivos.find(d => d.id === dispositivoSeleccionado);
    if (!dispositivo) return { ok: false, message: 'No hay dispositivo seleccionado' };

    const res = await fetch(`${API_BASE}/api/eventos-rfid/nfc-tester`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Api-Key': dispositivo.apiKey || '',
      },
      body: JSON.stringify({
        epc: uid,
        reader_id: dispositivo.nombre,
        timestamp: new Date().toISOString(),
      }),
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

        const dispositivo = dispositivos.find(d => d.id === dispositivoSeleccionado);

        try {
          setStatus('sending');
          const result = await sendToApi(uid);
          setStatus(result.ok ? 'success' : 'error');
          addLog(uid, result.ok ? 'success' : 'error', result.message, dispositivo?.nombre || '-');
        } catch (err: any) {
          setStatus('error');
          addLog(uid, 'error', err.message || 'Error de conexion', dispositivo?.nombre || '-');
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

  const sendManual = async () => {
    if (!manualEpc.trim()) return;
    const epc = manualEpc.trim();
    setStatus('sending');
    setLastTag(epc);
    const dispositivo = dispositivos.find(d => d.id === dispositivoSeleccionado);
    try {
      const result = await sendToApi(epc);
      setStatus(result.ok ? 'success' : 'error');
      addLog(epc, result.ok ? 'success' : 'error', result.message, dispositivo?.nombre || '-');
    } catch (err: any) {
      setStatus('error');
      addLog(epc, 'error', err.message || 'Error de conexion', dispositivo?.nombre || '-');
    }
    setManualEpc('');
    setTimeout(() => setStatus('idle'), 2000);
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
        <div>
          <label className="block text-sm font-medium text-gray-600 dark:text-gray-300 mb-1">
            Dispositivo Lector
          </label>
          <select
            value={dispositivoSeleccionado}
            onChange={e => setDispositivoSeleccionado(e.target.value)}
            className="w-full border rounded-lg px-3 py-2 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
          >
            {dispositivos.length === 0 && <option value="">No hay dispositivos registrados</option>}
            {dispositivos.map(d => (
              <option key={d.id} value={d.id}>
                {d.nombre} ({d.tipoDispositivo}){d.nodoUbicacionNombre ? ` — ${d.nodoUbicacionNombre}` : ''}
              </option>
            ))}
          </select>
          {dispositivos.length === 0 && (
            <p className="text-xs text-gray-400 mt-1">Registra un dispositivo en /dispositivos primero.</p>
          )}
        </div>
      </div>

      {/* Manual Input */}
      <div className="bg-white dark:bg-gray-800 rounded-lg p-4 shadow space-y-3">
        <h2 className="font-semibold text-gray-700 dark:text-gray-200 flex items-center gap-2">
          <Keyboard size={16} />
          Envio Manual
        </h2>
        <p className="text-xs text-gray-400">Si el NFC no funciona, escribe el EPC de la tarjeta manualmente.</p>
        <div className="flex gap-2">
          <input
            value={manualEpc}
            onChange={e => setManualEpc(e.target.value)}
            onKeyDown={e => e.key === 'Enter' && sendManual()}
            placeholder="Ej: E28011700000020811603477"
            disabled={!dispositivoSeleccionado || status === 'sending'}
            className="flex-1 border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200 font-mono text-sm"
          />
          <button
            onClick={sendManual}
            disabled={!manualEpc.trim() || !dispositivoSeleccionado || status === 'sending'}
            className="px-4 py-2 bg-[#1e3a5f] hover:bg-[#2d5a8e] text-white rounded font-medium transition disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
          >
            <Send size={14} />
            Enviar
          </button>
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
            disabled={!dispositivoSeleccionado}
            className="px-6 py-3 bg-[#1e3a5f] hover:bg-[#2d5a8e] text-white rounded-lg font-medium transition disabled:opacity-50 disabled:cursor-not-allowed"
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
          Endpoint: <code className="font-mono">{API_BASE}/api/eventos-rfid/nfc-tester</code>
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
                  {log.dispositivo}
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
