import { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';
import type { Proveedor } from '../types';
import { Plus, Pencil, Trash2, ChevronLeft, ChevronRight } from 'lucide-react';

export default function Proveedores() {
  const { hasRole } = useAuth();
  const [proveedores, setProveedores] = useState<Proveedor[]>([]);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<Proveedor | null>(null);
  const [form, setForm] = useState({ nombre: '', razonSocial: '', ruc: '', direccion: '', telefono: '', email: '' });
  const [deleteTarget, setDeleteTarget] = useState<Proveedor | null>(null);

  const canManage = hasRole(['Administrador del Sistema', 'Asistente del Administrador del Sistema']);

  const load = async (p?: number) => {
    const currentPage = p ?? page;
    const { data } = await api.get(`/api/proveedores?page=${currentPage}&pageSize=${pageSize}`);
    if (data.success) {
      setProveedores(data.data.items);
      setTotal(data.data.total);
      setPage(data.data.page);
    }
  };

  useEffect(() => { load(1); }, [pageSize]);

  const openCreate = () => { setEditing(null); setForm({ nombre: '', razonSocial: '', ruc: '', direccion: '', telefono: '', email: '' }); setShowModal(true); };
  const openEdit = (p: Proveedor) => { setEditing(p); setForm({ nombre: p.nombre, razonSocial: p.razonSocial, ruc: p.ruc, direccion: p.direccion || '', telefono: p.telefono || '', email: p.email || '' }); setShowModal(true); };

  const save = async () => {
    if (editing) { await api.put(`/api/proveedores/${editing.id}`, form); }
    else { await api.post('/api/proveedores', form); }
    setShowModal(false); load(1);
  };

  const del = async () => {
    if (!deleteTarget) return;
    await api.delete(`/api/proveedores/${deleteTarget.id}`);
    setDeleteTarget(null); load(1);
  };

  const totalPages = Math.ceil(total / pageSize);

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold dark:text-white">Proveedores</h2>
        {canManage && (
          <button onClick={openCreate} className="flex items-center gap-2 bg-[#1e3a5f] text-white px-4 py-2 rounded-lg hover:bg-[#2d5a8e] transition">
            <Plus size={16} /> Nuevo Proveedor
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
                <th className="px-4 py-3 text-left dark:text-gray-200">Email</th>
                <th className="px-4 py-3 text-left dark:text-gray-200">Telefono</th>
                {canManage && <th className="px-4 py-3 text-left dark:text-gray-200">Acciones</th>}
              </tr>
            </thead>
            <tbody className="divide-y dark:divide-gray-700">
              {proveedores.map(p => (
                <tr key={p.id} className="hover:bg-gray-50 dark:hover:bg-gray-700">
                  <td className="px-4 py-3 dark:text-gray-200">{p.nombre}</td>
                  <td className="px-4 py-3 dark:text-gray-200">{p.ruc}</td>
                  <td className="px-4 py-3 dark:text-gray-200">{p.email || '-'}</td>
                  <td className="px-4 py-3 dark:text-gray-200">{p.telefono || '-'}</td>
                  {canManage && (
                    <td className="px-4 py-3 flex gap-2">
                      <button onClick={() => openEdit(p)} className="text-blue-500 hover:text-blue-700"><Pencil size={16} /></button>
                      <button onClick={() => setDeleteTarget(p)} className="text-red-500 hover:text-red-700"><Trash2 size={16} /></button>
                    </td>
                  )}
                </tr>
              ))}
              {proveedores.length === 0 && <tr><td colSpan={5} className="px-4 py-8 text-center text-gray-400 dark:text-gray-500">No hay proveedores</td></tr>}
            </tbody>
          </table>
        </div>
        <div className="flex items-center justify-between px-4 py-3 border-t dark:border-gray-700">
          <span className="text-sm text-gray-500 dark:text-gray-400">Mostrando {proveedores.length} de {total}</span>
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
            <h3 className="text-lg font-bold mb-4 dark:text-white">{editing ? 'Editar Proveedor' : 'Nuevo Proveedor'}</h3>
            <div className="space-y-3">
              <input placeholder="Nombre" value={form.nombre} onChange={e => setForm({ ...form, nombre: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Razon Social" value={form.razonSocial} onChange={e => setForm({ ...form, razonSocial: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="RUC" value={form.ruc} onChange={e => setForm({ ...form, ruc: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Direccion" value={form.direccion} onChange={e => setForm({ ...form, direccion: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Telefono" value={form.telefono} onChange={e => setForm({ ...form, telefono: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Email" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
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
            <p className="dark:text-gray-200">Eliminar proveedor <strong>{deleteTarget.nombre}</strong>?</p>
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
