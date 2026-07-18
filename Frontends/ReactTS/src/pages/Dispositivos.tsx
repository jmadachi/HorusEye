import { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';
import type { DispositivoRfid, Cliente } from '../types';
import { Plus, Pencil, Trash2, ChevronLeft, ChevronRight } from 'lucide-react';

export default function Dispositivos() {
  const { hasRole } = useAuth();
  const [dispositivos, setDispositivos] = useState<DispositivoRfid[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<DispositivoRfid | null>(null);
  const [form, setForm] = useState({ nombre: '', fabricante: '', modelo: '', direccionIP: '', ubicacion: '', clienteId: '', tipoDispositivo: 'FIXED', endpointAPI: '', direccionPredeterminada: 'BIDIRECCIONAL', apiKey: '', requiereDireccion: false });
  const [deleteTarget, setDeleteTarget] = useState<DispositivoRfid | null>(null);

  const canManage = hasRole(['Administrador del Sistema', 'Asistente del Administrador del Sistema', 'Administrador del Proveedor', 'Administrador del Cliente']);

  const load = async (p?: number) => {
    const currentPage = p ?? page;
    const { data } = await api.get(`/api/dispositivos?page=${currentPage}&pageSize=${pageSize}`);
    if (data.success) {
      setDispositivos(data.data.items);
      setTotal(data.data.total);
      setPage(data.data.page);
    }
  };

  const loadClientes = async () => {
    const { data } = await api.get('/api/clientes?pageSize=200');
    if (data.success) setClientes(data.data.items);
  };

  useEffect(() => { load(1); loadClientes(); }, [pageSize]);

  const openCreate = () => { setEditing(null); setForm({ nombre: '', fabricante: 'Chainway', modelo: 'U300', direccionIP: '', ubicacion: '', clienteId: '', tipoDispositivo: 'FIXED', endpointAPI: '', direccionPredeterminada: 'BIDIRECCIONAL', apiKey: '', requiereDireccion: false }); setShowModal(true); };
  const openEdit = (d: DispositivoRfid) => { setEditing(d); setForm({ nombre: d.nombre, fabricante: d.fabricante, modelo: d.modelo, direccionIP: d.direccionIP || '', ubicacion: d.ubicacion || '', clienteId: d.clienteId || '', tipoDispositivo: d.tipoDispositivo, endpointAPI: d.endpointAPI || '', direccionPredeterminada: d.direccionPredeterminada || 'BIDIRECCIONAL', apiKey: d.apiKey || '', requiereDireccion: d.requiereDireccion }); setShowModal(true); };

  const save = async () => {
    const body = { ...form, clienteId: form.clienteId || null };
    if (editing) { await api.put(`/api/dispositivos/${editing.id}`, body); }
    else { await api.post('/api/dispositivos', body); }
    setShowModal(false); load(1);
  };

  const del = async () => {
    if (!deleteTarget) return;
    await api.delete(`/api/dispositivos/${deleteTarget.id}`);
    setDeleteTarget(null); load(1);
  };

  const totalPages = Math.ceil(total / pageSize);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold dark:text-white">Dispositivos RFID</h2>
        {canManage && (
          <button onClick={openCreate} className="flex items-center gap-2 bg-[#1e3a5f] text-white px-4 py-2 rounded-lg hover:bg-[#2d5a8e] transition">
            <Plus size={16} /> Nuevo Dispositivo
          </button>
        )}
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 dark:bg-gray-700">
              <tr>
                <th className="px-4 py-3 text-left dark:text-gray-200">Nombre</th>
                <th className="px-4 py-3 text-left dark:text-gray-200">Fabricante</th>
                <th className="px-4 py-3 text-left dark:text-gray-200">Modelo</th>
                <th className="px-4 py-3 text-left dark:text-gray-200">Cliente</th>
                <th className="px-4 py-3 text-left dark:text-gray-200">Tipo</th>
                <th className="px-4 py-3 text-left dark:text-gray-200">Direccion</th>
                {canManage && <th className="px-4 py-3 text-left dark:text-gray-200">Acciones</th>}
              </tr>
            </thead>
            <tbody className="divide-y dark:divide-gray-700">
              {dispositivos.map(d => (
                <tr key={d.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-4 py-3 dark:text-gray-200">{d.nombre}</td>
                  <td className="px-4 py-3 dark:text-gray-200">{d.fabricante}</td>
                  <td className="px-4 py-3 dark:text-gray-200">{d.modelo}</td>
                  <td className="px-4 py-3 dark:text-gray-200">{d.clienteNombre || '-'}</td>
                  <td className="px-4 py-3"><span className="px-2 py-1 text-xs rounded-full bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">{d.tipoDispositivo}</span></td>
                  <td className="px-4 py-3"><span className={`px-2 py-1 text-xs rounded-full ${d.direccionPredeterminada === 'ENTRADA' ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200' : d.direccionPredeterminada === 'SALIDA' ? 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200' : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200'}`}>{d.direccionPredeterminada || 'BIDIRECCIONAL'}</span></td>
                  {canManage && (
                    <td className="px-4 py-3 flex gap-2">
                      <button onClick={() => openEdit(d)} className="text-blue-500 hover:text-blue-700"><Pencil size={16} /></button>
                      <button onClick={() => setDeleteTarget(d)} className="text-red-500 hover:text-red-700"><Trash2 size={16} /></button>
                    </td>
                  )}
                </tr>
              ))}
              {dispositivos.length === 0 && <tr><td colSpan={6} className="px-4 py-8 text-center text-gray-400 dark:text-gray-500">No hay dispositivos</td></tr>}
            </tbody>
          </table>
        </div>
        <div className="flex items-center justify-between px-4 py-3 border-t dark:border-gray-700">
          <span className="text-sm text-gray-500 dark:text-gray-400">Mostrando {dispositivos.length} de {total}</span>
          <div className="flex items-center gap-2">
            <select value={pageSize} onChange={e => { setPageSize(Number(e.target.value)); setPage(1); }} className="border dark:border-gray-600 rounded px-2 py-1 text-sm dark:bg-gray-700 dark:text-gray-200">
              {[5, 10, 15, 25, 50].map(n => <option key={n} value={n}>{n}</option>)}
            </select>
            <button onClick={() => load(page - 1)} disabled={page <= 1} className="p-1 rounded hover:bg-gray-200 dark:hover:bg-gray-700 disabled:opacity-30"><ChevronLeft size={18} /></button>
            <span className="text-sm dark:text-gray-200">{page}/{totalPages || 1}</span>
            <button onClick={() => load(page + 1)} disabled={page >= totalPages} className="p-1 rounded hover:bg-gray-200 dark:hover:bg-gray-700 disabled:opacity-30"><ChevronRight size={18} /></button>
          </div>
        </div>
      </div>

      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md max-h-[90vh] overflow-y-auto">
            <h3 className="text-lg font-bold mb-4 dark:text-white">{editing ? 'Editar Dispositivo' : 'Nuevo Dispositivo'}</h3>
            <div className="space-y-3">
              <input placeholder="Nombre" value={form.nombre} onChange={e => setForm({ ...form, nombre: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Fabricante" value={form.fabricante} onChange={e => setForm({ ...form, fabricante: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Modelo" value={form.modelo} onChange={e => setForm({ ...form, modelo: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Direccion IP" value={form.direccionIP} onChange={e => setForm({ ...form, direccionIP: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Ubicacion" value={form.ubicacion} onChange={e => setForm({ ...form, ubicacion: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <select value={form.clienteId} onChange={e => setForm({ ...form, clienteId: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200">
                <option value="">Sin cliente</option>
                {clientes.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
              </select>
              <select value={form.tipoDispositivo} onChange={e => setForm({ ...form, tipoDispositivo: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200">
                <option value="FIXED">Fijo</option>
                <option value="HANDHELD">Portatil</option>
                <option value="GATE">Arco</option>
              </select>
              <input placeholder="Endpoint API" value={form.endpointAPI} onChange={e => setForm({ ...form, endpointAPI: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <div>
                <label className="block text-xs font-medium mb-1 dark:text-gray-300">Direccion Predeterminada</label>
                <select value={form.direccionPredeterminada} onChange={e => setForm({ ...form, direccionPredeterminada: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200">
                  <option value="BIDIRECCIONAL">Bidireccional (el dispositivo indica)</option>
                  <option value="ENTRADA">Entrada (siempre INGRESO)</option>
                  <option value="SALIDA">Salida (siempre SALIDA)</option>
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium mb-1 dark:text-gray-300">API Key del Dispositivo</label>
                <input placeholder="Clave unica para autenticar este dispositivo" value={form.apiKey} onChange={e => setForm({ ...form, apiKey: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200 font-mono text-sm" />
              </div>
              <label className="flex items-center gap-2 dark:text-gray-200 text-sm">
                <input type="checkbox" checked={form.requiereDireccion} onChange={e => setForm({ ...form, requiereDireccion: e.target.checked })} className="rounded" />
                Requiere que el dispositivo envie tipoMovimiento
              </label>
            </div>
            <div className="flex justify-end gap-2 mt-4">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded">Cancelar</button>
              <button onClick={save} className="px-4 py-2 bg-[#1e3a5f] text-white rounded hover:bg-[#2d5a8e]">Guardar</button>
            </div>
          </div>
        </div>
      )}

      {deleteTarget && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-sm">
            <p className="dark:text-gray-200">Eliminar dispositivo <strong>{deleteTarget.nombre}</strong>?</p>
            <div className="flex justify-end gap-2 mt-4">
              <button onClick={() => setDeleteTarget(null)} className="px-4 py-2 text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded">Cancelar</button>
              <button onClick={del} className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700">Eliminar</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
