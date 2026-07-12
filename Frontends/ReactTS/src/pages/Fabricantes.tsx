import { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';
import type { FabricanteDispositivo, ApiResponse } from '../types';
import { Plus, Pencil, Trash2, ChevronDown, ChevronRight } from 'lucide-react';

export default function Fabricantes() {
  const { hasRole } = useAuth();
  const [fabricantes, setFabricantes] = useState<FabricanteDispositivo[]>([]);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<FabricanteDispositivo | null>(null);
  const [form, setForm] = useState({ nombre: '', descripcion: '', urlDocumentacion: '', endpointEjemplo: '' });
  const [expanded, setExpanded] = useState<Set<string>>(new Set());
  const [showCampoModal, setShowCampoModal] = useState(false);
  const [campoFabricanteId, setCampoFabricanteId] = useState('');
  const [campoForm, setCampoForm] = useState({ nombreCampoExterno: '', nombreCampoInterno: 'EPC', tipoDato: 'string', requerido: true, valorDefecto: '', ordenExtraccion: 0 });

  const canManage = hasRole(['Administrador del Sistema', 'Asistente del Administrador del Sistema']);

  const load = async () => {
    const { data } = await api.get('/api/fabricantes') as { data: ApiResponse<FabricanteDispositivo[]> };
    if (data.success) setFabricantes(data.data);
  };

  useEffect(() => { load(); }, []);

  const openCreate = () => { setEditing(null); setForm({ nombre: '', descripcion: '', urlDocumentacion: '', endpointEjemplo: '' }); setShowModal(true); };
  const openEdit = (f: FabricanteDispositivo) => { setEditing(f); setForm({ nombre: f.nombre, descripcion: f.descripcion || '', urlDocumentacion: f.urlDocumentacion || '', endpointEjemplo: f.endpointEjemplo || '' }); setShowModal(true); };

  const save = async () => {
    if (editing) { await api.put(`/api/fabricantes/${editing.id}`, form); }
    else { await api.post('/api/fabricantes', form); }
    setShowModal(false); load();
  };

  const del = async (id: string) => {
    await api.delete(`/api/fabricantes/${id}`);
    load();
  };

  const saveCampo = async () => {
    await api.post(`/api/fabricantes/${campoFabricanteId}/campos`, campoForm);
    setShowCampoModal(false); load();
  };

  const delCampo = async (campoId: string) => {
    await api.delete(`/api/fabricantes/campo/${campoId}`);
    load();
  };

  const toggleExpand = (id: string) => {
    setExpanded(prev => { const n = new Set(prev); if (n.has(id)) n.delete(id); else n.add(id); return n; });
  };

  const camposInternos = ['EPC', 'RSSI', 'Antenna', 'Timestamp', 'DeviceId', 'TID'];

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold dark:text-white">Fabricantes de Dispositivos</h2>
        {canManage && (
          <button onClick={openCreate} className="flex items-center gap-2 bg-[#1e3a5f] text-white px-4 py-2 rounded-lg hover:bg-[#2d5a8e] transition">
            <Plus size={16} /> Nuevo Fabricante
          </button>
        )}
      </div>

      <div className="space-y-3">
        {fabricantes.map(f => (
          <div key={f.id} className="bg-white dark:bg-gray-800 rounded-lg shadow">
            <div className="flex items-center justify-between p-4">
              <div className="flex items-center gap-3">
                <button onClick={() => toggleExpand(f.id)} className="text-gray-500">
                  {expanded.has(f.id) ? <ChevronDown size={18} /> : <ChevronRight size={18} />}
                </button>
                <div>
                  <h3 className="font-medium dark:text-gray-200">{f.nombre}</h3>
                  <p className="text-sm text-gray-500 dark:text-gray-400">{f.descripcion || 'Sin descripcion'} | {f.campos?.length || 0} campos</p>
                </div>
              </div>
              {canManage && (
                <div className="flex gap-2">
                  <button onClick={() => openEdit(f)} className="text-blue-500 hover:text-blue-700"><Pencil size={16} /></button>
                  <button onClick={() => del(f.id)} className="text-red-500 hover:text-red-700"><Trash2 size={16} /></button>
                </div>
              )}
            </div>
            {expanded.has(f.id) && f.campos && (
              <div className="border-t dark:border-gray-700 p-4">
                <div className="flex justify-between mb-2">
                  <h4 className="text-sm font-medium dark:text-gray-300">Campos del Payload</h4>
                  {canManage && (
                    <button onClick={() => { setCampoFabricanteId(f.id); setCampoForm({ nombreCampoExterno: '', nombreCampoInterno: 'EPC', tipoDato: 'string', requerido: true, valorDefecto: '', ordenExtraccion: f.campos?.length || 0 }); setShowCampoModal(true); }} className="text-sm text-blue-500 hover:text-blue-700">+ Agregar campo</button>
                  )}
                </div>
                <table className="w-full text-sm">
                  <thead><tr className="text-left text-gray-500 dark:text-gray-400">
                    <th className="pb-1">Campo Externo</th><th className="pb-1">Campo Interno</th><th className="pb-1">Tipo</th><th className="pb-1">Requerido</th><th className="pb-1">Orden</th><th className="pb-1"></th>
                  </tr></thead>
                  <tbody>
                    {f.campos.map(c => (
                      <tr key={c.id} className="border-t dark:border-gray-700">
                        <td className="py-1 dark:text-gray-200">{c.nombreCampoExterno}</td>
                        <td className="py-1 dark:text-gray-200">{c.nombreCampoInterno}</td>
                        <td className="py-1 dark:text-gray-200">{c.tipoDato}</td>
                        <td className="py-1 dark:text-gray-200">{c.requerido ? 'Si' : 'No'}</td>
                        <td className="py-1 dark:text-gray-200">{c.ordenExtraccion}</td>
                        <td className="py-1">{canManage && <button onClick={() => delCampo(c.id)} className="text-red-500 hover:text-red-700"><Trash2 size={14} /></button>}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {f.endpointEjemplo && (
                  <div className="mt-2"><p className="text-xs text-gray-500 dark:text-gray-400">Ejemplo:</p><pre className="text-xs bg-gray-100 dark:bg-gray-700 p-2 rounded mt-1 overflow-x-auto dark:text-gray-200">{f.endpointEjemplo}</pre></div>
                )}
              </div>
            )}
          </div>
        ))}
        {fabricantes.length === 0 && <p className="text-gray-400 dark:text-gray-500 text-center py-8">No hay fabricantes registrados</p>}
      </div>

      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md">
            <h3 className="text-lg font-bold mb-4 dark:text-white">{editing ? 'Editar Fabricante' : 'Nuevo Fabricante'}</h3>
            <div className="space-y-3">
              <input placeholder="Nombre" value={form.nombre} onChange={e => setForm({ ...form, nombre: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="Descripcion" value={form.descripcion} onChange={e => setForm({ ...form, descripcion: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <input placeholder="URL Documentacion" value={form.urlDocumentacion} onChange={e => setForm({ ...form, urlDocumentacion: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <textarea placeholder="Ejemplo de payload JSON" value={form.endpointEjemplo} onChange={e => setForm({ ...form, endpointEjemplo: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200 h-24 font-mono text-sm" />
            </div>
            <div className="flex justify-end gap-2 mt-4">
              <button onClick={() => setShowModal(false)} className="px-4 py-2 text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded">Cancelar</button>
              <button onClick={save} className="px-4 py-2 bg-[#1e3a5f] text-white rounded hover:bg-[#2d5a8e]">Guardar</button>
            </div>
          </div>
        </div>
      )}

      {showCampoModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md">
            <h3 className="text-lg font-bold mb-4 dark:text-white">Agregar Campo</h3>
            <div className="space-y-3">
              <input placeholder="Nombre campo externo (ej: epc)" value={campoForm.nombreCampoExterno} onChange={e => setCampoForm({ ...campoForm, nombreCampoExterno: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
              <select value={campoForm.nombreCampoInterno} onChange={e => setCampoForm({ ...campoForm, nombreCampoInterno: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200">
                {camposInternos.map(c => <option key={c} value={c}>{c}</option>)}
              </select>
              <select value={campoForm.tipoDato} onChange={e => setCampoForm({ ...campoForm, tipoDato: e.target.value })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200">
                <option value="string">String</option><option value="int">Int</option><option value="datetime">DateTime</option><option value="bool">Bool</option>
              </select>
              <label className="flex items-center gap-2 dark:text-gray-200"><input type="checkbox" checked={campoForm.requerido} onChange={e => setCampoForm({ ...campoForm, requerido: e.target.checked })} /> Requerido</label>
              <input type="number" placeholder="Orden" value={campoForm.ordenExtraccion} onChange={e => setCampoForm({ ...campoForm, ordenExtraccion: Number(e.target.value) })} className="w-full border dark:border-gray-600 rounded px-3 py-2 dark:bg-gray-700 dark:text-gray-200" />
            </div>
            <div className="flex justify-end gap-2 mt-4">
              <button onClick={() => setShowCampoModal(false)} className="px-4 py-2 text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded">Cancelar</button>
              <button onClick={saveCampo} className="px-4 py-2 bg-[#1e3a5f] text-white rounded hover:bg-[#2d5a8e]">Guardar</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
