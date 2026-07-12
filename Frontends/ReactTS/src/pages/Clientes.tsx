import { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';
import type { Cliente, Proveedor } from '../types';
import { Plus, Pencil, Trash2, ChevronLeft, ChevronRight } from 'lucide-react';

export default function Clientes() {
  const { hasRole } = useAuth();
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<Cliente | null>(null);
  const [form, setForm] = useState({ nombre: '', razonSocial: '', ruc: '', direccion: '', telefono: '', email: '', proveedorId: '' });
  const [deleteTarget, setDeleteTarget] = useState<Cliente | null>(null);

  const canManage = hasRole(['Administrador del Sistema', 'Asistente del Administrador del Sistema', 'Administrador del Proveedor']);

  const load = async (p?: number) => {
    const currentPage = p ?? page;
    const { data } = await api.get(`/api/clientes?page=${currentPage}&pageSize=${pageSize}`);
    if (data.success) {
      setClientes(data.data.items);
      setTotal(data.data.total);
      setPage(data.data.page);
    }
  };

  const loadProveedores = async () => {
    const { data } = await api.get('/api/proveedores?pageSize=200');
    if (data.success) setProveedores(data.data.items);
  };

  useEffect(() => { load(1); loadProveedores(); }, [pageSize]);

  const openCreate = () => { setEditing(null); setForm({ nombre: '', razonSocial: '', ruc: '', direccion: '', telefono: '', email: '', proveedorId: '' }); setShowModal(true); };
  const openEdit = (c: Cliente) => { setEditing(c); setForm({ nombre: c.nombre, razonSocial: c.razonSocial, ruc: c.ruc, direccion: c.direccion || '', telefono: c.telefono || '', email: c.email || '', proveedorId: c.proveedorId || '' }); setShowModal(true); };

  const save = async () => {
    const body = { ...form, proveedorId: form.proveedorId || null };
    if (editing) { await api.put(`/api/clientes/${editing.id}`, body); }
    else { await api.post('/api/clientes', body); }
    setShowModal(false); load(1);
  };

  const del = async () => {
    if (!deleteTarget) return;
    await api.delete(`/api/clientes/${deleteTarget.id}`);
    setDeleteTarget(null); load(1);
  };

  const totalPages = Math.ceil(total / pageSize);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold dark:text-white">Clientes</h2>
        {canManage && (
          <button onClick={openCreate} className="flex items-center gap-2 bg-[#1e3a5f] text-white px-4 py-2 rounded-lg hover:bg-[#2d5a8e] transition">
            <Plus size={16} /> Nuevo Cliente
          </button>
        )}
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 dark:bg-gray-700">
              <tr>
                <th className="px-4 py-3 text-left dark:text-gray-200">Nombre</th>
                <th className="px-4 py-3 text-left dark:text-gray-200">RUC</th>
                <th className="px-4 py-3 text-left dark:text-gray-200">Proveedor</th>
                <th className="px-4 py-3 text-left dark:text-gray-200">Email</th>
                {canManage && <th className="px-4 py-3 text-left dark:text-gray-200">Acciones</th>}
              </tr>
            </thead>
            <tbody className="divide-y dark:divide-gray-700">
              {clientes.map(c => (
                <tr key={c.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-4 py-3 dark:text-gray-200">{c.nombre}</td>
                  <td className="px-4 py-3 dark:text-gray-200">{c.ruc}</td>
                  <td className="px-4 py-3 dark:text-gray-200">{c.proveedorNombre || '-'}</td>
                  <td className="px-4 py-3 dark:text-gray-200">{c.email || '-'}</td>
                  {canManage && (
                    <td className="px-4 py-3 flex gap-2">
                      <button onClick={() => openEdit(c)} className="text-blue-500 hover:text-blue-700"><Pencil size={16} /></button>
                      <button onClick={() => setDeleteTarget(c)} className="text-red-500 hover:text-red-700"><Trash2 size={16} /></button>
                    </td>
                  )}
                </tr>
              ))}
              {clientes.length === 0 && <tr><td colSpan={5} className="px-4 py-8 text-center text-gray-400 dark:text-gray-500">No hay clientes</td></tr>}
            </tbody>
          </table>
        </div>
        <div className="flex items-center justify-between px-4 py-3 border-t dark:border-gray-700">
          <span className="text-sm text-gray-500 dark:text-gray-400">Mostrando {clientes.length} de {total}</span>
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
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md">
            <h3 className="text-lg font-bold mb-4 dark:text-white">{editing ? 'Editar Cliente' : 'Nuevo Cliente'}</h3>
            <div className="space-y-3">
              <input placeholder="Nombre" value={form.nombre} onChange={e => setForm({ ...form, nombre: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Razon Social" value={form.razonSocial} onChange={e => setForm({ ...form, razonSocial: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="RUC" value={form.ruc} onChange={e => setForm({ ...form, ruc: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Direccion" value={form.direccion} onChange={e => setForm({ ...form, direccion: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Telefono" value={form.telefono} onChange={e => setForm({ ...form, telefono: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Email" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <select value={form.proveedorId} onChange={e => setForm({ ...form, proveedorId: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200">
                <option value="">Sin proveedor</option>
                {proveedores.map(p => <option key={p.id} value={p.id}>{p.nombre}</option>)}
              </select>
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
            <p className="dark:text-gray-200">Eliminar cliente <strong>{deleteTarget.nombre}</strong>?</p>
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
