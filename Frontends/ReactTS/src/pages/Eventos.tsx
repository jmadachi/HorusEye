import { useEffect, useState } from 'react';
import api from '../services/api';
import type { Evento, Cliente, NodoUbicacion, DispositivoRfid, ApiResponse } from '../types';
import { Plus, Pencil, Trash2, Zap } from 'lucide-react';

export default function Eventos() {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [clienteSeleccionado, setClienteSeleccionado] = useState('');
  const [nodos, setNodos] = useState<NodoUbicacion[]>([]);
  const [nodoSeleccionado, setNodoSeleccionado] = useState('');
  const [dispositivos, setDispositivos] = useState<DispositivoRfid[]>([]);
  const [eventos, setEventos] = useState<Evento[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<Evento | null>(null);
  const [form, setForm] = useState({ nombre: '', nodoUbicacionId: '', lectores: [{ dispositivoRfidId: '', orden: 1 }] });

  const loadClientes = async () => {
    const { data } = await api.get('/api/clientes?pageSize=200') as { data: ApiResponse<{ items: Cliente[] }> };
    if (data.success) setClientes(data.data.items);
  };

  const loadNodos = async () => {
    if (!clienteSeleccionado) { setNodos([]); return; }
    const { data } = await api.get(`/api/ubicaciones?clienteId=${clienteSeleccionado}&pageSize=200`) as { data: ApiResponse<{ items: NodoUbicacion[] }> };
    if (data.success) setNodos(data.data.items);
  };

  const loadDispositivos = async () => {
    if (!clienteSeleccionado) { setDispositivos([]); return; }
    const { data } = await api.get(`/api/dispositivos?clienteId=${clienteSeleccionado}&pageSize=200`) as { data: ApiResponse<{ items: DispositivoRfid[] }> };
    if (data.success) setDispositivos(data.data.items.filter(d => d.tipoDispositivo === 'FIXED' || d.tipoDispositivo === 'HANDHELD'));
  };

  const loadEventos = async () => {
    const params = nodoSeleccionado
      ? `nodoUbicacionId=${nodoSeleccionado}`
      : clienteSeleccionado
        ? `clienteId=${clienteSeleccionado}`
        : '';
    const { data } = await api.get(`/api/eventos?${params}&pageSize=200`) as { data: ApiResponse<{ items: Evento[] }> };
    if (data.success) setEventos(data.data.items);
  };

  useEffect(() => { loadClientes(); }, []);
  useEffect(() => { loadNodos(); loadDispositivos(); setNodoSeleccionado(''); }, [clienteSeleccionado]);
  useEffect(() => { loadEventos(); }, [clienteSeleccionado, nodoSeleccionado]);

  const openCreate = () => {
    setEditing(null);
    setForm({ nombre: '', nodoUbicacionId: nodoSeleccionado, lectores: [{ dispositivoRfidId: '', orden: 1 }] });
    setShowModal(true);
  };

  const openEdit = (e: Evento) => {
    setEditing(e);
    setForm({
      nombre: e.nombre,
      nodoUbicacionId: e.nodoUbicacionId,
      lectores: e.lectores.map(l => ({ dispositivoRfidId: l.dispositivoRfidId, orden: l.orden }))
    });
    setShowModal(true);
  };

  const addLector = () => {
    setForm({ ...form, lectores: [...form.lectores, { dispositivoRfidId: '', orden: form.lectores.length + 1 }] });
  };

  const removeLector = (idx: number) => {
    const newLectores = form.lectores.filter((_, i) => i !== idx).map((l, i) => ({ ...l, orden: i + 1 }));
    setForm({ ...form, lectores: newLectores });
  };

  const updateLector = (idx: number, field: string, value: string | number) => {
    const newLectores = [...form.lectores];
    newLectores[idx] = { ...newLectores[idx], [field]: value };
    setForm({ ...form, lectores: newLectores });
  };

  const save = async () => {
    const payload = {
      ...form,
      clienteId: clienteSeleccionado,
      lectores: form.lectores.filter(l => l.dispositivoRfidId).map((l, i) => ({ ...l, orden: i + 1 }))
    };
    if (editing) {
      await api.put(`/api/eventos/${editing.id}`, payload);
    } else {
      await api.post('/api/eventos', payload);
    }
    setShowModal(false);
    loadEventos();
  };

  const del = async (id: string) => {
    if (!confirm('Eliminar este evento?')) return;
    await api.delete(`/api/eventos/${id}`);
    loadEventos();
  };

  const getDispositivoNombre = (id: string) => dispositivos.find(d => d.id === id)?.nombre || id;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold dark:text-white">Eventos</h2>
        {nodoSeleccionado && (
          <button onClick={openCreate} className="flex items-center gap-2 bg-[#1e3a5f] text-white px-4 py-2 rounded-lg hover:bg-[#2d5a8e] transition">
            <Plus size={16} /> Nuevo Evento
          </button>
        )}
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-4 grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium dark:text-gray-200 mb-1">Cliente</label>
          <select value={clienteSeleccionado} onChange={e => setClienteSeleccionado(e.target.value)}
            className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200">
            <option value="">Seleccionar cliente...</option>
            {clientes.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium dark:text-gray-200 mb-1">Ubicacion</label>
          <select value={nodoSeleccionado} onChange={e => setNodoSeleccionado(e.target.value)}
            className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200">
            <option value="">Todas las ubicaciones</option>
            {nodos.filter(n => !n.padreId).map(n => (
              <optgroup key={n.id} label={`${n.tipoNodo}: ${n.nombre}`}>
                {nodos.filter(c => c.padreId === n.id).map(c => (
                  <option key={c.id} value={c.id}>{c.tipoNodo}: {c.nombre}</option>
                ))}
              </optgroup>
            ))}
          </select>
        </div>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
        <div className="p-4">
          {eventos.length === 0 ? (
            <p className="text-gray-400 dark:text-gray-500 text-sm">
              {nodoSeleccionado ? 'No hay eventos para esta ubicacion. Seleccione una ubicacion para crear eventos.' : 'Seleccione un cliente y una ubicacion para ver los eventos.'}
            </p>
          ) : (
            <div className="space-y-3">
              {eventos.map(e => (
                <div key={e.id} className="border dark:border-gray-600 rounded-lg p-4">
                  <div className="flex items-center justify-between mb-2">
                    <div className="flex items-center gap-2">
                      <Zap size={16} className="text-yellow-500" />
                      <span className="font-semibold dark:text-white">{e.nombre}</span>
                      {e.nodoUbicacionNombre && (
                        <span className="text-xs px-2 py-0.5 rounded bg-gray-200 dark:bg-gray-600 dark:text-gray-300">
                          {e.nodoUbicacionNombre}
                        </span>
                      )}
                    </div>
                    <div className="flex items-center gap-2">
                      <button onClick={() => openEdit(e)} className="text-blue-500 hover:text-blue-700"><Pencil size={14} /></button>
                      <button onClick={() => del(e.id)} className="text-red-500 hover:text-red-700"><Trash2 size={14} /></button>
                    </div>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {e.lectores.map((l, i) => (
                      <span key={l.id} className="text-xs px-2 py-1 rounded bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300">
                        {i + 1}. {l.dispositivoNombre || getDispositivoNombre(l.dispositivoRfidId)}
                      </span>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-lg">
            <h3 className="text-lg font-bold mb-4 dark:text-white">{editing ? 'Editar Evento' : 'Nuevo Evento'}</h3>
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium dark:text-gray-200 mb-1">Nombre del Evento</label>
                <input placeholder="ej: Entrada, Salida, Traslado" value={form.nombre}
                  onChange={e => setForm({ ...form, nombre: e.target.value })}
                  className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              </div>
              <div>
                <label className="block text-sm font-medium dark:text-gray-200 mb-1">Secuencia de Lectores</label>
                <p className="text-xs text-gray-400 mb-2">El orden define la secuencia. El primer lector es el primero en detectar el tag.</p>
                <div className="space-y-2">
                  {form.lectores.map((l, i) => (
                    <div key={i} className="flex items-center gap-2">
                      <span className="text-sm font-medium text-gray-500 w-6">{i + 1}.</span>
                      <select value={l.dispositivoRfidId} onChange={e => updateLector(i, 'dispositivoRfidId', e.target.value)}
                        className="flex-1 border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200">
                        <option value="">Seleccionar lector...</option>
                        {dispositivos.map(d => <option key={d.id} value={d.id}>{d.nombre} ({d.tipoDispositivo})</option>)}
                      </select>
                      {form.lectores.length > 1 && (
                        <button onClick={() => removeLector(i)} className="text-red-500 hover:text-red-700"><Trash2 size={14} /></button>
                      )}
                    </div>
                  ))}
                </div>
                <button onClick={addLector} className="mt-2 text-sm text-blue-500 hover:text-blue-700 flex items-center gap-1">
                  <Plus size={14} /> Agregar lector
                </button>
              </div>
            </div>
            <div className="flex justify-end gap-2 mt-4">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded">Cancelar</button>
              <button onClick={save} className="px-4 py-2 bg-[#1e3a5f] text-white rounded hover:bg-[#2d5a8e]">Guardar</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
