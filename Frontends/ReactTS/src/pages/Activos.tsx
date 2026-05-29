import { useEffect, useState } from 'react';
import api from '../services/api';
import { useAuth } from '../context/AuthContext';
import type { Activo, Tag } from '../types';
import { Plus, Pencil, Trash2, X, AlertTriangle, ChevronLeft, ChevronRight } from 'lucide-react';

export default function Activos() {
  const [activos, setActivos] = useState<Activo[]>([]);
  const [tags, setTags] = useState<Tag[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<Activo | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Activo | null>(null);
  const [form, setForm] = useState({ placa: '', nombre: '', categoria: '', tenedorResponsable: '', tagId: '' });
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const totalPages = Math.ceil(total / pageSize);
  const { hasRole } = useAuth();
  const isGestion = hasRole('Usuario de Gestión');

  const loadActivos = async (p?: number, ps?: number) => {
    const currentPage = p ?? page;
    const currentSize = ps ?? pageSize;
    const { data } = await api.get(`/api/activos?page=${currentPage}&pageSize=${currentSize}`);
    if (data.success) {
      setActivos(data.data.items);
      setTotal(data.data.total);
      setPage(data.data.page);
    }
  };

  const loadTags = async () => {
    const { data } = await api.get('/api/tags/disponibles');
    if (data.success) setTags(data.data);
  };

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadActivos(1);
    loadTags();
  }, []);

  const openCreate = () => {
    setEditing(null);
    setForm({ placa: '', nombre: '', categoria: '', tenedorResponsable: '', tagId: '' });
    setShowModal(true);
  };

  const openEdit = (activo: Activo) => {
    setEditing(activo);
    setForm({
      placa: activo.placa,
      nombre: activo.nombre,
      categoria: activo.categoria,
      tenedorResponsable: activo.tenedorResponsable || '',
      tagId: activo.tagId || ''
    });
    setShowModal(true);
  };

  const handleSave = async () => {
    try {
      if (editing) {
        await api.put(`/api/activos/${editing.id}`, form);
      } else {
        await api.post('/api/activos', form);
      }
      setShowModal(false);
      loadActivos(1);
      loadTags();
    } catch (err) {
      console.error('Error saving activo:', err);
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      await api.delete(`/api/activos/${deleteTarget.id}`);
      setDeleteTarget(null);
      loadActivos(1);
      loadTags();
    } catch (err) {
      console.error('Error deleting activo:', err);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-gray-800 dark:text-gray-100">Gestión de Activos</h2>
        {isGestion && (
          <button
            onClick={openCreate}
            className="flex items-center gap-2 bg-[#1e3a5f] text-white px-4 py-2 rounded hover:bg-[#2d5a8e] transition cursor-pointer"
          >
            <Plus size={18} /> Registrar Activo
          </button>
        )}
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Placa</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Nombre</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Categoría</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Responsable</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Ubicación</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">TAG</th>
              {isGestion && <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Acciones</th>}
            </tr>
          </thead>
          <tbody>
            {activos.map((a) => (
              <tr key={a.id} className="border-t border-gray-100 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700">
                <td className="px-4 py-3 font-medium">{a.placa}</td>
                <td className="px-4 py-3">{a.nombre}</td>
                <td className="px-4 py-3">{a.categoria}</td>
                <td className="px-4 py-3">{a.tenedorResponsable || '-'}</td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-0.5 rounded text-xs font-medium ${
                    a.estadoUbicacion === 'DENTRO_INSTALACIONES'
                      ? 'bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300'
                      : 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/40 dark:text-yellow-300'
                  }`}>
                    {a.estadoUbicacion === 'DENTRO_INSTALACIONES' ? 'Dentro' : 'Fuera'}
                  </span>
                </td>
                <td className="px-4 py-3 text-xs text-gray-500 dark:text-gray-400">{a.tagId || '-'}</td>
                {isGestion && (
                  <td className="px-4 py-3">
                    <div className="flex gap-2">
                      <button onClick={() => openEdit(a)} className="p-1 hover:bg-gray-100 dark:hover:bg-gray-700 rounded cursor-pointer">
                        <Pencil size={16} className="text-blue-600" />
                      </button>
                      <button onClick={() => setDeleteTarget(a)} className="p-1 hover:bg-gray-100 dark:hover:bg-gray-700 rounded cursor-pointer">
                        <Trash2 size={16} className="text-red-600" />
                      </button>
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {total > 0 && (
        <div className="flex items-center justify-between bg-white dark:bg-gray-800 rounded-lg shadow px-4 py-3">
          <div className="flex items-center gap-3">
            <span className="text-sm text-gray-500 dark:text-gray-400">
              Mostrando {(page - 1) * pageSize + 1}-{Math.min(page * pageSize, total)} de {total} activos
            </span>
            <select
              value={pageSize}
              onChange={(e) => {
                const newSize = Number(e.target.value);
                setPageSize(newSize);
                setPage(1);
                loadActivos(1, newSize);
              }}
              className="text-sm border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-2 py-1 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f] cursor-pointer"
            >
              <option value={5}>5</option>
              <option value={10}>10</option>
              <option value={15}>15</option>
              <option value={25}>25</option>
              <option value={50}>50</option>
            </select>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={() => loadActivos(page - 1)}
              disabled={page <= 1}
              className="p-1.5 rounded hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-30 disabled:cursor-not-allowed cursor-pointer"
            >
              <ChevronLeft size={18} />
            </button>
            {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
              <button
                key={p}
                onClick={() => loadActivos(p)}
                className={`px-3 py-1 rounded text-sm font-medium cursor-pointer ${
                  p === page
                    ? 'bg-[#1e3a5f] text-white'
                    : 'hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-600 dark:text-gray-300'
                }`}
              >
                {p}
              </button>
            ))}
            <button
              onClick={() => loadActivos(page + 1)}
              disabled={page >= totalPages}
              className="p-1.5 rounded hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-30 disabled:cursor-not-allowed cursor-pointer"
            >
              <ChevronRight size={18} />
            </button>
          </div>
        </div>
      )}

      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg w-full max-w-lg">
            <div className="flex items-center justify-between px-6 py-4 border-b dark:border-gray-700">
              <h3 className="font-semibold text-lg">
                {editing ? 'Editar Activo' : 'Registrar Activo'}
              </h3>
              <button onClick={() => setShowModal(false)} className="cursor-pointer">
                <X size={20} />
              </button>
            </div>
            <div className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Placa *</label>
                <input
                  value={form.placa}
                  onChange={(e) => setForm({ ...form, placa: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Nombre *</label>
                <input
                  value={form.nombre}
                  onChange={(e) => setForm({ ...form, nombre: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Categoría *</label>
                <select
                  value={form.categoria}
                  onChange={(e) => setForm({ ...form, categoria: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
                >
                  <option value="">Seleccionar...</option>
                  <option value="Sillas">Sillas</option>
                  <option value="Impresoras">Impresoras</option>
                  <option value="Electrodomésticos">Electrodomésticos</option>
                  <option value="Computadores">Computadores</option>
                  <option value="Herramientas">Herramientas</option>
                  <option value="Otros">Otros</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Responsable</label>
                <input
                  value={form.tenedorResponsable}
                  onChange={(e) => setForm({ ...form, tenedorResponsable: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-1">Tag RFID</label>
                <select
                  value={form.tagId}
                  onChange={(e) => setForm({ ...form, tagId: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
                >
                  <option value="">Sin TAG</option>
                  {tags.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.id}
                    </option>
                  ))}
                </select>
              </div>
              <div className="flex justify-end gap-3 pt-4">
                <button
                  onClick={() => setShowModal(false)}
                  className="px-4 py-2 border border-gray-300 dark:border-gray-600 dark:text-gray-200 rounded hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer"
                >
                  Cancelar
                </button>
                <button
                  onClick={handleSave}
                  className="px-4 py-2 bg-[#1e3a5f] text-white rounded hover:bg-[#2d5a8e] cursor-pointer"
                >
                  {editing ? 'Actualizar' : 'Registrar'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {deleteTarget && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg w-full max-w-md p-6">
            <div className="flex items-center gap-3 text-red-600 mb-4">
              <AlertTriangle size={24} />
              <h3 className="font-semibold text-lg">Confirmar Eliminación</h3>
            </div>
            <p className="text-gray-600 dark:text-gray-300 mb-6">
              ¿Estás seguro de eliminar el activo <strong>{deleteTarget.placa}</strong> ({deleteTarget.nombre})?
              Esta acción no se puede deshacer.
            </p>
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setDeleteTarget(null)}
                className="px-4 py-2 border border-gray-300 dark:border-gray-600 dark:text-gray-200 rounded hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer"
              >
                Cancelar
              </button>
              <button
                onClick={handleDelete}
                className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 cursor-pointer"
              >
                Eliminar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
