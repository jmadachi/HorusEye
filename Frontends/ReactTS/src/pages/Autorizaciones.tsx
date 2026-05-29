import { useEffect, useState } from 'react';
import api from '../services/api';
import { useAuth } from '../context/AuthContext';
import type { Activo } from '../types';
import { Plus, X, ShieldOff, Trash2, CheckCircle, XCircle } from 'lucide-react';

interface Autorizacion {
  id: number;
  activoId: string;
  activoPlaca: string;
  activoNombre: string;
  autorizadoPor: string;
  fechaAutorizacion: string;
  fechaVencimiento: string | null;
  activa: boolean;
}

export default function Autorizaciones() {
  const [autorizaciones, setAutorizaciones] = useState<Autorizacion[]>([]);
  const [activos, setActivos] = useState<Activo[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState({ activoId: '', autorizadoPor: '', fechaVencimiento: '' });
  const [error, setError] = useState('');
  const { hasRole } = useAuth();
  const isGestion = hasRole('Usuario de Gestión');

  const loadAutorizaciones = async () => {
    try {
      const { data } = await api.get('/api/autorizaciones');
      if (data.success) setAutorizaciones(data.data);
    } catch { /* ignore */ }
  };

  const loadActivos = async () => {
    try {
      const { data } = await api.get('/api/activos');
      if (data.success) setActivos(data.data);
    } catch { /* ignore */ }
  };

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadAutorizaciones();
    loadActivos();
  }, []);

  const openCreate = () => {
    setForm({ activoId: '', autorizadoPor: '', fechaVencimiento: '' });
    setError('');
    setShowModal(true);
  };

  const handleCreate = async () => {
    setError('');
    if (!form.activoId || !form.autorizadoPor) {
      setError('Activo y autorizado por son requeridos');
      return;
    }
    try {
      const payload: Record<string, unknown> = {
        activoId: form.activoId,
        autorizadoPor: form.autorizadoPor
      };
      if (form.fechaVencimiento) {
        payload.fechaVencimiento = new Date(form.fechaVencimiento).toISOString();
      }
      await api.post('/api/autorizaciones', payload);
      setShowModal(false);
      loadAutorizaciones();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      setError(axiosErr.response?.data?.message || 'Error al crear autorización');
    }
  };

  const handleRevocar = async (id: number) => {
    try {
      await api.put(`/api/autorizaciones/${id}/revocar`);
      loadAutorizaciones();
    } catch { /* ignore */ }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('¿Eliminar esta autorización definitivamente?')) return;
    try {
      await api.delete(`/api/autorizaciones/${id}`);
      loadAutorizaciones();
    } catch { /* ignore */ }
  };

  const isVencida = (a: Autorizacion) =>
    a.activa && a.fechaVencimiento && new Date(a.fechaVencimiento) < new Date();

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-gray-800 dark:text-gray-100">Autorizaciones de Salida</h2>
        {isGestion && (
          <button
            onClick={openCreate}
            className="flex items-center gap-2 bg-[#1e3a5f] text-white px-4 py-2 rounded hover:bg-[#2d5a8e] transition cursor-pointer"
          >
            <Plus size={18} /> Nueva Autorización
          </button>
        )}
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Activo</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Placa</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Autorizado Por</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Fecha Autorización</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Vence</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Estado</th>
              {isGestion && <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Acciones</th>}
            </tr>
          </thead>
          <tbody>
            {autorizaciones.map((a) => {
              const vencida = isVencida(a);
              return (
                <tr key={a.id} className="border-t border-gray-100 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-4 py-3 font-medium">{a.activoNombre}</td>
                  <td className="px-4 py-3">{a.activoPlaca}</td>
                  <td className="px-4 py-3">{a.autorizadoPor}</td>
                  <td className="px-4 py-3">{new Date(a.fechaAutorizacion).toLocaleDateString()}</td>
                  <td className="px-4 py-3">
                    {a.fechaVencimiento ? new Date(a.fechaVencimiento).toLocaleDateString() : '-'}
                  </td>
                  <td className="px-4 py-3">
                    {vencida ? (
                      <span className="flex items-center gap-1 text-xs font-medium text-gray-500">
                        <XCircle size={14} /> Vencida
                      </span>
                    ) : a.activa ? (
                      <span className="flex items-center gap-1 text-xs font-medium text-green-600 dark:text-green-400">
                        <CheckCircle size={14} /> Activa
                      </span>
                    ) : (
                      <span className="flex items-center gap-1 text-xs font-medium text-red-600 dark:text-red-400">
                        <XCircle size={14} /> Revocada
                      </span>
                    )}
                  </td>
                  {isGestion && (
                    <td className="px-4 py-3">
                      <div className="flex gap-2">
                        {a.activa && !vencida && (
                          <button onClick={() => handleRevocar(a.id)}
                            className="p-1.5 text-amber-600 hover:bg-amber-100 rounded dark:text-amber-400 dark:hover:bg-amber-900/40 cursor-pointer"
                            title="Revocar">
                            <ShieldOff size={14} />
                          </button>
                        )}
                        <button onClick={() => handleDelete(a.id)}
                          className="p-1.5 text-red-600 hover:bg-red-100 rounded dark:text-red-400 dark:hover:bg-red-900/40 cursor-pointer"
                          title="Eliminar">
                          <Trash2 size={14} />
                        </button>
                      </div>
                    </td>
                  )}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg w-full max-w-lg p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="font-semibold text-lg">Nueva Autorización de Salida</h3>
              <button onClick={() => setShowModal(false)} className="cursor-pointer"><X size={20} /></button>
            </div>

            {error && (
              <div className="bg-red-50 dark:bg-red-900/30 text-red-700 dark:text-red-300 px-4 py-2 rounded mb-4 text-sm">{error}</div>
            )}

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1 dark:text-gray-200">Activo *</label>
                <select
                  value={form.activoId}
                  onChange={(e) => setForm({ ...form, activoId: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
                >
                  <option value="">Seleccionar activo...</option>
                  {activos.map((act) => (
                    <option key={act.id} value={act.id}>
                      {act.placa} - {act.nombre}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1 dark:text-gray-200">Autorizado Por *</label>
                <input
                  value={form.autorizadoPor}
                  onChange={(e) => setForm({ ...form, autorizadoPor: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
                  placeholder="Nombre de quien autoriza"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1 dark:text-gray-200">Fecha de Vencimiento (opcional)</label>
                <input
                  type="date"
                  value={form.fechaVencimiento}
                  onChange={(e) => setForm({ ...form, fechaVencimiento: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
                />
              </div>
              <div className="flex justify-end gap-3 pt-2">
                <button onClick={() => setShowModal(false)}
                  className="px-4 py-2 border border-gray-300 dark:border-gray-600 dark:text-gray-200 dark:hover:bg-gray-700 rounded cursor-pointer">
                  Cancelar
                </button>
                <button onClick={handleCreate}
                  className="px-4 py-2 bg-[#1e3a5f] text-white rounded cursor-pointer">
                  Crear
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}