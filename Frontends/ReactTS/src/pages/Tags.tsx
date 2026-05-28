import { useEffect, useState } from 'react';
import api from '../services/api';
import { useAuth } from '../context/AuthContext';
import type { Tag } from '../types';
import { Plus, AlertTriangle } from 'lucide-react';

export default function TagsPage() {
  const [tags, setTags] = useState<Tag[]>([]);
  const [showRegister, setShowRegister] = useState(false);
  const [newTagId, setNewTagId] = useState('');
  const [showDamage, setShowDamage] = useState<{ tagId: string; descripcion: string } | null>(null);
  const { hasRole } = useAuth();
  const isGestion = hasRole('Usuario de Gestión');

  useEffect(() => { loadTags(); }, []);

  const loadTags = async () => {
    const { data } = await api.get('/api/tags');
    if (data.success) setTags(data.data);
  };

  const registerTag = async () => {
    try {
      await api.post('/api/tags', JSON.stringify(newTagId), {
        headers: { 'Content-Type': 'application/json' }
      });
      setNewTagId('');
      setShowRegister(false);
      loadTags();
    } catch (err) {
      console.error('Error registering tag:', err);
    }
  };

  const updateEstado = async (tagId: string, estado: string) => {
    try {
      await api.put(`/api/tags/${tagId}/estado`, JSON.stringify(estado), {
        headers: { 'Content-Type': 'application/json' }
      });
      loadTags();
    } catch (err) {
      console.error('Error updating tag:', err);
    }
  };

  const reportarDanio = async () => {
    if (!showDamage) return;
    try {
      await api.post(`/api/tags/${showDamage.tagId}/reportar-danio`,
        JSON.stringify(showDamage.descripcion),
        { headers: { 'Content-Type': 'application/json' } }
      );
      setShowDamage(null);
      loadTags();
    } catch (err) {
      console.error('Error reporting damage:', err);
    }
  };

  const getEstadoColor = (estado: string) => {
    const colors: Record<string, string> = {
      'DISPONIBLE': 'bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300',
      'ASIGNADO': 'bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300',
      'REGISTRADO': 'bg-gray-100 text-gray-800 dark:bg-gray-600 dark:text-gray-200',
      'DAÑADO': 'bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-300'
    };
    return colors[estado] || 'bg-gray-100';
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-gray-800 dark:text-gray-100">Tags RFID</h2>
        {isGestion && (
          <button
            onClick={() => setShowRegister(true)}
            className="flex items-center gap-2 bg-[#1e3a5f] text-white px-4 py-2 rounded hover:bg-[#2d5a8e] transition cursor-pointer"
          >
            <Plus size={18} /> Registrar Tag
          </button>
        )}
      </div>

      {showRegister && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg w-full max-w-md p-6">
            <h3 className="font-semibold text-lg mb-4">Registrar Nuevo Tag</h3>
            <input
              value={newTagId}
              onChange={(e) => setNewTagId(e.target.value)}
              placeholder="ID del TAG (EPC/UID)"
              className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 mb-4 focus:outline-none focus:ring-2 focus:ring-[#1e3a5f]"
            />
            <div className="flex justify-end gap-3">
              <button onClick={() => setShowRegister(false)}
                className="px-4 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded cursor-pointer">
                Cancelar
              </button>
              <button onClick={registerTag}
                className="px-4 py-2 bg-[#1e3a5f] text-white rounded cursor-pointer">
                Registrar
              </button>
            </div>
          </div>
        </div>
      )}

      {showDamage && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg w-full max-w-md p-6">
            <div className="flex items-center gap-3 text-red-600 mb-4">
              <AlertTriangle size={24} />
              <h3 className="font-semibold text-lg">Reportar Daño - {showDamage.tagId}</h3>
            </div>
            <textarea
              value={showDamage.descripcion}
              onChange={(e) => setShowDamage({ ...showDamage, descripcion: e.target.value })}
              placeholder="Descripción del daño..."
              rows={3}
              className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded px-3 py-2 mb-4 focus:outline-none focus:ring-2 focus:ring-red-500"
            />
            <div className="flex justify-end gap-3">
              <button onClick={() => setShowDamage(null)}
                className="px-4 py-2 border border-gray-300 dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100 rounded cursor-pointer">
                Cancelar
              </button>
              <button onClick={reportarDanio}
                className="px-4 py-2 bg-red-600 text-white rounded cursor-pointer">
                Reportar
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">ID Tag (EPC/UID)</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Estado</th>
              <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Fecha Registro</th>
              {isGestion && <th className="text-left px-4 py-3 text-gray-600 dark:text-gray-300">Acciones</th>}
            </tr>
          </thead>
          <tbody>
            {tags.map((t) => (
              <tr key={t.id} className="border-t border-gray-100 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700">
                <td className="px-4 py-3 font-mono text-xs">{t.id}</td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-0.5 rounded text-xs font-medium ${getEstadoColor(t.estado)}`}>
                    {t.estado}
                  </span>
                </td>
                <td className="px-4 py-3 text-gray-500 dark:text-gray-400">
                  {new Date(t.fechaRegistro).toLocaleString()}
                </td>
                {isGestion && (
                  <td className="px-4 py-3">
                    <div className="flex gap-2">
                      {t.estado !== 'DAÑADO' && (
                        <>
                          <button
                            onClick={() => updateEstado(t.id, 'DISPONIBLE')}
                            className="text-xs px-2 py-1 bg-green-100 text-green-700 rounded hover:bg-green-200 dark:bg-green-900/40 dark:text-green-300 dark:hover:bg-green-900/60 cursor-pointer"
                          >
                            Disponible
                          </button>
                          <button
                            onClick={() => setShowDamage({ tagId: t.id, descripcion: '' })}
                            className="text-xs px-2 py-1 bg-red-100 text-red-700 rounded hover:bg-red-200 dark:bg-red-900/40 dark:text-red-300 dark:hover:bg-red-900/60 cursor-pointer"
                          >
                            Dañado
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
